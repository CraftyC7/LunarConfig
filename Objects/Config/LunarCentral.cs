using AsmResolver.PE.File;
using BepInEx;
using BepInEx.Configuration;
using Dawn;
using DunGen.Graph;
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

namespace LunarConfig.Objects.Config
{
    public class LunarCentral
    {
        public Dictionary<string, LunarConfigFile> files = new Dictionary<string, LunarConfigFile>();

        public static Dictionary<string, string> items = new Dictionary<string, string>();
        public static Dictionary<string, string> enemies = new Dictionary<string, string>();
        public static Dictionary<string, SelectableLevel> moons = new Dictionary<string, SelectableLevel>();
        //public static Dictionary<string, EnemyType> enemies = new Dictionary<string, EnemyType>();
        //public static Dictionary<string, ExtendedDungeonFlow> dungeons = new Dictionary<string, ExtendedDungeonFlow>();

        //public HashSet<string> foundTags = new HashSet<string>();
        //public bool useLLLTags = false;

        public static bool clearOrphans = false;
        public static bool backCompat = true;
        public static Dictionary<SelectableLevel, bool> definedChallengeMoons = new Dictionary<SelectableLevel, bool>();
        public static Dictionary<SelectableLevel, bool> definedChallengeMoonTimes = new Dictionary<SelectableLevel, bool>();

        //public static HashSet<string> currentStrings = new HashSet<string>();
        //public static HashSet<string> currentTags = new HashSet<string>();

        public static bool centralInitialized = false;
        public static bool itemsInitialized = false;
        public static bool enemiesInitialized = false;
        public static bool moonsInitialized = false;

        public static bool itemWeightsInitialized = false;
        public static bool enemyWeightsInitialized = false;

        public static bool configureItems = false;
        public static bool configureEnemies = false;
        public static bool configureMoons = false;

        public static HashSet<string> enabledItemSettings = new HashSet<string>();
        public static HashSet<string> enabledEnemySettings = new HashSet<string>();
        public static HashSet<string> enabledMoonSettings = new HashSet<string>();

        public static Dictionary<NamespacedKey<DawnMoonInfo>, string> cachedSpawnableScrap = new Dictionary<NamespacedKey<DawnMoonInfo>, string>();
        public static Dictionary<NamespacedKey<DawnMoonInfo>, string> cachedDaytimeEnemies = new Dictionary<NamespacedKey<DawnMoonInfo>, string>();
        public static Dictionary<NamespacedKey<DawnMoonInfo>, string> cachedInteriorEnemies = new Dictionary<NamespacedKey<DawnMoonInfo>, string>();
        public static Dictionary<NamespacedKey<DawnMoonInfo>, string> cachedOutsideEnemies = new Dictionary<NamespacedKey<DawnMoonInfo>, string>();

        public static Dictionary<string, Dictionary<NamespacedKey<DawnMoonInfo>, int>> defaultItemWeights = new Dictionary<string, Dictionary<NamespacedKey<DawnMoonInfo>, int>>();
        public static Dictionary<string, Dictionary<NamespacedKey<DawnMoonInfo>, int>> defaultDaytimeWeights = new Dictionary<string, Dictionary<NamespacedKey<DawnMoonInfo>, int>>();
        public static Dictionary<string, Dictionary<NamespacedKey<DawnMoonInfo>, int>> defaultInteriorWeights = new Dictionary<string, Dictionary<NamespacedKey<DawnMoonInfo>, int>>();
        public static Dictionary<string, Dictionary<NamespacedKey<DawnMoonInfo>, int>> defaultOutsideWeights = new Dictionary<string, Dictionary<NamespacedKey<DawnMoonInfo>, int>>();

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

