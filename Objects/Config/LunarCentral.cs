using AsmResolver.PE.File;
using BepInEx;
using BepInEx.Configuration;
using Dawn;
using Dawn.Internal;
using DunGen;
using DunGen.Graph;
using Dusk;
using Dusk.Weights;
using HarmonyLib;
using LethalLib.Modules;
using Steamworks.Ugc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.UIElements;
using static ES3;
using static Unity.Properties.TypeUtility;

namespace LunarConfig.Objects.Config
{
    public class LunarCentral
    {
        public Dictionary<string, LunarConfigFile> files = new Dictionary<string, LunarConfigFile>();

        public static Dictionary<string, string> items = new Dictionary<string, string>();
        public static Dictionary<string, string> enemies = new Dictionary<string, string>();
        public static Dictionary<string, string> moons = new Dictionary<string, string>();
        public static Dictionary<string, string> dungeons = new Dictionary<string, string>();
        public static Dictionary<string, string> mapObjects = new Dictionary<string, string>();

        public static Dictionary<string, NamespacedKey<DawnMoonInfo>> moonKeys = new Dictionary<string, NamespacedKey<DawnMoonInfo>>();

        public static bool clearOrphans = false;
        public static bool backCompat = true;
        public static Dictionary<SelectableLevel, bool> definedChallengeMoons = new Dictionary<SelectableLevel, bool>();
        public static Dictionary<SelectableLevel, bool> definedChallengeMoonTimes = new Dictionary<SelectableLevel, bool>();

        public static bool centralInitialized = false;
        public static bool itemsInitialized = false;
        public static bool enemiesInitialized = false;
        public static bool moonsInitialized = false;
        public static bool dungeonsInitialized = false;
        public static bool mapObjectsInitialized = false;

        public static bool itemWeightsInitialized = false;
        public static bool enemyWeightsInitialized = false;
        public static bool dungeonWeightsInitialized = false;
        public static bool mapObjectCurvesInitialized = false;

        public static bool configureItems = false;
        public static bool configureEnemies = false;
        public static bool configureMoons = false;
        public static bool configureDungeons = false;
        public static bool configureMapObjects = false;

        public static HashSet<string> enabledItemSettings = new HashSet<string>();
        public static HashSet<string> enabledEnemySettings = new HashSet<string>();
        public static HashSet<string> enabledMoonSettings = new HashSet<string>();
        public static HashSet<string> enabledDungeonSettings = new HashSet<string>();
        public static HashSet<string> enabledMapObjectSettings = new HashSet<string>();

        public static Dictionary<string, string> cachedSpawnableScrap = new Dictionary<string, string>();
        public static Dictionary<NamespacedKey<DawnMoonInfo>, string> cachedDaytimeEnemies = new Dictionary<NamespacedKey<DawnMoonInfo>, string>();
        public static Dictionary<NamespacedKey<DawnMoonInfo>, string> cachedInteriorEnemies = new Dictionary<NamespacedKey<DawnMoonInfo>, string>();
        public static Dictionary<NamespacedKey<DawnMoonInfo>, string> cachedOutsideEnemies = new Dictionary<NamespacedKey<DawnMoonInfo>, string>();
        public static Dictionary<NamespacedKey<DawnMoonInfo>, string> cachedDungeons = new Dictionary<NamespacedKey<DawnMoonInfo>, string>();
        public static Dictionary<NamespacedKey<DawnMoonInfo>, Dictionary<string, string>> cachedInsideMapObjects = new Dictionary<NamespacedKey<DawnMoonInfo>, Dictionary<string, string>>();
        public static Dictionary<NamespacedKey<DawnMoonInfo>, Dictionary<string, string>> cachedOutsideMapObjects = new Dictionary<NamespacedKey<DawnMoonInfo>, Dictionary<string, string>>();

        public static Dictionary<string, Dictionary<NamespacedKey<DawnMoonInfo>, int>> defaultDaytimeWeights = new Dictionary<string, Dictionary<NamespacedKey<DawnMoonInfo>, int>>();
        public static Dictionary<string, Dictionary<NamespacedKey<DawnMoonInfo>, int>> defaultInteriorWeights = new Dictionary<string, Dictionary<NamespacedKey<DawnMoonInfo>, int>>();
        public static Dictionary<string, Dictionary<NamespacedKey<DawnMoonInfo>, int>> defaultOutsideWeights = new Dictionary<string, Dictionary<NamespacedKey<DawnMoonInfo>, int>>();
        public static Dictionary<string, Dictionary<NamespacedKey<DawnMoonInfo>, int>> defaultDungeonWeights = new Dictionary<string, Dictionary<NamespacedKey<DawnMoonInfo>, int>>();
        public static Dictionary<string, Dictionary<NamespacedKey<DawnMoonInfo>, AnimationCurve>> defaultInsideMapObjectCurves = new Dictionary<string, Dictionary<NamespacedKey<DawnMoonInfo>, AnimationCurve>>();
        public static Dictionary<string, Dictionary<NamespacedKey<DawnMoonInfo>, AnimationCurve>> defaultOutsideMapObjectCurves = new Dictionary<string, Dictionary<NamespacedKey<DawnMoonInfo>, AnimationCurve>>();

        public static Dictionary<string, string> itemWeightString = new Dictionary<string, string>();

        public static HashSet<DawnMoonInfo> notConfiguredScrapMoons = new HashSet<DawnMoonInfo>();
        public static HashSet<DawnMoonInfo> notConfiguredDaytimeMoons = new HashSet<DawnMoonInfo>();
        public static HashSet<DawnMoonInfo> notConfiguredInteriorMoons = new HashSet<DawnMoonInfo>();
        public static HashSet<DawnMoonInfo> notConfiguredOutsideMoons = new HashSet<DawnMoonInfo>();
        public static HashSet<DawnMoonInfo> notConfiguredDungeonMoons = new HashSet<DawnMoonInfo>();
        public static HashSet<DawnMoonInfo> notConfiguredInsideMapObjectMoons = new HashSet<DawnMoonInfo>();
        public static HashSet<DawnMoonInfo> notConfiguredOutsideMapObjectMoons = new HashSet<DawnMoonInfo>();

        public LunarCentral() { }

        // UTILS
        public static string UUIDify(string uuid)
        {
            return uuid.Replace("=", "").Replace("\n", "").Replace("\t", "").Replace("\\", "").Replace("\"", "").Replace("\'", "").Replace("[", "").Replace("]", "");
        }

        public static string CleanString(string str)
        {
            return RemoveWhitespace(str.ToLower());
        }

        public static string CleanNumber(string str)
        {
            return RemoveWhitespace(str).Replace("=", "").Replace("+", "").Replace("*", "").Replace("/", "");
        }

        // Taken from LLL
        public static string RemoveWhitespace(string input)
        {
            return new string((from c in input.ToCharArray()
                               where !char.IsWhiteSpace(c)
                               select c).ToArray());
        }

        public string CurveToString(AnimationCurve curve)
        {
            return string.Join(" ; ", curve.keys.Select(kf => $"{kf.time.ToString(System.Globalization.CultureInfo.InvariantCulture)},{kf.value.ToString(System.Globalization.CultureInfo.InvariantCulture)}"));
        }

        public string GetDawnUUID(Dictionary<string, string> dic, string uuid)
        {
            return dic[CleanString(uuid)];
        }

        // Taken from lethal.wiki
        public static void ClearOrphanedEntries(ConfigFile cfg)
        {
            if (clearOrphans)
            {
                PropertyInfo orphanedEntriesProp = AccessTools.Property(typeof(ConfigFile), "OrphanedEntries");
                var orphanedEntries = (Dictionary<ConfigDefinition, string>)orphanedEntriesProp.GetValue(cfg);
                orphanedEntries.Clear();
            }
        }

        bool TryGetEntryWithPrefix(Dictionary<string, LunarConfigEntry> dict, string prefix, out LunarConfigEntry entry)
        {
            foreach (var kvp in dict)
            {
                if (kvp.Key.StartsWith(prefix, StringComparison.Ordinal))
                {
                    entry = kvp.Value;
                    return true;
                }
            }

            entry = null;
            return false;
        }

        public static AnimationCurve StringToCurve(string data)
        {
            AnimationCurve curve = new AnimationCurve();

            if (string.IsNullOrWhiteSpace(data))
                return curve;

            foreach (var pair in data.Split(';'))
            {
                var parts = pair.Split(',');
                if (parts.Length != 2) continue;

                string timeStr = parts[0].Trim();
                string valueStr = parts[1].Trim();

                if (float.TryParse(timeStr, out float time) && float.TryParse(valueStr, out float value))
                {
                    curve.AddKey(time, value);
                }
            }

            return curve;
        }

        public static void TrySetDaytimeWeight(string item, int rarity, NamespacedKey<DawnMoonInfo> moon)
        {
            if (!defaultDaytimeWeights.TryGetValue(item, out var moonDict))
            {
                moonDict = new Dictionary<NamespacedKey<DawnMoonInfo>, int>();
                defaultDaytimeWeights[item] = moonDict;
            }

            moonDict[moon] = rarity;
        }

        public static void TrySetInteriorWeight(string item, int rarity, NamespacedKey<DawnMoonInfo> moon)
        {
            if (!defaultInteriorWeights.TryGetValue(item, out var moonDict))
            {
                moonDict = new Dictionary<NamespacedKey<DawnMoonInfo>, int>();
                defaultInteriorWeights[item] = moonDict;
            }

            moonDict[moon] = rarity;
        }

        public static void TrySetOutsideWeight(string item, int rarity, NamespacedKey<DawnMoonInfo> moon)
        {
            if (!defaultOutsideWeights.TryGetValue(item, out var moonDict))
            {
                moonDict = new Dictionary<NamespacedKey<DawnMoonInfo>, int>();
                defaultOutsideWeights[item] = moonDict;
            }

            moonDict[moon] = rarity;
        }

        public static void TrySetDungeonWeight(string item, int rarity, NamespacedKey<DawnMoonInfo> moon)
        {
            if (!defaultDungeonWeights.TryGetValue(item, out var moonDict))
            {
                moonDict = new Dictionary<NamespacedKey<DawnMoonInfo>, int>();
                defaultDungeonWeights[item] = moonDict;
            }

            moonDict[moon] = rarity;
        }

        public static void TrySetInsideCurve(string item, AnimationCurve curve, NamespacedKey<DawnMoonInfo> moon)
        {
            if (curve.keys.Length > 0)
            {
                if (!defaultInsideMapObjectCurves.TryGetValue(item, out var moonDict))
                {
                    moonDict = new Dictionary<NamespacedKey<DawnMoonInfo>, AnimationCurve>();
                    defaultInsideMapObjectCurves[item] = moonDict;
                }

                moonDict[moon] = curve;
            }
        }

        public static void TrySetOutsideCurve(string item, AnimationCurve curve, NamespacedKey<DawnMoonInfo> moon)
        {
            if (curve.keys.Length > 0)
            {
                if (!defaultOutsideMapObjectCurves.TryGetValue(item, out var moonDict))
                {
                    moonDict = new Dictionary<NamespacedKey<DawnMoonInfo>, AnimationCurve>();
                    defaultOutsideMapObjectCurves[item] = moonDict;
                }

                moonDict[moon] = curve;
            }
        }

        // FOR USE WITH DAWN IDs
        public static string ComprehendWeights(string weightString)
        {
            Dictionary<string, int> baseWeights = new Dictionary<string, int>();
            Dictionary<string, float> multiplierWeights = new Dictionary<string, float>();
            Dictionary<string, int> overwrittenWeights = new Dictionary<string, int>();

            string[] entries = weightString.Split(",");

            foreach (var entry in entries)
            {
                try
                {
                    string[] splitEntry = entry.Split(":");
                    string id = splitEntry[0] + ":" + splitEntry[1];
                    string weight = splitEntry[2];

                    if (weight.Contains("*"))
                    {
                        multiplierWeights[id] = multiplierWeights.GetValueOrDefault(id, 1) * float.Parse(CleanNumber(weight));
                    }
                    else if (weight.Contains("/"))
                    {
                        multiplierWeights[id] = multiplierWeights.GetValueOrDefault(id, 1) / float.Parse(CleanNumber(weight));
                    }
                    else if (weight.Contains("="))
                    {
                        overwrittenWeights[id] = int.Parse(CleanNumber(weight));
                    }
                    else
                    {
                        baseWeights[id] = baseWeights.GetValueOrDefault(id, 0) + int.Parse(CleanNumber(weight));
                    }
                }
                catch { }
            }

            foreach (var multiplier in multiplierWeights)
            {
                string key = multiplier.Key;
                baseWeights[key] = (int)Math.Round(baseWeights.GetValueOrDefault(key, 0) * multiplier.Value);
            }

            foreach (var over in overwrittenWeights)
            {
                string key = over.Key;
                baseWeights[key] = over.Value;
            }

            baseWeights = baseWeights.Where(kvp => kvp.Value > 0).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            return string.Join(", ", baseWeights.Select(kvp => $"{kvp.Key}=+{kvp.Value}"));
        }

        /*
        public static void RefreshMatchers()
        {
            currentStrings.Clear();
            currentTags.Clear();

            var planet = LevelManager.CurrentExtendedLevel;
            if (planet != null)
            {
                foreach (var tag in planet.ContentTags)
                    currentTags.Add(ConfigHelper.SanitizeString(tag.contentTagName));

                currentStrings.Add(ConfigHelper.SanitizeString(planet.NumberlessPlanetName));
            }

            var flow = DungeonManager.CurrentExtendedDungeonFlow;
            if (flow != null) 
            {
                currentStrings.Add(ConfigHelper.SanitizeString(flow.DungeonName));
                currentStrings.Add(ConfigHelper.SanitizeString(flow.DungeonFlow.name));
            }

            var weather = WeatherManager.GetCurrentLevelWeather();
            if (weather != null)
                currentStrings.Add(ConfigHelper.SanitizeString(weather.Name));

            foreach (var st in currentStrings)
            {
                MiniLogger.LogInfo($"MATCHER : {st}");
            }

            foreach (var st in currentTags)
            {
                MiniLogger.LogInfo($"TAG : {st}");
            }
        }
        */

        // LUNAR
        public void InitConfig()
        {
            LunarConfigEntry centralConfig = files[LunarConfig.CENTRAL_FILE_NAME].entries["Configuration"];
        }

