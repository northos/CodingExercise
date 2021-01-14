using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace ParagonCodingExercise.Flights
{
    public class FlightCollection
    {
        public List<Flight> Flights;

        public FlightCollection()
        {
            Flights = new List<Flight>();
        }

        public FlightCollection(List<Flight> flightsList)
        {
            Flights = flightsList;
        }

        public void WriteToFile(string filePath)
        {
            // Output file will be overwritten if it already exists
            string json = JsonSerializer.Serialize<List<Flight>>(Flights);
            File.WriteAllText(filePath, json);
        }
    }
}
