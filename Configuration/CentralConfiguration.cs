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
        public bool clearTraps = false;
        public bool useTrapCurves = false;
        public List<string> tags = new List<string>();
        public List<string> itemPools = new List<string>();
        public List<string> enemyPools = new List<string>();

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
                "## Clears all traps on all moons.\n" +
                "# Setting type: Boolean\n" +
                $"Clear Traps? = {config.clearTraps.ToString().ToLower()}\n" +
                "[Tag Configuration]\n" +
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
                "## Enemy pools registered with LunarConfig.\n" +
                "## Separate pools with commas.\n" +
                "# Setting type: String\n" +
                $"Enemy Pools = {string.Join(", ", config.enemyPools)}\n";

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
            this.clearTraps = TryParseBool(GetValue("Clear Traps?"));
            this.useTrapCurves = TryParseBool(GetValue("Use Trap Curves?"));

            string tagLine = GetValue("Tags");
            this.tags = string.IsNullOrWhiteSpace(tagLine)
                ? new List<string>()
                : Regex.Split(tagLine, @"[\s,]+").Where(tag => !string.IsNullOrWhiteSpace(tag)).ToList();

            string itemLine = GetValue("Item Pools");
            this.itemPools = string.IsNullOrWhiteSpace(itemLine)
                ? new List<string>()
                : Regex.Split(itemLine, @"[\s,]+").Where(tag => !string.IsNullOrWhiteSpace(tag)).ToList();

            string enemyLine = GetValue("Enemy Pools");
            this.enemyPools = string.IsNullOrWhiteSpace(enemyLine)
                ? new List<string>()
                : Regex.Split(enemyLine, @"[\s,]+").Where(tag => !string.IsNullOrWhiteSpace(tag)).ToList();
        }
    }
}
