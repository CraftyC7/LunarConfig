using System;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Unity.Netcode;
using LunarConfig.Objects;
using System.IO;
using LunarConfig.Config_Entries;
using Steamworks.Ugc;
using MonoMod.RuntimeDetour;

namespace LunarConfig.Patches
{
    internal class RoundManagerPatch
    {
        [HarmonyPatch(typeof(RoundManager), "Awake")]
        [HarmonyPriority(800)]
        [HarmonyPostfix]
        private static void onStartPrefix(RoundManager __instance)
        {
            try
            {
                NetworkManager manager = UnityEngine.Object.FindObjectOfType<NetworkManager>();
                HashSet<string> registeredItems = new HashSet<string>();
                ItemConfiguration itemConfig;
                Dictionary<string, ItemInfo> configuredItems = new Dictionary<string, ItemInfo>();

                MiniLogger.LogInfo("Beginning Logging...");

                
                if (File.Exists(LunarConfig.ITEM_FILE))
                {
                    itemConfig = new ItemConfiguration(File.ReadAllText(LunarConfig.ITEM_FILE));
                    foreach (ItemEntry entry in Objects.parseItemConfiguration.parseConfiguration(itemConfig.itemConfig))
                    {
                        try
                        {
                            ItemInfo item = Config_Entries.parseItemEntry.parseEntry(entry.configString);
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
                    foreach (EnemyEntry entry in Objects.parseEnemyConfiguration.parseConfiguration(enemyConfig.enemyConfig))
                    {
                        try
                        {
                            EnemyInfo enemy = Config_Entries.parseEnemyEntry.parseEntry(entry.configString);
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
                MoonConfiguration moonConfig;
                Dictionary<string, MoonInfo> configuredMoons = new Dictionary<string, MoonInfo>();

                if (File.Exists(LunarConfig.MOON_FILE))
                {
                    moonConfig = new MoonConfiguration(File.ReadAllText(LunarConfig.MOON_FILE));
                    foreach (MoonEntry entry in Objects.parseMoonConfiguration.parseConfiguration(moonConfig.moonConfig))
                    {
                        try
                        {
                            MoonInfo moon = Config_Entries.parseMoonEntry.parseEntry(entry.configString);
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
                }

                Directory.CreateDirectory(Path.GetDirectoryName(LunarConfig.ITEM_FILE)!);
                File.WriteAllText(LunarConfig.ITEM_FILE, itemConfig.itemConfig);
                File.WriteAllText(LunarConfig.ENEMY_FILE, enemyConfig.enemyConfig);
                File.WriteAllText(LunarConfig.MOON_FILE, moonConfig.moonConfig);

                MiniLogger.LogInfo("Logged items!");
            }
            catch (Exception e) 
            {
                MiniLogger.LogError($"An error occured, please report this!\n{e}");
            }
        }
    }
}
