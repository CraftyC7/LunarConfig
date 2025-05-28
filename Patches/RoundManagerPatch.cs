using System;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Unity.Netcode;
using LunarConfig.Objects;

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
                ConfigInfo configInfo = new ConfigInfo();

                HashSet<string> registeredItems = new HashSet<string>();

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
                        //Item was found twice!
                    }
                    else
                    {
                        configInfo.addItem(item);
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
                        configInfo.addEnemy(enemy);
                        registeredEnemies.Add(enemy.name);
                    }
                }

                MiniLogger.LogInfo("Beginning Logging...");

                configInfo.writeItems(LunarConfig.ITEM_FILE);

                MiniLogger.LogInfo("Logged items!");
            }
            catch (Exception e) 
            {
                MiniLogger.LogError($"An error occured, please report this! {e}");
            }
        }
    }
}
