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
                        //Item is missing prefab!
                    }
                    else if (!manager.NetworkConfig.Prefabs.Contains(enemy.enemyPrefab))
                    {
                        //Item not a real item!
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

                Directory.CreateDirectory(Path.GetDirectoryName(LunarConfig.ITEM_FILE)!);
                File.WriteAllText(LunarConfig.ITEM_FILE, itemConfig.itemConfig);
                File.WriteAllText(LunarConfig.ENEMY_FILE, enemyConfig.enemyConfig);

                MiniLogger.LogInfo("Logged items!");
            }
            catch (Exception e) 
            {
                MiniLogger.LogError($"An error occured, please report this!\n{e}");
            }
        }
    }
}
