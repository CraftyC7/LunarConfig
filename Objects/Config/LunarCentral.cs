using BepInEx.Configuration;
using CodeRebirthLib;
using DunGen.Graph;
using HarmonyLib;
using LethalLevelLoader;
using LethalLib.Modules;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using WeatherRegistry;
using ConfigHelper = LethalLevelLoader.ConfigHelper;

namespace LunarConfig.Objects.Config
{
    public class LunarCentral
    {
        public Dictionary<string, LunarConfigFile> files = new Dictionary<string, LunarConfigFile>();

        public Dictionary<string, Item> items = new Dictionary<string, Item>();
        public Dictionary<string, EnemyType> enemies = new Dictionary<string, EnemyType>();
        public Dictionary<string, ExtendedDungeonFlow> dungeons = new Dictionary<string, ExtendedDungeonFlow>();

        public HashSet<string> foundTags = new HashSet<string>();
        public bool useLLLTags = false;
        public static bool clearOrphans = false;

        public static HashSet<string> currentStrings = new HashSet<string>();
        public static HashSet<string> currentTags = new HashSet<string>();

        public LunarCentral() { }

        public string UUIDify(string uuid)
        {
            return uuid.Replace("=", "").Replace("\n", "").Replace("\t", "").Replace("\\", "").Replace("\"", "").Replace("\'", "").Replace("[", "").Replace("]", "");
        }

        public string CurveToString(AnimationCurve curve)
        {
            return string.Join(" ; ", curve.keys.Select(kf => $"{kf.time},{kf.value}"));
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
        
        public void InitConfig()
        {
            InitCollections();

            FixDungeons();

            LunarConfigEntry centralConfig = files[LunarConfig.CENTRAL_FILE_NAME].entries["Configuration"];

            if (centralConfig.GetValue<bool>("Configure Items")) { InitItems(); }
            if (centralConfig.GetValue<bool>("Configure Enemies")) { InitEnemies(); }
            if (centralConfig.GetValue<bool>("Configure Moons")) { InitMoons(); }
            if (centralConfig.GetValue<bool>("Configure Dungeons")) { InitDungeons(); }
            if (centralConfig.GetValue<bool>("Configure Map Objects")) { InitMapObjects(); }
            if (centralConfig.GetValue<bool>("Configure Outside Map Objects")) { InitOutsideMapObjects(); }
        }

        public void InitCollections()
        {
            CollectItems();
            CollectEnemies();
            CollectDungeons();
            foreach (var str in dungeons.Keys)
            {
                MiniLogger.LogInfo($"DUNGEON: {str}");
            }
        }

        public void CollectItems()
        {
            foreach (ExtendedItem extendedItem in PatchedContent.ExtendedItems)
            {
                Item item = extendedItem.Item;
                items[ConfigHelper.SanitizeString(item.itemName)] = item;
                items[ConfigHelper.SanitizeString(item.name)] = item;
            }

            foreach (Items.ScrapItem scrapItem in Items.scrapItems)
            {
                Item item = scrapItem.item;
                items[ConfigHelper.SanitizeString(item.itemName)] = item;
                items[ConfigHelper.SanitizeString(item.name)] = item;
            }

            foreach (Items.ShopItem scrapItem in Items.shopItems)
            {
                Item item = scrapItem.item;
                items[ConfigHelper.SanitizeString(item.itemName)] = item;
                items[ConfigHelper.SanitizeString(item.name)] = item;
            }

            foreach (Items.PlainItem scrapItem in Items.plainItems)
            {
                Item item = scrapItem.item;
                items[ConfigHelper.SanitizeString(item.itemName)] = item;
                items[ConfigHelper.SanitizeString(item.name)] = item;
            }
        }

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
                dungeons[ConfigHelper.SanitizeString(extendedDungeon.name)] = extendedDungeon;
            }
        }

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
            configEntry.AddField("Run Late (CentralConfig Port)", "IMPORTANT: This setting will make LunarConfig initialize later than usual, breaking some of it's functionality.\nThis should only be used if you are trying to port settings from a mod like CentralConfig, and should be turned off after the first use.\nLunar Config isn't perfect at porting, but it can get most things right, also remember to delete all Lunar Config files before trying this for the best result!", false);
            configEntry.AddField("Clear Orphaned Entries", "WARNING: Enabling this will delete any config entries that get disabled when the configuration is refreshed!", false);
            clearOrphans = configEntry.GetValue<bool>("Clear Orphaned Entries");

