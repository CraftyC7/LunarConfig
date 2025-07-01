using LunarConfig.Objects.Entries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace LunarConfig.Objects.Configuration
{
    internal class DungeonConfiguration
    {
        public string dungeonConfig { get; set; }

        public DungeonConfiguration(List<DungeonEntry> entries)
        {
            dungeonConfig = string.Join("\n\n", entries);
        }

        public DungeonConfiguration(string entries)
        {
            dungeonConfig = entries;
        }

        public void AddEntry(DungeonEntry entry)
        {
            dungeonConfig += entry.configString + "\n\n";
        }

        public void AddEntry(string entry)
        {
            dungeonConfig += "\n\n" + entry;
        }
    }

    internal static class parseDungeonConfiguration
    {
        public static List<DungeonEntry> parseConfiguration(string configText)
        {
            List<DungeonEntry> dungeons = new List<DungeonEntry>();
            foreach (var dungeon in Regex.Split(configText, @"(?=\[.*?\])"))
            {
                if (!string.IsNullOrWhiteSpace(dungeon))
                {
                    dungeons.Add(new DungeonEntry(dungeon));
                }
            }
            return dungeons;
        }
    }
}
