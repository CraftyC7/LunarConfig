using LunarConfig.Configuration.Entries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace LunarConfig.Configuration
{
    internal class EnemyConfiguration
    {
        public string enemyConfig { get; set; }

        public EnemyConfiguration(List<EnemyEntry> entries)
        {
            enemyConfig = string.Join("\n\n", entries);
        }

        public EnemyConfiguration(string entries)
        {
            enemyConfig = entries;
        }

        public void AddEntry(EnemyEntry entry)
        {
            enemyConfig += entry.configString + "\n\n";
        }

        public void AddEntry(string entry)
        {
            enemyConfig += "\n\n" + entry;
        }
    }

    internal static class parseEnemyConfiguration
    {
        public static List<EnemyEntry> parseConfiguration(string configText)
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