        public void InitCentral()
        {
            MiniLogger.LogInfo("Initializing Central");
            LunarConfigFile centralFile = AddFile(LunarConfig.CENTRAL_FILE, LunarConfig.CENTRAL_FILE_NAME);
            centralFile.file.SaveOnConfigSet = false;

            LunarConfigEntry configEntry = centralFile.AddEntry("Configuration");
            configEntry.AddField("Configure Items", "Check this to generate and use configuration files for items.", true);
            configEntry.AddField("Configure Enemies", "Check this to generate and use configuration files for enemies.", true);
            configEntry.AddField("Configure Moons", "Check this to generate and use configuration files for moons.", true);
            configEntry.AddField("Configure Dungeons", "Check this to generate and use configuration files for dungeons.", true);
            configEntry.AddField("Configure Map Objects", "Check this to generate and use configuration files for map objects.", true);
            configEntry.AddField("Enable Backwards Compat", "Allows Lunar to look for config entries that are named using the previous v0.1.x system, I would advise turning this off after you have all your previous values.", false);
            configEntry.AddField("Clear Orphaned Entries", "WARNING: Enabling this will delete any config entries that get disabled when the configuration is refreshed!", false);
            backCompat = configEntry.GetValue<bool>("Enable Backwards Compat");
            clearOrphans = configEntry.GetValue<bool>("Clear Orphaned Entries");
            configureItems = configEntry.GetValue<bool>("Configure Items");
            configureEnemies = configEntry.GetValue<bool>("Configure Enemies");
            configureMoons = configEntry.GetValue<bool>("Configure Moons");
            configureDungeons = configEntry.GetValue<bool>("Configure Dungeons");
            configureMapObjects = configEntry.GetValue<bool>("Configure Map Objects");

            if (configureItems)
            {
                LunarConfigEntry configItems = centralFile.AddEntry("Enabled Item Settings");
                configItems.AddField("Display Name", "Disable this to disable configuring this property in item config entries.", true);
                configItems.AddField("Scan Name", "Disable this to disable configuring this property in item config entries.", true);
                configItems.AddField("Scan Subtext", "Disable this to disable configuring this property in item config entries.", true);
                configItems.AddField("Scan Min Range", "Disable this to disable configuring this property in item config entries.", true);
                configItems.AddField("Scan Max Range", "Disable this to disable configuring this property in item config entries.", true);
                configItems.AddField("Minimum Value", "Disable this to disable configuring this property in item config entries.", true);
                configItems.AddField("Maximum Value", "Disable this to disable configuring this property in item config entries.", true);
                configItems.AddField("Weight", "Disable this to disable configuring this property in item config entries.", true);
                configItems.AddField("Conductivity", "Disable this to disable configuring this property in item config entries.", true);
                configItems.AddField("Two-Handed", "Disable this to disable configuring this property in item config entries.", true);
                configItems.AddField("Is Scrap?", "Disable this to disable configuring this property in item config entries.", true);

                configItems.AddField("Sold In Shop?", "Enable this to enable configuring this property in item config entries.", false);
                configItems.AddField("Info Node Text", "Enable this to enable configuring this property in item config entries.", false);
                configItems.AddField("Request Node Text", "Enable this to enable configuring this property in item config entries.", false);
                configItems.AddField("Receipt Node Text", "Enable this to enable configuring this property in item config entries.", false);
                configItems.AddField("Cost", "Disable this to disable configuring this property in item config entries.", true);

                foreach (var setting in configItems.fields.Keys)
                {
                    if (configItems.GetValue<bool>(setting))
                    {
                        enabledItemSettings.Add(setting);
                    }
                }
            }

            if (configureEnemies)
            {
                LunarConfigEntry configEnemies = centralFile.AddEntry("Enabled Enemy Settings");
                configEnemies.AddField("Display Name", "Disable this to disable configuring this property in enemy config entries.", true);
                configEnemies.AddField("Scan Name", "Disable this to disable configuring this property in enemy config entries.", true);
                configEnemies.AddField("Scan Subtext", "Disable this to disable configuring this property in enemy config entries.", true);
                configEnemies.AddField("Scan Min Range", "Disable this to disable configuring this property in enemy config entries.", true);
                configEnemies.AddField("Scan Max Range", "Disable this to disable configuring this property in enemy config entries.", true);
                configEnemies.AddField("Can See Through Fog?", "Disable this to disable configuring this property in enemy config entries.", true);
                configEnemies.AddField("Door Speed Multiplier", "Disable this to disable configuring this property in enemy config entries.", true);
                configEnemies.AddField("Is Daytime Enemy?", "Disable this to disable configuring this property in enemy config entries.", true);
                configEnemies.AddField("Is Outdoor Enemy?", "Disable this to disable configuring this property in enemy config entries.", true);
                configEnemies.AddField("Loudness Multiplier", "Disable this to disable configuring this property in enemy config entries.", true);
                configEnemies.AddField("Max Count", "Disable this to disable configuring this property in enemy config entries.", true);
                configEnemies.AddField("Power Level", "Disable this to disable configuring this property in enemy config entries.", true);
                configEnemies.AddField("Probability Curve", "Disable this to disable configuring this property in enemy config entries.", true);
                configEnemies.AddField("Use Falloff?", "Disable this to disable configuring this property in enemy config entries.", true);
                configEnemies.AddField("Falloff Curve", "Disable this to disable configuring this property in enemy config entries.", true);
                configEnemies.AddField("Group Spawn Count", "Disable this to disable configuring this property in enemy config entries.", true);
                configEnemies.AddField("Normalized Time To Leave", "Disable this to disable configuring this property in enemy config entries.", true);
                configEnemies.AddField("Enemy HP", "Disable this to disable configuring this property in enemy config entries.", true);
                configEnemies.AddField("Can Die?", "Disable this to disable configuring this property in enemy config entries.", true);
                configEnemies.AddField("Destroy On Death?", "Disable this to disable configuring this property in enemy config entries.", true);
                configEnemies.AddField("Can Destroy?", "Disable this to disable configuring this property in enemy config entries.", true);
                configEnemies.AddField("Can Stun?", "Disable this to disable configuring this property in enemy config entries.", true);
                configEnemies.AddField("Stun Difficulty", "Disable this to disable configuring this property in enemy config entries.", true);
                configEnemies.AddField("Stun Time", "Disable this to disable configuring this property in enemy config entries.", true);

                configEnemies.AddField("Bestiary Text", "Enable this to enable configuring this property in enemy config entries.", false);
                configEnemies.AddField("Bestiary Keyword", "Enable this to enable configuring this property in enemy config entries.", false);

                foreach (var setting in configEnemies.fields.Keys)
                {
                    if (configEnemies.GetValue<bool>(setting))
                    {
                        enabledEnemySettings.Add(setting);
                    }
                }
            }

            if (configureMoons)
            {
                LunarConfigEntry configMoons = centralFile.AddEntry("Enabled Moon Settings");
                configMoons.AddField("Display Name", "Disable this to disable configuring this property in moon config entries.", true);
                configMoons.AddField("Risk Level", "Disable this to disable configuring this property in moon config entries.", true);
                configMoons.AddField("Description", "Disable this to disable configuring this property in moon config entries.", true);
                configMoons.AddField("Route Price", "Disable this to disable configuring this property in moon config entries.", true);
                configMoons.AddField("Is Hidden?", "Disable this to disable configuring this property in moon config entries.", true);
                configMoons.AddField("Is Locked?", "Disable this to disable configuring this property in moon config entries.", true);
                configMoons.AddField("Can Be Challenge Moon?", "Disable this to disable configuring this property in moon config entries.", true);
                configMoons.AddField("Has Time?", "Disable this to disable configuring this property in moon config entries.", true);
                configMoons.AddField("Time Multiplier", "Disable this to disable configuring this property in moon config entries.", true);
                configMoons.AddField("Daytime Probability Range", "Disable this to disable configuring this property in moon config entries.", true);
                configMoons.AddField("Daytime Curve", "Disable this to disable configuring this property in moon config entries.", true);
                configMoons.AddField("Max Daytime Power", "Disable this to disable configuring this property in moon config entries.", true);
                configMoons.AddField("Spawnable Daytime Enemies", "Disable this to disable configuring this property in moon config entries.", true);
                configMoons.AddField("Interior Probability Range", "Disable this to disable configuring this property in moon config entries.", true);
                configMoons.AddField("Interior Curve", "Disable this to disable configuring this property in moon config entries.", true);
                configMoons.AddField("Max Interior Power", "Disable this to disable configuring this property in moon config entries.", true);
                configMoons.AddField("Spawnable Interior Enemies", "Disable this to disable configuring this property in moon config entries.", true);
                configMoons.AddField("Outside Curve", "Disable this to disable configuring this property in moon config entries.", true);
                configMoons.AddField("Max Outside Power", "Disable this to disable configuring this property in moon config entries.", true);
                configMoons.AddField("Spawnable Outside Enemies", "Disable this to disable configuring this property in moon config entries.", true);
                configMoons.AddField("Min Scrap", "Disable this to disable configuring this property in moon config entries.", true);
                configMoons.AddField("Max Scrap", "Disable this to disable configuring this property in moon config entries.", true);
                configMoons.AddField("Interior Multiplier", "Disable this to disable configuring this property in moon config entries.", true);
                configMoons.AddField("Value Multiplier", "Disable this to disable configuring this property in moon config entries.", true);
                configMoons.AddField("Amount Multiplier", "Disable this to disable configuring this property in moon config entries.", true);
                configMoons.AddField("Spawnable Scrap", "Disable this to disable configuring this property in moon config entries.", true);
                configMoons.AddField("Possible Interiors", "Disable this to disable configuring this property in moon config entries.", true);
                configMoons.AddField("Tags", "Disable this to disable configuring this property in moon config entries.", true);

                foreach (var setting in configMoons.fields.Keys)
                {
                    if (configMoons.GetValue<bool>(setting))
                    {
                        enabledMoonSettings.Add(setting);
                    }
                }
            }

            if (configureDungeons)
            {
                LunarConfigEntry configDungeons = centralFile.AddEntry("Enabled Dungeon Settings");
                configDungeons.AddField("Random Size Min", "Disable this to disable configuring this property in item config entries.", true);
                configDungeons.AddField("Random Size Max", "Disable this to disable configuring this property in item config entries.", true);
                configDungeons.AddField("Map Tile Size", "Disable this to disable configuring this property in item config entries.", true);

                foreach (var setting in configDungeons.fields.Keys)
                {
                    if (configDungeons.GetValue<bool>(setting))
                    {
                        enabledDungeonSettings.Add(setting);
                    }
                }
            }

            if (configureMapObjects)
            {
                LunarConfigEntry configMapObjects = centralFile.AddEntry("Enabled Map Object Settings");
                configMapObjects.AddField("(Inside) Face Away From Wall?", "Disable this to disable configuring this property in map object config entries.", true);
                configMapObjects.AddField("(Inside) Face Towards Wall?", "Disable this to disable configuring this property in map object config entries.", true);
                configMapObjects.AddField("(Inside) Disallow Near Entrance?", "Disable this to disable configuring this property in map object config entries.", true);
                configMapObjects.AddField("(Inside) Require Distance Between Spawns?", "Disable this to disable configuring this property in map object config entries.", true);
                configMapObjects.AddField("(Inside) Flush Against Wall?", "Disable this to disable configuring this property in map object config entries.", true);
                configMapObjects.AddField("(Inside) Spawn Against Wall?", "Disable this to disable configuring this property in map object config entries.", true);
                configMapObjects.AddField("(Inside) Level Curves", "Disable this to disable configuring this property in map object config entries.", true);
                configMapObjects.AddField("(Outside) Align With Terrain?", "Disable this to disable configuring this property in map object config entries.", true);
                configMapObjects.AddField("(Outside) Object Width", "Disable this to disable configuring this property in map object config entries.", true);
                configMapObjects.AddField("(Outside) Spawnable Floor Tags", "Disable this to disable configuring this property in map object config entries.", true);
                configMapObjects.AddField("(Outside) Face Away From Wall?", "Disable this to disable configuring this property in map object config entries.", true);
                configMapObjects.AddField("(Outside) Level Curves", "Disable this to disable configuring this property in map object config entries.", true);

                foreach (var setting in configMapObjects.fields.Keys)
                {
                    if (configMapObjects.GetValue<bool>(setting))
                    {
                        enabledMapObjectSettings.Add(setting);
                    }
                }
            }

            /*
            

            if (configEntry.GetValue<bool>("Configure Moons"))
            {
                LunarConfigEntry configMoons = centralFile.AddEntry("Enabled Moon Settings");
                configMoons.AddField("Display Name", "Disable this to disable configuring this property in moon config entries.", true);
                configMoons.AddField("Risk Level", "Disable this to disable configuring this property in moon config entries.", true);
                configMoons.AddField("Description", "Disable this to disable configuring this property in moon config entries.", true);
                configMoons.AddField("Route Price", "Disable this to disable configuring this property in moon config entries.", true);
                configMoons.AddField("Is Hidden?", "Disable this to disable configuring this property in moon config entries.", true);
                configMoons.AddField("Is Locked?", "Disable this to disable configuring this property in moon config entries.", true);
                configMoons.AddField("Can Be Challenge Moon?", "Disable this to disable configuring this property in moon config entries.", true);
                configMoons.AddField("Has Time?", "Disable this to disable configuring this property in moon config entries.", true);
                configMoons.AddField("Time Multiplier", "Disable this to disable configuring this property in moon config entries.", true);
                configMoons.AddField("Daytime Probability Range", "Disable this to disable configuring this property in moon config entries.", true);
                configMoons.AddField("Daytime Curve", "Disable this to disable configuring this property in moon config entries.", true);
                configMoons.AddField("Max Daytime Power", "Disable this to disable configuring this property in moon config entries.", true);
                configMoons.AddField("Spawnable Daytime Enemies", "Disable this to disable configuring this property in moon config entries.", true);
                configMoons.AddField("Interior Probability Range", "Disable this to disable configuring this property in moon config entries.", true);
                configMoons.AddField("Interior Curve", "Disable this to disable configuring this property in moon config entries.", true);
                configMoons.AddField("Max Interior Power", "Disable this to disable configuring this property in moon config entries.", true);
                configMoons.AddField("Spawnable Interior Enemies", "Disable this to disable configuring this property in moon config entries.", true);
                configMoons.AddField("Outside Curve", "Disable this to disable configuring this property in moon config entries.", true);
                configMoons.AddField("Max Outside Power", "Disable this to disable configuring this property in moon config entries.", true);
                configMoons.AddField("Spawnable Outside Enemies", "Disable this to disable configuring this property in moon config entries.", true);
                configMoons.AddField("Min Scrap", "Disable this to disable configuring this property in moon config entries.", true);
                configMoons.AddField("Max Scrap", "Disable this to disable configuring this property in moon config entries.", true);
                configMoons.AddField("Value Multiplier", "Disable this to disable configuring this property in moon config entries.", true);
                configMoons.AddField("Amount Multiplier", "Disable this to disable configuring this property in moon config entries.", true);
                configMoons.AddField("Spawnable Scrap", "Disable this to disable configuring this property in moon config entries.", true);
                configMoons.AddField("Interior Multiplier", "Disable this to disable configuring this property in moon config entries.", true);
                configMoons.AddField("Possible Interiors", "Disable this to disable configuring this property in moon config entries.", true);
                configMoons.AddField("Tags", "Disable this to disable configuring this property in moon config entries.", true);
            }

            if (configEntry.GetValue<bool>("Configure Dungeons"))
            {
                LunarConfigEntry configDungeons = centralFile.AddEntry("Enabled Dungeon Settings");
                configDungeons.AddField("Enable Dynamic Restriction", "Disable this to disable configuring this property in item config entries.", true);
                configDungeons.AddField("Dynamic Dungeon Size Lerp Rate", "Disable this to disable configuring this property in item config entries.", true);
                configDungeons.AddField("Dynamic Dungeon Size Min", "Disable this to disable configuring this property in item config entries.", true);
                configDungeons.AddField("Dynamic Dungeon Size Max", "Disable this to disable configuring this property in item config entries.", true);
                configDungeons.AddField("Random Size Min", "Disable this to disable configuring this property in item config entries.", true);
                configDungeons.AddField("Random Size Max", "Disable this to disable configuring this property in item config entries.", true);
                configDungeons.AddField("Map Tile Size", "Disable this to disable configuring this property in item config entries.", true);
            }

            if (configEntry.GetValue<bool>("Configure Map Objects"))
            {
                LunarConfigEntry configMapObjects = centralFile.AddEntry("Enabled Map Object Settings");
                configMapObjects.AddField("Face Away From Wall?", "Disable this to disable configuring this property in map object config entries.", true);
                configMapObjects.AddField("Face Towards Wall?", "Disable this to disable configuring this property in map object config entries.", true);
                configMapObjects.AddField("Disallow Near Entrance?", "Disable this to disable configuring this property in map object config entries.", true);
                configMapObjects.AddField("Require Distance Between Spawns?", "Disable this to disable configuring this property in map object config entries.", true);
                configMapObjects.AddField("Flush Against Wall?", "Disable this to disable configuring this property in map object config entries.", true);
                configMapObjects.AddField("Spawn Against Wall?", "Disable this to disable configuring this property in map object config entries.", true);

                foreach (var level in PatchedContent.ExtendedLevels)
                {
                    configMapObjects.AddField($"Level Curve - {UUIDify(level.NumberlessPlanetName)}", "Disable this to disable configuring this property in map object config entries.", true);
                }
            }

            if (configEntry.GetValue<bool>("Configure Outside Map Objects"))
            {
                LunarConfigEntry configOutsideMapObjects = centralFile.AddEntry("Enabled Outside Map Object Settings");

                foreach (var level in PatchedContent.ExtendedLevels)
                {
                    configOutsideMapObjects.AddField($"Level Curve - {UUIDify(level.NumberlessPlanetName)}", "Disable this to disable configuring this property in outside map object config entries.", true);
                }
            }
            */

            ClearOrphanedEntries(centralFile.file);
            centralFile.file.Save();
            centralFile.file.SaveOnConfigSet = true;

            centralInitialized = true;
        }

