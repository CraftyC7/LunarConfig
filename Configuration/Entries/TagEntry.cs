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
    internal class TagEntry
    {
        public string configString { get; set; }

        public TagEntry(TagInfo info)
        {
            configString =
                $"[{info.tagID}]\n" +
                "## The multiplier applied to the peak of map object (trap) curves.\n" +
                "# Setting type: Float\n" +
                $"Map Object Peak Multiplier = {info.mapObjectPeakMultiplier}\n";
            
            /*
            foreach (var multi in info.mapObjectMultipliers)
            {
                configString +=
                    "## The multiplier on the object curve of the trap.\n" +
                    "# Setting type: Float\n" +
                    $"Map Object Multiplier - {multi.Key} = {multi.Value}\n";
            }
            */

            foreach (var multi in info.itemPoolMultipliers)
            {
                configString +=
                    "## The multiplier on the weight of items in the pool.\n" +
                    "# Setting type: Float\n" +
                    $"Item Pool Multiplier - {multi.Key} = {multi.Value}\n";
            }

            foreach (var multi in info.enemyPoolMultipliers)
            {
                configString +=
                    "## The multiplier on the weight of enemies in the pool.\n" +
                    "# Setting type: Float\n" +
                    $"Enemy Pool Multiplier - {multi.Key} = {multi.Value}\n";
            }

            foreach (var multi in info.dungeonMultipliers)
            {
                configString +=
                    "## The multiplier on the weight of the interior.\n" +
                    "# Setting type: Float\n" +
                    $"Dungeon Multiplier - {multi.Key} = {multi.Value}\n";
            }

            configString += "\n";
        }

        public TagEntry(string info)
        {
            configString = info;
        }
    }

    internal static class parseTagEntry
    {
        public static TagInfo parseEntry(string entry)
        {
            string GetValue(string key)
            {
                var match = Regex.Match(entry, $@"{key}\s*=\s*(.+)");
                return match.Success ? match.Groups[1].Value.Trim() : "";
            }

            //var mapObjectMultipliers = new Dictionary<string, float>();
            var itemPoolMultipliers = new Dictionary<string, float>();
            var enemyPoolMultipliers = new Dictionary<string, float>();
            var dungeonMultipliers = new Dictionary<string, float>();

            //var mapObjectRegex = new Regex(@"^Map Object Multiplier\s*-\s*(\w+)\s*=\s*([\d.]+)", RegexOptions.Multiline);
            var itemPoolRegex = new Regex(@"^Item Pool Multiplier\s*-\s*(\w+)\s*=\s*([\d.]+)", RegexOptions.Multiline);
            var enemyPoolRegex = new Regex(@"^Enemy Pool Multiplier\s*-\s*(\w+)\s*=\s*([\d.]+)", RegexOptions.Multiline);
            var dungeonRegex = new Regex(@"^Dungeon Multiplier\s*-\s*(\w+)\s*=\s*([\d.]+)", RegexOptions.Multiline);

            /*
            foreach (Match match in mapObjectRegex.Matches(entry))
            {
                string key = match.Groups[1].Value;
                float value = float.Parse(match.Groups[2].Value);
                mapObjectMultipliers[key] = value;
            }
            */

            foreach (Match match in itemPoolRegex.Matches(entry))
            {
                string key = match.Groups[1].Value;
                float value = float.Parse(match.Groups[2].Value);
                itemPoolMultipliers[key] = value;
            }

            foreach (Match match in enemyPoolRegex.Matches(entry))
            {
                string key = match.Groups[1].Value;
                float value = float.Parse(match.Groups[2].Value);
                enemyPoolMultipliers[key] = value;
            }

            foreach (Match match in dungeonRegex.Matches(entry))
            {
                string key = match.Groups[1].Value;
                float value = float.Parse(match.Groups[2].Value);
                dungeonMultipliers[key] = value;
            }

            TagInfo info = new TagInfo(
                Regex.Match(entry, @"\[(.*?)\]").Groups[1].Value,
                float.Parse(GetValue("Map Object Peak Multiplier")),
                //mapObjectMultipliers,
                itemPoolMultipliers,
                enemyPoolMultipliers,
                dungeonMultipliers
                );

            return info;
        }
    }
}