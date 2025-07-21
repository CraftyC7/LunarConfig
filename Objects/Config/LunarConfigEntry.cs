using BepInEx.Configuration;
using DunGen.Graph;
using LethalLevelLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Schema;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using static Steamworks.InventoryItem;

namespace LunarConfig.Objects.Config
{
    public class LunarConfigEntry
    {
        public string name;
        public ConfigFile file;
        public Dictionary<string, ConfigEntryBase> fields = new Dictionary<string, ConfigEntryBase>();
        
        public LunarConfigEntry(ConfigFile file, string name) 
        {
            this.name = name;
            this.file = file;
        }

        public void AddField<T>(string key, string description, T defaultValue)
        {
            fields[key] = file.Bind(name, key, defaultValue, description);
        }

        public void AddFields(List<(string, string, object)> fieldList)
        {
            foreach (var field in fieldList)
            {
                fields[field.Item1] = file.Bind(name, field.Item1, field.Item3, field.Item2);
            }
        }

        public T GetValue<T>(string key)
        {
            ConfigEntry<T> entry = (ConfigEntry<T>)fields[key];
            return entry.Value;
        }

        public void SetValue<T>(string key, ref T obj)
        {
            T value = GetValue<T>(key);
            if (!EqualityComparer<T>.Default.Equals(obj, value))
            {
                obj = value;
            }
        }

        public void SetCurve(string key, ref AnimationCurve obj)
        {
            AnimationCurve value = LunarCentral.StringToCurve(GetValue<string>(key));
            if (obj != value)
            {
                obj = value;
            }
        }

        public void SetItems(string key, ref List<SpawnableItemWithRarity> obj)
        {
            List<(string, string)> stringList = GetValue<string>(key).Split(",").Select(entry => entry.Split(':')).Where(parts => parts.Length == 2).Select(parts => (parts[0], parts[1])).ToList();
            Dictionary<Item, int> value = new Dictionary<Item, int>();
            Dictionary<Item, float> multipliers = new Dictionary<Item, float>();

            // Stolen from LLL, thanks Batby <3!
            foreach (ExtendedItem extendedItem in PatchedContent.ExtendedItems)
            {
                Item item = extendedItem.Item;
                foreach (var entry in stringList)
                {
                    if (ConfigHelper.SanitizeString(item.itemName).Contains(ConfigHelper.SanitizeString(entry.Item1)) || ConfigHelper.SanitizeString(entry.Item1).Contains(ConfigHelper.SanitizeString(item.itemName)))
                    {
                        if (entry.Item2.Contains("*"))
                        {
                            if (float.TryParse(entry.Item2.Replace("*", ""), out float multi))
                            {
                                multipliers[item] = multipliers.GetValueOrDefault(item, 1) * multi;
                            }
                        }
                        else
                        {
                            if (int.TryParse(entry.Item2, out int rarity))
                            {
                                value[item] = value.GetValueOrDefault(item, 0) + rarity;
                            }
                        }
                    }
                }
            }

            List<SpawnableItemWithRarity> items = new List<SpawnableItemWithRarity>();

            foreach (var item in value)
            {
                int rarity = Mathf.CeilToInt(item.Value * multipliers.GetValueOrDefault(item.Key, 1));

                if (rarity > 0)
                {
                    SpawnableItemWithRarity spawnableItem = new SpawnableItemWithRarity();
                    spawnableItem.spawnableItem = item.Key;
                    spawnableItem.rarity = rarity;
                    items.Add(spawnableItem);
                }
            }
            
            if (obj != items)
            {
                obj = items;
            }
        }

