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
        public int interiorThreshold = 3;
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
                "## Under what weight should interiors be disregarded.\n" +
                "# Setting type: Integer\n" +
                $"Interior Threshold = {config.interiorThreshold}\n" +
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
            string GetValue(string key)
            {
                var match = Regex.Match(configString, $@"{key}\s*=\s*(.+)");
                return match.Success ? match.Groups[1].Value.Trim() : "";
            }

            this.clearItems = bool.Parse(GetValue(@"Clear Scrap\?").ToLower());
            this.clearEnemies = bool.Parse(GetValue(@"Clear Enemies\?").ToLower());
            this.clearDungeons = bool.Parse(GetValue(@"Clear Dungeons\?").ToLower());
            this.clearTraps = bool.Parse(GetValue(@"Clear Traps\?").ToLower());
            this.useTrapCurves = bool.Parse(GetValue(@"Use Trap Curves\?").ToLower());
            this.interiorThreshold = int.Parse(GetValue("Interior Threshold"));
            this.tags = Regex.Split(GetValue("Tags"), @"[\s,]+").Where(tag => !string.IsNullOrWhiteSpace(tag)).ToList();
            this.itemPools = Regex.Split(GetValue("Item Pools"), @"[\s,]+").Where(tag => !string.IsNullOrWhiteSpace(tag)).ToList();
            this.enemyPools = Regex.Split(GetValue("Enemy Pools"), @"[\s,]+").Where(tag => !string.IsNullOrWhiteSpace(tag)).ToList();
        }
    }
}