        public static void TrySetItemWeight(string item, int rarity, NamespacedKey<DawnMoonInfo> moon)
        {
            if (!defaultItemWeights.TryGetValue(item, out var moonDict))
            {
                moonDict = new Dictionary<NamespacedKey<DawnMoonInfo>, int>();
                defaultItemWeights[item] = moonDict;
            }
            
            moonDict[moon] = rarity;
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

        /*
        public void CollectEnemies()
        {
            foreach (ExtendedEnemyType extendedEnemy in PatchedContent.ExtendedEnemyTypes)
            {
                EnemyType enemy = extendedEnemy.EnemyType;
                enemies[ConfigHelper.SanitizeString(enemy.enemyName)] = enemy;

                if (enemy.enemyPrefab != null)
                {
                    ScanNodeProperties enemyScanNode = enemy.enemyPrefab.GetComponentInChildren<ScanNodeProperties>();
                    if (enemyScanNode != null)
                    {
                        enemies[ConfigHelper.SanitizeString(enemyScanNode.headerText.ToLower().RemoveWhitespace())] = enemy;
                    }
                }
            }

            foreach (Enemies.SpawnableEnemy enemyType in Enemies.spawnableEnemies)
            {
                EnemyType enemy = enemyType.enemy;
                enemies[ConfigHelper.SanitizeString(enemy.enemyName)] = enemy;

                if (enemy.enemyPrefab != null)
                {
                    ScanNodeProperties enemyScanNode = enemy.enemyPrefab.GetComponentInChildren<ScanNodeProperties>();
                    if (enemyScanNode != null)
                    {
                        enemies[ConfigHelper.SanitizeString(enemyScanNode.headerText.ToLower().RemoveWhitespace())] = enemy;
                    }
                }
            }
        }

        public void CollectDungeons()
        {
            foreach (ExtendedDungeonFlow extendedDungeon in PatchedContent.ExtendedDungeonFlows)
            {
                if (extendedDungeon.DungeonName != "Facility" || extendedDungeon.DungeonFlow.name == "Level1Flow")
                {
                    dungeons[ConfigHelper.SanitizeString(extendedDungeon.DungeonName)] = extendedDungeon;
                }
                dungeons[ConfigHelper.SanitizeString(extendedDungeon.DungeonFlow.name)] = extendedDungeon;
            }
        }
        */
        public void InitCentral()
        {
            LunarConfigFile centralFile = AddFile(LunarConfig.CENTRAL_FILE, LunarConfig.CENTRAL_FILE_NAME);
            centralFile.file.SaveOnConfigSet = false;

            LunarConfigEntry configEntry = centralFile.AddEntry("Configuration");
            configEntry.AddField("Configure Items", "Check this to generate and use configuration files for items.", true);
            configEntry.AddField("Configure Enemies", "Check this to generate and use configuration files for enemies.", true);
            configEntry.AddField("Configure Moons", "Check this to generate and use configuration files for moons.", true);
            configEntry.AddField("Configure Dungeons", "Check this to generate and use configuration files for dungeons.", true);
            configEntry.AddField("Configure Map Objects", "Check this to generate and use configuration files for map objects.", true);
            configEntry.AddField("Configure Outside Map Objects", "Check this to generate and use configuration files for outside map objects.", true);
            configEntry.AddField("Enable Backwards Compat", "Allows Lunar to look for config entries that are named using the previous v0.1.x system, I would advise turning this off after you have all your previous values.", false);
            configEntry.AddField("Clear Orphaned Entries", "WARNING: Enabling this will delete any config entries that get disabled when the configuration is refreshed!", false);
            backCompat = configEntry.GetValue<bool>("Enable Backwards Compat");
            clearOrphans = configEntry.GetValue<bool>("Clear Orphaned Entries");
            configureItems = configEntry.GetValue<bool>("Configure Items");
            configureEnemies = configEntry.GetValue<bool>("Configure Enemies");
            configureMoons = configEntry.GetValue<bool>("Configure Moons");

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
                configItems.AddField("Cost", "Disable this to disable configuring this property in item config entries.", true);

                configItems.AddField("Info Node Text", "Enable this to enable configuring this property in item config entries.", false);
                configItems.AddField("Request Node Text", "Enable this to enable configuring this property in item config entries.", false);
                configItems.AddField("Receipt Node Text", "Enable this to enable configuring this property in item config entries.", false);

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
                        ScanNodeProperties itemScanNode = null;
                        DawnShopItemInfo shopInfo = null;
                        DawnPurchaseInfo purchaseInfo = null;
                        TerminalNode infoNode = null;
                        TerminalNode requestNode = null;
                        TerminalNode receiptNode = null;

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

                        if (infoNode != null) { itemEntry.TryAddField(enabledItemSettings, "Info Node Text", "The text of the terminal when viewing the info of an item. New lines are represented by semi-colons.", infoNode.displayText.Replace("\n", ";")); }
                        if (requestNode != null) { itemEntry.TryAddField(enabledItemSettings, "Request Node Text", "The text of the terminal when requesting an item. New lines are represented by semi-colons.", requestNode.displayText.Replace("\n", ";")); }
                        if (receiptNode != null) { itemEntry.TryAddField(enabledItemSettings, "Receipt Node Text", "The text of the terminal after purchasing an item. New lines are represented by semi-colons.", receiptNode.displayText.Replace("\n", ";")); }

                        if (purchaseInfo != null) { itemEntry.TryAddField(enabledItemSettings, "Cost", "The cost of the item if it is sold in the shop.", purchaseInfo.Cost.Provide()); }

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
            InitItemWeights();
        }

        public void InitItemWeights()
        {
            MiniLogger.LogInfo("Checking Item Weights...");
            if (enabledMoonSettings.Contains("Spawnable Scrap") && configureMoons && moonsInitialized && itemsInitialized && !itemWeightsInitialized)
            {
                MiniLogger.LogInfo("Configuring Item Weights");

                foreach (var cache in cachedSpawnableScrap)
                {
                    foreach (var item in cache.Value.Split(","))
                    {
                        string[] splits = item.Split(":");

                        string id = splits[0];
                        int rarity = int.Parse(CleanString(splits[1]));
                        TrySetItemWeight(GetDawnUUID(items, id), rarity, cache.Key);
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
                        }

                        if (!dawnItem.HasTag(DawnLibTags.LunarConfig)) { dawnItem.Internal_AddTag(DawnLibTags.LunarConfig); }

                        WeightTableBuilder<DawnMoonInfo> itemWeightBuilder = new();

                        if (defaultItemWeights.TryGetValue(uuid, out Dictionary<NamespacedKey<DawnMoonInfo>, int> moonVars))
                        {
                            foreach (var moon in moonVars)
                            {
                                itemWeightBuilder.AddWeight(moon.Key, moon.Value);
                            }
                        }

                        ProviderTable<int?, DawnMoonInfo> newTable = itemWeightBuilder.Build();

                        // SET WEIGHTS
                        if (scrapInfo != null)
                        {
                            scrapInfo.Weights = newTable;
                        }
                        else
                        {
                            dawnItem.ScrapInfo = new DawnScrapItemInfo(newTable);
                        }
                    }
                    catch (Exception e)
                    {
                        MiniLogger.LogError($"LunarConfig encountered an issue while configuring weights for {uuid}, please report this!\n{e}");
                    }
                }
                itemWeightsInitialized = true;
            }
        }

