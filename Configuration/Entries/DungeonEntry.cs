using LunarConfig.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using static UnityEngine.InputSystem.InputControlScheme.MatchResult;

namespace LunarConfig.Configuration.Entries
{
    internal class DungeonEntry
    {
        public string configString { get; set; }

        public DungeonEntry(DungeonInfo info)
        {
            configString =
                $"[{info.dungeonID}]\n" +
                "## Tags with weights allocated to this dungeon.\n" +
                "## Represented as tag : weight.\n" +
                "# Setting type: String\n" +
                $"(LunarConfig) Tags = {string.Join(", ", info.tags)}\n\n";
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

            Dictionary<string, int> ParseList(string input)
            {
                Dictionary<string, int> result = new();

                foreach (string pair in input.Split(','))
                {
                    string[] parts = pair.Split(':');
                    if (parts.Length == 2)
                    {
                        string key = parts[0].Trim();
                        if (int.TryParse(parts[1].Trim(), out int value))
                        {
                            result[key] = value;
                        }
                    }
                }

                return result;
            }

            DungeonInfo info = new DungeonInfo(
                Regex.Match(entry, @"\[(.*?)\]").Groups[1].Value,
                ParseList(GetValue("Tags"))
                );

            return info;
        }
    }
}