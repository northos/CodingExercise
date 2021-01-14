using ParagonCodingExercise.Airports;
using ParagonCodingExercise.Events;
using ParagonCodingExercise.Flights;
using System;
using System.Collections.Generic;

namespace ParagonCodingExercise
{
    class Program
    {
        private static string AirportsFilePath = @".\Resources\airports.json";

        // Location of ADS-B events
        private static string AdsbEventsFilePath = @".\Resources\events.txt";

        // Write generated flights here
        private static string OutputFilePath = @".\Resources\flights.txt";

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

            // Organize all ADS-B events by aircraft ID
            Dictionary<string, List<AdsbEvent>> eventsByID = new Dictionary<string, List<AdsbEvent>>();
            foreach (AdsbEvent adsbEvent in events.Events)
            {
                if(!eventsByID.ContainsKey(adsbEvent.Identifier))
                {
                    eventsByID.Add(adsbEvent.Identifier, new List<AdsbEvent>());
                }
                eventsByID[adsbEvent.Identifier].Add(adsbEvent);
            }
            
            // Create collection of identifiable flights
            FlightCollection flights = new FlightCollection();

            // For each aircraft identifier, sort the logged events by timestamp and step through to identify flights
            foreach (string identifier in eventsByID.Keys)
            {
                List<AdsbEvent> eventLog = eventsByID[identifier];
                eventLog.Sort((x, y) => x.Timestamp.CompareTo(y.Timestamp));

                string flightState = "unknown";
                Airport lastAirport = new Airport();
                DateTime departureTime = DateTime.MinValue;
                foreach (AdsbEvent adsbEvent in eventLog)
                {
                    // Find closest airport to the logged coordinates, as long as the event has coordinate values
                    GeoCoordinate eventLoc = new GeoCoordinate(adsbEvent.Latitude ?? double.NaN, adsbEvent.Longitude ?? double.NaN);
                    if (eventLoc.HasLocation())
                    {
                        Airport closestAirport = airports.GetClosestAirport(eventLoc);
                        GeoCoordinate airportLoc = new GeoCoordinate(closestAirport.Latitude, closestAirport.Longitude);
                        double airportDistance = eventLoc.GetDistanceTo(airportLoc);

                        // If event was logged within 2 miles of an airport and the aircraft was last known to be flying,
                        //  assume it has landed at that airport, and create a new Flight record
                        if (airportDistance <= 2f && flightState == "flying")
                        {
                            flightState = "landed";
                            flights.Flights.Add(new Flight
                            {
                                AircraftIdentifier = identifier,
                                DepartureTime = departureTime,
                                DepartureAirport = lastAirport.Identifier,
                                ArrivalTime = adsbEvent.Timestamp,
                                ArrivalAirport = closestAirport.Identifier
                            });
                            lastAirport = closestAirport;
                        }
                        // If the aircraft was last known to be at an airport, or has no known status, assume it has departed and record the time
                        else if (flightState == "landed" || flightState == "unknown")
                        {
                            flightState = "flying";
                            departureTime = adsbEvent.Timestamp;
                        }
                    }
                }
            }

            // Write the output data
            flights.WriteToFile(OutputFilePath);
        }
    }
}
