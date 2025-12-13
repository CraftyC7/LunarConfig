using HarmonyLib;
using LunarConfig.Objects.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LunarConfig.Patches
{
    internal class StartOfRoundPatch
    {
        [HarmonyPatch(typeof(StartOfRound), "SetTimeAndPlanetToSavedSettings")]
        [HarmonyPriority(-2000)]
        [HarmonyPrefix]
        private static void challengePrefix()
        {
            try
            {
                foreach (var (moon, setting) in LunarCentral.definedChallengeMoons)
                {
                    moon.planetHasTime = setting;
                }
            }
            catch (Exception e)
            {
                MiniLogger.LogError($"An error occured while setting challenge moons, please report this!{e}");
            }
        }

        [HarmonyPatch(typeof(StartOfRound), "SetTimeAndPlanetToSavedSettings")]
        [HarmonyPriority(-2000)]
        [HarmonyPostfix]
        private static void challengePostfix()
        {
            try
            {
                foreach (var (moon, setting) in LunarCentral.definedChallengeMoonTimes)
                {
                    moon.planetHasTime = setting;
                }
            }
            catch (Exception e)
            {
                MiniLogger.LogError($"An error occured while setting challenge moons, please report this!{e}");
            }
        }
    }
}
