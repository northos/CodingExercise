using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace ParagonCodingExercise.Events
{
    public class AdsbEventCollection
    {
        public List<AdsbEvent> Events;

        public AdsbEventCollection(List<AdsbEvent> eventsList)
        {
            Events = eventsList;
        }

        public static AdsbEventCollection LoadFromFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("File not found", filePath);
            }

            List<AdsbEvent> events = new List<AdsbEvent>();
            using TextReader reader = new StreamReader(filePath);
            string json = reader.ReadLine();
            while (json != null)
            {
                events.Add(AdsbEvent.FromJson(json));
                json = reader.ReadLine();
            }
            return new AdsbEventCollection(events);
        }
    }
}
