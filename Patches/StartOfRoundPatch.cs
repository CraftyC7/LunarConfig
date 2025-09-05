using HarmonyLib;
using LethalLevelLoader;
using LunarConfig.Objects.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LunarConfig.Patches
{
    internal class StartOfRoundPatch
    {
        // For initial game startup
        [HarmonyPatch(typeof(Terminal), "Start")]
        [HarmonyPriority(800)]
        [HarmonyPostfix]
        private static void terminalPostfix()
        {
            try
            {
                MiniLogger.LogInfo("Load Patch");
                LunarCentral lunarCentral = LunarConfig.central;

                if (lunarCentral.files.Keys.Contains(LunarConfig.CENTRAL_FILE_NAME) && lunarCentral.files.Keys.Contains(LunarConfig.MOON_FILE_NAME))
                {
                    LunarConfigEntry centralConfig = lunarCentral.files[LunarConfig.CENTRAL_FILE_NAME].entries["Configuration"];

                    LunarCentral.RefreshMatchers();

                    if (centralConfig.GetValue<bool>("Configure Moons"))
                    {
                        MiniLogger.LogInfo("Changing Moons");
                        LunarConfigFile moonFile = lunarCentral.files[LunarConfig.MOON_FILE_NAME];

                        LunarConfigEntry enabledEntry = lunarCentral.files[LunarConfig.CENTRAL_FILE_NAME].entries["Enabled Moon Settings"];
                        HashSet<string> enabledSettings = new HashSet<string>();

                        foreach (var setting in enabledEntry.fields.Keys)
                        {
                            if (enabledEntry.GetValue<bool>(setting))
                            {
                                enabledSettings.Add(setting);
                            }
                        }

                        List<string> overridenSettings = new List<string>();

                        // LLL/Vanilla Moons
                        foreach (var extendedMoon in PatchedContent.ExtendedLevels)
                        {
                            try
                            {
                                LunarConfigEntry configuredMoon = moonFile.entries[lunarCentral.UUIDify($"LLL - {extendedMoon.NumberlessPlanetName} ({extendedMoon.UniqueIdentificationName})")];
                                SelectableLevel moon = extendedMoon.SelectableLevel;

                                if (configuredMoon.GetValue<bool>("Configure Content"))
                                {
                                    if (enabledSettings.Contains("Display Name")) { configuredMoon.SetValue("Display Name", ref moon.PlanetName); }
                                    if (enabledSettings.Contains("Risk Level")) { configuredMoon.SetValue("Risk Level", ref moon.riskLevel); }
                                    if (enabledSettings.Contains("Description")) { moon.LevelDescription = configuredMoon.GetValue<string>("Description").Replace(";", "\n"); }
                                    if (enabledSettings.Contains("Route Price")) { extendedMoon.RoutePrice = configuredMoon.GetValue<int>("Route Price"); }
                                    if (enabledSettings.Contains("Is Hidden?")) { extendedMoon.IsRouteHidden = configuredMoon.GetValue<bool>("Is Hidden?"); }
                                    if (enabledSettings.Contains("Is Locked?")) { extendedMoon.IsRouteLocked = configuredMoon.GetValue<bool>("Is Locked?"); }
                                }
                            }
                            catch (Exception e)
                            {
                                MiniLogger.LogError($"An error occured while setting moon values, please report this!\n{e}");
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MiniLogger.LogError($"An error occured while setting moon values, please report this!\n{e}");
            }
        }

        // For anytime level is changed
        [HarmonyPatch(typeof(StartOfRound), "ChangeLevel")]
        [HarmonyPriority(400)]
        [HarmonyPostfix]
        private static void onLoadLevel()
        {
            try
            {
                MiniLogger.LogInfo("Load Patch");
                LunarCentral lunarCentral = LunarConfig.central;

                if (lunarCentral.files.Keys.Contains(LunarConfig.CENTRAL_FILE_NAME) && lunarCentral.files.Keys.Contains(LunarConfig.MOON_FILE_NAME))
                {
                    LunarConfigEntry centralConfig = lunarCentral.files[LunarConfig.CENTRAL_FILE_NAME].entries["Configuration"];

                    LunarCentral.RefreshMatchers();

                    if (centralConfig.GetValue<bool>("Configure Moons"))
                    {
                        MiniLogger.LogInfo("Changing Moons");
                        LunarConfigFile moonFile = lunarCentral.files[LunarConfig.MOON_FILE_NAME];

                        LunarConfigEntry enabledEntry = lunarCentral.files[LunarConfig.CENTRAL_FILE_NAME].entries["Enabled Moon Settings"];
                        HashSet<string> enabledSettings = new HashSet<string>();

                        foreach (var setting in enabledEntry.fields.Keys)
                        {
                            if (enabledEntry.GetValue<bool>(setting))
                            {
                                enabledSettings.Add(setting);
                            }
                        }

                        List<string> overridenSettings = new List<string>();

                        // LLL/Vanilla Moons
                        foreach (var extendedMoon in PatchedContent.ExtendedLevels)
                        {
                            try
                            {
                                LunarConfigEntry configuredMoon = moonFile.entries[lunarCentral.UUIDify($"LLL - {extendedMoon.NumberlessPlanetName} ({extendedMoon.UniqueIdentificationName})")];
                                SelectableLevel moon = extendedMoon.SelectableLevel;

                                if (configuredMoon.GetValue<bool>("Configure Content"))
                                {
                                    if (enabledSettings.Contains("Display Name")) { configuredMoon.SetValue("Display Name", ref moon.PlanetName); }
                                    if (enabledSettings.Contains("Risk Level")) { configuredMoon.SetValue("Risk Level", ref moon.riskLevel); }
                                    if (enabledSettings.Contains("Description")) { moon.LevelDescription = configuredMoon.GetValue<string>("Description").Replace(";", "\n"); }
                                    if (enabledSettings.Contains("Route Price")) { extendedMoon.RoutePrice = configuredMoon.GetValue<int>("Route Price"); }
                                    if (enabledSettings.Contains("Is Hidden?")) { extendedMoon.IsRouteHidden = configuredMoon.GetValue<bool>("Is Hidden?"); }
                                    if (enabledSettings.Contains("Is Locked?")) { extendedMoon.IsRouteLocked = configuredMoon.GetValue<bool>("Is Locked?"); }
                                }
                            }
                            catch (Exception e)
                            {
                                MiniLogger.LogError($"An error occured while setting moon values, please report this!\n{e}");
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MiniLogger.LogError($"An error occured while setting moon values, please report this!\n{e}");
            }
        }

        [HarmonyPatch(typeof(StartOfRound), "SetTimeAndPlanetToSavedSettings")]
        [HarmonyPriority(-2000)]
        [HarmonyPrefix]
        private static void challengePrefix()
        {
            try
            {
                StartOfRound instance = StartOfRound.Instance;

                LunarCentral lunarCentral = LunarConfig.central;

                if (lunarCentral.files.Keys.Contains(LunarConfig.CENTRAL_FILE_NAME) && lunarCentral.files.Keys.Contains(LunarConfig.MOON_FILE_NAME))
                {
                    LunarConfigFile centralFile = lunarCentral.files[LunarConfig.CENTRAL_FILE_NAME];
                    LunarConfigEntry centralConfig = centralFile.entries["Configuration"];

                    if (centralConfig.GetValue<bool>("Configure Moons"))
                    {
                        LunarConfigFile moonFile = lunarCentral.files[LunarConfig.MOON_FILE_NAME];

                        LunarConfigEntry enabledEntry = centralFile.entries["Enabled Moon Settings"];
                        HashSet<string> enabledSettings = new HashSet<string>();

                        foreach (var setting in enabledEntry.fields.Keys)
                        {
                            if (enabledEntry.GetValue<bool>(setting))
                            {
                                enabledSettings.Add(setting);
                            }
                        }

                        // LLL/Vanilla Moons
                        foreach (var extendedMoon in PatchedContent.ExtendedLevels)
                        {
                            SelectableLevel moon = extendedMoon.SelectableLevel;
                            LunarConfigEntry configuredMoon = moonFile.entries[lunarCentral.UUIDify($"LLL - {extendedMoon.NumberlessPlanetName} ({extendedMoon.UniqueIdentificationName})")];

                            if (configuredMoon.GetValue<bool>("Configure Content"))
                            {
                                if (enabledSettings.Contains("Can Be Challenge Moon?")) { configuredMoon.SetValue("Can Be Challenge Moon?", ref moon.planetHasTime); }
                            }
                        }
                    }
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
                StartOfRound instance = StartOfRound.Instance;

                LunarCentral lunarCentral = LunarConfig.central;

                if (lunarCentral.files.Keys.Contains(LunarConfig.CENTRAL_FILE_NAME) && lunarCentral.files.Keys.Contains(LunarConfig.MOON_FILE_NAME))
                {
                    LunarConfigFile centralFile = lunarCentral.files[LunarConfig.CENTRAL_FILE_NAME];
                    LunarConfigEntry centralConfig = centralFile.entries["Configuration"];

                    if (centralConfig.GetValue<bool>("Configure Moons"))
                    {
                        LunarConfigFile moonFile = lunarCentral.files[LunarConfig.MOON_FILE_NAME];

                        LunarConfigEntry enabledEntry = centralFile.entries["Enabled Moon Settings"];
                        HashSet<string> enabledSettings = new HashSet<string>();

                        foreach (var setting in enabledEntry.fields.Keys)
                        {
                            if (enabledEntry.GetValue<bool>(setting))
                            {
                                enabledSettings.Add(setting);
                            }
                        }

                        // LLL/Vanilla Moons
                        foreach (var extendedMoon in PatchedContent.ExtendedLevels)
                        {
                            SelectableLevel moon = extendedMoon.SelectableLevel;
                            LunarConfigEntry configuredMoon = moonFile.entries[lunarCentral.UUIDify($"LLL - {extendedMoon.NumberlessPlanetName} ({extendedMoon.UniqueIdentificationName})")];

                            if (configuredMoon.GetValue<bool>("Configure Content"))
                            {
                                if (enabledSettings.Contains("Has Time?")) { configuredMoon.SetValue("Has Time?", ref moon.planetHasTime); }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MiniLogger.LogError($"An error occured while setting challenge moons, please report this!{e}");
            }
        }
    }
}
