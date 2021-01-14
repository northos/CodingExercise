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

            using TextReader reader = new StreamReader(filePath);
            var json = reader.ReadToEnd();

            var events = JsonSerializer.Deserialize<List<AdsbEvent>>(json);
            return new AdsbEventCollection(events);
        }
    }
}
