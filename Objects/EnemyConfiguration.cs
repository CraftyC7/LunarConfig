using LunarConfig.Config_Entries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace LunarConfig.Objects
{
    internal class EnemyConfiguration
    {
        public String enemyConfig { get; set; }

        public EnemyConfiguration(List<EnemyEntry> entries) 
        {
            enemyConfig = string.Join("\n\n", entries);
        }

        public EnemyConfiguration(String entries)
        {
            enemyConfig = entries;
        }

        public void AddEntry(EnemyEntry entry)
        {
            enemyConfig += entry.configString + "\n\n";
        }

        public void AddEntry(String entry)
        {
            enemyConfig += "\n\n" + entry;
        }
    }

    internal static class parseEnemyConfiguration
    {
        public static List<EnemyEntry> parseConfiguration(String configText) 
        {
            List<EnemyEntry> enemies = new List<EnemyEntry>();
            foreach (var enemy in Regex.Split(configText, @"(?=\[.*?\])"))
            {
                if (!string.IsNullOrWhiteSpace(enemy))
                {
                    enemies.Add(new EnemyEntry(enemy));
                }
            }
            return enemies;
        }
    }
}