        public void InitItems()
        {
            if (!centralInitialized)
            {
                InitCentral();
            }

            MiniLogger.LogInfo("Initializing Items");
            if (configureItems)
            {
                LunarConfigFile itemFile = AddFile(LunarConfig.ITEM_FILE, LunarConfig.ITEM_FILE_NAME);
                itemFile.file.SaveOnConfigSet = false;

                foreach (var item in LethalContent.Items)
                {
                    string uuid = UUIDify(item.Key.ToString());

                    try
                    {
                        DawnItemInfo dawnItem = item.Value;
                        LunarConfigEntry itemEntry = itemFile.AddEntry(uuid);

                        Item itemObj = dawnItem.Item;
                        ScanNodeProperties? itemScanNode = null;
                        DawnShopItemInfo? shopInfo = null;
                        DawnPurchaseInfo? purchaseInfo = null;
                        TerminalNode? infoNode = null;
                        TerminalNode? requestNode = null;
                        TerminalNode? receiptNode = null;

                        if (itemObj.spawnPrefab != null)
                        {
                            itemScanNode = itemObj.spawnPrefab.GetComponentInChildren<ScanNodeProperties>();
                        }

                        if (dawnItem.ShopInfo != null)
                        {
                            shopInfo = dawnItem.ShopInfo;

                            if (shopInfo.InfoNode != null)
                            {
                                infoNode = shopInfo.InfoNode;
                            }

                            if (shopInfo.RequestNode != null)
                            {
                                requestNode = shopInfo.RequestNode;
                            }

                            if (shopInfo.ReceiptNode != null)
                            {
                                receiptNode = shopInfo.ReceiptNode;
                            }

                            if (shopInfo.DawnPurchaseInfo != null)
                            {
                                purchaseInfo = shopInfo.DawnPurchaseInfo;
                            }
                        }

                        // GETTING VALUES (for config)
                        itemEntry.AddField("Configure Content", "Enable to change any of the settings below.", false);
                        itemEntry.AddField("Appropriate Aliases", "These are the names which LunarConfig will recognize as this object in other config options.\nThey are case-insensitve and do not regard whitespace.", $"{itemObj.itemName}, {itemObj.name}");
                        itemEntry.TryAddField(enabledItemSettings, "Display Name", "Specifies the name that appears on the item's tooltip.", itemObj.itemName);

                        if (itemScanNode != null)
                        {
                            itemEntry.TryAddField(enabledItemSettings, "Scan Name", "Specifies the name of the item that appears on its scan node.", itemScanNode.headerText);
                            itemEntry.TryAddField(enabledItemSettings, "Scan Subtext", "Specifies the subtext that appears on the item's scan node. NOTE: This setting may be overridden if the item has a scrap value.", itemScanNode.subText);
                            itemEntry.TryAddField(enabledItemSettings, "Scan Min Range", "Specifies the minimum distance the scan node can be scanned.", itemScanNode.minRange);
                            itemEntry.TryAddField(enabledItemSettings, "Scan Max Range", "Specifies the maximum distance the scan node can be scanned.", itemScanNode.maxRange);
                        }

                        itemEntry.TryAddField(enabledItemSettings, "Minimum Value", "The minimum scrap value and item can have.\nTypically multiplied by 0.4, setting not applicable to non-scrap.\nDoes not work on items like Apparatus and items from enemies (Hives, Double-barrel).", itemObj.minValue);
                        itemEntry.TryAddField(enabledItemSettings, "Maximum Value", "The maximum scrap value and item can have.\nTypically multiplied by 0.4, setting not applicable to non-scrap.\nDoes not work on items like Apparatus and items from enemies (Hives, Double-barrel).", itemObj.maxValue);
                        itemEntry.TryAddField(enabledItemSettings, "Weight", "Specifies the weight of an item.\nCalculated with: (x - 1) * 105 = weight in pounds.", itemObj.weight);
                        itemEntry.TryAddField(enabledItemSettings, "Conductivity", "Specifies whether an item is conductive.", itemObj.isConductiveMetal);
                        itemEntry.TryAddField(enabledItemSettings, "Two-Handed", "Specifies whether an item is two-handed.", itemObj.twoHanded);
                        itemEntry.TryAddField(enabledItemSettings, "Is Scrap?", "Specifies if an item is scrap or gear.\nThis decides whether an item can be sold to the company for credits.", itemObj.isScrap);

                        string defaultInfoText = "This is probably an item.";
                        string defaultRequestText = "You are trying to buy an item.";
                        string defaultReceiptText = "You bought an item!";
                        int defaultCost = 15;

                        if (infoNode != null) { defaultInfoText = infoNode.displayText.Replace("\n", ";"); }
                        if (requestNode != null) { defaultRequestText = requestNode.displayText.Replace("\n", ";"); }
                        if (receiptNode != null) { defaultReceiptText = receiptNode.displayText.Replace("\n", ";"); }
                        if (purchaseInfo != null) { defaultCost = purchaseInfo.Cost.Provide(); }

                        itemEntry.TryAddField(enabledItemSettings, "Sold In Shop?", "Whether or not an item is sold in the shop. If you are enabling this on an item that has it false by default, I advise you change the settings below.", shopInfo != null);
                        itemEntry.TryAddField(enabledItemSettings, "Info Node Text", "The text of the terminal when viewing the info of an item. New lines are represented by semi-colons.", defaultInfoText);
                        itemEntry.TryAddField(enabledItemSettings, "Request Node Text", "The text of the terminal when requesting an item. New lines are represented by semi-colons.", defaultRequestText);
                        itemEntry.TryAddField(enabledItemSettings, "Receipt Node Text", "The text of the terminal after purchasing an item. New lines are represented by semi-colons.", defaultReceiptText);
                        itemEntry.TryAddField(enabledItemSettings, "Cost", "The cost of the item if it is sold in the shop.", defaultCost);

                        // LLL Backcompat
                        if (TryGetEntryWithPrefix(itemFile.entries, $"LLL - {itemObj.itemName}", out LunarConfigEntry oldLLLEntry))
                        {
                            foreach (var field in oldLLLEntry.fields)
                            {
                                if (itemEntry.fields.TryGetValue(field.Key, out ConfigEntryBase value))
                                {
                                    value.BoxedValue = field.Value.BoxedValue;
                                }
                            }
                        }

                        // LL Backcompat
                        if (TryGetEntryWithPrefix(itemFile.entries, $"LL - {itemObj.itemName}", out LunarConfigEntry oldLLEntry))
                        {
                            foreach (var field in oldLLEntry.fields)
                            {
                                if (itemEntry.fields.TryGetValue(field.Key, out ConfigEntryBase value))
                                {
                                    value.BoxedValue = field.Value.BoxedValue;
                                }
                            }
                        }

                        // SETTING VALUES
                        if (itemEntry.GetValue<bool>("Configure Content"))
                        {
                            dawnItem.Internal_AddTag(DawnLibTags.LunarConfig);

                            foreach (var key in itemEntry.GetValue<string>("Appropriate Aliases").Split(","))
                            {
                                if (!key.IsNullOrWhiteSpace()) { items[CleanString(key)] = uuid; }
                            }

                            itemEntry.TrySetValue(enabledItemSettings, "Display Name", ref itemObj.itemName);

                            if (itemScanNode != null)
                            {
                                itemEntry.TrySetValue(enabledItemSettings, "Scan Name", ref itemScanNode.headerText);
                                itemEntry.TrySetValue(enabledItemSettings, "Scan Subtext", ref itemScanNode.subText);
                                itemEntry.TrySetValue(enabledItemSettings, "Scan Min Range", ref itemScanNode.minRange);
                                itemEntry.TrySetValue(enabledItemSettings, "Scan Max Range", ref itemScanNode.maxRange);
                            }

                            itemEntry.TrySetValue(enabledItemSettings, "Minimum Value", ref itemObj.minValue);
                            itemEntry.TrySetValue(enabledItemSettings, "Maximum Value", ref itemObj.maxValue);
                            itemEntry.TrySetValue(enabledItemSettings, "Weight", ref itemObj.weight);
                            itemEntry.TrySetValue(enabledItemSettings, "Conductivity", ref itemObj.isConductiveMetal);
                            itemEntry.TrySetValue(enabledItemSettings, "Two-Handed", ref itemObj.twoHanded);
                            itemEntry.TrySetValue(enabledItemSettings, "Is Scrap?", ref itemObj.isScrap);

                            if (enabledItemSettings.Contains("Sold In Shop?"))
                            {
                                if (shopInfo != null)
                                {
                                    if (!itemEntry.GetValue<bool>("Sold In Shop?") && purchaseInfo != null)
                                    {
                                        purchaseInfo.PurchasePredicate = new ConstantTerminalPredicate(TerminalPurchaseResult.Hidden().SetFailure(true));
                                    }
                                }
                                else
                                {
                                    if (itemEntry.GetValue<bool>("Sold In Shop?"))
                                    {
                                        TerminalNode infoTemp = ScriptableObject.CreateInstance<TerminalNode>();
                                        TerminalNode requestTemp = ScriptableObject.CreateInstance<TerminalNode>();
                                        TerminalNode receiptTemp = ScriptableObject.CreateInstance<TerminalNode>();
                                        infoTemp.name = $"info_{uuid}";
                                        infoTemp.displayText = "This is probably an item.";
                                        requestTemp.name = $"request_{uuid}";
                                        requestTemp.displayText = "You are trying to buy an item.";
                                        requestTemp.itemCost = 15;
                                        receiptTemp.name = $"receipt_{uuid}";
                                        receiptTemp.displayText = "You bought an item!";
                                        receiptTemp.itemCost = 15;

                                        DawnPurchaseInfo purchaseNew =  new DawnPurchaseInfo(new SimpleProvider<int>(15), ITerminalPurchasePredicate.AlwaysSuccess());
                                        dawnItem.ShopInfo = new DawnShopItemInfo(purchaseNew, infoTemp, requestTemp, receiptTemp);
                                        shopInfo = dawnItem.ShopInfo;
                                        infoNode = shopInfo.InfoNode;
                                        requestNode = shopInfo.RequestNode;
                                        receiptNode = shopInfo.ReceiptNode;
                                        purchaseInfo = shopInfo.DawnPurchaseInfo;

                                        shopInfo.ParentInfo = dawnItem;

                                        ItemRegistrationHandler.TryRegisterItemIntoShop(itemObj);
                                    }
                                }
                            }

                            if (infoNode != null) { infoNode.displayText = itemEntry.GetValue<string>("Info Node Text").Replace(";", "\n"); }
                            if (requestNode != null) { requestNode.displayText = itemEntry.GetValue<string>("Request Node Text").Replace(";", "\n"); }
                            if (receiptNode != null) { receiptNode.displayText = itemEntry.GetValue<string>("Receipt Node Text").Replace(";", "\n"); }

                            if (purchaseInfo != null && enabledItemSettings.Contains("Cost")) { purchaseInfo.Cost = new SimpleProvider<int>(itemEntry.GetValue<int>("Cost")); }
                        }
                        else
                        {
                            items[CleanString(item.Value.Item.itemName)] = uuid;
                            items[CleanString(item.Value.Item.name)] = uuid;
                        }
                    }
                    catch (Exception e)
                    {
                        MiniLogger.LogError($"LunarConfig encountered an issue while configuring {uuid}, please report this!\n{e}");
                    }
                }

                ClearOrphanedEntries(itemFile.file);
                itemFile.file.Save();
                itemFile.file.SaveOnConfigSet = true;
            }
            else
            {
                foreach (var item in LethalContent.Items)
                {
                    string uuid = UUIDify(item.Key.ToString());

                    items[CleanString(item.Value.Item.itemName)] = uuid;
                    items[CleanString(item.Value.Item.name)] = uuid;
                }
            }

            itemsInitialized = true;

            MiniLogger.LogInfo("Completed Initializing Items");
            InitItemWeights();
        }

        public void InitItemWeights()
        {
            if (enabledMoonSettings.Contains("Spawnable Scrap") && configureMoons && moonsInitialized && itemsInitialized && !itemWeightsInitialized)
            {
                MiniLogger.LogInfo("Initializing Item Weights");

                foreach (var cache in cachedSpawnableScrap)
                {
                    foreach (var item in cache.Value.Split(","))
                    {
                        string[] splits = item.Split(":");

                        string id = splits[0];

                        string dawnID = GetDawnUUID(items, id);
                        itemWeightString[dawnID] = itemWeightString.GetValueOrDefault(dawnID, "") + cache.Key + ":" + CleanString(splits[1]) + ",";
                    }
                }

                foreach (var item in LethalContent.Items)
                {
                    string uuid = UUIDify(item.Key.ToString());

                    try
                    {
                        DawnItemInfo dawnItem = item.Value;
                        DawnScrapItemInfo scrapInfo = null;

                        if (dawnItem.ScrapInfo != null)
                        {
                            scrapInfo = dawnItem.ScrapInfo;

                            string key = item.Key.ToString();

                            foreach (var moon in notConfiguredScrapMoons)
                            {
                                int? rarity = scrapInfo.Weights.GetFor(moon);
                                if (rarity != null && rarity > 0) { itemWeightString[key] = itemWeightString.GetValueOrDefault(key, "") + moon.Key.ToString() + ":" + rarity + ","; }
                            }
                        }

                        if (!dawnItem.HasTag(DawnLibTags.LunarConfig)) { dawnItem.Internal_AddTag(DawnLibTags.LunarConfig); }

                        WeightTableBuilder<DawnMoonInfo> weightBuilder = new();
                        SpawnWeightsPreset weights = new();

                        List<NamespacedConfigWeight> Moons = NamespacedConfigWeight.ConvertManyFromString(ComprehendWeights(itemWeightString.GetValueOrDefault(item.Key.ToString(), "")));

                        weights.SetupSpawnWeightsPreset(Moons, new List<NamespacedConfigWeight>(), new List<NamespacedConfigWeight>());

                        weightBuilder.SetGlobalWeight(weights);

                        // SET WEIGHTS
                        if (scrapInfo != null)
                        {
                            scrapInfo.Weights = weightBuilder.Build();
                        }
                        else
                        {
                            dawnItem.ScrapInfo = new DawnScrapItemInfo(weightBuilder.Build());
                        }
                    }
                    catch (Exception e)
                    {
                        MiniLogger.LogError($"LunarConfig encountered an issue while configuring weights for {uuid}, please report this!\n{e}");
                    }
                }
                itemWeightsInitialized = true;
                MiniLogger.LogInfo("Completed Initializing Item Weights");
            }
        }

        public void InitEnemies()
        {
            if (!centralInitialized)
            {
                InitCentral();
            }

            MiniLogger.LogInfo("Initializing Enemies");
            if (configureEnemies)
            {
                LunarConfigFile enemyFile = AddFile(LunarConfig.ENEMY_FILE, LunarConfig.ENEMY_FILE_NAME);
                enemyFile.file.SaveOnConfigSet = false;

                foreach (var enemy in LethalContent.Enemies)
                {
                    string uuid = UUIDify(enemy.Key.ToString());

                    try
                    {
                        DawnEnemyInfo dawnEnemy = enemy.Value;
                        LunarConfigEntry enemyEntry = enemyFile.AddEntry(uuid);
                        EnemyType enemyObj = dawnEnemy.EnemyType;
                        ScanNodeProperties enemyScanNode = null;
                        string scanName = null;
                        EnemyAI enemyAI = null;
                        TerminalNode bestiary = null;
                        TerminalKeyword word = null;

                        if (enemyObj.enemyPrefab != null)
                        {
                            enemyScanNode = enemyObj.enemyPrefab.GetComponentInChildren<ScanNodeProperties>();
                            if (enemyScanNode != null)
                                scanName = enemyScanNode.headerText;

                            enemyAI = enemyObj.enemyPrefab.GetComponent<EnemyAI>();
                        }

                        if (dawnEnemy.BestiaryNode != null)
                        {
                            bestiary = dawnEnemy.BestiaryNode;
                        }

                        if (dawnEnemy.NameKeyword != null)
                        {
                            word = dawnEnemy.NameKeyword;
                        }

                        // GETTING VALUES (for config)
                        enemyEntry.AddField("Configure Content", "Enable to change any of the settings below.", false);
                        enemyEntry.AddField("Appropriate Aliases", "These are the names which LunarConfig will recognize as this object in other config options.\nThey are case-insensitve and do not regard whitespace.", $"{enemyObj.enemyName}, {scanName}");

                        if (enemyScanNode != null)
                        {
                            enemyEntry.TryAddField(enabledEnemySettings, "Scan Name", "Specifies the name of the enemy that appears on its scan node.", enemyScanNode.headerText);
                            enemyEntry.TryAddField(enabledEnemySettings, "Scan Subtext", "Specifies the subtext that appears on the enemy's scan node.", enemyScanNode.subText);
                            enemyEntry.TryAddField(enabledEnemySettings, "Scan Min Range", "Specifies the minimum distance the scan node can be scanned.", enemyScanNode.minRange);
                            enemyEntry.TryAddField(enabledEnemySettings, "Scan Max Range", "Specifies the maximum distance the scan node can be scanned.", enemyScanNode.maxRange);
                        }

                        enemyEntry.TryAddField(enabledEnemySettings, "Can See Through Fog?", "Specifies if an enemy can see through fog in foggy weather.", enemyObj.canSeeThroughFog);
                        enemyEntry.TryAddField(enabledEnemySettings, "Door Speed Multiplier", "Decides the speed at which enemies can open doors.\nCalculated with: 1 / x = time to open door in seconds.", enemyObj.doorSpeedMultiplier);
                        enemyEntry.TryAddField(enabledEnemySettings, "Loudness Multiplier", "Multiplies the volume of an enemy's sounds.", enemyObj.loudnessMultiplier);
                        enemyEntry.TryAddField(enabledEnemySettings, "Max Count", "The maximum amount of an enemy that can be alive.", enemyObj.MaxCount);
                        enemyEntry.TryAddField(enabledEnemySettings, "Power Level", "The power level an enemy occupies.", enemyObj.PowerLevel);
                        enemyEntry.TryAddField(enabledEnemySettings, "Probability Curve", "Multiplies enemy spawn weight depending on time of day.\nKeyframes represented by x,y and separated by semicolons.", CurveToString(enemyObj.probabilityCurve));
                        enemyEntry.TryAddField(enabledEnemySettings, "Use Falloff?", "Whether or not to use the falloff curve.", enemyObj.useNumberSpawnedFalloff);
                        enemyEntry.TryAddField(enabledEnemySettings, "Falloff Curve", "Multiplier to enemy spawn weight depending on how many are already spawned.\nKeyframes represented by x,y and separated by semicolons.", CurveToString(enemyObj.numberSpawnedFalloff));
                        enemyEntry.TryAddField(enabledEnemySettings, "Group Spawn Count", "The amount of entities that will spawn of this type at once.\nNOTICE: In order for this setting to work, you may need VentSpawnFixes or a similar mod.", enemyObj.spawnInGroupsOf);
                        enemyEntry.TryAddField(enabledEnemySettings, "Normalized Time To Leave", "The time that an enemy leaves represented between 0 and 1 for the start and end of the day respectively.\nWARNING: Changing this for enemies that do not normally leave during the day may cause issues.", enemyObj.normalizedTimeInDayToLeave);

                        if (enemyAI != null) { enemyEntry.TryAddField(enabledEnemySettings, "Enemy HP", "The amount of HP an enemy has.", enemyAI.enemyHP); }

                        enemyEntry.TryAddField(enabledEnemySettings, "Can Die?", "Whether or not an enemy can die.", enemyObj.canDie);
                        enemyEntry.TryAddField(enabledEnemySettings, "Destroy On Death?", "Whether or not an enemy is destroyed on death.", enemyObj.destroyOnDeath);
                        enemyEntry.TryAddField(enabledEnemySettings, "Can Destroy?", "Whether or not an enemy can be destroyed.", enemyObj.canBeDestroyed);
                        enemyEntry.TryAddField(enabledEnemySettings, "Can Stun?", "Whether or not an enemy can be stunned.\nWARNING: Enabling this for enemies that have it disabled by default will likely cause issues, as the enemy most likely does not have stunning mechanics.", enemyObj.canBeStunned);
                        enemyEntry.TryAddField(enabledEnemySettings, "Stun Difficulty", "Modifies the difficulty of using the zap gun on this enemy.", enemyObj.stunGameDifficultyMultiplier);
                        enemyEntry.TryAddField(enabledEnemySettings, "Stun Time", "Modifies the duration this enemy stays stunned.", enemyObj.stunTimeMultiplier);

                        if (bestiary != null) { enemyEntry.TryAddField(enabledEnemySettings, "Bestiary Text", "The text of the terminal when viewing the bestiary of an enemy. New lines are represented by semi-colons.", bestiary.displayText.Replace("\n", ";")); }
                        
                        if (word != null) { enemyEntry.TryAddField(enabledEnemySettings, "Bestiary Keyword", "The keyword to view the bestiary entry of an enemy.", word.word); }

                        // LLL Backcompat
                        if (TryGetEntryWithPrefix(enemyFile.entries, $"LLL - {enemyObj.enemyName}", out LunarConfigEntry oldLLLEntry))
                        {
                            foreach (var field in oldLLLEntry.fields)
                            {
                                if (enemyEntry.fields.TryGetValue(field.Key, out ConfigEntryBase value))
                                {
                                    value.BoxedValue = field.Value.BoxedValue;
                                }
                            }
                        }

                        // LL Backcompat
                        if (TryGetEntryWithPrefix(enemyFile.entries, $"LL - {enemyObj.enemyName}", out LunarConfigEntry oldLLEntry))
                        {
                            foreach (var field in oldLLEntry.fields)
                            {
                                if (enemyEntry.fields.TryGetValue(field.Key, out ConfigEntryBase value))
                                {
                                    value.BoxedValue = field.Value.BoxedValue;
                                }
                            }
                        }

                        // SETTING VALUES
                        if (enemyEntry.GetValue<bool>("Configure Content"))
                        {
                            dawnEnemy.Internal_AddTag(DawnLibTags.LunarConfig);

                            foreach (var key in enemyEntry.GetValue<string>("Appropriate Aliases").Split(","))
                            {
                                if (!key.IsNullOrWhiteSpace()) { enemies[CleanString(key)] = uuid; }
                            }

                            if (enemyScanNode != null)
                            {
                                enemyEntry.TrySetValue(enabledEnemySettings, "Scan Name", ref enemyScanNode.headerText);
                                enemyEntry.TrySetValue(enabledEnemySettings, "Scan Subtext", ref enemyScanNode.subText);
                                enemyEntry.TrySetValue(enabledEnemySettings, "Scan Min Range", ref enemyScanNode.minRange);
                                enemyEntry.TrySetValue(enabledEnemySettings, "Scan Max Range", ref enemyScanNode.maxRange);
                            }
    
                            enemyEntry.TrySetValue(enabledEnemySettings, "Can See Through Fog?", ref enemyObj.canSeeThroughFog);
                            enemyEntry.TrySetValue(enabledEnemySettings, "Door Speed Multiplier", ref enemyObj.doorSpeedMultiplier);
                            enemyEntry.TrySetValue(enabledEnemySettings, "Loudness Multiplier", ref enemyObj.loudnessMultiplier);
                            enemyEntry.TrySetValue(enabledEnemySettings, "Max Count", ref enemyObj.MaxCount);
                            enemyEntry.TrySetValue(enabledEnemySettings, "Power Level", ref enemyObj.PowerLevel);
                            if (enabledEnemySettings.Contains("Probability Curve")) { enemyObj.probabilityCurve = StringToCurve(enemyEntry.GetValue<string>("Probability Curve")); }
                            enemyEntry.TrySetValue(enabledEnemySettings, "Use Falloff?", ref enemyObj.useNumberSpawnedFalloff);
                            if (enabledEnemySettings.Contains("Falloff Curve")) { enemyObj.numberSpawnedFalloff = StringToCurve(enemyEntry.GetValue<string>("Falloff Curve")); }
                            enemyEntry.TrySetValue(enabledEnemySettings, "Group Spawn Count", ref enemyObj.spawnInGroupsOf);
                            enemyEntry.TrySetValue(enabledEnemySettings, "Normalized Time To Leave", ref enemyObj.normalizedTimeInDayToLeave);

                            if (enemyAI != null) { enemyEntry.TrySetValue(enabledEnemySettings, "Enemy HP", ref enemyAI.enemyHP); }

                            enemyEntry.TrySetValue(enabledEnemySettings, "Can Die?", ref enemyObj.canDie);
                            enemyEntry.TrySetValue(enabledEnemySettings, "Destroy On Death?", ref enemyObj.destroyOnDeath);
                            enemyEntry.TrySetValue(enabledEnemySettings, "Can Destroy?", ref enemyObj.canBeDestroyed);
                            enemyEntry.TrySetValue(enabledEnemySettings, "Can Stun?", ref enemyObj.canBeStunned);
                            enemyEntry.TrySetValue(enabledEnemySettings, "Stun Difficulty", ref enemyObj.stunGameDifficultyMultiplier);
                            enemyEntry.TrySetValue(enabledEnemySettings, "Stun Time", ref enemyObj.stunTimeMultiplier);

                            if (bestiary != null && enabledEnemySettings.Contains("Bestiary Text")) { bestiary.displayText = enemyEntry.GetValue<string>("Bestiary Text").Replace(";", "\n"); }

                            if (word != null) { enemyEntry.TrySetValue(enabledEnemySettings, "Bestiary Keyword", ref word.word); }
                        }
                        else
                        {
                            enemies[CleanString(enemyObj.enemyName)] = uuid;
                            if (!scanName.IsNullOrWhiteSpace()) { enemies[CleanString(scanName)] = uuid; }
                        }
                    }
                    catch (Exception e)
                    {
                        MiniLogger.LogError($"LunarConfig encountered an issue while configuring {uuid}, please report this!\n{e}");
                    }
                }

                ClearOrphanedEntries(enemyFile.file);
                enemyFile.file.Save();
                enemyFile.file.SaveOnConfigSet = true;
            }
            else
            {
                foreach (var enemy in LethalContent.Enemies)
                {
                    string uuid = UUIDify(enemy.Key.ToString());
                    EnemyType enemyObj = enemy.Value.EnemyType;

                    string scanName = null;

                    if (enemyObj.enemyPrefab != null)
                    {
                        var enemyScanNode = enemyObj.enemyPrefab.GetComponentInChildren<ScanNodeProperties>();
                        if (enemyScanNode != null)
                            scanName = enemyScanNode.headerText;
                    }

                    enemies[CleanString(enemyObj.enemyName)] = uuid;
                    if (!scanName.IsNullOrWhiteSpace()) { enemies[CleanString(scanName)] = uuid; }
                }
            }

            enemiesInitialized = true;
            MiniLogger.LogInfo("Completed Initializing Enemies");

            InitEnemyWeights();
        }

