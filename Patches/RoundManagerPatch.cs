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
                    foreach (ItemEntry entry in Objects.parseConfiguration.parseItemConfiguration(itemConfig.itemConfig))
                    {
                        try
                        {
                            ItemInfo item = Config_Entries.parseEntry.parseItemEntry(entry.configString);
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
                        //Item was found twice!
                    }
                    else
                    {
                        registeredEnemies.Add(enemy.name);
                    }
                }

                Directory.CreateDirectory(Path.GetDirectoryName(LunarConfig.ITEM_FILE)!);
                File.WriteAllText(LunarConfig.ITEM_FILE, itemConfig.itemConfig);

                MiniLogger.LogInfo("Logged items!");
            }
            catch (Exception e) 
            {
                MiniLogger.LogError($"An error occured, please report this!\n{e}");
            }
        }
    }
}
