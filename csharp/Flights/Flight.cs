using System;

namespace ParagonCodingExercise.Flights
{
    public class Flight
    {
        public string AircraftIdentifier { get; set; }

        public DateTime DepartureTime { get; set; }

        public string DepartureAirport { get; set; }

        public DateTime ArrivalTime { get; set; }

        public string ArrivalAirport { get; set; }
    }
}
