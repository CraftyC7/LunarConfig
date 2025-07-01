using LunarConfig.Objects.Info;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using static UnityEngine.InputSystem.InputControlScheme.MatchResult;
using Match = System.Text.RegularExpressions.Match;

namespace LunarConfig.Objects.Entries
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

            foreach (var multi in info.interiorEnemyPoolMultipliers)
            {
                configString +=
                    "## The multiplier on the weight of enemies in the pool.\n" +
                    "# Setting type: Float\n" +
                    $"Interior Enemy Pool Multiplier - {multi.Key} = {multi.Value}\n";
            }

            foreach (var multi in info.exteriorEnemyPoolMultipliers)
            {
                configString +=
                    "## The multiplier on the weight of enemies in the pool.\n" +
                    "# Setting type: Float\n" +
                    $"Exterior Enemy Pool Multiplier - {multi.Key} = {multi.Value}\n";
            }

            foreach (var multi in info.daytimeEnemyPoolMultipliers)
            {
                configString +=
                    "## The multiplier on the weight of enemies in the pool.\n" +
                    "# Setting type: Float\n" +
                    $"Daytime Enemy Pool Multiplier - {multi.Key} = {multi.Value}\n";
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
            var interiorEnemyPoolMultipliers = new Dictionary<string, float>();
            var exteriorEnemyPoolMultipliers = new Dictionary<string, float>();
            var daytimeEnemyPoolMultipliers = new Dictionary<string, float>();
            var dungeonMultipliers = new Dictionary<string, float>();

            //var mapObjectRegex = new Regex(@"^Map Object Multiplier\s*-\s*(\w+)\s*=\s*([\d.]+)", RegexOptions.Multiline);
            var itemPoolRegex = new Regex(@"^Item Pool Multiplier\s*-\s*(\w+)\s*=\s*([\d.]+)", RegexOptions.Multiline);
            var interiorEnemyPoolRegex = new Regex(@"^Interior Enemy Pool Multiplier\s*-\s*(\w+)\s*=\s*([\d.]+)", RegexOptions.Multiline);
            var exteriorEnemyPoolRegex = new Regex(@"^Exterior Enemy Pool Multiplier\s*-\s*(\w+)\s*=\s*([\d.]+)", RegexOptions.Multiline);
            var daytimeEnemyPoolRegex = new Regex(@"^Daytime Enemy Pool Multiplier\s*-\s*(\w+)\s*=\s*([\d.]+)", RegexOptions.Multiline);
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

            foreach (Match match in interiorEnemyPoolRegex.Matches(entry))
            {
                string key = match.Groups[1].Value;
                float value = float.Parse(match.Groups[2].Value);
                interiorEnemyPoolMultipliers[key] = value;
            }

            foreach (Match match in exteriorEnemyPoolRegex.Matches(entry))
            {
                string key = match.Groups[1].Value;
                float value = float.Parse(match.Groups[2].Value);
                exteriorEnemyPoolMultipliers[key] = value;
            }

            foreach (Match match in daytimeEnemyPoolRegex.Matches(entry))
            {
                string key = match.Groups[1].Value;
                float value = float.Parse(match.Groups[2].Value);
                daytimeEnemyPoolMultipliers[key] = value;
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
                interiorEnemyPoolMultipliers,
                exteriorEnemyPoolMultipliers,
                daytimeEnemyPoolMultipliers,
                dungeonMultipliers
                );

            return info;
        }
    }
}