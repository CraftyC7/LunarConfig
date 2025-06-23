using System;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Unity.Netcode;
using LunarConfig.Objects;
using System.IO;
using Steamworks.Ugc;
using MonoMod.RuntimeDetour;
using DunGen.Graph;
using static UnityEngine.Rendering.HighDefinition.ScalableSettingLevelParameter;
using System.ComponentModel;
using System.Reflection;
using LunarConfig.Configuration.Entries;
using LunarConfig.Configuration;

namespace LunarConfig.Patches
{
    internal class RoundManagerPatch
    {
        [HarmonyPatch(typeof(RoundManager), "Awake")]
        [HarmonyPriority(0)]
        [HarmonyPostfix]
        private static void onStartPrefix(RoundManager __instance)
        {
            try
            {
                NetworkManager manager = UnityEngine.Object.FindObjectOfType<NetworkManager>();
                
                CentralConfiguration central;

                if (File.Exists(LunarConfig.CENTRAL_FILE))
                {
                    central = new CentralConfiguration(File.ReadAllText(LunarConfig.CENTRAL_FILE));
                }
                else
                {
                    central = new CentralConfiguration();
                    central.CreateConfiguration(central);
                }

                HashSet<string> registeredItems = new HashSet<string>();
                ItemConfiguration itemConfig;
                Dictionary<string, ItemInfo> configuredItems = new Dictionary<string, ItemInfo>();

                MiniLogger.LogInfo("Beginning Logging...");

                
                if (File.Exists(LunarConfig.ITEM_FILE))
                {
                    itemConfig = new ItemConfiguration(File.ReadAllText(LunarConfig.ITEM_FILE));
                    foreach (ItemEntry entry in parseItemConfiguration.parseConfiguration(itemConfig.itemConfig))
                    {
                        try
                        {
                            ItemInfo item = parseItemEntry.parseEntry(entry.configString);
                            registeredItems.Add(item.itemID);
                            configuredItems.Add(item.itemID, item);
                        }
                        catch (Exception e)
                        {
                            MiniLogger.LogError($"Item Configuration File contains invalid entry, skipping entry!\n{e}");
                        }
                    }
                }
                else
                {
                    itemConfig = new ItemConfiguration("");
                }

                foreach (var item in Resources.FindObjectsOfTypeAll<Item>())
                {
                    if (item.spawnPrefab == null)
                    {
                        //Item is missing prefab!
                    }
                    else if (!manager.NetworkConfig.Prefabs.Contains(item.spawnPrefab))
                    {
                        //Item not a real item!
                    }
                    else if (registeredItems.Contains(item.name))
                    {
                        ItemInfo configuredItem = configuredItems[item.name];
                        item.itemName = configuredItem.displayName;
                        item.minValue = configuredItem.minValue;
                        item.maxValue = configuredItem.maxValue;
                        item.weight = configuredItem.weight;
                        item.isConductiveMetal = configuredItem.conductive;
                        item.twoHanded = configuredItem.twoHanded;
                        item.isScrap = configuredItem.isScrap;
                    }
                    else
                    {
                        itemConfig.AddEntry(new ItemEntry(new ItemInfo(item)));
                        MiniLogger.LogInfo($"Recorded {item.name}");
                        registeredItems.Add(item.name);
                    }
                }

                HashSet<string> registeredEnemies = new HashSet<string>();
                EnemyConfiguration enemyConfig;
                Dictionary<string, EnemyInfo> configuredEnemies = new Dictionary<string, EnemyInfo>();

                if (File.Exists(LunarConfig.ENEMY_FILE))
                {
                    enemyConfig = new EnemyConfiguration(File.ReadAllText(LunarConfig.ENEMY_FILE));
                    foreach (EnemyEntry entry in parseEnemyConfiguration.parseConfiguration(enemyConfig.enemyConfig))
                    {
                        try
                        {
                            EnemyInfo enemy = parseEnemyEntry.parseEntry(entry.configString);
                            registeredEnemies.Add(enemy.enemyID);
                            configuredEnemies.Add(enemy.enemyID, enemy);
                        }
                        catch (Exception e)
                        {
                            MiniLogger.LogError($"Enemy Configuration File contains invalid entry, skipping entry!\n{e}");
                        }
                    }
                }
                else
                {
                    enemyConfig = new EnemyConfiguration("");
                }

                foreach (var enemy in Resources.FindObjectsOfTypeAll<EnemyType>())
                {
                    if (enemy.enemyPrefab == null)
                    {
                        //Enemy is missing prefab!
                    }
                    else if (!manager.NetworkConfig.Prefabs.Contains(enemy.enemyPrefab))
                    {
                        //Enemy not a real enemy!
                    }
                    else if (registeredEnemies.Contains(enemy.name))
                    {
                        EnemyInfo configuredEnemy = configuredEnemies[enemy.name];
                        enemy.enemyName = configuredEnemy.displayName;
                        enemy.canSeeThroughFog = configuredEnemy.canSeeThroughFog;
                        enemy.doorSpeedMultiplier = configuredEnemy.doorSpeedMultiplier;
                        enemy.isDaytimeEnemy = configuredEnemy.isDaytimeEnemy;
                        enemy.isOutsideEnemy = configuredEnemy.isOutsideEnemy;
                        enemy.loudnessMultiplier = configuredEnemy.loudnessMultiplier;
                        enemy.MaxCount = configuredEnemy.maxCount;
                        enemy.PowerLevel = configuredEnemy.powerLevel;
                        enemy.probabilityCurve = configuredEnemy.probabilityCurve;
                        enemy.enemyPrefab.GetComponent<EnemyAI>().enemyHP = configuredEnemy.enemyHP;
                    }
                    else
                    {
                        enemyConfig.AddEntry(new EnemyEntry(new EnemyInfo(enemy)));
                        MiniLogger.LogInfo($"Recorded {enemy.name}");
                        registeredEnemies.Add(enemy.name);
                    }
                }
                
                HashSet<string> registeredMoons = new HashSet<string>();
                HashSet<string> registeredMapObjects = new HashSet<string>();
                //HashSet<string> registeredOutsideMapObjects = new HashSet<string>();
                MoonConfiguration moonConfig;
                MapObjectConfiguration mapObjectConfig;
                //OutsideMapObjectConfiguration outsideMapObjectConfig;
                Dictionary<string, MoonInfo> configuredMoons = new Dictionary<string, MoonInfo>();
                Dictionary<string, MapObjectInfo> configuredMapObjects = new Dictionary<string, MapObjectInfo>();
                //Dictionary<string, OutsideMapObjectInfo> configuredOutsideMapObjects = new Dictionary<string, OutsideMapObjectInfo>();

                if (File.Exists(LunarConfig.MOON_FILE))
                {
                    moonConfig = new MoonConfiguration(File.ReadAllText(LunarConfig.MOON_FILE));
                    foreach (MoonEntry entry in parseMoonConfiguration.parseConfiguration(moonConfig.moonConfig))
                    {
                        try
                        {
                            MoonInfo moon = parseMoonEntry.parseEntry(entry.configString);
                            registeredMoons.Add(moon.moonID);
                            configuredMoons.Add(moon.moonID, moon);
                        }
                        catch (Exception e)
                        {
                            MiniLogger.LogError($"Moon Configuration File contains invalid entry, skipping entry!\n{e}");
                        }
                    }
                }
                else
                {
                    moonConfig = new MoonConfiguration("");
                }

                if (File.Exists(LunarConfig.MAP_OBJECT_FILE))
                {
                    mapObjectConfig = new MapObjectConfiguration(File.ReadAllText(LunarConfig.MAP_OBJECT_FILE));
                    foreach (MapObjectEntry entry in parseMapObjectConfiguration.parseConfiguration(mapObjectConfig.mapObjectConfig))
                    {
                        try
                        {
                            MapObjectInfo mapObject = parseMapObjectEntry.parseEntry(entry.configString);
                            registeredMapObjects.Add(mapObject.objID);
                            configuredMapObjects.Add(mapObject.objID, mapObject);
                        }
                        catch (Exception e)
                        {
                            MiniLogger.LogError($"Map Object Configuration File contains invalid entry, skipping entry!\n{e}");
                        }
                    }
                }
                else
                {
                    mapObjectConfig = new MapObjectConfiguration("");
                }

                /*

                if (File.Exists(LunarConfig.OUTSIDE_MAP_OBJECT_FILE))
                {
                    outsideMapObjectConfig = new OutsideMapObjectConfiguration(File.ReadAllText(LunarConfig.OUTSIDE_MAP_OBJECT_FILE));
                    foreach (OutsideMapObjectEntry entry in Objects.parseOutsideMapObjectConfiguration.parseConfiguration(outsideMapObjectConfig.outsideMapObjectConfig))
                    {
                        try
                        {
                            OutsideMapObjectInfo mapObject = Config_Entries.parseOutsideMapObjectEntry.parseEntry(entry.configString);
                            registeredOutsideMapObjects.Add(mapObject.objID);
                            configuredOutsideMapObjects.Add(mapObject.objID, mapObject);
                        }
                        catch (Exception e)
                        {
                            MiniLogger.LogError($"Outside Map Object Configuration File contains invalid entry, skipping entry!\n{e}");
                        }
                    }
                }
                else
                {
                    outsideMapObjectConfig = new OutsideMapObjectConfiguration("");
                }

                */

                foreach (var moon in Resources.FindObjectsOfTypeAll<SelectableLevel>())
                {
                    if (registeredMoons.Contains(moon.name))
                    {
                        MoonInfo configuredMoon = configuredMoons[moon.name];
                        moon.PlanetName = configuredMoon.displayName;
                        moon.riskLevel = configuredMoon.risk;
                        moon.LevelDescription = configuredMoon.description;
                        moon.planetHasTime = configuredMoon.hasTime;
                        moon.DaySpeedMultiplier = configuredMoon.timeMultiplier;
                        moon.daytimeEnemiesProbabilityRange = configuredMoon.daytimeProbabilityRange;
                        moon.daytimeEnemySpawnChanceThroughDay = configuredMoon.daytimeCurve;
                        moon.maxDaytimeEnemyPowerCount = configuredMoon.maxDaytimePower;
                        moon.spawnProbabilityRange = configuredMoon.interiorProbabilityRange;
                        moon.enemySpawnChanceThroughoutDay = configuredMoon.interiorCurve;
                        moon.maxEnemyPowerCount = configuredMoon.maxInteriorPower;
                        moon.outsideEnemySpawnChanceThroughDay = configuredMoon.outsideCurve;
                        moon.maxOutsideEnemyPowerCount = configuredMoon.maxOutsidePower;
                        moon.minScrap = configuredMoon.minScrap;
                        moon.maxScrap = configuredMoon.maxScrap;
                        moon.factorySizeMultiplier = configuredMoon.interiorSizeMultiplier;
                    }
                    else
                    {
                        moonConfig.AddEntry(new MoonEntry(new MoonInfo(moon)));
                        MiniLogger.LogInfo($"Recorded {moon.name}");
                        registeredMoons.Add(moon.name);
                    }

                    foreach (SpawnableMapObject spawnableObject in moon.spawnableMapObjects)
                    {
                        GameObject prefab = spawnableObject.prefabToSpawn;
                        if (!manager.NetworkConfig.Prefabs.Contains(prefab))
                        {
                            // No object networked
                        }
                        else if (configuredMapObjects.Keys.Contains(prefab.name))
                        {
                            MapObjectInfo configuredMapObject = configuredMapObjects[prefab.name];
                            spawnableObject.spawnFacingAwayFromWall = configuredMapObject.faceAwayWall;
                            spawnableObject.spawnFacingWall = configuredMapObject.faceWall;
                            spawnableObject.disallowSpawningNearEntrances = configuredMapObject.disallowNearEntrance;
                            spawnableObject.requireDistanceBetweenSpawns = configuredMapObject.requireDistanceBetweenSpawns;
                            spawnableObject.spawnWithBackFlushAgainstWall = configuredMapObject.spawnFlushAgainstWall;
                            spawnableObject.spawnWithBackToWall = configuredMapObject.spawnAgainstWall;
                        }
                        else if (!registeredMapObjects.Contains(prefab.name))
                        {
                            registeredMapObjects.Add(prefab.name);
                            MiniLogger.LogInfo($"Recorded {prefab.name}");
                            mapObjectConfig.AddEntry(new MapObjectEntry(new MapObjectInfo(spawnableObject)));
                        }
                    }

                    /*

                    foreach (SpawnableOutsideObjectWithRarity spawnableObject in moon.spawnableOutsideObjects)
                    {
                        GameObject prefab = spawnableObject.spawnableObject.prefabToSpawn;
                        if (configuredOutsideMapObjects.Keys.Contains(spawnableObject.spawnableObject.name))
                        {
                            OutsideMapObjectInfo configuredMapObject = configuredOutsideMapObjects[spawnableObject.spawnableObject.name];
                            spawnableObject.spawnableObject.objectWidth = configuredMapObject.objWidth;
                            spawnableObject.spawnableObject.spawnFacingAwayFromWall = configuredMapObject.faceAwayWall;
                        }
                        else if (!registeredOutsideMapObjects.Contains(spawnableObject.spawnableObject.name))
                        {
                            registeredOutsideMapObjects.Add(spawnableObject.spawnableObject.name);
                            MiniLogger.LogInfo($"Recorded {spawnableObject.spawnableObject.name}");
                            outsideMapObjectConfig.AddEntry(new OutsideMapObjectEntry(new OutsideMapObjectInfo(spawnableObject)));
                        }
                    }

                    */
                }

                HashSet<string> registeredDungeons = new HashSet<string>();
                DungeonConfiguration dungeonConfig;
                Dictionary<string, DungeonInfo> configuredDungeons = new Dictionary<string, DungeonInfo>();

                if (File.Exists(LunarConfig.DUNGEON_FILE))
                {
                    dungeonConfig = new DungeonConfiguration(File.ReadAllText(LunarConfig.DUNGEON_FILE));
                    foreach (DungeonEntry entry in parseDungeonConfiguration.parseConfiguration(dungeonConfig.dungeonConfig))
                    {
                        try
                        {
                            DungeonInfo dungeon = parseDungeonEntry.parseEntry(entry.configString);
                            registeredDungeons.Add(dungeon.dungeonID);
                            configuredDungeons.Add(dungeon.dungeonID, dungeon);
                        }
                        catch (Exception e)
                        {
                            MiniLogger.LogError($"Dungeon Configuration File contains invalid entry, skipping entry!\n{e}");
                        }
                    }
                }
                else
                {
                    dungeonConfig = new DungeonConfiguration("");
                }

                foreach (var dungeon in Resources.FindObjectsOfTypeAll<DungeonFlow>())
                {
                    if (registeredDungeons.Contains(dungeon.name))
                    {
                        // Dungeon already recorded
                    }
                    else
                    {
                        dungeonConfig.AddEntry(new DungeonEntry(new DungeonInfo(dungeon, registeredMapObjects)));
                        MiniLogger.LogInfo($"Recorded {dungeon.name}");
                        registeredDungeons.Add(dungeon.name);
                    }
                }

                HashSet<string> registeredTags = new HashSet<string>();
                TagConfiguration tagConfig;
                List<TagInfo> configuredTags = new List<TagInfo>();

                if (File.Exists(LunarConfig.TAG_FILE))
                {
                    tagConfig = new TagConfiguration(File.ReadAllText(LunarConfig.TAG_FILE));
                    string newTagConfigString = "";
                    foreach (TagEntry entry in parseTagConfiguration.parseConfiguration(tagConfig.tagConfig))
                    {
                        try
                        {
                            TagInfo tag = parseTagEntry.parseEntry(entry.configString);
                            //List<string> mapObjects = tag.mapObjectMultipliers.Keys.ToList();
                            List<string> itemPools = tag.itemPoolMultipliers.Keys.ToList();
                            List<string> enemyPools = tag.enemyPoolMultipliers.Keys.ToList();
                            List<string> dungeons = tag.dungeonMultipliers.Keys.ToList();

                            /*
                            foreach (var obj in registeredMapObjects)
                            {
                                if (mapObjects.Contains(obj))
                                {
                                    continue;
                                }
                                else
                                {
                                    tag.mapObjectMultipliers.Add(obj, 1);
                                }
                            }
                            */

                            foreach (var obj in central.itemPools)
                            {
                                if (itemPools.Contains(obj))
                                {
                                    continue;
                                }
                                else
                                {
                                    tag.itemPoolMultipliers.Add(obj, 1);
                                }
                            }

                            foreach (var obj in central.enemyPools)
                            {
                                if (enemyPools.Contains(obj))
                                {
                                    continue;
                                }
                                else
                                {
                                    tag.enemyPoolMultipliers.Add(obj, 1);
                                }
                            }

                            foreach (var obj in registeredDungeons)
                            {
                                if (dungeons.Contains(obj))
                                {
                                    continue;
                                }
                                else
                                {
                                    tag.dungeonMultipliers.Add(obj, 1f);
                                }
                            }

                            newTagConfigString += new TagEntry(tag).configString;
                            registeredTags.Add(tag.tagID);
                            configuredTags.Add(tag);
                        }
                        catch (Exception e)
                        {
                            MiniLogger.LogError($"Tag Configuration File contains invalid entry, skipping entry!\n{e}");
                        }
                    }

                    if (newTagConfigString != tagConfig.tagConfig) 
                    {
                        tagConfig.tagConfig = newTagConfigString;
                        MiniLogger.LogInfo("Tag Configuration Updated!");
                    }
                }
                else
                {
                    tagConfig = new TagConfiguration("");
                }

                foreach (var tag in central.tags)
                {
                    if (registeredTags.Contains(tag))
                    {
                        // Tag already recorded
                    }
                    else
                    {
                        TagEntry tagEntry = new TagEntry(new TagInfo(
                            tag, 
                            1, 
                            //registeredMapObjects.ToDictionary(k => k, v => 1f),
                            central.itemPools.ToDictionary(k => k, v => 1f),
                            central.enemyPools.ToDictionary(k => k, v => 1f),
                            registeredDungeons.ToDictionary(k => k, v => 1f)
                            ));
                        tagConfig.AddEntry(tagEntry);
                        MiniLogger.LogInfo($"Recorded {tag}");
                        registeredTags.Add(tag);
                    }
                }

                Directory.CreateDirectory(Path.GetDirectoryName(LunarConfig.ITEM_FILE)!);
                File.WriteAllText(LunarConfig.ITEM_FILE, itemConfig.itemConfig);
                File.WriteAllText(LunarConfig.ENEMY_FILE, enemyConfig.enemyConfig);
                File.WriteAllText(LunarConfig.MOON_FILE, moonConfig.moonConfig);
                File.WriteAllText(LunarConfig.MAP_OBJECT_FILE, mapObjectConfig.mapObjectConfig);
                //File.WriteAllText(LunarConfig.OUTSIDE_MAP_OBJECT_FILE, outsideMapObjectConfig.outsideMapObjectConfig);
                File.WriteAllText(LunarConfig.DUNGEON_FILE, dungeonConfig.dungeonConfig);
                File.WriteAllText(LunarConfig.TAG_FILE, tagConfig.tagConfig);
                File.WriteAllText(LunarConfig.CENTRAL_FILE, central.CreateConfiguration(central));

                MiniLogger.LogInfo("Logging complete!");
            }
            catch (Exception e) 
            {
                MiniLogger.LogError($"An error occured, please report this!\n{e}");
            }
        }
    }
}