        public void InitEnemyWeights()
        {
            if (configureMoons && moonsInitialized && enemiesInitialized && !enemyWeightsInitialized)
            {
                if (enabledMoonSettings.Contains("Spawnable Daytime Enemies"))
                {
                    MiniLogger.LogInfo("Initializing Daytime Weights");

                    foreach (var cache in cachedDaytimeEnemies)
                    {
                        foreach (var item in cache.Value.Split(","))
                        {
                            string[] splits = item.Split(":");

                            string id = splits[0];
                            int rarity = int.Parse(CleanString(splits[1]));

                            TrySetDaytimeWeight(GetDawnUUID(enemies, id), rarity, cache.Key);
                        }
                    }

                    foreach (var enemy in LethalContent.Enemies)
                    {
                        string uuid = UUIDify(enemy.Key.ToString());

                        try
                        {
                            DawnEnemyInfo dawnEnemy = enemy.Value;
                            DawnEnemyLocationInfo location = null;

                            if (dawnEnemy.Daytime != null)
                            {
                                location = dawnEnemy.Daytime;

                                string key = enemy.Key.ToString();

                                foreach (var moon in notConfiguredDaytimeMoons)
                                {
                                    int? rarity = location.Weights.GetFor(moon);
                                    if (rarity != null && rarity > 0) { TrySetDaytimeWeight(key, (int)rarity, moon.TypedKey); }
                                }
                            }

                            if (!dawnEnemy.HasTag(DawnLibTags.LunarConfig)) { dawnEnemy.Internal_AddTag(DawnLibTags.LunarConfig); }

                            WeightTableBuilder<DawnMoonInfo> enemyWeightBuilder = new();

                            if (defaultDaytimeWeights.TryGetValue(uuid, out Dictionary<NamespacedKey<DawnMoonInfo>, int> moonVars))
                            {
                                foreach (var moon in moonVars)
                                {
                                    enemyWeightBuilder.AddWeight(moon.Key, moon.Value);
                                }
                            }

                            ProviderTable<int?, DawnMoonInfo> newTable = enemyWeightBuilder.Build();

                            // SET WEIGHTS
                            if (location != null)
                            {
                                location.Weights = newTable;
                            }
                            else
                            {
                                dawnEnemy.Daytime = new DawnEnemyLocationInfo(newTable);
                            }
                        }
                        catch (Exception e)
                        {
                            MiniLogger.LogError($"LunarConfig encountered an issue while configuring weights for {uuid}, please report this!\n{e}");
                        }
                    }

                    MiniLogger.LogInfo("Completed Initializing Daytime Weights");
                }

                if (enabledMoonSettings.Contains("Spawnable Interior Enemies"))
                {
                    MiniLogger.LogInfo("Initializing Interior Weights");

                    foreach (var cache in cachedInteriorEnemies)
                    {
                        foreach (var item in cache.Value.Split(","))
                        {
                            string[] splits = item.Split(":");

                            string id = splits[0];
                            int rarity = int.Parse(CleanString(splits[1]));
                            TrySetInteriorWeight(GetDawnUUID(enemies, id), rarity, cache.Key);
                        }
                    }

                    foreach (var enemy in LethalContent.Enemies)
                    {
                        string uuid = UUIDify(enemy.Key.ToString());

                        try
                        {
                            DawnEnemyInfo dawnEnemy = enemy.Value;
                            DawnEnemyLocationInfo location = null;

                            if (dawnEnemy.Inside != null)
                            {
                                location = dawnEnemy.Inside;

                                string key = enemy.Key.ToString();

                                foreach (var moon in notConfiguredInteriorMoons)
                                {
                                    int? rarity = location.Weights.GetFor(moon);
                                    if (rarity != null && rarity > 0) { TrySetInteriorWeight(key, (int)rarity, moon.TypedKey); }
                                }
                            }

                            if (!dawnEnemy.HasTag(DawnLibTags.LunarConfig)) { dawnEnemy.Internal_AddTag(DawnLibTags.LunarConfig); }

                            WeightTableBuilder<DawnMoonInfo> enemyWeightBuilder = new();

                            if (defaultInteriorWeights.TryGetValue(uuid, out Dictionary<NamespacedKey<DawnMoonInfo>, int> moonVars))
                            {
                                foreach (var moon in moonVars)
                                {
                                    enemyWeightBuilder.AddWeight(moon.Key, moon.Value);
                                }
                            }

                            ProviderTable<int?, DawnMoonInfo> newTable = enemyWeightBuilder.Build();

                            // SET WEIGHTS
                            if (location != null)
                            {
                                location.Weights = newTable;
                            }
                            else
                            {
                                dawnEnemy.Inside = new DawnEnemyLocationInfo(newTable);
                            }
                        }
                        catch (Exception e)
                        {
                            MiniLogger.LogError($"LunarConfig encountered an issue while configuring weights for {uuid}, please report this!\n{e}");
                        }
                    }

                    MiniLogger.LogInfo("Completed Initializing Interior Weights");
                }

                if (enabledMoonSettings.Contains("Spawnable Outside Enemies"))
                {
                    MiniLogger.LogInfo("Initializing Outside Weights");

                    foreach (var cache in cachedOutsideEnemies)
                    {
                        foreach (var item in cache.Value.Split(","))
                        {
                            string[] splits = item.Split(":");

                            string id = splits[0];
                            int rarity = int.Parse(CleanString(splits[1]));
                            TrySetOutsideWeight(GetDawnUUID(enemies, id), rarity, cache.Key);
                        }
                    }

                    foreach (var enemy in LethalContent.Enemies)
                    {
                        string uuid = UUIDify(enemy.Key.ToString());

                        try
                        {
                            DawnEnemyInfo dawnEnemy = enemy.Value;
                            DawnEnemyLocationInfo location = null;

                            if (dawnEnemy.Outside != null)
                            {
                                location = dawnEnemy.Outside;

                                string key = enemy.Key.ToString();

                                foreach (var moon in notConfiguredOutsideMoons)
                                {
                                    int? rarity = location.Weights.GetFor(moon);
                                    if (rarity != null && rarity > 0) { TrySetOutsideWeight(key, (int)rarity, moon.TypedKey); }
                                }
                            }

                            if (!dawnEnemy.HasTag(DawnLibTags.LunarConfig)) { dawnEnemy.Internal_AddTag(DawnLibTags.LunarConfig); }

                            WeightTableBuilder<DawnMoonInfo> enemyWeightBuilder = new();

                            if (defaultInteriorWeights.TryGetValue(uuid, out Dictionary<NamespacedKey<DawnMoonInfo>, int> moonVars))
                            {
                                foreach (var moon in moonVars)
                                {
                                    enemyWeightBuilder.AddWeight(moon.Key, moon.Value);
                                }
                            }

                            ProviderTable<int?, DawnMoonInfo> newTable = enemyWeightBuilder.Build();

                            // SET WEIGHTS
                            if (location != null)
                            {
                                location.Weights = newTable;
                            }
                            else
                            {
                                dawnEnemy.Outside = new DawnEnemyLocationInfo(newTable);
                            }
                        }
                        catch (Exception e)
                        {
                            MiniLogger.LogError($"LunarConfig encountered an issue while configuring weights for {uuid}, please report this!\n{e}");
                        }
                    }

                    MiniLogger.LogInfo("Completed Initializing Outside Weights");
                }

                enemyWeightsInitialized = true;
            }
        }

        public void InitDungeons()
        {
            if (!centralInitialized)
            {
                InitCentral();
            }

            MiniLogger.LogInfo("Initializing Dungeons");
            if (configureDungeons)
            {
                LunarConfigFile dungeonFile = AddFile(LunarConfig.DUNGEON_FILE, LunarConfig.DUNGEON_FILE_NAME);
                dungeonFile.file.SaveOnConfigSet = false;

                foreach (var dungeon in LethalContent.Dungeons)
                {
                    string uuid = UUIDify(dungeon.Key.ToString());

                    try
                    {
                        DawnDungeonInfo dawnDungeon = dungeon.Value;
                        LunarConfigEntry dungeonEntry = dungeonFile.AddEntry(uuid);


                        DungeonFlow flow = dawnDungeon.DungeonFlow;

                        // GETTING VALUES (for config)
                        dungeonEntry.AddField("Configure Content", "Enable to change any of the settings below.", false);
                        dungeonEntry.AddField("Appropriate Aliases", "These are the names which LunarConfig will recognize as this object in other config options.\nThey are case-insensitve and do not regard whitespace.", $"{flow.name}");

                        dungeonEntry.TryAddField(enabledDungeonSettings, "Random Size Min", "The minimum length of dungeon branches.\nHaving a different min and max allows variation between the size of a dungeon on the same moon.", flow.Length.Min);
                        dungeonEntry.TryAddField(enabledDungeonSettings, "Random Size Max", "The maximum length of dungeon branches.\nHaving a different min and max allows variation between the size of a dungeon on the same moon.", flow.Length.Max);
                        dungeonEntry.TryAddField(enabledDungeonSettings, "Map Tile Size", "Increase this setting to decrease the size of the dungeon overall.", dawnDungeon.MapTileSize);

                        // SETTING VALUES
                        if (dungeonEntry.GetValue<bool>("Configure Content"))
                        {
                            dawnDungeon.Internal_AddTag(DawnLibTags.LunarConfig);

                            foreach (var key in dungeonEntry.GetValue<string>("Appropriate Aliases").Split(","))
                            {
                                if (!key.IsNullOrWhiteSpace()) { dungeons[CleanString(key)] = uuid; }
                            }

                            dungeonEntry.TrySetValue(enabledDungeonSettings, "Random Size Min", ref flow.Length.Min);
                            dungeonEntry.TrySetValue(enabledDungeonSettings, "Random Size Max", ref flow.Length.Max);
                            if (enabledDungeonSettings.Contains("Map Tile Size")) { dawnDungeon.MapTileSize = dungeonEntry.GetValue<float>("Map Tile Size"); }
                        }
                        else
                        {
                            dungeons[CleanString(flow.name)] = uuid;
                        }
                    }
                    catch (Exception e)
                    {
                        MiniLogger.LogError($"LunarConfig encountered an issue while configuring {uuid}, please report this!\n{e}");
                    }
                }

                ClearOrphanedEntries(dungeonFile.file);
                dungeonFile.file.Save();
                dungeonFile.file.SaveOnConfigSet = true;
            }
            else
            {
                foreach (var dungeon in LethalContent.Dungeons)
                {
                    string uuid = UUIDify(dungeon.Key.ToString());

                    dungeons[CleanString(dungeon.Value.DungeonFlow.name)] = uuid;
                }
            }

            dungeonsInitialized = true;
            MiniLogger.LogInfo("Completed Initializing Dungeons");

            InitDungeonWeights();
        }

        public void InitDungeonWeights()
        {
            if (enabledMoonSettings.Contains("Possible Interiors") && configureMoons && moonsInitialized && dungeonsInitialized && !dungeonWeightsInitialized)
            {
                MiniLogger.LogInfo("Initializing Dungeon Weights");

                foreach (var cache in cachedDungeons)
                {
                    foreach (var item in cache.Value.Split(","))
                    {
                        string[] splits = item.Split(":");

                        string id = splits[0];
                        int rarity = int.Parse(CleanString(splits[1]));
                        TrySetDungeonWeight(GetDawnUUID(dungeons, id), rarity, cache.Key);
                    }
                }

                foreach (var dungeon in LethalContent.Dungeons)
                {
                    string uuid = UUIDify(dungeon.Key.ToString());

                    try
                    {
                        DawnDungeonInfo dawnDungeon = dungeon.Value;

                        string key = dungeon.Key.ToString();

                        foreach (var moon in notConfiguredDungeonMoons)
                        {
                            int? rarity = dungeon.Value.Weights.GetFor(moon);
                            if (rarity != null && rarity > 0) { TrySetDungeonWeight(key, (int)rarity, moon.TypedKey); }
                        }

                        if (!dawnDungeon.HasTag(DawnLibTags.LunarConfig)) { dawnDungeon.Internal_AddTag(DawnLibTags.LunarConfig); }

                        WeightTableBuilder<DawnMoonInfo> dungeonWeightBuilder = new();

                        if (defaultDungeonWeights.TryGetValue(uuid, out Dictionary<NamespacedKey<DawnMoonInfo>, int> moonVars))
                        {
                            foreach (var moon in moonVars)
                            {
                                dungeonWeightBuilder.AddWeight(moon.Key, moon.Value);
                            }
                        }

                        ProviderTable<int?, DawnMoonInfo> newTable = dungeonWeightBuilder.Build();

                        // SET WEIGHTS
                        dawnDungeon.Weights = newTable;
                    }
                    catch (Exception e)
                    {
                        MiniLogger.LogError($"LunarConfig encountered an issue while configuring weights for {uuid}, please report this!\n{e}");
                    }
                }
                dungeonWeightsInitialized = true;
                MiniLogger.LogInfo("Completed Initializing Dungeon Weights");
            }
        }

