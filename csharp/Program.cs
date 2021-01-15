using ParagonCodingExercise.Airports;
using ParagonCodingExercise.Events;
using ParagonCodingExercise.Flights;
using System;
using System.Collections.Generic;

namespace ParagonCodingExercise
{
    class Program
    {
        private enum FlightStatus
        {
            Unknown,
            Air,
            Ground
        }

        private static string AirportsFilePath = @".\Resources\airports.json";

        // Location of ADS-B events
        private static string AdsbEventsFilePath = @".\Resources\events.txt";

        // Write generated flights here
        private static string OutputFilePath = @".\Resources\flights.txt";

        private static double DistanceThreshold = 3f;
        private static double AltitudeThreshold = 200f;
        private static double SpeedThreshold = 50f;
        private static int TimeThreshold = 10;

        static void Main(string[] args)
        {
            Execute();

            Console.ReadKey();
        }

        private static void Execute()
        {
            // Load the input data
            AirportCollection airports = AirportCollection.LoadFromFile(AirportsFilePath);
            AdsbEventCollection events = AdsbEventCollection.LoadFromFile(AdsbEventsFilePath);
            
            // Create collection of identifiable flights
            FlightCollection flights = CalculateFlights(airports, events);

            // Write the output data
            flights.WriteToFile(OutputFilePath);
        }

        private static FlightCollection CalculateFlights(AirportCollection airports, AdsbEventCollection events)
        {
            // Organize all ADS-B events by aircraft ID
            FlightCollection flights = new FlightCollection();
            Dictionary<string, List<AdsbEvent>> eventsByID = new Dictionary<string, List<AdsbEvent>>();
            foreach (AdsbEvent adsbEvent in events.Events)
            {
                if(!eventsByID.ContainsKey(adsbEvent.Identifier))
                {
                    eventsByID.Add(adsbEvent.Identifier, new List<AdsbEvent>());
                    eventsByID[adsbEvent.Identifier].Add(adsbEvent);
                }
                else
                {
                    // Remove events that are soon after the previous one to improve performance
                    int eventCount = eventsByID[adsbEvent.Identifier].Count;
                    AdsbEvent lastEvent = eventsByID[adsbEvent.Identifier][eventCount - 1];
                    TimeSpan timeDiff = adsbEvent.Timestamp - lastEvent.Timestamp;
                    if (timeDiff.TotalSeconds >= TimeThreshold)
                    {
                        eventsByID[adsbEvent.Identifier].Add(adsbEvent);
                    }
                }
            }

            // For each aircraft identifier, step through the logged events in sequence to identify flights
            foreach (string identifier in eventsByID.Keys)
            {
                List<AdsbEvent> eventLog = eventsByID[identifier];
                FlightStatus flightStatus = FlightStatus.Unknown;
                Airport lastAirport = new Airport();
                DateTime lastGroundTime = DateTime.MinValue;
                foreach (AdsbEvent adsbEvent in eventLog)
                {
                    // Find closest airport to the logged coordinates, as long as the event has coordinate values
                    GeoCoordinate eventLoc = new GeoCoordinate(adsbEvent.Latitude ?? double.NaN, adsbEvent.Longitude ?? double.NaN);
                    if (eventLoc.HasLocation())
                    {
                        Airport closestAirport = airports.GetClosestAirport(eventLoc);
                        GeoCoordinate airportLoc = new GeoCoordinate(closestAirport.Latitude, closestAirport.Longitude);
                        double airportDistance = eventLoc.GetDistanceTo(airportLoc);
                        double altitudeDiff = adsbEvent.Altitude.HasValue ? Math.Abs(adsbEvent.Altitude.Value - closestAirport.Elevation) : 0f;
                        double speed = adsbEvent.Speed ?? 0f;

                        // If the event was logged close to an airport, assume the aircraft has landed at that airport
                        if (airportDistance <= DistanceThreshold && altitudeDiff <= AltitudeThreshold && speed <= SpeedThreshold)
                        {
                            // If this is not the first event (status unknown) and the new closest airport is different, create a new completed flight record
                            if (flightStatus != FlightStatus.Unknown && closestAirport.Identifier != lastAirport.Identifier)
                            {
                                flights.Flights.Add(new Flight
                                {
                                    AircraftIdentifier = identifier,
                                    DepartureTime = lastGroundTime,
                                    DepartureAirport = lastAirport.Identifier,
                                    ArrivalTime = adsbEvent.Timestamp,
                                    ArrivalAirport = closestAirport.Identifier
                                });
                            }
                            // In any case, update the status for the new airport
                            flightStatus = FlightStatus.Ground;
                            lastAirport = closestAirport;
                            lastGroundTime = adsbEvent.Timestamp;
                        }
                        // If the aircraft is not close to an airport and was not previously flying, set the status to airborne
                        else if ((airportDistance > DistanceThreshold || altitudeDiff > AltitudeThreshold) && flightStatus != FlightStatus.Air)
                        {
                            flightStatus = FlightStatus.Air;
                        }
                        // Otherwise, assume no status change
                    }
                }

                // If all events have been read and the aircraft was last known to be flying, record a final flight with no arrival
                if (flightStatus == FlightStatus.Air)
                {
                    flights.Flights.Add(new Flight
                    {
                        AircraftIdentifier = identifier,
                        DepartureTime = lastGroundTime,
                        DepartureAirport = lastAirport.Identifier,
                        ArrivalTime = DateTime.MaxValue,
                        ArrivalAirport = null
                    });
                }
            }

            return flights;
        }
    }
}
