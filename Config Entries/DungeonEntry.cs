using LunarConfig.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using static UnityEngine.InputSystem.InputControlScheme.MatchResult;

namespace LunarConfig.Config_Entries
{
    internal class DungeonEntry
    {
        public string configString { get; set; }

        public DungeonEntry(DungeonInfo info)
        {
            configString =
                $"[{info.dungeonID}]\n" +
                "## Tags allocated to the dungeon.\n" +
                "## Separate tags with commas.\n" +
                "# Setting type: String\n" +
                $"Tags = {string.Join(", ", info.tags)}\n\n";
        }

        public DungeonEntry(String info)
        {
            configString = info;
        }
    }

    internal static class parseDungeonEntry
    {
        public static DungeonInfo parseEntry(String entry)
        {
            string GetValue(string key)
            {
                var match = Regex.Match(entry, $@"{key}\s*=\s*(.+)");
                return match.Success ? match.Groups[1].Value.Trim() : "";
            }

            DungeonInfo info = new DungeonInfo(
                Regex.Match(entry, @"\[(.*?)\]").Groups[1].Value,
                Regex.Split(GetValue("Tags"), @"[\s,]+").Where(tag => !string.IsNullOrWhiteSpace(tag)).ToList()
                );

            return info;
        }
    }
}