        public void InitMapObjects()
        {
            if (!centralInitialized)
            {
                InitCentral();
            }

            MiniLogger.LogInfo("Initializing Map Objects");
            if (configureMapObjects)
            {
                LunarConfigFile mapObjectFile = AddFile(LunarConfig.MAP_OBJECT_FILE, LunarConfig.MAP_OBJECT_FILE_NAME);
                mapObjectFile.file.SaveOnConfigSet = false;

                foreach (var obj in LethalContent.MapObjects)
                {
                    string uuid = UUIDify(obj.Key.ToString());

                    try
                    {
                        DawnMapObjectInfo dawnObj = obj.Value;
                        LunarConfigEntry objEntry = mapObjectFile.AddEntry(uuid);

                        DawnInsideMapObjectInfo insideInfo = null;
                        DawnOutsideMapObjectInfo outsideInfo = null;

                        if (dawnObj.InsideInfo != null)
                        {
                            insideInfo = dawnObj.InsideInfo;
                        }

                        if (dawnObj.OutsideInfo != null)
                        {
                            outsideInfo = dawnObj.OutsideInfo;
                        }

                        // GETTING VALUES (for config)
                        objEntry.AddField("Configure Content", "Enable to change any of the settings below.", false);
                        objEntry.AddField("Appropriate Aliases", "These are the names which LunarConfig will recognize as this object in other config options.\nThey are case-insensitve and do not regard whitespace.", $"{dawnObj.MapObject.name}");

                        if (insideInfo != null)
                        {
                            objEntry.TryAddField(enabledMapObjectSettings, "(Inside) Face Away From Wall?", "Specifies whether the object should spawn facing away from a wall.", insideInfo.SpawnFacingAwayFromWall);
                            objEntry.TryAddField(enabledMapObjectSettings, "(Inside) Face Towards Wall?", "Specifies whether the object should spawn facing towards a wall.", insideInfo.SpawnFacingWall);
                            objEntry.TryAddField(enabledMapObjectSettings, "(Inside) Disallow Near Entrance?", "Specifies whether the object should not spawn near an entrance.", insideInfo.DisallowSpawningNearEntrances);
                            objEntry.TryAddField(enabledMapObjectSettings, "(Inside) Require Distance Between Spawns?", "Specifies whether the object should require distance between its spawns.", insideInfo.RequireDistanceBetweenSpawns);
                            objEntry.TryAddField(enabledMapObjectSettings, "(Inside) Flush Against Wall?", "Specifies whether the object should spawn flush against a wall.", insideInfo.SpawnWithBackFlushAgainstWall);
                            objEntry.TryAddField(enabledMapObjectSettings, "(Inside) Spawn Against Wall?", "Specifies whether the object should spawn against a wall.", insideInfo.SpawnWithBackToWall);
                        }
                        else
                        {
                            objEntry.TryAddField(enabledMapObjectSettings, "(Inside) Face Away From Wall?", "Specifies whether the object should spawn facing away from a wall.", false);
                            objEntry.TryAddField(enabledMapObjectSettings, "(Inside) Face Towards Wall?", "Specifies whether the object should spawn facing towards a wall.", false);
                            objEntry.TryAddField(enabledMapObjectSettings, "(Inside) Disallow Near Entrance?", "Specifies whether the object should not spawn near an entrance.", false);
                            objEntry.TryAddField(enabledMapObjectSettings, "(Inside) Require Distance Between Spawns?", "Specifies whether the object should require distance between its spawns.", false);
                            objEntry.TryAddField(enabledMapObjectSettings, "(Inside) Flush Against Wall?", "Specifies whether the object should spawn flush against a wall.", false);
                            objEntry.TryAddField(enabledMapObjectSettings, "(Inside) Spawn Against Wall?", "Specifies whether the object should spawn against a wall.", false);
                        }

                        if (outsideInfo != null)
                        {
                            objEntry.TryAddField(enabledMapObjectSettings, "(Outside) Align With Terrain?", "Specifies whether the object should spawn aligned to the terrain.", outsideInfo.AlignWithTerrain);
                            objEntry.TryAddField(enabledMapObjectSettings, "(Outside) Object Width", "Specifies the width of an object. (Don't ask, I don't know either)", outsideInfo.ObjectWidth);
                            objEntry.TryAddField(enabledMapObjectSettings, "(Outside) Spawnable Floor Tags", "Specifies the tags of floor an object can spawn on.", string.Join(", ", outsideInfo.SpawnableFloorTags));
                            objEntry.TryAddField(enabledMapObjectSettings, "(Outside) Face Away From Wall?", "Specifies whether the object should spawn facing away from a wall.", outsideInfo.SpawnFacingAwayFromWall);
                        }
                        else
                        {
                            objEntry.TryAddField(enabledMapObjectSettings, "(Outside) Align With Terrain?", "Specifies whether the object should spawn aligned to the terrain.", true);
                            objEntry.TryAddField(enabledMapObjectSettings, "(Outside) Object Width", "Specifies the width of an object. (Don't ask, I don't know either)", 1);
                            objEntry.TryAddField(enabledMapObjectSettings, "(Outside) Spawnable Floor Tags", "Specifies the tags of floor an object can spawn on.", "");
                            objEntry.TryAddField(enabledMapObjectSettings, "(Outside) Face Away From Wall?", "Specifies whether the object should spawn facing away from a wall.", false);
                        }

                        // SETTING VALUES
                        if (objEntry.GetValue<bool>("Configure Content"))
                        {
                            dawnObj.Internal_AddTag(DawnLibTags.LunarConfig);

                            foreach (var key in objEntry.GetValue<string>("Appropriate Aliases").Split(","))
                            {
                                if (!key.IsNullOrWhiteSpace()) { mapObjects[CleanString(key)] = uuid; }
                            }
                            
                            if (insideInfo != null)
                            {
                                CurveTableBuilder<DawnMoonInfo> blankTable = new();

                                dawnObj.InsideInfo = new DawnInsideMapObjectInfo(blankTable.Build(), false, false, false, false, false, false);
                                insideInfo = dawnObj.InsideInfo;
                            }

                            if (enabledMapObjectSettings.Contains("(Inside) Face Away From Wall?")) { insideInfo.SpawnFacingAwayFromWall = objEntry.GetValue<bool>("(Inside) Face Away From Wall?"); }
                            if (enabledMapObjectSettings.Contains("(Inside) Face Towards Wall?")) { insideInfo.SpawnFacingWall = objEntry.GetValue<bool>("(Inside) Face Towards Wall?"); }
                            if (enabledMapObjectSettings.Contains("(Inside) Disallow Near Entrance?")) { insideInfo.DisallowSpawningNearEntrances = objEntry.GetValue<bool>("(Inside) Disallow Near Entrance?"); }
                            if (enabledMapObjectSettings.Contains("(Inside) Require Distance Between Spawns?")) { insideInfo.RequireDistanceBetweenSpawns = objEntry.GetValue<bool>("(Inside) Require Distance Between Spawns?"); }
                            if (enabledMapObjectSettings.Contains("(Inside) Flush Against Wall?")) { insideInfo.SpawnWithBackFlushAgainstWall = objEntry.GetValue<bool>("(Inside) Flush Against Wall?"); }
                            if (enabledMapObjectSettings.Contains("(Inside) Spawn Against Wall?")) { insideInfo.SpawnWithBackToWall = objEntry.GetValue<bool>("(Inside) Spawn Against Wall?"); }

                            if (outsideInfo != null)
                            {
                                CurveTableBuilder<DawnMoonInfo> blankTable = new();

                                dawnObj.OutsideInfo = new DawnOutsideMapObjectInfo(blankTable.Build(), false, 1, Array.Empty<string>(), Vector3.zero, true, 0);
                                outsideInfo = dawnObj.OutsideInfo;
                            }

                            if (enabledMapObjectSettings.Contains("(Outside) Align With Terrain?")) { outsideInfo.AlignWithTerrain = objEntry.GetValue<bool>("(Outside) Align With Terrain?"); }
                            if (enabledMapObjectSettings.Contains("(Outside) Object Width")) { outsideInfo.ObjectWidth = objEntry.GetValue<int>("(Outside) Object Width"); }

                            string[] tags = objEntry.GetValue<string>("(Outside) Spawnable Floor Tags").Split(",");

                            for (int i = 0; i < tags.Length; i++)
                            {
                                tags.SetValue(tags[i].Trim(), i);
                            }

                            if (enabledMapObjectSettings.Contains("(Outside) Spawnable Floor Tags")) { outsideInfo.SpawnableFloorTags = tags; }
                            if (enabledMapObjectSettings.Contains("(Outside) Face Away From Wall?")) { outsideInfo.SpawnFacingAwayFromWall = objEntry.GetValue<bool>("(Outside) Face Away From Wall?"); }
                        }
                        else
                        {
                            mapObjects[CleanString(dawnObj.MapObject.name)] = uuid;
                        }
                    }
                    catch (Exception e)
                    {
                        MiniLogger.LogError($"LunarConfig encountered an issue while configuring {uuid}, please report this!\n{e}");
                    }
                }

                ClearOrphanedEntries(mapObjectFile.file);
                mapObjectFile.file.Save();
                mapObjectFile.file.SaveOnConfigSet = true;
            }
            else
            {
                foreach (var obj in LethalContent.MapObjects)
                {
                    string uuid = UUIDify(obj.Key.ToString());

                    items[CleanString(obj.Value.MapObject.name)] = uuid;
                }
            }

            mapObjectsInitialized = true;
            MiniLogger.LogInfo("Completed Initializing Map Objects");

            InitMapObjectCurves();
        }

        public void InitMapObjectCurves()
        {
            MiniLogger.LogInfo($"Tryna initialize some curvesd type shi {configureMoons} {moonsInitialized} {mapObjectsInitialized} {mapObjectCurvesInitialized} plus soem other shi {enabledMapObjectSettings.Contains("(Inside) Level Curves")} {enabledMapObjectSettings.Contains("(Outside) Level Curves")}");
            if (configureMoons && moonsInitialized && mapObjectsInitialized && !mapObjectCurvesInitialized)
            {
                if (enabledMapObjectSettings.Contains("(Inside) Level Curves"))
                {
                    MiniLogger.LogInfo("Initializing Inside Object Curves");

                    foreach (var pair in cachedInsideMapObjects)
                    {
                        NamespacedKey<DawnMoonInfo> dawnMoon = pair.Key;

                        foreach (var kvp in pair.Value)
                        {
                            string id = kvp.Key;
                            if (!kvp.Value.IsNullOrWhiteSpace())
                            {
                                AnimationCurve curve = StringToCurve(CleanString(kvp.Value));

                                TrySetInsideCurve(GetDawnUUID(mapObjects, id), curve, dawnMoon);
                            }
                        }
                    }

                    foreach (var obj in LethalContent.MapObjects)
                    {
                        string uuid = UUIDify(obj.Key.ToString());

                        try
                        {
                            DawnMapObjectInfo dawnObj = obj.Value;
                            DawnInsideMapObjectInfo dawnCurveInfo = null;

                            if (dawnObj.InsideInfo != null)
                            {
                                dawnCurveInfo = dawnObj.InsideInfo;

                                string key = obj.Key.ToString();

                                foreach (var moon in notConfiguredInsideMapObjectMoons)
                                {
                                    AnimationCurve? rarity = dawnCurveInfo.SpawnWeights.GetFor(moon);
                                    if (rarity != null) { TrySetInsideCurve(key, rarity, moon.TypedKey); }
                                }
                            }

                            if (!dawnObj.HasTag(DawnLibTags.LunarConfig)) { dawnObj.Internal_AddTag(DawnLibTags.LunarConfig); }

                            CurveTableBuilder<DawnMoonInfo> curveBuilder = new();

                            if (defaultInsideMapObjectCurves.TryGetValue(uuid, out Dictionary<NamespacedKey<DawnMoonInfo>, AnimationCurve> moonVars))
                            {
                                foreach (var moon in moonVars)
                                {
                                    curveBuilder.AddCurve(moon.Key, moon.Value);
                                }
                            }

                            ProviderTable<AnimationCurve?, DawnMoonInfo> newTable = curveBuilder.Build();

                            // SET WEIGHTS
                            if (dawnCurveInfo != null)
                            {
                                dawnCurveInfo.SpawnWeights = newTable;
                            }
                            else
                            {
                                dawnObj.InsideInfo = new DawnInsideMapObjectInfo(newTable, false, false, false, false, false, false);
                            }
                        }
                        catch (Exception e)
                        {
                            MiniLogger.LogError($"LunarConfig encountered an issue while configuring weights for {uuid}, please report this!\n{e}");
                        }
                    }

                    MiniLogger.LogInfo("Completed Initializing Inside Object Curves");
                }

                if (enabledMapObjectSettings.Contains("(Outside) Level Curves"))
                {
                    MiniLogger.LogInfo("Initializing Outside Object Curves");

                    foreach (var pair in cachedOutsideMapObjects)
                    {
                        NamespacedKey<DawnMoonInfo> dawnMoon = pair.Key;

                        foreach (var kvp in pair.Value)
                        {
                            string id = kvp.Key;
                            if (!kvp.Value.IsNullOrWhiteSpace())
                            {
                                AnimationCurve curve = StringToCurve(CleanString(kvp.Value));

                                TrySetOutsideCurve(GetDawnUUID(mapObjects, id), curve, dawnMoon);
                            }
                        }
                    }

                    foreach (var obj in LethalContent.MapObjects)
                    {
                        string uuid = UUIDify(obj.Key.ToString());

                        try
                        {
                            DawnMapObjectInfo dawnObj = obj.Value;
                            DawnOutsideMapObjectInfo dawnCurveInfo = null;

                            if (dawnObj.OutsideInfo != null)
                            {
                                dawnCurveInfo = dawnObj.OutsideInfo;

                                string key = obj.Key.ToString();

                                foreach (var moon in notConfiguredOutsideMapObjectMoons)
                                {
                                    AnimationCurve? rarity = dawnCurveInfo.SpawnWeights.GetFor(moon);
                                    if (rarity != null) { TrySetOutsideCurve(key, (AnimationCurve)rarity, moon.TypedKey); }
                                }
                            }

                            if (!dawnObj.HasTag(DawnLibTags.LunarConfig)) { dawnObj.Internal_AddTag(DawnLibTags.LunarConfig); }

                            CurveTableBuilder<DawnMoonInfo> curveBuilder = new();

                            if (defaultOutsideMapObjectCurves.TryGetValue(uuid, out Dictionary<NamespacedKey<DawnMoonInfo>, AnimationCurve> moonVars))
                            {
                                foreach (var moon in moonVars)
                                {
                                    curveBuilder.AddCurve(moon.Key, moon.Value);
                                }
                            }

                            ProviderTable<AnimationCurve?, DawnMoonInfo> newTable = curveBuilder.Build();

                            // SET WEIGHTS
                            if (dawnCurveInfo != null)
                            {
                                dawnCurveInfo.SpawnWeights = newTable;
                            }
                            else
                            {
                                dawnObj.OutsideInfo = new DawnOutsideMapObjectInfo(newTable, false, 12, Array.Empty<string>(), Vector3.zero, false, 0);
                            }
                        }
                        catch (Exception e)
                        {
                            MiniLogger.LogError($"LunarConfig encountered an issue while configuring weights for {uuid}, please report this!\n{e}");
                        }
                    }

                    MiniLogger.LogInfo("Completed Initializing Outside Object Curves");
                }

                mapObjectCurvesInitialized = true;
            }
        }

