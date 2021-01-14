using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace ParagonCodingExercise.Airports
{
    public class AirportCollection
    {
        public List<Airport> Airports;

        public AirportCollection(List<Airport> airportsList)
        {
            Airports = airportsList;
        }

        public static AirportCollection LoadFromFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("File not found", filePath);
            }

            using TextReader reader = new StreamReader(filePath);
            var json = reader.ReadToEnd();

            var airports = JsonSerializer.Deserialize<List<Airport>>(json);
            return new AirportCollection(airports);
        }

        public Airport GetClosestAirport(GeoCoordinate coordinate)
        {
            throw new NotImplementedException();
        }
    }
}