        public void SetEnemies(string key, ref List<SpawnableEnemyWithRarity> obj)
        {
            List<(string, string)> stringList = GetValue<string>(key).Split(",").Select(entry => entry.Split(':')).Where(parts => parts.Length == 2).Select(parts => (parts[0], parts[1])).ToList();
            Dictionary<EnemyType, int> value = new Dictionary<EnemyType, int>();
            Dictionary<EnemyType, float> multipliers = new Dictionary<EnemyType, float>();

            // Stolen from LLL, thanks Batby <3!
            foreach (ExtendedEnemyType extendedEnemy in PatchedContent.ExtendedEnemyTypes)
            {
                EnemyType enemy = extendedEnemy.EnemyType;
                foreach (var entry in stringList)
                {
                    if (enemy.enemyName.ToLower().Contains(entry.Item1.ToLower()))
                    {
                        if (entry.Item2.Contains("*"))
                        {
                            if (float.TryParse(entry.Item2.Replace("*", ""), out float multi))
                            {
                                multipliers[enemy] = multipliers.GetValueOrDefault(enemy, 1) * multi;
                            }
                        }
                        else
                        {
                            if (int.TryParse(entry.Item2, out int rarity))
                            {
                                value[enemy] = value.GetValueOrDefault(enemy, 0) + rarity;
                            }
                        }
                    }
                }
            }

            foreach (ExtendedEnemyType extendedEnemy in PatchedContent.ExtendedEnemyTypes)
            {
                EnemyType enemy = extendedEnemy.EnemyType;
                foreach (var entry in stringList)
                {
                    if (enemy.enemyPrefab != null)
                    {
                        ScanNodeProperties enemyScanNode = enemy.enemyPrefab.GetComponentInChildren<ScanNodeProperties>();
                        if (enemyScanNode != null)
                        {
                            if (enemyScanNode.headerText.ToLower().Contains(entry.Item1.ToLower()) || entry.Item1.ToLower().Contains(enemyScanNode.headerText.ToLower()))
                            {
                                if (entry.Item2.Contains("*"))
                                {
                                    if (float.TryParse(entry.Item2.Replace("*", ""), out float multi))
                                    {
                                        multipliers[enemy] = multipliers.GetValueOrDefault(enemy, 1) * multi;
                                    }
                                }
                                else
                                {
                                    if (int.TryParse(entry.Item2, out int rarity))
                                    {
                                        value[enemy] = value.GetValueOrDefault(enemy, 0) + rarity;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            List<SpawnableEnemyWithRarity> enemies = new List<SpawnableEnemyWithRarity>();

            foreach (var enemy in value)
            {
                int rarity = Mathf.CeilToInt(enemy.Value * multipliers.GetValueOrDefault(enemy.Key, 1));

                if (rarity > 0)
                {
                    SpawnableEnemyWithRarity spawnableEnemy = new SpawnableEnemyWithRarity();
                    spawnableEnemy.enemyType = enemy.Key;
                    spawnableEnemy.rarity = rarity;
                    enemies.Add(spawnableEnemy);
                }
            }

            if (obj != enemies)
            {
                obj = enemies;
            }
        }
        
        public void SetDungeons(string key, ExtendedLevel level)
        {
            List<(string, string)> stringList = GetValue<string>(key).Split(",").Select(entry => entry.Split(':')).Where(parts => parts.Length == 2).Select(parts => (parts[0], parts[1])).ToList();
            Dictionary<ExtendedDungeonFlow, int> value = new Dictionary<ExtendedDungeonFlow, int>();
            Dictionary<ExtendedDungeonFlow, float> multipliers = new Dictionary<ExtendedDungeonFlow, float>();

            foreach (ExtendedDungeonFlow extendedDungeon in PatchedContent.ExtendedDungeonFlows)
            {
                if (extendedDungeon.name.ToLower() == "level13exitsextendeddungeonflow" || extendedDungeon.name.ToLower() == "level1extralargeextendeddungeonflow")
                    continue;

                DungeonFlow flow = extendedDungeon.DungeonFlow;
                foreach (var entry in stringList)
                {
                    if (ConfigHelper.SanitizeString(flow.name) == ConfigHelper.SanitizeString(entry.Item1) || ConfigHelper.SanitizeString(extendedDungeon.DungeonName) == ConfigHelper.SanitizeString(entry.Item1))
                    {
                        if (entry.Item2.Contains("*"))
                        {
                            if (float.TryParse(entry.Item2.Replace("*", ""), out float multi))
                            {
                                multipliers[extendedDungeon] = multipliers.GetValueOrDefault(extendedDungeon, 1) * multi;
                            }
                        }
                        else
                        {
                            if (int.TryParse(entry.Item2, out int rarity))
                            {
                                value[extendedDungeon] = value.GetValueOrDefault(extendedDungeon, 0) + rarity;
                            }
                        }
                    }
                }
            }

            foreach (var flow in PatchedContent.ExtendedDungeonFlows)
            {
                int rarity = Mathf.CeilToInt(value.GetValueOrDefault(flow, 0) * multipliers.GetValueOrDefault(flow, 1));

                if (flow.LevelMatchingProperties.GetDynamicRarity(level) != rarity)
                {
                    List<StringWithRarity> dungeonRarities = flow.LevelMatchingProperties.planetNames;
                    dungeonRarities.RemoveAll(entry => entry.Name == level.NumberlessPlanetName);
                    if (rarity > 0)
                    {
                        dungeonRarities.Add(new StringWithRarity(level.NumberlessPlanetName, rarity));
                    }
                    flow.LevelMatchingProperties.planetNames = dungeonRarities;
                }
            }
        }
    }
}
