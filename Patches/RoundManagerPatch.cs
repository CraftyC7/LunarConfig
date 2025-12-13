using System;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using LunarConfig.Objects.Config;
using LethalLib.Modules;
using Dawn;

namespace LunarConfig.Patches
{
    internal class RoundManagerPatch
    {
        [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.SpawnScrapInLevel))]
        [HarmonyPriority(400)]
        [HarmonyBefore("mrov.WeatherRegistry")]
        [HarmonyPrefix]
        private static void resetScrapMultipliers(RoundManager __instance)
        {
            try
            {
                __instance.scrapAmountMultiplier = 1;
                __instance.scrapValueMultiplier = 0.4f;
            }
            catch (Exception e)
            {
                MiniLogger.LogError($"An error occured while setting moon values, please report this!\n{e}");
            }
        }

        [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.SpawnScrapInLevel))]
        [HarmonyPriority(0)]
        [HarmonyAfter("mrov.WeatherRegistry")]
        [HarmonyPrefix]
        private static void onScrapSpawnPrefix(RoundManager __instance)
        {
            LunarCentral lunarCentral = LunarConfig.central;

            try
            {
                if (LunarCentral.configureMoons)
                {
                    LunarConfigFile moonFile = lunarCentral.files[LunarConfig.MOON_FILE_NAME];
                
                    DawnMoonInfo dawnMoon = __instance.currentLevel.GetDawnInfo();
                    string uuid = LunarCentral.UUIDify(dawnMoon.Key.ToString());
                    LunarConfigEntry configuredMoon = moonFile.entries[$"{LunarCentral.NiceifyDawnUUID(uuid)} - {uuid}"];
                    HashSet<string> enabledSettings = LunarCentral.enabledMoonSettings;

                    if (configuredMoon.GetValue<bool>("Configure Content"))
                    {
                        if (enabledSettings.Contains("Amount Multiplier")) { __instance.scrapAmountMultiplier *= configuredMoon.GetValue<float>("Amount Multiplier"); }
                        if (enabledSettings.Contains("Value Multiplier")) { __instance.scrapValueMultiplier *= configuredMoon.GetValue<float>("Value Multiplier"); }
                    }
                }
            }
            catch (Exception e)
            {
                MiniLogger.LogError($"An error occured while setting moon values, please report this!\n{e}");
            }
        }
    }
}