        public void InitMoons()
        {
            if (!centralInitialized)
            {
                InitCentral();
            }

            MiniLogger.LogInfo("Initializing Moons");
            if (configureMoons)
            {
                LunarConfigFile moonFile = AddFile(LunarConfig.MOON_FILE, LunarConfig.MOON_FILE_NAME);
                moonFile.file.SaveOnConfigSet = false;

                foreach (var moon in LethalContent.Moons)
                {
                    string uuid = UUIDify(moon.Key.ToString());

                    try
                    {
                        DawnMoonInfo dawnMoon = moon.Value;
                        LunarConfigEntry moonEntry = moonFile.AddEntry(uuid);
                        SelectableLevel moonObj = dawnMoon.Level;
                        DawnPurchaseInfo purchaseInfo = null;
                        TerminalNode infoNode = null;
                        TerminalNode requestNode = null;
                        TerminalNode receiptNode = null;
                        string numberlessName = dawnMoon.GetNumberlessPlanetName();

                        if (dawnMoon.DawnPurchaseInfo != null)
                        {
                            purchaseInfo = dawnMoon.DawnPurchaseInfo;
                        }

                        moonKeys[uuid] = dawnMoon.TypedKey;

                        // GETTING VALUES (for config)
                        moonEntry.AddField("Configure Content", "Enable to change any of the settings below.", false);
                        moonEntry.AddField("Appropriate Aliases", "These are the names which LunarConfig will recognize as this object in other config options.\nThey are case-insensitve and do not regard whitespace.", $"{numberlessName}");

                        moonEntry.TryAddField(enabledMoonSettings, "Display Name", "Changes the name of the moon.\nDoes not modify terminal commands/nodes.", moonObj.PlanetName);
                        moonEntry.TryAddField(enabledMoonSettings, "Risk Level", "Changes the risk level of the moon.\nThis setting is only cosmetic.", moonObj.riskLevel);
                        moonEntry.TryAddField(enabledMoonSettings, "Description", "The description given to the moon.\nNew lines are represented by semi-colons.\nDoes not modify terminal commands/nodes.", moonObj.LevelDescription.Replace("\n", ";"));
                        moonEntry.TryAddField(enabledMoonSettings, "Has Time?", "Defines whether a moon has time.", moonObj.planetHasTime);
                        moonEntry.TryAddField(enabledMoonSettings, "Can Be Challenge Moon?", "Defines whether or not a moon can be selected for the weekly challenge moon.", moonObj.planetHasTime);
                        moonEntry.TryAddField(enabledMoonSettings, "Time Multiplier", "Multiplies the speed at which time progresses on a moon.\nIn order to not cause issues, it is best to only modify this setting while using Moon Day Speed Multiplier Patcher by WhiteSpike.", moonObj.DaySpeedMultiplier);
                        moonEntry.TryAddField(enabledMoonSettings, "Daytime Probability Range", "The amount of daytime enemies spawned that can differ from the curve.\nFor instance, if this value is 3, and at the current time and spawn cycle 2 daytime enemies should spawn, anywhere between 0 and 5 can spawn.", moonObj.daytimeEnemiesProbabilityRange);
                        moonEntry.TryAddField(enabledMoonSettings, "Daytime Curve", "Decides the amount of daytime enemies that spawn as the day progresses.\nKeyframes represented by x,y and separated by semicolons.", CurveToString(moonObj.daytimeEnemySpawnChanceThroughDay));
                        moonEntry.TryAddField(enabledMoonSettings, "Max Daytime Power", "The amount of daytime power capacity that a moon has.", moonObj.maxDaytimeEnemyPowerCount);
                        moonEntry.TryAddField(enabledMoonSettings, "Interior Probability Range", "The amount of interior enemies spawned that can differ from the curve.\nFor instance, if this value is 3, and at the current time and spawn cycle 2 interior enemies should spawn, anywhere between 0 and 5 can spawn.", moonObj.spawnProbabilityRange);
                        moonEntry.TryAddField(enabledMoonSettings, "Interior Curve", "Decides the amount of interior enemies that spawn as the day progresses.\nKeyframes represented by x,y and separated by semicolons.", CurveToString(moonObj.enemySpawnChanceThroughoutDay));
                        moonEntry.TryAddField(enabledMoonSettings, "Max Interior Power", "The amount of interior power capacity that a moon has.", moonObj.maxEnemyPowerCount);
                        moonEntry.TryAddField(enabledMoonSettings, "Outside Curve", "Decides the amount of outside enemies that spawn as the day progresses.\nKeyframes represented by x,y and separated by semicolons.", CurveToString(moonObj.outsideEnemySpawnChanceThroughDay));
                        moonEntry.TryAddField(enabledMoonSettings, "Max Outside Power", "The amount of outside power capacity that a moon has.", moonObj.maxOutsideEnemyPowerCount);
                        moonEntry.TryAddField(enabledMoonSettings, "Min Scrap", "The minimum amount of scrap items that can spawn on a moon.", moonObj.minScrap);
                        moonEntry.TryAddField(enabledMoonSettings, "Max Scrap", "The maximum amount of scrap items that can spawn on a moon.", moonObj.maxScrap);
                        moonEntry.TryAddField(enabledMoonSettings, "Interior Multiplier", "Changes the size of the interior generated.", moonObj.factorySizeMultiplier);

                        //if (enabledSettings.Contains("Value Multiplier")) { moonEntry.AddField("Value Multiplier", "The multiplier applied to the value of a moon's scrap.", 1f); }
                        //if (enabledSettings.Contains("Amount Multiplier")) { moonEntry.AddField("Amount Multiplier", "The multiplier applied to the amount of scrap a moon has.", 1f); }
                        //if (enabledSettings.Contains("Tags")) { moonEntry.AddField("Tags", "Tags allocated to the moon.\nSeparate tags with commas.", defaultTags); }

                        if (purchaseInfo != null)
                        {
                            moonEntry.TryAddField(enabledMoonSettings, "Route Price", "Changes the price to route to the moon.", purchaseInfo.Cost.Provide());
                            moonEntry.TryAddField(enabledMoonSettings, "Is Hidden?", "Changes if the moon is hidden in the terminal.", purchaseInfo.PurchasePredicate.CanPurchase() is TerminalPurchaseResult.HiddenPurchaseResult);
                            moonEntry.TryAddField(enabledMoonSettings, "Is Locked?", "Changes if the moon is locked in the terminal.", purchaseInfo.PurchasePredicate.CanPurchase() is TerminalPurchaseResult.FailedPurchaseResult);
                        }

                        string defaultScrap = "";
                        foreach (var item in LethalContent.Items)
                        {
                            DawnItemInfo ite = item.Value;

                            if (ite.ScrapInfo == null) { continue; }

                            int? rarity = ite.ScrapInfo.Weights.GetFor(dawnMoon);

                            if (rarity != null && rarity > 0)
                            {
                                if (defaultScrap != "")
                                {
                                    defaultScrap += ", ";
                                }
                                defaultScrap += ite.Item.itemName + ":" + rarity;
                            }
                        }

                        string defaultDayEnemies = "";
                        string defaultInteriorEnemies = "";
                        string defaultOutsideEnemies = "";
                        foreach (var enemy in LethalContent.Enemies)
                        {
                            DawnEnemyInfo ene = enemy.Value;

                            if (ene.Daytime != null)
                            {
                                int? rarity = ene.Daytime.Weights.GetFor(dawnMoon);

                                if (rarity != null && rarity > 0)
                                {
                                    if (defaultDayEnemies != "")
                                    {
                                        defaultDayEnemies += ", ";
                                    }
                                    defaultDayEnemies += ene.EnemyType.enemyName + ":" + rarity;
                                }
                            }

                            if (ene.Inside != null)
                            {
                                int? rarity = ene.Inside.Weights.GetFor(dawnMoon);

                                if (rarity != null && rarity > 0)
                                {
                                    if (defaultInteriorEnemies != "")
                                    {
                                        defaultInteriorEnemies += ", ";
                                    }
                                    defaultInteriorEnemies += ene.EnemyType.enemyName + ":" + rarity;
                                }
                            }

                            if (ene.Outside != null)
                            {
                                int? rarity = ene.Outside.Weights.GetFor(dawnMoon);

                                if (rarity != null && rarity > 0)
                                {
                                    if (defaultOutsideEnemies != "")
                                    {
                                        defaultOutsideEnemies += ", ";
                                    }
                                    defaultOutsideEnemies += ene.EnemyType.enemyName + ":" + rarity;
                                }
                            }
                        }

                        string defaultDungeons = "";
                        foreach (var dungeon in LethalContent.Dungeons)
                        {
                            DawnDungeonInfo dun = dungeon.Value;
                            int? rarity = dun.Weights.GetFor(dawnMoon);

                            if (rarity != null && rarity > 0)
                            {
                                if (defaultDungeons != "")
                                {
                                    defaultDungeons += ", ";
                                }

                                defaultDungeons += dun.DungeonFlow.name + ":" + rarity;
                            }
                        }

                        moonEntry.TryAddField(enabledMoonSettings, "Spawnable Scrap", "The base scrap that can spawn on the moon.\nDenoted with NAME:RARITY, separated with commas.", defaultScrap);
                        moonEntry.TryAddField(enabledMoonSettings, "Spawnable Daytime Enemies", "The base daytime enemies that can spawn on the moon.\nDenoted with NAME:RARITY, separated with commas.", defaultDayEnemies);
                        moonEntry.TryAddField(enabledMoonSettings, "Spawnable Interior Enemies", "The base interior enemies that can spawn on the moon.\nDenoted with NAME:RARITY, separated with commas.", defaultInteriorEnemies);
                        moonEntry.TryAddField(enabledMoonSettings, "Spawnable Outside Enemies", "The base outside enemies that can spawn on the moon.\nDenoted with NAME:RARITY, separated with commas.", defaultOutsideEnemies);
                        moonEntry.TryAddField(enabledMoonSettings, "Possible Interiors", "The base interiors that can spawn on the moon.\nDenoted with NAME:RARITY, separated with commas.", defaultDungeons);

                        foreach (var obj in LethalContent.MapObjects.Values)
                        {
                            if (enabledMapObjectSettings.Contains("(Inside) Level Curves"))
                            {
                                AnimationCurve? inCurve = null;

                                if (obj.InsideInfo != null)
                                {
                                    inCurve = obj.InsideInfo.SpawnWeights.GetFor(dawnMoon);
                                }

                                string inString = null;
                                if (inCurve == null) { inString = ""; } else { inString = CurveToString(inCurve); }

                                moonEntry.AddField($"Inside Curve - {obj.MapObject.name}", "The animation curve of this object spawning on the interior of this moon.", inString);
                            }

                            if (enabledMapObjectSettings.Contains("(Outside) Level Curves"))
                            {
                                AnimationCurve? outCurve = null;

                                if (obj.OutsideInfo != null)
                                {
                                    outCurve = obj.OutsideInfo.SpawnWeights.GetFor(dawnMoon);
                                }

                                string outString = null;
                                if (outCurve == null) { outString = ""; } else { outString = CurveToString(outCurve); }

                                moonEntry.AddField($"Outside Curve - {obj.MapObject.name}", "The animation curve of this object spawning on the interior of this moon.", outString);
                            }
                        }

                        // LLL Backcompat
                        if (TryGetEntryWithPrefix(moonFile.entries, $"LLL - {numberlessName}", out LunarConfigEntry oldLLLEntry))
                        {
                            foreach (var field in oldLLLEntry.fields)
                            {
                                if (moonEntry.fields.TryGetValue(field.Key, out ConfigEntryBase value))
                                {
                                    value.BoxedValue = field.Value.BoxedValue;
                                }
                            }
                        }

                        // SETTING VALUES
                        if (moonEntry.GetValue<bool>("Configure Content"))
                        {
                            dawnMoon.Internal_AddTag(DawnLibTags.LunarConfig);

                            foreach (var key in moonEntry.GetValue<string>("Appropriate Aliases").Split(","))
                            {
                                if (!key.IsNullOrWhiteSpace()) { moons[CleanString(key)] = uuid; }
                            }

                            moonEntry.TrySetValue(enabledMoonSettings, "Display Name", ref moonObj.PlanetName);
                            moonEntry.TrySetValue(enabledMoonSettings, "Risk Level", ref moonObj.riskLevel);
                            if (enabledMoonSettings.Contains("Description")) { moonObj.LevelDescription = moonEntry.GetValue<string>("Description").Replace(";", "\n"); }
                            moonEntry.TrySetValue(enabledMoonSettings, "Has Time?", ref moonObj.planetHasTime);
                            moonEntry.TrySetValue(enabledMoonSettings, "Time Multiplier", ref moonObj.DaySpeedMultiplier);
                            if (enabledMoonSettings.Contains("Can Be Challenge Moon?")) { definedChallengeMoons[moonObj] = moonEntry.GetValue<bool>("Can Be Challenge Moon?"); }
                            if (enabledMoonSettings.Contains("Can Be Challenge Moon?")) { definedChallengeMoonTimes[moonObj] = moonEntry.GetValue<bool>("Has Time?"); }
                            moonEntry.TrySetValue(enabledMoonSettings, "Daytime Probability Range", ref moonObj.daytimeEnemiesProbabilityRange);
                            if (enabledMoonSettings.Contains("Daytime Curve")) { moonObj.daytimeEnemySpawnChanceThroughDay = StringToCurve(moonEntry.GetValue<string>("Daytime Curve")); }
                            moonEntry.TrySetValue(enabledMoonSettings, "Max Daytime Power", ref moonObj.maxDaytimeEnemyPowerCount);
                            moonEntry.TrySetValue(enabledMoonSettings, "Interior Probability Range", ref moonObj.spawnProbabilityRange);
                            if (enabledMoonSettings.Contains("Interior Curve")) { moonObj.enemySpawnChanceThroughoutDay = StringToCurve(moonEntry.GetValue<string>("Interior Curve")); }
                            moonEntry.TrySetValue(enabledMoonSettings, "Max Interior Power", ref moonObj.maxEnemyPowerCount);
                            if (enabledMoonSettings.Contains("Outside Curve")) { moonObj.outsideEnemySpawnChanceThroughDay = StringToCurve(moonEntry.GetValue<string>("Outside Curve")); }
                            moonEntry.TrySetValue(enabledMoonSettings, "Max Outside Power", ref moonObj.maxOutsideEnemyPowerCount);
                            moonEntry.TrySetValue(enabledMoonSettings, "Min Scrap", ref moonObj.minScrap);
                            moonEntry.TrySetValue(enabledMoonSettings, "Max Scrap", ref moonObj.maxScrap);
                            moonEntry.TrySetValue(enabledMoonSettings, "Interior Multiplier", ref moonObj.factorySizeMultiplier);

                            if (purchaseInfo != null)
                            {
                                if (enabledMoonSettings.Contains("Route Price")) { purchaseInfo.Cost = new SimpleProvider<int>(moonEntry.GetValue<int>("Route Price")); }
                                if (enabledMoonSettings.Contains("Is Hidden?") || enabledMoonSettings.Contains("Is Locked?"))
                                {
                                    ITerminalPurchasePredicate predicate = ITerminalPurchasePredicate.AlwaysSuccess();

                                    if (moonEntry.GetValue<bool>("Is Locked?"))
                                    {
                                        if (moonEntry.GetValue<bool>("Is Hidden?"))
                                        {
                                            predicate = new ConstantTerminalPredicate(TerminalPurchaseResult.Hidden().SetFailure(true));
                                        }
                                        else
                                        {
                                            TerminalNode _node = ScriptableObject.CreateInstance<TerminalNode>();
                                            _node.displayText = "Route Locked!";
                                            predicate = ITerminalPurchasePredicate.AlwaysFail(_node);
                                        }
                                    }
                                    else
                                    {
                                        if (moonEntry.GetValue<bool>("Is Hidden?"))
                                        {
                                            predicate = new ConstantTerminalPredicate(TerminalPurchaseResult.Hidden());
                                        }
                                    }

                                    purchaseInfo.PurchasePredicate = predicate;
                                }
                            }

                            if (enabledMoonSettings.Contains("Spawnable Scrap")) { cachedSpawnableScrap[uuid] = moonEntry.GetValue<string>("Spawnable Scrap"); }
                            if (enabledMoonSettings.Contains("Spawnable Daytime Enemies")) { cachedDaytimeEnemies[dawnMoon.TypedKey] = moonEntry.GetValue<string>("Spawnable Daytime Enemies"); }
                            if (enabledMoonSettings.Contains("Spawnable Interior Enemies")) { cachedInteriorEnemies[dawnMoon.TypedKey] = moonEntry.GetValue<string>("Spawnable Interior Enemies"); }
                            if (enabledMoonSettings.Contains("Spawnable Outside Enemies")) { cachedOutsideEnemies[dawnMoon.TypedKey] = moonEntry.GetValue<string>("Spawnable Outside Enemies"); }
                            if (enabledMoonSettings.Contains("Possible Interiors")) { cachedDungeons[dawnMoon.TypedKey] = moonEntry.GetValue<string>("Possible Interiors"); }
                            if (enabledMapObjectSettings.Contains("(Inside) Level Curves"))
                            {
                                foreach (var field in moonEntry.fields)
                                {
                                    if (field.Key.StartsWith("Inside Curve - "))
                                    {
                                        if (!cachedInsideMapObjects.TryGetValue(dawnMoon.TypedKey, out var moonDict))
                                        {
                                            moonDict = new Dictionary<string, string>();
                                        }

                                        moonDict.Add(field.Key.Replace("Inside Curve - ", ""), (string)field.Value.BoxedValue);
                                    }
                                }
                            }
                            if (enabledMapObjectSettings.Contains("(Outside) Level Curves"))
                            {
                                foreach (var field in moonEntry.fields)
                                {
                                    if (field.Key.StartsWith("Outside Curve - "))
                                    {
                                        if (!cachedOutsideMapObjects.TryGetValue(dawnMoon.TypedKey, out var moonDict))
                                        {
                                            moonDict = new Dictionary<string, string>();
                                        }

                                        moonDict.Add(field.Key.Replace("Outside Curve - ", ""), (string)field.Value.BoxedValue);
                                    }
                                }
                            }
                        }
                        else
                        {
                            moons[numberlessName] = uuid;

                            if (enabledMoonSettings.Contains("Spawnable Scrap"))
                            {
                                notConfiguredScrapMoons.Add(dawnMoon);
                            }

                            if (enabledMoonSettings.Contains("Spawnable Daytime Enemies"))
                            {
                                notConfiguredDaytimeMoons.Add(dawnMoon);
                            }

                            if (enabledMoonSettings.Contains("Spawnable Interior Enemies"))
                            {
                                notConfiguredInteriorMoons.Add(dawnMoon);
                            }

                            if (enabledMoonSettings.Contains("Spawnable Outside Enemies"))
                            {
                                notConfiguredOutsideMoons.Add(dawnMoon);
                            }

                            if (enabledMoonSettings.Contains("Possible Interiors"))
                            {
                                notConfiguredDungeonMoons.Add(dawnMoon);
                            }

                            if (enabledMapObjectSettings.Contains("(Inside) Level Curves"))
                            {
                                notConfiguredInsideMapObjectMoons.Add(dawnMoon);
                            }

                            if (enabledMapObjectSettings.Contains("(Outside) Level Curves"))
                            {
                                notConfiguredOutsideMapObjectMoons.Add(dawnMoon);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        MiniLogger.LogError($"LunarConfig encountered an issue while configuring {uuid}, please report this!\n{e}");
                    }
                }

                ClearOrphanedEntries(moonFile.file);
                moonFile.file.Save();
                moonFile.file.SaveOnConfigSet = true;
            }
            else
            {
                foreach (var moon in LethalContent.Moons)
                {
                    string uuid = UUIDify(moon.Key.ToString());

                    moons[moon.Value.GetNumberlessPlanetName()] = uuid;
                    moonKeys[uuid] = moon.Value.TypedKey;
                }
            }

            moonsInitialized = true;
            MiniLogger.LogInfo("Completed Initializing Moons");

            InitMapObjectCurves();
            InitItemWeights();
            InitEnemyWeights();
            InitDungeonWeights();
        }

        /*
        public void InitEnemies()
        {
            MiniLogger.LogInfo("Initializing Enemy Configuration...");

            LunarConfigFile enemyFile = AddFile(LunarConfig.ENEMY_FILE, LunarConfig.ENEMY_FILE_NAME);
            enemyFile.file.SaveOnConfigSet = false;

            LunarConfigEntry enabledEntry = files[LunarConfig.CENTRAL_FILE_NAME].entries["Enabled Enemy Settings"];
            HashSet<string> enabledSettings = new HashSet<string>();

            foreach (var setting in enabledEntry.fields.Keys)
            {
                if (enabledEntry.GetValue<bool>(setting))
                {
                    enabledSettings.Add(setting);
                }
            }

            HashSet<string> registeredEnemies = new HashSet<string>();

            // LLL/Vanilla Content
            foreach (var enemy in PatchedContent.ExtendedEnemyTypes)
            {
                string enemyUUID = UUIDify($"LLL - {enemy.EnemyType.enemyName} ({enemy.UniqueIdentificationName})");
                if (!registeredEnemies.Contains(enemyUUID))
                {
                    EnemyType enemyObj = enemy.EnemyType;
                    LunarConfigEntry enemyEntry = enemyFile.AddEntry(enemyUUID);
                    MiniLogger.LogInfo($"Recording {enemy.name}...");

                    string scanName = "";
                    ScanNodeProperties enemyScanNode = null;

                    if (enemy.EnemyType.enemyPrefab != null)
                    {
                        enemyScanNode = enemy.EnemyType.enemyPrefab.GetComponentInChildren<ScanNodeProperties>();
                        if (enemyScanNode != null)
                            scanName = enemyScanNode.headerText;
                    }

                    enemyEntry.AddField("Configure Content", "Enable to change any of the settings below.", false);
                    enemyEntry.AddField("Appropriate Aliases", "Changing this setting will do nothing, these are the names which LunarConfig will recognize as this object in other config options.\nThey are case-insensitve and do not regard whitespace.", $"{enemy.EnemyType.enemyName}, {scanName}");

                    if (enabledSettings.Contains("Display Name")) { enemyEntry.AddField("Display Name", "Specifies the name of the enemy.", enemy.EnemyDisplayName); }
                    if (enemyScanNode != null && enabledSettings.Contains("Scan Name")) { enemyEntry.AddField("Scan Name", "Specifies the name of the enemy that appears on its scan node.", enemyScanNode.headerText); }
                    if (enemyScanNode != null && enabledSettings.Contains("Scan Subtext")) { enemyEntry.AddField("Scan Subtext", "Specifies the subtext that appears on the enemy's scan node.", enemyScanNode.subText); }
                    if (enemyScanNode != null && enabledSettings.Contains("Scan Min Range")) { enemyEntry.AddField("Scan Min Range", "Specifies the minimum distance the scan node can be scanned.", enemyScanNode.minRange); }
                    if (enemyScanNode != null && enabledSettings.Contains("Scan Max Range")) { enemyEntry.AddField("Scan Max Range", "Specifies the maximum distance the scan node can be scanned.", enemyScanNode.maxRange); }
                    if (enabledSettings.Contains("Can See Through Fog?")) { enemyEntry.AddField("Can See Through Fog?", "Specifies if an enemy can see through fog in foggy weather.", enemyObj.canSeeThroughFog); }
                    if (enabledSettings.Contains("Door Speed Multiplier")) { enemyEntry.AddField("Door Speed Multiplier", "Decides the speed at which enemies can open doors.\nCalculated with: 1 / x = time to open door in seconds.", enemyObj.doorSpeedMultiplier); }
                    if (enabledSettings.Contains("Is Daytime Enemy?")) { enemyEntry.AddField("Is Daytime Enemy?", "Whether an enemy is a daytime enemy.", enemyObj.isDaytimeEnemy); }
                    if (enabledSettings.Contains("Is Outdoor Enemy?")) { enemyEntry.AddField("Is Outdoor Enemy?", "Whether an enemy is a outdoor enemy.", enemyObj.isOutsideEnemy); }
                    if (enabledSettings.Contains("Loudness Multiplier")) { enemyEntry.AddField("Loudness Multiplier", "Multiplies the volume of an enemy's sounds.", enemyObj.loudnessMultiplier); }
                    if (enabledSettings.Contains("Max Count")) { enemyEntry.AddField("Max Count", "The maximum amount of an enemy that can be alive.", enemyObj.MaxCount); }
                    if (enabledSettings.Contains("Power Level")) { enemyEntry.AddField("Power Level", "The power level an enemy occupies.", enemyObj.PowerLevel); }
                    if (enabledSettings.Contains("Probability Curve")) { enemyEntry.AddField("Probability Curve", "Multiplies enemy spawn weight depending on time of day.\nKeyframes represented by x,y and separated by semicolons.", CurveToString(enemyObj.probabilityCurve)); }
                    if (enabledSettings.Contains("Use Falloff?")) { enemyEntry.AddField("Use Falloff?", "Whether or not to use the falloff curve.", enemyObj.useNumberSpawnedFalloff); }
                    if (enabledSettings.Contains("Falloff Curve")) { enemyEntry.AddField("Falloff Curve", "Multiplier to enemy spawn weight depending on how many are already spawned.\nKeyframes represented by x,y and separated by semicolons.", CurveToString(enemyObj.numberSpawnedFalloff)); }
                    if (enabledSettings.Contains("Group Spawn Count")) { enemyEntry.AddField("Group Spawn Count", "The amount of entities that will spawn of this type at once.", enemyObj.spawnInGroupsOf); }
                    if (enabledSettings.Contains("Normalized Time To Leave")) { enemyEntry.AddField("Normalized Time To Leave", "The time that an enemy leaves represented between 0 and 1 for the start and end of the day respectively.\nWARNING: Changing this for enemies that do not normally leave during the day may cause issues.", enemyObj.normalizedTimeInDayToLeave); }

                    EnemyAI enemyAI = enemyObj.enemyPrefab.GetComponent<EnemyAI>();
                    if (enemyAI != null && enabledSettings.Contains("Enemy HP")) { enemyEntry.AddField("Enemy HP", "The amount of HP an enemy has.", enemyAI.enemyHP); }
                    
                    if (enabledSettings.Contains("Can Die?")) { enemyEntry.AddField("Can Die?", "Whether or not an enemy can die.", enemyObj.canDie); }
                    if (enabledSettings.Contains("Destroy On Death?")) { enemyEntry.AddField("Destroy On Death?", "Whether or not an enemy is destroyed on death.", enemyObj.destroyOnDeath); }
                    if (enabledSettings.Contains("Can Destroy?")) { enemyEntry.AddField("Can Destroy?", "Whether or not an enemy can be destroyed.", enemyObj.canBeDestroyed); }
                    if (enabledSettings.Contains("Can Stun?")) { enemyEntry.AddField("Can Stun?", "Whether or not an enemy can be stunned.\nWARNING: Enabling this for enemies that have it disabled by default will likely cause issues, as the enemy most likely does not have stunning mechanics.", enemyObj.canBeStunned); }
                    if (enabledSettings.Contains("Stun Difficulty")) { enemyEntry.AddField("Stun Difficulty", "Modifies the difficulty of using the zap gun on this enemy.", enemyObj.stunGameDifficultyMultiplier); }
                    if (enabledSettings.Contains("Stun Time")) { enemyEntry.AddField("Stun Time", "Modifies the duration this enemy stays stunned.", enemyObj.stunTimeMultiplier); }
                    MiniLogger.LogInfo($"Recorded {enemy.name}");
                }
            }

            // LL/CRLib Content
            foreach (var enemy in Enemies.spawnableEnemies)
            {
                string enemyUUID = UUIDify($"LL - {enemy.enemy.enemyName} ({enemy.modName}.{enemy.enemy.name})");
                if (!registeredEnemies.Contains(enemyUUID))
                {
                    EnemyType enemyObj = enemy.enemy;
                    LunarConfigEntry enemyEntry = enemyFile.AddEntry(enemyUUID);
                    MiniLogger.LogInfo($"Recording {enemyObj.name}...");

                    string scanName = "";
                    ScanNodeProperties enemyScanNode = null;

                    if (enemy.enemy.enemyPrefab != null)
                    {
                        enemyScanNode = enemy.enemy.enemyPrefab.GetComponentInChildren<ScanNodeProperties>();
                        if (enemyScanNode != null)
                            scanName = enemyScanNode.headerText;
                    }

                    enemyEntry.AddField("Configure Content", "Enable to change any of the settings below.", false);
                    enemyEntry.AddField("Appropriate Aliases", "Changing this setting will do nothing, these are the names which LunarConfig will recognize as this object in other config options.\nThey are case-insensitve and do not regard whitespace.", $"{enemy.enemy.enemyName}, {scanName}");

                    if (enabledSettings.Contains("Display Name")) { enemyEntry.AddField("Display Name", "Specifies the name of the enemy.", enemyObj.enemyName); }
                    if (enemyScanNode != null && enabledSettings.Contains("Scan Name")) { enemyEntry.AddField("Scan Name", "Specifies the name of the enemy that appears on its scan node.", enemyScanNode.headerText); }
                    if (enemyScanNode != null && enabledSettings.Contains("Scan Subtext")) { enemyEntry.AddField("Scan Subtext", "Specifies the subtext that appears on the enemy's scan node.", enemyScanNode.subText); }
                    if (enemyScanNode != null && enabledSettings.Contains("Scan Min Range")) { enemyEntry.AddField("Scan Min Range", "Specifies the minimum distance the scan node can be scanned.", enemyScanNode.minRange); }
                    if (enemyScanNode != null && enabledSettings.Contains("Scan Max Range")) { enemyEntry.AddField("Scan Max Range", "Specifies the maximum distance the scan node can be scanned.", enemyScanNode.maxRange); }
                    if (enabledSettings.Contains("Can See Through Fog?")) { enemyEntry.AddField("Can See Through Fog?", "Specifies if an enemy can see through fog in foggy weather.", enemyObj.canSeeThroughFog); }
                    if (enabledSettings.Contains("Door Speed Multiplier")) { enemyEntry.AddField("Door Speed Multiplier", "Decides the speed at which enemies can open doors.\nCalculated with: 1 / x = time to open door in seconds.", enemyObj.doorSpeedMultiplier); }
                    if (enabledSettings.Contains("Is Daytime Enemy?")) { enemyEntry.AddField("Is Daytime Enemy?", "Whether an enemy is a daytime enemy.", enemyObj.isDaytimeEnemy); }
                    if (enabledSettings.Contains("Is Outdoor Enemy?")) { enemyEntry.AddField("Is Outdoor Enemy?", "Whether an enemy is a outdoor enemy.", enemyObj.isOutsideEnemy); }
                    if (enabledSettings.Contains("Loudness Multiplier")) { enemyEntry.AddField("Loudness Multiplier", "Multiplies the volume of an enemy's sounds.", enemyObj.loudnessMultiplier); }
                    if (enabledSettings.Contains("Max Count")) { enemyEntry.AddField("Max Count", "The maximum amount of an enemy that can be alive.", enemyObj.MaxCount); }
                    if (enabledSettings.Contains("Power Level")) { enemyEntry.AddField("Power Level", "The power level an enemy occupies.", enemyObj.PowerLevel); }
                    if (enabledSettings.Contains("Probability Curve")) { enemyEntry.AddField("Probability Curve", "Multiplies enemy spawn weight depending on time of day.\nKeyframes represented by x,y and separated by semicolons.", CurveToString(enemyObj.probabilityCurve)); }
                    if (enabledSettings.Contains("Use Falloff?")) { enemyEntry.AddField("Use Falloff?", "Whether or not to use the falloff curve.", enemyObj.useNumberSpawnedFalloff); }
                    if (enabledSettings.Contains("Falloff Curve")) { enemyEntry.AddField("Falloff Curve", "Multiplier to enemy spawn weight depending on how many are already spawned.\nKeyframes represented by x,y and separated by semicolons.", CurveToString(enemyObj.numberSpawnedFalloff)); }
                    if (enabledSettings.Contains("Group Spawn Count")) { enemyEntry.AddField("Group Spawn Count", "The amount of entities that will spawn of this type at once.", enemyObj.spawnInGroupsOf); }
                    if (enabledSettings.Contains("Normalized Time To Leave")) { enemyEntry.AddField("Normalized Time To Leave", "The time that an enemy leaves represented between 0 and 1 for the start and end of the day respectively.\nWARNING: Changing this for enemies that do not normally leave during the day may cause issues.", enemyObj.normalizedTimeInDayToLeave); }

                    EnemyAI enemyAI = enemyObj.enemyPrefab.GetComponent<EnemyAI>();
                    if (enemyAI != null && enabledSettings.Contains("Enemy HP")) { enemyEntry.AddField("Enemy HP", "The amount of HP an enemy has.", enemyAI.enemyHP); }

                    if (enabledSettings.Contains("Can Die?")) { enemyEntry.AddField("Can Die?", "Whether or not an enemy can die.", enemyObj.canDie); }
                    if (enabledSettings.Contains("Destroy On Death?")) { enemyEntry.AddField("Destroy On Death?", "Whether or not an enemy is destroyed on death.", enemyObj.destroyOnDeath); }
                    if (enabledSettings.Contains("Can Destroy?")) { enemyEntry.AddField("Can Destroy?", "Whether or not an enemy can be destroyed.", enemyObj.canBeDestroyed); }
                    if (enabledSettings.Contains("Can Stun?")) { enemyEntry.AddField("Can Stun?", "Whether or not an enemy can be stunned.\nWARNING: Enabling this for enemies that have it disabled by default will likely cause issues, as the enemy most likely does not have stunning mechanics.", enemyObj.canBeStunned); }
                    if (enabledSettings.Contains("Stun Difficulty")) { enemyEntry.AddField("Stun Difficulty", "Modifies the difficulty of using the zap gun on this enemy.", enemyObj.stunGameDifficultyMultiplier); }
                    if (enabledSettings.Contains("Stun Time")) { enemyEntry.AddField("Stun Time", "Modifies the duration this enemy stays stunned.", enemyObj.stunTimeMultiplier); }
                    MiniLogger.LogInfo($"Recorded {enemyObj.name}");
                }
            }

            ClearOrphanedEntries(enemyFile.file);
            enemyFile.file.Save();
            enemyFile.file.SaveOnConfigSet = true;

            MiniLogger.LogInfo("Enemy Configuration Initialized!");
        }

        public void InitMoons()
        {
            MiniLogger.LogInfo("Initializing Moon Configuration...");

            LunarConfigFile moonFile = AddFile(LunarConfig.MOON_FILE, LunarConfig.MOON_FILE_NAME);
            moonFile.file.SaveOnConfigSet = false;

            LunarConfigEntry enabledEntry = files[LunarConfig.CENTRAL_FILE_NAME].entries["Enabled Moon Settings"];
            HashSet<string> enabledSettings = new HashSet<string>();

            foreach (var setting in enabledEntry.fields.Keys)
            {
                if (enabledEntry.GetValue<bool>(setting))
                {
                    enabledSettings.Add(setting);
                }
            }

            HashSet<string> registeredMoons = new HashSet<string>();

            // LLL/Vanilla Content
            foreach (var moon in PatchedContent.ExtendedLevels)
            {
                string moonUUID = UUIDify($"LLL - {moon.NumberlessPlanetName} ({moon.UniqueIdentificationName})");
                if (!registeredMoons.Contains(moonUUID))
                {
                    SelectableLevel moonObj = moon.SelectableLevel;
                    LunarConfigEntry moonEntry = moonFile.AddEntry(moonUUID);
                    MiniLogger.LogInfo($"Recording {moon.name}...");
                    moonEntry.AddField("Configure Content", "Enable to change any of the settings below.", false);
                    moonEntry.AddField("Appropriate Aliases", "Changing this setting will do nothing, these are the names which LunarConfig will recognize as this object in other config options.\nThey are case-insensitve and do not regard whitespace.", $"{moon.NumberlessPlanetName}");

                    if (enabledSettings.Contains("Display Name")) { moonEntry.AddField("Display Name", "Changes the name of the moon.\nDoes not modify terminal commands/nodes.", moonObj.PlanetName); }
                    if (enabledSettings.Contains("Risk Level")) { moonEntry.AddField("Risk Level", "Changes the risk level of the moon.\nThis setting is only cosmetic.", moonObj.riskLevel); }
                    if (enabledSettings.Contains("Description")) { moonEntry.AddField("Description", "The description given to the moon.\nNew lines are represented by semi-colons.\nDoes not modify terminal commands/nodes.", moonObj.LevelDescription.Replace("\n", ";")); }

                    if (enabledSettings.Contains("Route Price")) { moonEntry.AddField("Route Price", "Changes the price to route to the moon.", moon.RoutePrice); }
                    if (enabledSettings.Contains("Is Hidden?")) { moonEntry.AddField("Is Hidden?", "Changes if the moon is hidden in the terminal.", moon.IsRouteHidden); }
                    if (enabledSettings.Contains("Is Locked?")) { moonEntry.AddField("Is Locked?", "Changes if the moon is locked in the terminal.", moon.IsRouteLocked); }

                    if (enabledSettings.Contains("Can Be Challenge Moon?")) { moonEntry.AddField("Can Be Challenge Moon?", "Defines whether or not a moon can be selected for the weekly challenge moon.", moonObj.planetHasTime); }
                    if (enabledSettings.Contains("Has Time?")) { moonEntry.AddField("Has Time?", "Defines whether a moon has time.", moonObj.planetHasTime); }
                    if (enabledSettings.Contains("Time Multiplier")) { moonEntry.AddField("Time Multiplier", "Multiplies the speed at which time progresses on a moon.", moonObj.DaySpeedMultiplier); }

                    string defaultDaytimeEnemies = "";
                    foreach (var enemy in moonObj.DaytimeEnemies)
                    {
                        if (enemy.rarity > 0)
                        {
                            if (defaultDaytimeEnemies != "")
                            {
                                defaultDaytimeEnemies += ", ";
                            }
                            defaultDaytimeEnemies += enemy.enemyType.enemyName + ":" + enemy.rarity;
                        }
                    }

                    if (enabledSettings.Contains("Daytime Probability Range")) { moonEntry.AddField("Daytime Probability Range", "The amount of daytime enemies spawned that can differ from the curve.\nFor instance, if this value is 3, and at the current time and spawn cycle 2 daytime enemies should spawn, anywhere between 0 and 5 can spawn.", moonObj.daytimeEnemiesProbabilityRange); }
                    if (enabledSettings.Contains("Daytime Curve")) { moonEntry.AddField("Daytime Curve", "Decides the amount of daytime enemies that spawn as the day progresses.\nKeyframes represented by x,y and separated by semicolons.", CurveToString(moonObj.daytimeEnemySpawnChanceThroughDay)); }
                    if (enabledSettings.Contains("Max Daytime Power")) { moonEntry.AddField("Max Daytime Power", "The amount of daytime power capacity that a moon has.", moonObj.maxDaytimeEnemyPowerCount); }
                    if (enabledSettings.Contains("Spawnable Daytime Enemies")) { moonEntry.AddField("Spawnable Daytime Enemies", "The base daytime enemies that can spawn on the moon.\nDenoted with NAME:RARITY, separated with commas.", defaultDaytimeEnemies); }

                    string defaultInsideEnemies = "";
                    foreach (var enemy in moonObj.Enemies)
                    {
                        if (enemy.rarity > 0)
                        {
                            if (defaultInsideEnemies != "")
                            {
                                defaultInsideEnemies += ", ";
                            }
                            defaultInsideEnemies += enemy.enemyType.enemyName + ":" + enemy.rarity;
                        }
                    }

                    if (enabledSettings.Contains("Interior Probability Range")) { moonEntry.AddField("Interior Probability Range", "The amount of interior enemies spawned that can differ from the curve.\nFor instance, if this value is 3, and at the current time and spawn cycle 2 interior enemies should spawn, anywhere between 0 and 5 can spawn.", moonObj.spawnProbabilityRange); }
                    if (enabledSettings.Contains("Interior Curve")) { moonEntry.AddField("Interior Curve", "Decides the amount of interior enemies that spawn as the day progresses.\nKeyframes represented by x,y and separated by semicolons.", CurveToString(moonObj.enemySpawnChanceThroughoutDay)); }
                    if (enabledSettings.Contains("Max Interior Power")) { moonEntry.AddField("Max Interior Power", "The amount of interior power capacity that a moon has.", moonObj.maxEnemyPowerCount); }
                    if (enabledSettings.Contains("Spawnable Interior Enemies")) { moonEntry.AddField("Spawnable Interior Enemies", "The base interior enemies that can spawn on the moon.\nDenoted with NAME:RARITY, separated with commas.", defaultInsideEnemies); }

                    string defaultOutsideEnemies = "";
                    foreach (var enemy in moonObj.OutsideEnemies)
                    {
                        if (enemy.rarity > 0)
                        {
                            if (defaultOutsideEnemies != "")
                            {
                                defaultOutsideEnemies += ", ";
                            }
                            defaultOutsideEnemies += enemy.enemyType.enemyName + ":" + enemy.rarity;
                        }
                    }

                    if (enabledSettings.Contains("Outside Curve")) { moonEntry.AddField("Outside Curve", "Decides the amount of outside enemies that spawn as the day progresses.\nKeyframes represented by x,y and separated by semicolons.", CurveToString(moonObj.outsideEnemySpawnChanceThroughDay)); }
                    if (enabledSettings.Contains("Max Outside Power")) { moonEntry.AddField("Max Outside Power", "The amount of outside power capacity that a moon has.", moonObj.maxOutsideEnemyPowerCount); }
                    if (enabledSettings.Contains("Spawnable Outside Enemies")) { moonEntry.AddField("Spawnable Outside Enemies", "The base outside enemies that can spawn on the moon.\nDenoted with NAME:RARITY, separated with commas.", defaultOutsideEnemies); }

                    string defaultScrap = "";
                    foreach (var item in  moonObj.spawnableScrap)
                    {
                        if (item.rarity > 0)
                        {
                            if (defaultScrap != "") 
                            {
                                defaultScrap += ", ";
                            }
                            defaultScrap += item.spawnableItem.itemName + ":" + item.rarity;
                        }
                    }

                    if (enabledSettings.Contains("Min Scrap")) { moonEntry.AddField("Min Scrap", "The minimum amount of scrap items that can spawn on a moon.", moonObj.minScrap); }
                    if (enabledSettings.Contains("Max Scrap")) { moonEntry.AddField("Max Scrap", "The maximum amount of scrap items that can spawn on a moon.", moonObj.maxScrap); }
                    if (enabledSettings.Contains("Value Multiplier")) { moonEntry.AddField("Value Multiplier", "The multiplier applied to the value of a moon's scrap.", 1f); }
                    if (enabledSettings.Contains("Amount Multiplier")) { moonEntry.AddField("Amount Multiplier", "The multiplier applied to the amount of scrap a moon has.", 1f); }
                    if (enabledSettings.Contains("Spawnable Scrap")) { moonEntry.AddField("Spawnable Scrap", "The base scrap that can spawn on the moon.\nDenoted with NAME:RARITY, separated with commas.", defaultScrap); }

                    string defaultDungeons = "";
                    foreach (var flow in PatchedContent.ExtendedDungeonFlows)
                    {
                        foreach (var entry in flow.LevelMatchingProperties.planetNames)
                        {
                            if (entry.Rarity > 0 && entry.Name.ToLower() == moon.NumberlessPlanetName.ToLower())
                            {
                                if (defaultDungeons != "")
                                {
                                    defaultDungeons += ", ";
                                }
                                defaultDungeons += flow.DungeonName + ":" + entry.Rarity;
                                break;
                            }
                        }
                    }

                    if (enabledSettings.Contains("Interior Multiplier")) { moonEntry.AddField("Interior Multiplier", "Changes the size of the interior generated.", moonObj.factorySizeMultiplier); }
                    if (enabledSettings.Contains("Possible Interiors")) { moonEntry.AddField("Possible Interiors", "The base interiors that can spawn on the moon.\nDenoted with NAME:RARITY, separated with commas.", defaultDungeons); }

                    string defaultTags = "";
                    foreach (var tag in moon.ContentTags)
                    {
                        if (defaultTags != "")
                        {
                            defaultTags += ", ";
                        }
                        defaultTags += tag.contentTagName;

                        if (useLLLTags)
                            foundTags.Add(UUIDify(tag.contentTagName).RemoveWhitespace());
                    }

                    if (enabledSettings.Contains("Tags")) { moonEntry.AddField("Tags", "Tags allocated to the moon.\nSeparate tags with commas.", defaultTags); }
                    MiniLogger.LogInfo($"Recorded {moon.name}");
                    registeredMoons.Add(moonUUID);
                }
            }

            ClearOrphanedEntries(moonFile.file);
            moonFile.file.Save();
            moonFile.file.SaveOnConfigSet = true;

            MiniLogger.LogInfo("Moon Configuration Initialized!");
        }

        public void InitDungeons()
        {
            MiniLogger.LogInfo("Initializing Dungeon Configuration...");

            LunarConfigFile dungeonFile = AddFile(LunarConfig.DUNGEON_FILE, LunarConfig.DUNGEON_FILE_NAME);
            dungeonFile.file.SaveOnConfigSet = false;

            LunarConfigEntry enabledEntry = files[LunarConfig.CENTRAL_FILE_NAME].entries["Enabled Dungeon Settings"];
            HashSet<string> enabledSettings = new HashSet<string>();

            foreach (var setting in enabledEntry.fields.Keys)
            {
                if (enabledEntry.GetValue<bool>(setting))
                {
                    enabledSettings.Add(setting);
                }
            }

            HashSet<string> registeredDungeons = new HashSet<string>();

            // LLL/Vanilla Content
            foreach (var dungeon in PatchedContent.ExtendedDungeonFlows)
            {
                string dungeonUUID = UUIDify($"LLL - {dungeon.DungeonName} ({dungeon.UniqueIdentificationName})");
                if (!registeredDungeons.Contains(dungeonUUID))
                {
                    DungeonFlow dungeonObj = dungeon.DungeonFlow;
                    LunarConfigEntry dungeonEntry = dungeonFile.AddEntry(dungeonUUID);
                    MiniLogger.LogInfo($"Recording {dungeon.name}...");
                    dungeonEntry.AddField("Configure Content", "Enable to change any of the settings below.", false);
                    dungeonEntry.AddField("Appropriate Aliases", "Changing this setting will do nothing, these are the names which LunarConfig will recognize as this object in other config options.\nThey are case-insensitve and do not regard whitespace.", $"{dungeon.DungeonName}, {dungeon.DungeonFlow.name}");

                    if (enabledSettings.Contains("Enable Dynamic Restriction")) { dungeonEntry.AddField("Enable Dynamic Restriction", "I don't know.", dungeon.IsDynamicDungeonSizeRestrictionEnabled); }
                    if (enabledSettings.Contains("Dynamic Dungeon Size Lerp Rate")) { dungeonEntry.AddField("Dynamic Dungeon Size Lerp Rate", "I don't know.", dungeon.DynamicDungeonSizeLerpRate); }
                    if (enabledSettings.Contains("Dynamic Dungeon Size Min")) { dungeonEntry.AddField("Dynamic Dungeon Size Min", "I don't know.", dungeon.DynamicDungeonSizeMinMax.x); }
                    if (enabledSettings.Contains("Dynamic Dungeon Size Max")) { dungeonEntry.AddField("Dynamic Dungeon Size Max", "I don't know.", dungeon.DynamicDungeonSizeMinMax.y); }
                    if (enabledSettings.Contains("Random Size Min")) { dungeonEntry.AddField("Random Size Min", "I don't know.", dungeon.DungeonFlow.Length.Min); }
                    if (enabledSettings.Contains("Random Size Max")) { dungeonEntry.AddField("Random Size Max", "I don't know.", dungeon.DungeonFlow.Length.Max); }
                    if (enabledSettings.Contains("Map Tile Size")) { dungeonEntry.AddField("Map Tile Size", "I don't know.", dungeon.MapTileSize); }
                    MiniLogger.LogInfo($"Recorded {dungeon.name}");
                    registeredDungeons.Add(dungeonUUID);
                }
            }

            ClearOrphanedEntries(dungeonFile.file);
            dungeonFile.file.Save();
            dungeonFile.file.SaveOnConfigSet = true;

            MiniLogger.LogInfo("Dungeon Configuration Initialized!");
        }

        public void InitMapObjects()
        {
            MiniLogger.LogInfo("Initializing Map Object Configuration...");

            LunarConfigFile mapObjectFile = AddFile(LunarConfig.MAP_OBJECT_FILE, LunarConfig.MAP_OBJECT_FILE_NAME);
            mapObjectFile.file.SaveOnConfigSet = false;

            LunarConfigEntry enabledEntry = files[LunarConfig.CENTRAL_FILE_NAME].entries["Enabled Map Object Settings"];
            HashSet<string> enabledSettings = new HashSet<string>();

            foreach (var setting in enabledEntry.fields.Keys)
            {
                if (enabledEntry.GetValue<bool>(setting))
                {
                    enabledSettings.Add(setting);
                }
            }

            HashSet<string> registeredMapObjects = new HashSet<string>();

            // LL/CRLib Content
            foreach (var mapObject in MapObjects.mapObjects)
            {
                if (mapObject.mapObject != null)
                {
                    string mapObjectUUID = UUIDify($"LL - {mapObject.mapObject.prefabToSpawn.name}");
                    if (!registeredMapObjects.Contains(mapObjectUUID))
                    {
                        SpawnableMapObject mapObj = mapObject.mapObject;
                        LunarConfigEntry mapObjectEntry = mapObjectFile.AddEntry(mapObjectUUID);
                        MiniLogger.LogInfo($"Recording {mapObj.prefabToSpawn.name}...");
                        mapObjectEntry.AddField("Configure Content", "Enable to change any of the settings below.", false);
                        if (enabledSettings.Contains("Face Away From Wall?")) { mapObjectEntry.AddField("Face Away From Wall?", "Whether or not the object should face away from walls.", mapObj.spawnFacingAwayFromWall); }
                        if (enabledSettings.Contains("Face Towards Wall?")) { mapObjectEntry.AddField("Face Towards Wall?", "Whether or not the object should face towards walls.", mapObj.spawnFacingWall); }
                        if (enabledSettings.Contains("Disallow Near Entrance?")) { mapObjectEntry.AddField("Disallow Near Entrance?", "Whether or not the object should not spawn near entrances.", mapObj.disallowSpawningNearEntrances); }
                        if (enabledSettings.Contains("Require Distance Between Spawns?")) { mapObjectEntry.AddField("Require Distance Between Spawns?", "Whether or not the object should spawn away from others.", mapObj.requireDistanceBetweenSpawns); }
                        if (enabledSettings.Contains("Flush Against Wall?")) { mapObjectEntry.AddField("Flush Against Wall?", "Whether or not the object should spawn flush against walls.", mapObj.spawnWithBackFlushAgainstWall); }
                        if (enabledSettings.Contains("Spawn Against Wall?")) { mapObjectEntry.AddField("Spawn Against Wall?", "Whether or not the object should spawn against walls.", mapObj.spawnWithBackToWall); }
                        mapObjectEntry.AddField("Base Curve", "The default curve to use if a field is empty.", "0,0 ; 1,0");

                        foreach (var level in PatchedContent.ExtendedLevels)
                        {
                            string curve = "";
                            foreach (var obj in level.SelectableLevel.spawnableMapObjects)
                            {
                                if (obj.prefabToSpawn == mapObject.mapObject.prefabToSpawn)
                                {
                                    curve = CurveToString(obj.numberToSpawn);
                                }
                            }
                            if (enabledSettings.Contains($"Level Curve - {UUIDify(level.NumberlessPlanetName)}")) { mapObjectEntry.AddField($"Level Curve - {UUIDify(level.NumberlessPlanetName)}", $"The spawn curve for this trap on {level.NumberlessPlanetName}.", curve); }
                        }

                        MiniLogger.LogInfo($"Recorded {mapObj.prefabToSpawn.name}");
                        registeredMapObjects.Add(mapObjectUUID);
                    }
                }
            }

            // Vanilla Content
            foreach (var moon in PatchedContent.ExtendedLevels)
            {
                foreach (var mapObj in moon.SelectableLevel.spawnableMapObjects)
                {
                    string mapObjectUUID = UUIDify($"LL - {mapObj.prefabToSpawn.name}");
                    if (!registeredMapObjects.Contains(mapObjectUUID) && !registeredMapObjects.Contains(UUIDify($"Vanilla - {mapObj.prefabToSpawn.name}")))
                    {
                        mapObjectUUID = UUIDify($"Vanilla - {mapObj.prefabToSpawn.name}");
                        LunarConfigEntry mapObjectEntry = mapObjectFile.AddEntry(mapObjectUUID);
                        MiniLogger.LogInfo($"Recording {mapObj.prefabToSpawn.name}...");
                        mapObjectEntry.AddField("Configure Content", "Enable to change any of the settings below.", false);
                        if (enabledSettings.Contains("Face Away From Wall?")) { mapObjectEntry.AddField("Face Away From Wall?", "Whether or not the object should face away from walls.", mapObj.spawnFacingAwayFromWall); }
                        if (enabledSettings.Contains("Face Towards Wall?")) { mapObjectEntry.AddField("Face Towards Wall?", "Whether or not the object should face towards walls.", mapObj.spawnFacingWall); }
                        if (enabledSettings.Contains("Disallow Near Entrance?")) { mapObjectEntry.AddField("Disallow Near Entrance?", "Whether or not the object should not spawn near entrances.", mapObj.disallowSpawningNearEntrances); }
                        if (enabledSettings.Contains("Require Distance Between Spawns?")) { mapObjectEntry.AddField("Require Distance Between Spawns?", "Whether or not the object should spawn away from others.", mapObj.requireDistanceBetweenSpawns); }
                        if (enabledSettings.Contains("Flush Against Wall?")) { mapObjectEntry.AddField("Flush Against Wall?", "Whether or not the object should spawn flush against walls.", mapObj.spawnWithBackFlushAgainstWall); }
                        if (enabledSettings.Contains("Spawn Against Wall?")) { mapObjectEntry.AddField("Spawn Against Wall?", "Whether or not the object should spawn against walls.", mapObj.spawnWithBackToWall); }
                        mapObjectEntry.AddField("Base Curve", "The default curve to use if a field is empty.", "0,0 ; 1,0");

                        foreach (var level in PatchedContent.ExtendedLevels)
                        {
                            string curve = "";
                            foreach (var obj in level.SelectableLevel.spawnableMapObjects)
                            {
                                if (obj.prefabToSpawn == mapObj.prefabToSpawn)
                                {
                                    curve = CurveToString(obj.numberToSpawn);
                                }
                            }
                            if (enabledSettings.Contains($"Level Curve - {UUIDify(level.NumberlessPlanetName)}")) { mapObjectEntry.AddField($"Level Curve - {UUIDify(level.NumberlessPlanetName)}", $"The spawn curve for this trap on {level.NumberlessPlanetName}.", curve); }
                        }

                        MiniLogger.LogInfo($"Recorded {mapObj.prefabToSpawn.name}");
                        registeredMapObjects.Add(mapObjectUUID);
                    }
                }
            }

            ClearOrphanedEntries(mapObjectFile.file);
            mapObjectFile.file.Save();
            mapObjectFile.file.SaveOnConfigSet = true;

            MiniLogger.LogInfo("Map Object Configuration Initialized!");
        }

        public void InitOutsideMapObjects()
        {
            MiniLogger.LogInfo("Initializing Outside Map Object Configuration...");

            LunarConfigFile mapObjectFile = AddFile(LunarConfig.OUTSIDE_MAP_OBJECT_FILE, LunarConfig.OUTSIDE_MAP_OBJECT_FILE_NAME);
            mapObjectFile.file.SaveOnConfigSet = false;

            LunarConfigEntry enabledEntry = files[LunarConfig.CENTRAL_FILE_NAME].entries["Enabled Outside Map Object Settings"];
            HashSet<string> enabledSettings = new HashSet<string>();

            foreach (var setting in enabledEntry.fields.Keys)
            {
                if (enabledEntry.GetValue<bool>(setting))
                {
                    enabledSettings.Add(setting);
                }
            }

            HashSet<string> registeredMapObjects = new HashSet<string>();

            // CRLib Content
            foreach (var mapObject in CRMod.AllMapObjects())
            {
                if (mapObject.OutsideSpawnMechanics != null)
                {
                    string mapObjectUUID = UUIDify($"CRLib - {mapObject.GameObject.name}");
                    if (!registeredMapObjects.Contains(mapObjectUUID))
                    {
                        LunarConfigEntry mapObjectEntry = mapObjectFile.AddEntry(mapObjectUUID);
                        MiniLogger.LogInfo($"Recording {mapObject.ObjectName}...");
                        mapObjectEntry.AddField("Configure Content", "Enable to change any of the settings below.", false);
                        mapObjectEntry.AddField("Base Curve", "The default curve to use if a field is empty.", "0,0 ; 1,0");

                        foreach (var level in PatchedContent.ExtendedLevels)
                        {
                            if (enabledSettings.Contains($"Level Curve - {level.NumberlessPlanetName}")) { mapObjectEntry.AddField($"Level Curve - {level.NumberlessPlanetName}", $"The spawn curve for this trap on {level.NumberlessPlanetName}.", CurveToString(mapObject.OutsideSpawnMechanics.CurveFunction(level.SelectableLevel))); }
                        }

                        MiniLogger.LogInfo($"Recorded {mapObject.ObjectName}");
                        registeredMapObjects.Add(mapObjectUUID);
                    }
                }
            }

            // Vanilla Content
            foreach (var moon in PatchedContent.ExtendedLevels)
            {
                foreach (var mapObj in moon.SelectableLevel.spawnableOutsideObjects)
                {
                    string mapObjectUUID = UUIDify($"Vanilla - {mapObj.spawnableObject.name}");
                    if (!registeredMapObjects.Contains(UUIDify($"Vanilla - {mapObj.spawnableObject.name}")) && !registeredMapObjects.Contains(UUIDify($"CRLib - {mapObj.spawnableObject.name}")) && !registeredMapObjects.Contains(UUIDify($"LL - {mapObj.spawnableObject.name}")))
                    {
                        LunarConfigEntry mapObjectEntry = mapObjectFile.AddEntry(mapObjectUUID);
                        MiniLogger.LogInfo($"Recording {mapObj.spawnableObject.name}...");
                        mapObjectEntry.AddField("Configure Content", "Enable to change any of the settings below.", false);
                        mapObjectEntry.AddField("Base Curve", "The default curve to use if a field is empty.", "0,0 ; 1,0");

                        foreach (var level in PatchedContent.ExtendedLevels)
                        {
                            string curve = "";
                            foreach (var obj in level.SelectableLevel.spawnableOutsideObjects)
                            {
                                if (obj.spawnableObject == mapObj.spawnableObject)
                                {
                                    curve = CurveToString(obj.randomAmount);
                                }
                            }
                            if (enabledSettings.Contains($"Level Curve - {level.NumberlessPlanetName}")) { mapObjectEntry.AddField($"Level Curve - {level.NumberlessPlanetName}", $"The spawn curve for this trap on {level.NumberlessPlanetName}.", curve); }
                        }

                        MiniLogger.LogInfo($"Recorded {mapObj.spawnableObject.name}");
                        registeredMapObjects.Add(mapObjectUUID);
                    }
                }
            }

            ClearOrphanedEntries(mapObjectFile.file);
            mapObjectFile.file.Save();
            mapObjectFile.file.SaveOnConfigSet = true;

            MiniLogger.LogInfo("Outside Map Object Configuration Initialized!");
        }

        public void FixDungeons()
        {
            foreach (var flow in PatchedContent.ExtendedDungeonFlows)
            {
                Dictionary<ExtendedLevel, int> rarities = new Dictionary<ExtendedLevel, int>();
                LevelMatchingProperties props = flow.LevelMatchingProperties;

                foreach (var moon in PatchedContent.ExtendedLevels)
                {
                    rarities[moon] = props.GetDynamicRarity(moon);
                }
                
                props.authorNames.Clear();
                props.currentWeather.Clear();
                props.planetNames.Clear();
                props.currentRoutePrice.Clear();
                props.levelTags.Clear();
                props.modNames.Clear();
                
                foreach (var rarity in rarities)
                {
                    if (rarity.Value > 0)
                    {
                        props.planetNames.Add(new StringWithRarity(rarity.Key.NumberlessPlanetName, rarity.Value));
                    }
                }

                flow.LevelMatchingProperties = props;
            }
        }
        */

        public LunarConfigFile AddFile(string path, string name)
        {
            LunarConfigFile file = new LunarConfigFile(path);
            files[name] = file;
            return file;
        }
    }
}
