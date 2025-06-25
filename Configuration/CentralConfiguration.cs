using LunarConfig.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace LunarConfig.Configuration
{
    internal class CentralConfiguration
    {
        public bool clearItems = false;
        public bool clearEnemies = false;
        public bool clearDungeons = false;
        public bool logPools = false;
        public bool useTrapCurves = false;
        public List<string> tags = new List<string>();
        public List<string> itemPools = new List<string>();
        public List<string> interiorEnemyPools = new List<string>();
        public List<string> exteriorEnemyPools = new List<string>();
        public List<string> daytimeEnemyPools = new List<string>();

        public CentralConfiguration() { }

        public string CreateConfiguration(CentralConfiguration config)
        {
            string configString =
                "[Clear Weight Pools]\n" +
                "## Clears all scrap on all moons.\n" +
                "# Setting type: Boolean\n" +
                $"Clear Scrap? = {config.clearItems.ToString().ToLower()}\n" +
                "## Clears all enemies on all moons.\n" +
                "# Setting type: Boolean\n" +
                $"Clear Enemies? = {config.clearEnemies.ToString().ToLower()}\n" +
                "## Clears all interiors on all moons.\n" +
                "# Setting type: Boolean\n" +
                $"Clear Dungeons? = {config.clearDungeons.ToString().ToLower()}\n" +
                "[Tag Configuration]\n" +
                "## Log all modified pools.\n" +
                "## Preferably for use only when debugging.\n" +
                "# Setting type: Boolean\n" +
                $"Log Pools? = {config.logPools.ToString().ToLower()}\n" +
                "## Allows use of LunarConfig's trap curves.\n" +
                "## May cause issues if not all base curves are set.\n" +
                "# Setting type: Boolean\n" +
                $"Use Trap Curves? = {config.useTrapCurves.ToString().ToLower()}\n" +
                "## Tags registered with LunarConfig.\n" +
                "## Separate tags with commas.\n" +
                "# Setting type: String\n" +
                $"Tags = {string.Join(", ", config.tags)}\n" +
                "## Item pools registered with LunarConfig.\n" +
                "## Separate pools with commas.\n" +
                "# Setting type: String\n" +
                $"Item Pools = {string.Join(", ", config.itemPools)}\n" +
                "## Interior Enemy pools registered with LunarConfig.\n" +
                "## Separate pools with commas.\n" +
                "# Setting type: String\n" +
                $"Interior Enemy Pools = {string.Join(", ", config.interiorEnemyPools)}\n" +
                "## Exterior Enemy pools registered with LunarConfig.\n" +
                "## Separate pools with commas.\n" +
                "# Setting type: String\n" +
                $"Exterior Enemy Pools = {string.Join(", ", config.exteriorEnemyPools)}\n" +
                "## Daytime Enemy pools registered with LunarConfig.\n" +
                "## Separate pools with commas.\n" +
                "# Setting type: String\n" +
                $"Daytime Enemy Pools = {string.Join(", ", config.daytimeEnemyPools)}\n";

            return configString;
        }

        public CentralConfiguration(string configString)
        {
            var lines = configString.Split('\n')
                                    .Select(l => l.Trim())
                                    .Where(l => !string.IsNullOrWhiteSpace(l) && !l.StartsWith("#"))
                                    .ToList();

            string GetValue(string key)
            {
                foreach (var line in lines)
                {
                    if (line.StartsWith(key + " ="))
                    {
                        var parts = line.Split(new[] { '=' }, 2);
                        return parts.Length > 1 ? parts[1].Trim() : "";
                    }
                }
                return "";
            }

            bool TryParseBool(string value) =>
                value.Equals("true", StringComparison.OrdinalIgnoreCase);

            this.clearItems = TryParseBool(GetValue("Clear Scrap?"));
            this.clearEnemies = TryParseBool(GetValue("Clear Enemies?"));
            this.clearDungeons = TryParseBool(GetValue("Clear Dungeons?"));
            this.logPools = TryParseBool(GetValue("Log Pools?"));
            this.useTrapCurves = TryParseBool(GetValue("Use Trap Curves?"));

            string tagLine = GetValue("Tags");
            this.tags = string.IsNullOrWhiteSpace(tagLine)
                ? new List<string>()
                : Regex.Split(tagLine, @"[\s,]+").Where(tag => !string.IsNullOrWhiteSpace(tag)).ToList();

            string itemLine = GetValue("Item Pools");
            this.itemPools = string.IsNullOrWhiteSpace(itemLine)
                ? new List<string>()
                : Regex.Split(itemLine, @"[\s,]+").Where(tag => !string.IsNullOrWhiteSpace(tag)).ToList();

            string interiorEnemyLine = GetValue("Interior Enemy Pools");
            this.interiorEnemyPools = string.IsNullOrWhiteSpace(interiorEnemyLine)
                ? new List<string>()
                : Regex.Split(interiorEnemyLine, @"[\s,]+").Where(tag => !string.IsNullOrWhiteSpace(tag)).ToList();

            string exteriorEnemyLine = GetValue("Exterior Enemy Pools");
            this.exteriorEnemyPools = string.IsNullOrWhiteSpace(exteriorEnemyLine)
                ? new List<string>()
                : Regex.Split(exteriorEnemyLine, @"[\s,]+").Where(tag => !string.IsNullOrWhiteSpace(tag)).ToList();

            string daytimeEnemyLine = GetValue("Daytime Enemy Pools");
            this.daytimeEnemyPools = string.IsNullOrWhiteSpace(daytimeEnemyLine)
                ? new List<string>()
                : Regex.Split(daytimeEnemyLine, @"[\s,]+").Where(tag => !string.IsNullOrWhiteSpace(tag)).ToList();
        }
    }
}
