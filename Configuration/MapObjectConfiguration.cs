using LunarConfig.Configuration.Entries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace LunarConfig.Configuration
{
    internal class MapObjectConfiguration
    {
        public string mapObjectConfig { get; set; }

        public MapObjectConfiguration(List<MapObjectEntry> entries)
        {
            mapObjectConfig = string.Join("\n\n", entries);
        }

        public MapObjectConfiguration(string entries)
        {
            mapObjectConfig = entries;
        }

        public void AddEntry(MapObjectEntry entry)
        {
            mapObjectConfig += entry.configString + "\n\n";
        }

        public void AddEntry(string entry)
        {
            mapObjectConfig += "\n\n" + entry;
        }
    }

    internal static class parseMapObjectConfiguration
    {
        public static List<MapObjectEntry> parseConfiguration(string configText)
        {
            List<MapObjectEntry> mapObjects = new List<MapObjectEntry>();
            foreach (var mapObject in Regex.Split(configText, @"(?=\[.*?\])"))
            {
                if (!string.IsNullOrWhiteSpace(mapObject))
                {
                    mapObjects.Add(new MapObjectEntry(mapObject));
                }
            }
            return mapObjects;
        }
    }
}