        public void InitEnemies()
        {
            if (!centralInitialized)
            {
                InitCentral();
            }

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
                }

                enemyWeightsInitialized = true;
            }
        }

        public void InitMoons()
        {
            if (!centralInitialized)
            {
                InitCentral();
            }

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
                        foreach (var item in moonObj.spawnableScrap)
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

                        string defaultDayEnemies = "";
                        foreach (var item in moonObj.DaytimeEnemies)
                        {
                            if (item.rarity > 0)
                            {
                                if (defaultDayEnemies != "")
                                {
                                    defaultDayEnemies += ", ";
                                }
                                defaultDayEnemies += item.enemyType.enemyName + ":" + item.rarity;
                            }
                        }

                        string defaultInteriorEnemies = "";
                        foreach (var item in moonObj.Enemies)
                        {
                            if (item.rarity > 0)
                            {
                                if (defaultInteriorEnemies != "")
                                {
                                    defaultInteriorEnemies += ", ";
                                }
                                defaultInteriorEnemies += item.enemyType.enemyName + ":" + item.rarity;
                            }
                        }

                        string defaultOutsideEnemies = "";
                        foreach (var item in moonObj.OutsideEnemies)
                        {
                            if (item.rarity > 0)
                            {
                                if (defaultOutsideEnemies != "")
                                {
                                    defaultOutsideEnemies += ", ";
                                }
                                defaultOutsideEnemies += item.enemyType.enemyName + ":" + item.rarity;
                            }
                        }

                        moonEntry.TryAddField(enabledMoonSettings, "Spawnable Scrap", "The base scrap that can spawn on the moon.\nDenoted with NAME:RARITY, separated with commas.", defaultScrap);
                        moonEntry.TryAddField(enabledMoonSettings, "Spawnable Daytime Enemies", "The base daytime enemies that can spawn on the moon.\nDenoted with NAME:RARITY, separated with commas.", defaultDayEnemies);
                        moonEntry.TryAddField(enabledMoonSettings, "Spawnable Interior Enemies", "The base interior enemies that can spawn on the moon.\nDenoted with NAME:RARITY, separated with commas.", defaultInteriorEnemies);
                        moonEntry.TryAddField(enabledMoonSettings, "Spawnable Outside Enemies", "The base outside enemies that can spawn on the moon.\nDenoted with NAME:RARITY, separated with commas.", defaultOutsideEnemies);

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
                                if (!key.IsNullOrWhiteSpace()) { moons[CleanString(key)] = moonObj; }
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
                                        if (moonEntry.GetValue<bool>("Is Locked?"))
                                        {
                                            predicate = new ConstantTerminalPredicate(TerminalPurchaseResult.Hidden());
                                        }
                                    }

                                    purchaseInfo.PurchasePredicate = predicate;
                                }
                            }

                            if (enabledMoonSettings.Contains("Spawnable Scrap")) { cachedSpawnableScrap[dawnMoon.TypedKey] = moonEntry.GetValue<string>("Spawnable Scrap"); }
                            if (enabledMoonSettings.Contains("Spawnable Daytime Enemies")) { cachedDaytimeEnemies[dawnMoon.TypedKey] = moonEntry.GetValue<string>("Spawnable Daytime Enemies"); }
                            if (enabledMoonSettings.Contains("Spawnable Interior Enemies")) { cachedInteriorEnemies[dawnMoon.TypedKey] = moonEntry.GetValue<string>("Spawnable Interior Enemies"); }
                            if (enabledMoonSettings.Contains("Spawnable Outside Enemies")) { cachedOutsideEnemies[dawnMoon.TypedKey] = moonEntry.GetValue<string>("Spawnable Outside Enemies"); }
                        }
                        else
                        {
                            moons[numberlessName] = moonObj;

                            if (enabledMoonSettings.Contains("Spawnable Scrap"))
                            {
                                foreach (var scrap in moonObj.spawnableScrap)
                                {
                                    try
                                    {
                                        TrySetItemWeight(scrap.spawnableItem.GetDawnInfo().Key.ToString(), scrap.rarity, dawnMoon.TypedKey);
                                    }
                                    catch (Exception e)
                                    {
                                        MiniLogger.LogWarning($"Encountered incorrectly formatted scrap item on {numberlessName}");
                                    }
                                }
                            }

                            if (enabledMoonSettings.Contains("Spawnable Daytime Enemies"))
                            {
                                foreach (var enemy in moonObj.DaytimeEnemies)
                                {
                                    try
                                    {
                                        TrySetDaytimeWeight(enemy.enemyType.GetDawnInfo().Key.ToString(), enemy.rarity, dawnMoon.TypedKey);
                                    }
                                    catch (Exception e)
                                    {
                                        MiniLogger.LogWarning($"Encountered incorrectly formatted daytime enemy on {numberlessName}");
                                    }
                                }
                            }

                            if (enabledMoonSettings.Contains("Spawnable Interior Enemies"))
                            {
                                foreach (var enemy in moonObj.Enemies)
                                {
                                    try
                                    {
                                        TrySetInteriorWeight(enemy.enemyType.GetDawnInfo().Key.ToString(), enemy.rarity, dawnMoon.TypedKey);
                                    }
                                    catch (Exception e)
                                    {
                                        MiniLogger.LogWarning($"Encountered incorrectly formatted interior enemy on {numberlessName}");
                                    }
                                }
                            }

                            if (enabledMoonSettings.Contains("Spawnable Outside Enemies"))
                            {
                                foreach (var enemy in moonObj.OutsideEnemies)
                                {
                                    try
                                    {
                                        TrySetOutsideWeight(enemy.enemyType.GetDawnInfo().Key.ToString(), enemy.rarity, dawnMoon.TypedKey);
                                    }
                                    catch (Exception e)
                                    {
                                        MiniLogger.LogWarning($"Encountered incorrectly formatted outside enemy on {numberlessName}");
                                    }
                                }
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

            moonsInitialized = true;
            InitItemWeights();
            InitEnemyWeights();
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
