using LunarConfig.Objects.Entries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace LunarConfig.Objects.Configuration
{
    internal class OutsideMapObjectConfiguration
    {
        public string outsideMapObjectConfig { get; set; }

        public OutsideMapObjectConfiguration(List<OutsideMapObjectEntry> entries)
        {
            outsideMapObjectConfig = string.Join("\n\n", entries);
        }

        public OutsideMapObjectConfiguration(string entries)
        {
            outsideMapObjectConfig = entries;
        }

        public void AddEntry(OutsideMapObjectEntry entry)
        {
            outsideMapObjectConfig += entry.configString + "\n\n";
        }

        public void AddEntry(string entry)
        {
            outsideMapObjectConfig += "\n\n" + entry;
        }
    }

    internal static class parseOutsideMapObjectConfiguration
    {
        public static List<OutsideMapObjectEntry> parseConfiguration(string configText)
        {
            List<OutsideMapObjectEntry> mapObjects = new List<OutsideMapObjectEntry>();
            foreach (var mapObject in Regex.Split(configText, @"(?=\[.*?\])"))
            {
                if (!string.IsNullOrWhiteSpace(mapObject))
                {
                    mapObjects.Add(new OutsideMapObjectEntry(mapObject));
                }
            }
            return mapObjects;
        }
    }
}