            if (configEntry.GetValue<bool>("Configure Items"))
            {
                LunarConfigEntry configItems = centralFile.AddEntry("Enabled Item Settings");
                configItems.AddField("Display Name", "Disable this to disable configuring this property in item config entries.", true);
                configItems.AddField("Minimum Value", "Disable this to disable configuring this property in item config entries.", true);
                configItems.AddField("Maximum Value", "Disable this to disable configuring this property in item config entries.", true);
                configItems.AddField("Credits Worth", "Disable this to disable configuring this property in item config entries.", true);
                configItems.AddField("Weight", "Disable this to disable configuring this property in item config entries.", true);
                configItems.AddField("Conductivity", "Disable this to disable configuring this property in item config entries.", true);
                configItems.AddField("Two-Handed", "Disable this to disable configuring this property in item config entries.", true);
                configItems.AddField("Is Scrap?", "Disable this to disable configuring this property in item config entries.", true);
            }

            if (configEntry.GetValue<bool>("Configure Enemies"))
            {
                LunarConfigEntry configEnemies = centralFile.AddEntry("Enabled Enemy Settings");
                configEnemies.AddField("Display Name", "Disable this to disable configuring this property in enemy config entries.", true);
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
                configEnemies.AddField("Normalized Time To Leave", "Disable this to disable configuring this property in enemy config entries.", true);
                configEnemies.AddField("Enemy HP", "Disable this to disable configuring this property in enemy config entries.", true);
                configEnemies.AddField("Can Die?", "Disable this to disable configuring this property in enemy config entries.", true);
                configEnemies.AddField("Destroy On Death?", "Disable this to disable configuring this property in enemy config entries.", true);
                configEnemies.AddField("Can Destroy?", "Disable this to disable configuring this property in enemy config entries.", true);
                configEnemies.AddField("Can Stun?", "Disable this to disable configuring this property in enemy config entries.", true);
                configEnemies.AddField("Stun Difficulty", "Disable this to disable configuring this property in enemy config entries.", true);
                configEnemies.AddField("Stun Time", "Disable this to disable configuring this property in enemy config entries.", true);
            }

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

