using BepInEx.Configuration;
using DunGen.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

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

        public List<string> GetOverrideMatches(string key)
        {
            var c = LunarConfig.central;
            var overFields = new Dictionary<string, string>();
            var matchedValues = new List<string>();

            var pattern = @"(?<key>[^:,]+?)\s*:\s*(?<value>(?:(?![^:,]+?\s*:).)*)(?:,|$)";
            var matches = Regex.Matches(LunarCentral.CleanString(GetValue<string>("Override " + key)), pattern);

            foreach (Match match in matches)
            {
                var k = LunarCentral.CleanString(match.Groups["key"].Value);
                var v = match.Groups["value"].Value.Trim();
                overFields[k] = v;
            }

            foreach (var field in overFields)
            {
                string k = field.Key;
                string v = field.Value;

                if (LunarCentral.currentStrings.Contains(k))
                {
                    matchedValues.Add(v);
                    continue;
                }

                foreach (var tag in LunarCentral.currentTags)
                {
                    if (tag.StartsWith(k))
                    {
                        matchedValues.Add(v);
                        break;
                    }
                }
            }

            return matchedValues;
        }

        public void AddField<T>(string key, string description, T defaultValue)
        {
            fields[key] = file.Bind(name, key, defaultValue, description);
        }

        public void TryAddField<T>(HashSet<string> enabled, string key, string description, T defaultValue)
        {
            if (enabled.Contains(key) && !fields.Keys.Contains(key))
            {
                fields[key] = file.Bind(name, key, defaultValue, description);
            }
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
            T baseValue = ((ConfigEntry<T>)fields[key]).Value;

            return baseValue;
        }

        public void SetValue<T>(string key, ref T obj, bool over = false)
        {
            T value = GetValue<T>(key);

            if (!EqualityComparer<T>.Default.Equals(obj, value))
            {
                obj = value;
            }
        }

        public void TrySetValue<T>(HashSet<string> enabled, string key, ref T obj)
        {
            if (enabled.Contains(key))
            {
                T value = GetValue<T>(key);

                if (!EqualityComparer<T>.Default.Equals(obj, value))
                {
                    obj = value;
                }
            }
        }

        public void SetCurve(string key, ref AnimationCurve obj, bool over = false)
        {
            AnimationCurve value = LunarCentral.StringToCurve(GetValue<string>(key));

            if (obj != value)
            {
                obj = value;
            }
        }

        /*
        public void SetItems(string key, LunarCentral central, ref List<SpawnableItemWithRarity> obj, bool over = false)
        {
            string itemString = GetValue<string>(key);

            List<(string, string)> stringList = itemString.RemoveWhitespace().Split(",").Select(entry => entry.Split(':')).Where(parts => parts.Length == 2).Select(parts => (parts[0], parts[1])).ToList();
            Dictionary<Item, int> value = new Dictionary<Item, int>();
            Dictionary<Item, float> multipliers = new Dictionary<Item, float>();
            Dictionary<Item, int> setters = new Dictionary<Item, int>();

            foreach (var entry in stringList)
            {
                string sanitizedID = ConfigHelper.SanitizeString(entry.Item1);
                if (LunarCentral.items.TryGetValue(sanitizedID, out Item item))
                {
                    if (entry.Item2.Contains("*"))
                    {
                        if (float.TryParse(entry.Item2.Replace("*", ""), out float multi))
                        {
                            multipliers[item] = multipliers.GetValueOrDefault(item, 1) * multi;
                        }
                    }
                    else if (entry.Item2.Contains("="))
                    {
                        if (int.TryParse(entry.Item2.Replace("=", ""), out int set))
                        {
                            setters[item] = set;
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
                else
                {
                    MiniLogger.LogWarning($"Failed to parse {sanitizedID}");
                }
            }

            List<SpawnableItemWithRarity> items = new List<SpawnableItemWithRarity>();

            foreach (var item in value)
            {
                int rarity = Mathf.CeilToInt(item.Value * multipliers.GetValueOrDefault(item.Key, 1));
                rarity = setters.GetValueOrDefault(item.Key, rarity);
                MiniLogger.LogInfo($"Recognized {item.Key} with {rarity} rarity");

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

        public void SetEnemies(string key, LunarCentral central, ref List<SpawnableEnemyWithRarity> obj, bool over = false)
        {
            string enemyString = GetValue<string>(key);

            List<(string, string)> stringList = enemyString.RemoveWhitespace().Split(",").Select(entry => entry.Split(':')).Where(parts => parts.Length == 2).Select(parts => (parts[0], parts[1])).ToList();
            Dictionary<EnemyType, int> value = new Dictionary<EnemyType, int>();
            Dictionary<EnemyType, float> multipliers = new Dictionary<EnemyType, float>();
            Dictionary<EnemyType, int> setters = new Dictionary<EnemyType, int>();

            foreach (var entry in stringList)
            {
                string sanitizedID = ConfigHelper.SanitizeString(entry.Item1);
                if (LunarCentral.enemies.TryGetValue(sanitizedID, out EnemyType enemy))
                {
                    if (entry.Item2.Contains("*"))
                    {
                        if (float.TryParse(entry.Item2.Replace("*", ""), out float multi))
                        {
                            multipliers[enemy] = multipliers.GetValueOrDefault(enemy, 1) * multi;
                        }
                    }
                    else if (entry.Item2.Contains("="))
                    {
                        if (int.TryParse(entry.Item2.Replace("=", ""), out int set))
                        {
                            setters[enemy] = set;
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
                else
                {
                    MiniLogger.LogWarning($"Failed to parse {sanitizedID}");
                }
            }

            List<SpawnableEnemyWithRarity> enemies = new List<SpawnableEnemyWithRarity>();

            foreach (var enemy in value)
            {
                int rarity = Mathf.CeilToInt(enemy.Value * multipliers.GetValueOrDefault(enemy.Key, 1));
                rarity = setters.GetValueOrDefault(enemy.Key, rarity);
                MiniLogger.LogInfo($"Recognized {enemy.Key} with {rarity} rarity");

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
        
        public void SetDungeons(string key, LunarCentral central, ExtendedLevel level, bool over = false)
        {
            string dungeonString = GetValue<string>(key);

            List<(string, string)> stringList = dungeonString.RemoveWhitespace().Split(",").Select(entry => entry.Split(':')).Where(parts => parts.Length == 2).Select(parts => (parts[0], parts[1])).ToList();
            Dictionary<ExtendedDungeonFlow, int> value = new Dictionary<ExtendedDungeonFlow, int>();
            Dictionary<ExtendedDungeonFlow, float> multipliers = new Dictionary<ExtendedDungeonFlow, float>();
            Dictionary<ExtendedDungeonFlow, int> setters = new Dictionary<ExtendedDungeonFlow, int>();

            foreach (var entry in stringList)
            {
                string sanitizedID = ConfigHelper.SanitizeString(entry.Item1);
                if (LunarCentral.dungeons.TryGetValue(sanitizedID, out ExtendedDungeonFlow extendedDungeon))
                {
                    DungeonFlow flow = extendedDungeon.DungeonFlow;
                    if (entry.Item2.Contains("*"))
                    {
                        if (float.TryParse(entry.Item2.Replace("*", ""), out float multi))
                        {
                            multipliers[extendedDungeon] = multipliers.GetValueOrDefault(extendedDungeon, 1) * multi;
                        }
                    }
                    else if (entry.Item2.Contains("="))
                    {
                        if (int.TryParse(entry.Item2.Replace("=", ""), out int set))
                        {
                            setters[extendedDungeon] = set;
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
                else
                {
                    MiniLogger.LogWarning($"Failed to parse {sanitizedID}");
                }
            }

            foreach (var flow in PatchedContent.ExtendedDungeonFlows)
            {
                int rarity = Mathf.CeilToInt(value.GetValueOrDefault(flow, 0) * multipliers.GetValueOrDefault(flow, 1));
                if (setters.Keys.Contains(flow))
                    rarity = setters[flow];
                MiniLogger.LogInfo($"Recognized {flow.DungeonName} with {rarity} rarity");

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
        */
    }
}
