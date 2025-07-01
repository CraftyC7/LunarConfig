using HarmonyLib;
using LunarConfig.Objects;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LunarConfig.Patches
{
    internal class GameNetworkManagerPatch
    {
        private static Dictionary<string, MoonDifficultyInfo> GetDefaultDictionary()
        {
            Dictionary<string, MoonDifficultyInfo> defaultDictionary = new Dictionary<string, MoonDifficultyInfo>();
            foreach (var moon in Resources.FindObjectsOfTypeAll<SelectableLevel>())
            {
                defaultDictionary.Add(moon.name, new MoonDifficultyInfo());
            }
            return defaultDictionary;
        }

        [HarmonyPatch(typeof(GameNetworkManager), "SaveItemsInShip")]
        [HarmonyPrefix]
        private static void onGameSavePrefix(GameNetworkManager __instance)
        {
            try
            {
                if (RoundManagerPatch.shouldIncrement)
                {
                    MiniLogger.LogInfo("Beginning modifying save data...");

                    RoundManagerPatch.shouldIncrement = false;

                    string save = GameNetworkManager.Instance.currentSaveFileName;
                    Dictionary<string, MoonDifficultyInfo> moonInfo = ES3.Load("Lunar_Data", save, defaultValue: GetDefaultDictionary());

                    foreach (var info in moonInfo)
                    {
                        if (RoundManagerPatch.lastLevel != info.Key)
                        {
                            info.Value.DecrementHeat();
                        }
                        else
                        {
                            info.Value.IncrementHeat();
                        }
                    }

                    ES3.Save("Lunar_Data", moonInfo, save);

                    MiniLogger.LogInfo("Data saved!");
                }
            }
            catch (Exception e)
            {
                MiniLogger.LogError($"Failed to modify values at end of day, please report this!\n{e}");
            }
        }
    }
}