            ClearOrphanedEntries(centralFile.file);
            centralFile.file.Save();
            centralFile.file.SaveOnConfigSet = true;
        }

        public void InitItems()
        {
            MiniLogger.LogInfo("Initializing Item Configuration...");

            LunarConfigFile itemFile = AddFile(LunarConfig.ITEM_FILE, LunarConfig.ITEM_FILE_NAME);
            itemFile.file.SaveOnConfigSet = false;

            LunarConfigEntry enabledEntry = files[LunarConfig.CENTRAL_FILE_NAME].entries["Enabled Item Settings"];
            HashSet<string> enabledSettings = new HashSet<string>();

            foreach (var setting in enabledEntry.fields.Keys)
            {
                if (enabledEntry.GetValue<bool>(setting))
                {
                    enabledSettings.Add(setting);
                }
            }

            HashSet<string> registeredItems = new HashSet<string>();
            
            // LLL/Vanilla Content
            foreach (var item in PatchedContent.ExtendedItems)
            {
                string itemUUID = UUIDify($"LLL - {item.Item.itemName} ({item.UniqueIdentificationName})");
                if (!registeredItems.Contains(itemUUID))
                {
                    Item itemObj = item.Item;
                    LunarConfigEntry itemEntry = itemFile.AddEntry(itemUUID);
                    MiniLogger.LogInfo($"Recording {item.name}...");
                    itemEntry.AddField("Configure Content", "Enable to change any of the settings below.", false);
                    itemEntry.AddField("Appropriate Aliases", "Changing this setting will do nothing, these are the names which LunarConfig will recognize as this object in other config options.\nThey are case-insensitve and do not regard whitespace.", $"{ConfigHelper.SanitizeString(item.Item.itemName)}, {ConfigHelper.SanitizeString(item.Item.name)}");
                    if (enabledSettings.Contains("Display Name")) { itemEntry.AddField("Display Name", "Specifies the name that appears when scanning the item.", itemObj.itemName); }
                    if (enabledSettings.Contains("Minimum Value")) { itemEntry.AddField("Minimum Value", "The minimum scrap value and item can have.\nTypically multiplied by 0.4, setting not applicable to non-scrap.\nDoes not work on items like Apparatus and items from enemies (Hives, Double-barrel).", itemObj.minValue); }
                    if (enabledSettings.Contains("Maximum Value")) { itemEntry.AddField("Maximum Value", "The maximum scrap value and item can have.\nTypically multiplied by 0.4, setting not applicable to non-scrap.\nDoes not work on items like Apparatus and items from enemies (Hives, Double-barrel).", itemObj.maxValue); }
                    if (enabledSettings.Contains("Credits Worth")) { itemEntry.AddField("Credits Worth", "The value of an item if it is sold in the shop.", itemObj.creditsWorth); }
                    if (enabledSettings.Contains("Weight")) { itemEntry.AddField("Weight", "Specifies the weight of an item.\nCalculated with: (x - 1) * 105 = weight in pounds.", itemObj.weight); }
                    if (enabledSettings.Contains("Conductivity")) { itemEntry.AddField("Conductivity", "Specifies whether an item is conductive.", itemObj.isConductiveMetal); }
                    if (enabledSettings.Contains("Two-Handed")) { itemEntry.AddField("Two-Handed", "Specifies whether an item is two-handed.", itemObj.twoHanded); }
                    if (enabledSettings.Contains("Is Scrap?")) { itemEntry.AddField("Is Scrap?", "Specifies if an item is scrap or gear.\nThis decides whether an item can be sold to the company for credits.", itemObj.isScrap); }
                    MiniLogger.LogInfo($"Recorded {item.name}");
                    registeredItems.Add(itemUUID);
                }
            }

            // LL/CRLib Content
            foreach (var item in Items.scrapItems)
            {
                string itemUUID = UUIDify($"LL - {item.item.itemName} ({item.modName}.{item.item.name})");
                if (!registeredItems.Contains(itemUUID))
                {
                    Item itemObj = item.item;
                    LunarConfigEntry itemEntry = itemFile.AddEntry(itemUUID);
                    MiniLogger.LogInfo($"Recording {itemObj.name}...");
                    itemEntry.AddField("Configure Content", "Enable to change any of the settings below.", false);
                    itemEntry.AddField("Appropriate Aliases", "Changing this setting will do nothing, these are the names which LunarConfig will recognize as this object in other config options.\nThey are case-insensitve and do not regard whitespace.", $"{ConfigHelper.SanitizeString(item.item.itemName)}, {ConfigHelper.SanitizeString(item.item.name)}");
                    if (enabledSettings.Contains("Display Name")) { itemEntry.AddField("Display Name", "Specifies the name that appears when scanning the item.", itemObj.itemName); }
                    if (enabledSettings.Contains("Minimum Value")) { itemEntry.AddField("Minimum Value", "The minimum scrap value and item can have.\nTypically multiplied by 0.4, setting not applicable to non-scrap.\nDoes not work on items like Apparatus and items from enemies (Hives, Double-barrel).", itemObj.minValue); }
                    if (enabledSettings.Contains("Maximum Value")) { itemEntry.AddField("Maximum Value", "The maximum scrap value and item can have.\nTypically multiplied by 0.4, setting not applicable to non-scrap.\nDoes not work on items like Apparatus and items from enemies (Hives, Double-barrel).", itemObj.maxValue); }
                    if (enabledSettings.Contains("Credits Worth")) { itemEntry.AddField("Credits Worth", "The value of an item if it is sold in the shop.", itemObj.creditsWorth); }
                    if (enabledSettings.Contains("Weight")) { itemEntry.AddField("Weight", "Specifies the weight of an item.\nCalculated with: (x - 1) * 105 = weight in pounds.", itemObj.weight); }
                    if (enabledSettings.Contains("Conductivity")) { itemEntry.AddField("Conductivity", "Specifies whether an item is conductive.", itemObj.isConductiveMetal); }
                    if (enabledSettings.Contains("Two-Handed")) { itemEntry.AddField("Two-Handed", "Specifies whether an item is two-handed.", itemObj.twoHanded); }
                    if (enabledSettings.Contains("Is Scrap?")) { itemEntry.AddField("Is Scrap?", "Specifies if an item is scrap or gear.\nThis decides whether an item can be sold to the company for credits.", itemObj.isScrap); }
                    MiniLogger.LogInfo($"Recorded {itemObj.name}");
                    registeredItems.Add(itemUUID);
                }
            }

            foreach (var item in Items.shopItems)
            {
                string itemUUID = UUIDify($"LL - {item.item.itemName} ({item.modName}.{item.item.name})");
                if (!registeredItems.Contains(itemUUID))
                {
                    Item itemObj = item.item;
                    LunarConfigEntry itemEntry = itemFile.AddEntry(itemUUID);
                    MiniLogger.LogInfo($"Recording {itemObj.name}...");
                    itemEntry.AddField("Configure Content", "Enable to change any of the settings below.", false);
                    itemEntry.AddField("Appropriate Aliases", "Changing this setting will do nothing, these are the names which LunarConfig will recognize as this object in other config options.\nThey are case-insensitve and do not regard whitespace.", $"{ConfigHelper.SanitizeString(item.item.itemName)}, {ConfigHelper.SanitizeString(item.item.name)}");
                    if (enabledSettings.Contains("Display Name")) { itemEntry.AddField("Display Name", "Specifies the name that appears when scanning the item.", itemObj.itemName); }
                    if (enabledSettings.Contains("Minimum Value")) { itemEntry.AddField("Minimum Value", "The minimum scrap value and item can have.\nTypically multiplied by 0.4, setting not applicable to non-scrap.\nDoes not work on items like Apparatus and items from enemies (Hives, Double-barrel).", itemObj.minValue); }
                    if (enabledSettings.Contains("Maximum Value")) { itemEntry.AddField("Maximum Value", "The maximum scrap value and item can have.\nTypically multiplied by 0.4, setting not applicable to non-scrap.\nDoes not work on items like Apparatus and items from enemies (Hives, Double-barrel).", itemObj.maxValue); }
                    if (enabledSettings.Contains("Credits Worth")) { itemEntry.AddField("Credits Worth", "The value of an item if it is sold in the shop.", itemObj.creditsWorth); }
                    if (enabledSettings.Contains("Weight")) { itemEntry.AddField("Weight", "Specifies the weight of an item.\nCalculated with: (x - 1) * 105 = weight in pounds.", itemObj.weight); }
                    if (enabledSettings.Contains("Conductivity")) { itemEntry.AddField("Conductivity", "Specifies whether an item is conductive.", itemObj.isConductiveMetal); }
                    if (enabledSettings.Contains("Two-Handed")) { itemEntry.AddField("Two-Handed", "Specifies whether an item is two-handed.", itemObj.twoHanded); }
                    if (enabledSettings.Contains("Is Scrap?")) { itemEntry.AddField("Is Scrap?", "Specifies if an item is scrap or gear.\nThis decides whether an item can be sold to the company for credits.", itemObj.isScrap); }
                    MiniLogger.LogInfo($"Recorded {itemObj.name}");
                    registeredItems.Add(itemUUID);
                }
            }

            foreach (var item in Items.plainItems)
            {
                string itemUUID = UUIDify($"LL - {item.item.itemName} ({item.modName}.{item.item.name})");
                if (!registeredItems.Contains(itemUUID))
                {
                    Item itemObj = item.item;
                    LunarConfigEntry itemEntry = itemFile.AddEntry(itemUUID);
                    MiniLogger.LogInfo($"Recording {itemObj.name}...");
                    itemEntry.AddField("Configure Content", "Enable to change any of the settings below.", false);
                    itemEntry.AddField("Appropriate Aliases", "Changing this setting will do nothing, these are the names which LunarConfig will recognize as this object in other config options.\nThey are case-insensitve and do not regard whitespace.", $"{ConfigHelper.SanitizeString(item.item.itemName)}, {ConfigHelper.SanitizeString(item.item.name)}");
                    if (enabledSettings.Contains("Display Name")) { itemEntry.AddField("Display Name", "Specifies the name that appears when scanning the item.", itemObj.itemName); }
                    if (enabledSettings.Contains("Minimum Value")) { itemEntry.AddField("Minimum Value", "The minimum scrap value and item can have.\nTypically multiplied by 0.4, setting not applicable to non-scrap.\nDoes not work on items like Apparatus and items from enemies (Hives, Double-barrel).", itemObj.minValue); }
                    if (enabledSettings.Contains("Maximum Value")) { itemEntry.AddField("Maximum Value", "The maximum scrap value and item can have.\nTypically multiplied by 0.4, setting not applicable to non-scrap.\nDoes not work on items like Apparatus and items from enemies (Hives, Double-barrel).", itemObj.maxValue); }
                    if (enabledSettings.Contains("Credits Worth")) { itemEntry.AddField("Credits Worth", "The value of an item if it is sold in the shop.", itemObj.creditsWorth); }
                    if (enabledSettings.Contains("Weight")) { itemEntry.AddField("Weight", "Specifies the weight of an item.\nCalculated with: (x - 1) * 105 = weight in pounds.", itemObj.weight); }
                    if (enabledSettings.Contains("Conductivity")) { itemEntry.AddField("Conductivity", "Specifies whether an item is conductive.", itemObj.isConductiveMetal); }
                    if (enabledSettings.Contains("Two-Handed")) { itemEntry.AddField("Two-Handed", "Specifies whether an item is two-handed.", itemObj.twoHanded); }
                    if (enabledSettings.Contains("Is Scrap?")) { itemEntry.AddField("Is Scrap?", "Specifies if an item is scrap or gear.\nThis decides whether an item can be sold to the company for credits.", itemObj.isScrap); }
                    MiniLogger.LogInfo($"Recorded {itemObj.name}");
                    registeredItems.Add(itemUUID);
                }
            }

            ClearOrphanedEntries(itemFile.file);
            itemFile.file.Save();
            itemFile.file.SaveOnConfigSet = true;

            MiniLogger.LogInfo("Item Configuration Initialized!");
        }
        
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

                    if (enemy.EnemyType.enemyPrefab != null)
                    {
                        ScanNodeProperties enemyScanNode = enemy.EnemyType.enemyPrefab.GetComponentInChildren<ScanNodeProperties>();
                        if (enemyScanNode != null)
                            scanName = enemyScanNode.headerText;
                    }

                    enemyEntry.AddField("Configure Content", "Enable to change any of the settings below.", false);
                    enemyEntry.AddField("Appropriate Aliases", "Changing this setting will do nothing, these are the names which LunarConfig will recognize as this object in other config options.\nThey are case-insensitve and do not regard whitespace.", $"{enemy.EnemyType.enemyName}, {scanName}");

                    if (enabledSettings.Contains("Display Name")) { enemyEntry.AddField("Display Name", "Specifies the name of the enemy.", enemy.EnemyDisplayName); }
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
                    if (enabledSettings.Contains("Normalized Time To Leave")) { enemyEntry.AddField("Normalized Time To Leave", "The time that an enemy leaves represented between 0 and 1 for the start and end of the day respectively.\nWARNING: Changing this for enemies that do not normally leave during the day may cause issues.", enemyObj.normalizedTimeInDayToLeave); }
                    if (enabledSettings.Contains("Enemy HP")) { enemyEntry.AddField("Enemy HP", "The amount of HP an enemy has.", enemyObj.enemyPrefab.GetComponent<EnemyAI>().enemyHP); }
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

                    if (enemy.enemy.enemyPrefab != null)
                    {
                        ScanNodeProperties enemyScanNode = enemy.enemy.enemyPrefab.GetComponentInChildren<ScanNodeProperties>();
                        if (enemyScanNode != null)
                            scanName = enemyScanNode.headerText;
                    }

                    enemyEntry.AddField("Configure Content", "Enable to change any of the settings below.", false);
                    enemyEntry.AddField("Appropriate Aliases", "Changing this setting will do nothing, these are the names which LunarConfig will recognize as this object in other config options.\nThey are case-insensitve and do not regard whitespace.", $"{enemy.enemy.enemyName}, {scanName}");

                    if (enabledSettings.Contains("Display Name")) { enemyEntry.AddField("Display Name", "Specifies the name of the enemy.", enemyObj.enemyName); }
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
                    if (enabledSettings.Contains("Normalized Time To Leave")) { enemyEntry.AddField("Normalized Time To Leave", "The time that an enemy leaves represented between 0 and 1 for the start and end of the day respectively.\nWARNING: Changing this for enemies that do not normally leave during the day may cause issues.", enemyObj.normalizedTimeInDayToLeave); }
                    if (enabledSettings.Contains("Enemy HP")) { enemyEntry.AddField("Enemy HP", "The amount of HP an enemy has.", enemyObj.enemyPrefab.GetComponent<EnemyAI>().enemyHP); }
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
                    dungeonEntry.AddField("Appropriate Aliases", "Changing this setting will do nothing, these are the names which LunarConfig will recognize as this object in other config options.\nThey are case-insensitve and do not regard whitespace.", $"{dungeon.DungeonName}, {dungeon.DungeonFlow.name}");
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

        public LunarConfigFile AddFile(string path, string name)
        {
            LunarConfigFile file = new LunarConfigFile(path);
            files[name] = file;
            return file;
        }
    }
}
