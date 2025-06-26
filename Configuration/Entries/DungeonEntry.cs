using LunarConfig.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using static UnityEngine.InputSystem.InputControlScheme.MatchResult;
using Match = System.Text.RegularExpressions.Match;

namespace LunarConfig.Configuration.Entries
{
    internal class DungeonEntry
    {
        public string configString { get; set; }

        public DungeonEntry(DungeonInfo info)
        {
            configString =
                $"[{info.dungeonID}]\n" +
                "## Tags allocated to this dungeon.\n" +
                "## Separate tags with commas.\n" +
                "# Setting type: String\n" +
                $"(LunarConfig) Tags = {string.Join(", ", info.tags)}\n";

            foreach (var multi in info.mapObjectMultipliers)
            {
                configString +=
                    "## The multiplier on the object curve of the trap.\n" +
                    "# Setting type: Float\n" +
                    $"Map Object Multiplier - {multi.Key} = {multi.Value}\n";
            }

            configString += "\n";
        }

        public DungeonEntry(string info)
        {
            configString = info;
        }
    }

    internal static class parseDungeonEntry
    {
        public static DungeonInfo parseEntry(string entry)
        {
            string GetValue(string key)
            {
                var match = Regex.Match(entry, $@"{key}\s*=\s*(.+)");
                return match.Success ? match.Groups[1].Value.Trim() : "";
            }

            string GetTagValue(string key)
            {
                foreach (string line in entry.Split(new[] { '\r', '\n' }, StringSplitOptions.None))
                {
                    var trimmedLine = line.TrimStart();

                    if (trimmedLine.StartsWith(key))
                    {
                        var match = Regex.Match(trimmedLine, $@"^{Regex.Escape(key)}\s*=\s*(.*)$");
                        if (match.Success)
                            return match.Groups[1].Value.Trim();
                    }
                }
                return "";
            }

            var mapObjectMultipliers = new Dictionary<string, float>();
            var mapObjectRegex = new Regex(@"^Map Object Multiplier\s*-\s*(\w+)\s*=\s*([\d.]+)", RegexOptions.Multiline);

            foreach (Match match in mapObjectRegex.Matches(entry))
            {
                string key = match.Groups[1].Value;
                float value = float.Parse(match.Groups[2].Value);
                mapObjectMultipliers[key] = value;
            }

            DungeonInfo info = new DungeonInfo(
                Regex.Match(entry, @"\[(.*?)\]").Groups[1].Value,
                Regex
                    .Split(GetTagValue("(LunarConfig) Tags"), @"[\s,]+")
                    .Where(tag => !string.IsNullOrWhiteSpace(tag))
                    .ToList(),
                mapObjectMultipliers
                );

            return info;
        }
    }
}