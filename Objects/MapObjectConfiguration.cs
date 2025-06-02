using LunarConfig.Config_Entries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace LunarConfig.Objects
{
    internal class MapObjectConfiguration
    {
        public String mapObjectConfig { get; set; }

        public MapObjectConfiguration(List<MapObjectEntry> entries) 
        {
            mapObjectConfig = string.Join("\n\n", entries);
        }

        public MapObjectConfiguration(String entries)
        {
            mapObjectConfig = entries;
        }

        public void AddEntry(MapObjectEntry entry)
        {
            mapObjectConfig += entry.configString + "\n\n";
        }

        public void AddEntry(String entry)
        {
            mapObjectConfig += "\n\n" + entry;
        }
    }

    internal static class parseMapObjectConfiguration
    {
        public static List<MapObjectEntry> parseConfiguration(String configText) 
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
