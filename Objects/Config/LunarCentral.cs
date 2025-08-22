using BepInEx;
using BepInEx.Configuration;
using CodeRebirthLib;
using CodeRebirthLib.ContentManagement.Enemies;
using CodeRebirthLib.ContentManagement.Items;
using DunGen;
using DunGen.Graph;
using EasyTextEffects.Editor.MyBoxCopy.Extensions;
using HarmonyLib;
using JetBrains.Annotations;
using LethalLevelLoader;
using LethalLib.Modules;
using LunarConfig.Objects.Entries;
using LunarConfig.Objects.Info;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using WeatherRegistry;
using static UnityEngine.Rendering.DebugUI;
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
            InitCentral();

            InitCollections();

            FixDungeons();

            LunarConfigEntry centralConfig = files[LunarConfig.CENTRAL_FILE_NAME].entries["Configuration"];

            LunarConfigEntry tagConfig = files[LunarConfig.CENTRAL_FILE_NAME].entries["Tags"];

            foreach (var tag in tagConfig.GetValue<string>("Tags").Split(','))
            {
                MiniLogger.LogInfo(tagConfig.GetValue<string>("Tags"));
                foundTags.Add(UUIDify(tag).RemoveWhitespace());
            }

            useLLLTags = tagConfig.GetValue<bool>("Use Base LLL Tags");

            if (centralConfig.GetValue<bool>("Configure Items")) { InitItems(); }
            if (centralConfig.GetValue<bool>("Configure Enemies")) { InitEnemies(); }
            if (centralConfig.GetValue<bool>("Configure Moons")) { InitMoons(); }
            if (centralConfig.GetValue<bool>("Configure Dungeons")) { InitDungeons(); }
            if (centralConfig.GetValue<bool>("Configure Vehicles")) { InitVehicles(); }
            if (centralConfig.GetValue<bool>("Configure Map Objects")) { InitMapObjects(); }
            if (centralConfig.GetValue<bool>("Configure Outside Map Objects")) { InitOutsideMapObjects(); }
            if (centralConfig.GetValue<bool>("Configure Tags")) { InitTags(); }
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
            configEntry.AddField("Configure Items", "Check this to generate and use configuration files for items.", false);
            configEntry.AddField("Configure Enemies", "Check this to generate and use configuration files for enemies.", false);
            configEntry.AddField("Configure Moons", "Check this to generate and use configuration files for moons.", false);
            configEntry.AddField("Configure Dungeons", "Check this to generate and use configuration files for dungeons.", false);
            configEntry.AddField("Configure Vehicles", "Check this to generate and use configuration files for vehicles.", false);
            configEntry.AddField("Configure Map Objects", "Check this to generate and use configuration files for map objects.", false);
            configEntry.AddField("Configure Outside Map Objects", "Check this to generate and use configuration files for outside map objects.", false);
            configEntry.AddField("Configure Tags", "Check this to generate and use configuration files for tags.", false);
            configEntry.AddField("Configure Weathers", "Check this to generate and use configuration files for weathers.", false);

            LunarConfigEntry tagEntry = centralFile.AddEntry("Tags");
            tagEntry.AddField("Use Base LLL Tags", "Add LLL tags to tag configuration.", false);
            tagEntry.AddField("Use Interiors As Tags", "Add interiors to tag configuration.", false);
            tagEntry.AddField("Tags", "List of tags to add to tag configuration.", "");

            LunarConfigEntry overItemEntry = centralFile.AddEntry("Item Overrides");
            //overItemEntry.AddField("Override Display Name", "Enables overriding an item's display name on specific moons, tags, or interiors.", false);
            overItemEntry.AddField("Override Minimum Value", "Enables overriding an item's minimum value on specific moons, tags, weathers, or interiors.", false);
            overItemEntry.AddField("Override Maximum Value", "Enables overriding an item's maximum value on specific moons, tags, weathers, or interiors.", false);
            //overItemEntry.AddField("Override Weight", "Enables overriding an item's weight on specific moons, tags, or interiors.", false);
            //overItemEntry.AddField("Override Conductivity", "Enables overriding an item's conductivity on specific moons, tags, or interiors.", false);
            //overItemEntry.AddField("Override Two-Handed", "Enables overriding an item's two-handedness on specific moons, tags, or interiors.", false);
            //overItemEntry.AddField("Override Is Scrap?", "Enables overriding an item's scrap? on specific moons, tags, or interiors.", false);

            LunarConfigEntry overEnemyEntry = centralFile.AddEntry("Enemy Overrides");
            overEnemyEntry.AddField("Override Display Name", "Enables overriding an enemy's display name on specific moons, tags, or interiors.", false);
            overEnemyEntry.AddField("Override Can See Through Fog?", "Enables overriding whether an enemy can see through fog on specific moons, tags, weathers, or interiors.", false);
            overEnemyEntry.AddField("Override Door Speed Multiplier", "Enables overriding an enemy's door speed multiplier on specific moons, tags, weathers, or interiors.", false);
            overEnemyEntry.AddField("Override Is Daytime Enemy?", "Enables overriding an enemy is a daytime enemy on specific moons, tags, weathers, or interiors.", false);
            overEnemyEntry.AddField("Override Is Outdoor Enemy?", "Enables overriding an enemy is an outdoor enemy on specific moons, tags, weathers, or interiors.", false);
            overEnemyEntry.AddField("Override Loudness Multiplier", "Enables overriding an enemy's loudness multiplier on specific moons, tags, weathers, or interiors.", false);
            overEnemyEntry.AddField("Override Max Count", "Enables overriding an enemy's max count on specific moons, tags, weathers, or interiors.", false);
            overEnemyEntry.AddField("Override Power Level", "Enables overriding an enemy's power level on specific moons, tags, weathers, or interiors.", false);
            overEnemyEntry.AddField("Override Probability Curve", "Enables overriding an enemy's probability curve on specific moons, tags, weathers, or interiors.", false);
            overEnemyEntry.AddField("Override Use Falloff?", "Enables overriding whether an enemy uses the falloff curve on specific moons, tags, weathers, or interiors.", false);
            overEnemyEntry.AddField("Override Falloff Curve", "Enables overriding an enemy's falloff curve on specific moons, tags, weathers, or interiors.", false);
            overEnemyEntry.AddField("Override Enemy HP", "Enables overriding an enemy's HP on specific moons, tags, weathers, or interiors.", false);
            overEnemyEntry.AddField("Override Can Die?", "Enables overriding whether an enemy can die on specific moons, tags, weathers, or interiors.", false);
            overEnemyEntry.AddField("Override Destroy On Death?", "Enables overriding whether an enemy is destroyed on death on specific moons, tags, weathers, or interiors.", false);
            overEnemyEntry.AddField("Override Can Destroy?", "Enables overriding whether an enemy can be destroyed on specific moons, tags, weathers, or interiors.", false);
            overEnemyEntry.AddField("Override Can Stun?", "Enables overriding whether an enemy can be stunned on specific moons, tags, weathers, or interiors.", false);
            overEnemyEntry.AddField("Override Stun Difficulty", "Enables overriding an enemy's stun difficulty on specific moons, tags, weathers, or interiors.", false);
            overEnemyEntry.AddField("Override Stun Time", "Enables overriding an enemy's stun time on specific moons, tags, weathers, or interiors.", false);

            LunarConfigEntry overMoonEntry = centralFile.AddEntry("Moon Overrides");
            overMoonEntry.AddField("Override Route Price", "Enables overriding a moon's route price on specific moons, tags, weathers, or interiors.", false);
            overMoonEntry.AddField("Override Is Hidden?", "Enables overriding whether a moon is hidden on specific moons, tags, weathers, or interiors.", false);
            overMoonEntry.AddField("Override Is Locked?", "Enables overriding whether a moon is locked on specific moons, tags, weathers, or interiors.", false);
            overMoonEntry.AddField("Override Has Time?", "Enables overriding whether a moon has time on specific moons, tags, weathers, or interiors.", false);
            overMoonEntry.AddField("Override Time Multiplier", "Enables overriding a moon's time multiplier on specific moons, tags, weathers, or interiors.", false);
            overMoonEntry.AddField("Override Daytime Probability Range", "Enables overriding a moon's daytime probability range on specific moons, tags, weathers, or interiors.", false);
            overMoonEntry.AddField("Override Daytime Curve", "Enables overriding a moon's daytime curve on specific moons, tags, weathers, or interiors.", false);
            overMoonEntry.AddField("Override Max Daytime Power", "Enables overriding a moon's max daytime power on specific moons, tags, weathers, or interiors.", false);
            overMoonEntry.AddField("Override Spawnable Daytime Enemies", "Enables overriding a moon's spawnable daytime enemies on specific moons, tags, weathers, or interiors.", false);
            overMoonEntry.AddField("Override Interior Probability Range", "Enables overriding a moon's interior probability range on specific moons, tags, weathers, or interiors.", false);
            overMoonEntry.AddField("Override Interior Curve", "Enables overriding a moon's interior curve on specific moons, tags, weathers, or interiors.", false);
            overMoonEntry.AddField("Override Max Interior Power", "Enables overriding a moon's max interior power on specific moons, tags, weathers, or interiors.", false);
            overMoonEntry.AddField("Override Spawnable Interior Enemies", "Enables overriding a moon's spawnable interior enemies on specific moons, tags, weathers, or interiors.", false);
            overMoonEntry.AddField("Override Outside Curve", "Enables overriding a moon's outside curve on specific moons, tags, weathers, or interiors.", false);
            overMoonEntry.AddField("Override Max Outside Power", "Enables overriding a moon's max outside power on specific moons, tags, weathers, or interiors.", false);
            overMoonEntry.AddField("Override Spawnable Outside Enemies", "Enables overriding a moon's spawnable outside enemies on specific moons, tags, weathers, or interiors.", false);
            overMoonEntry.AddField("Override Min Scrap", "Enables overriding a moon's min scrap on specific moons, tags, weathers, or interiors.", false);
            overMoonEntry.AddField("Override Max Scrap", "Enables overriding a moon's max scrap on specific moons, tags, weathers, or interiors.", false);
            overMoonEntry.AddField("Override Value Multiplier", "Enables overriding a moon's value multiplier on specific moons, tags, weathers, or interiors.", false);
            overMoonEntry.AddField("Override Amount Multiplier", "Enables overriding a moon's amount multiplier on specific moons, tags, weathers, or interiors.", false);
            overMoonEntry.AddField("Override Spawnable Scrap", "Enables overriding a moon's spawnable scrap on specific moons, tags, weathers, or interiors.", false);
            overMoonEntry.AddField("Override Interior Multiplier", "Enables overriding a moon's interior multiplier on specific moons, tags, weathers, or interiors.", false);
            overMoonEntry.AddField("Override Possible Interiors", "Enables overriding a moon's possible interiors on specific moons, tags, weathers, or interiors.", false);
            overMoonEntry.AddField("Override Tags", "Enables overriding a moon's tags on specific moons, tags, weathers, or interiors.", false);

            centralFile.file.Save();
            centralFile.file.SaveOnConfigSet = true;
        }

        public void InitItems()
        {
            LunarConfigFile itemFile = AddFile(LunarConfig.ITEM_FILE, LunarConfig.ITEM_FILE_NAME);
            itemFile.file.SaveOnConfigSet = false;

            LunarConfigEntry overItemEntry = files[LunarConfig.CENTRAL_FILE_NAME].entries["Item Overrides"];
            Dictionary<string, bool> overridedSettings = new Dictionary<string, bool>();
            
            foreach (var entry in overItemEntry.fields.Keys)
            {
                overridedSettings[entry] = overItemEntry.GetValue<bool>(entry);
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
                    itemEntry.AddField("Display Name", "Specifies the name that appears when scanning the item.", itemObj.itemName);
                    //if (overridedSettings["Override Display Name"])
                    //    itemEntry.AddField("Override Display Name", "Override the display name when on a certain moon, interior, or tag.\nDenoted MOON/TAG/INTERIOR:New Value, separated with commas.", "");
                    itemEntry.AddField("Minimum Value", "The minimum scrap value and item can have.\nTypically multiplied by 0.4, setting not applicable to non-scrap.\nDoes not work on items like Apparatus and items from enemies (Hives, Double-barrel).", itemObj.minValue);
                    if (overridedSettings["Override Minimum Value"])
                        itemEntry.AddField("Override Minimum Value", "Override the minimum value when on a certain moon, interior, or tag.\nDenoted MOON/TAG/INTERIOR:New Value, separated with commas.", "");
                    itemEntry.AddField("Maximum Value", "The maximum scrap value and item can have.\nTypically multiplied by 0.4, setting not applicable to non-scrap.\nDoes not work on items like Apparatus and items from enemies (Hives, Double-barrel).", itemObj.maxValue);
                    if (overridedSettings["Override Maximum Value"])
                        itemEntry.AddField("Override Maximum Value", "Override the maximum value when on a certain moon, interior, or tag.\nDenoted MOON/TAG/INTERIOR:New Value, separated with commas.", "");
                    itemEntry.AddField("Weight", "Specifies the weight of an item.\nCalculated with: (x - 1) * 105 = weight in pounds.", itemObj.weight);
                    //if (overridedSettings["Override Weight"])
                    //    itemEntry.AddField("Override Weight", "Override the weight when on a certain moon, interior, or tag.\nDenoted MOON/TAG/INTERIOR:New Value, separated with commas.", "");
                    itemEntry.AddField("Conductivity", "Specifies whether an item is conductive.", itemObj.isConductiveMetal);
                    //if (overridedSettings["Override Conductivity"])
                    //    itemEntry.AddField("Override Conductivity", "Override the conductivity when on a certain moon, interior, or tag.\nDenoted MOON/TAG/INTERIOR:New Value, separated with commas.", "");
                    itemEntry.AddField("Two-Handed", "Specifies whether an item is two-handed.", itemObj.twoHanded);
                    //if (overridedSettings["Override Two-Handed"])
                    //    itemEntry.AddField("Override Two-Handed", "Override the two-handedness when on a certain moon, interior, or tag.\nDenoted MOON/TAG/INTERIOR:New Value, separated with commas.", "");
                    itemEntry.AddField("Is Scrap?", "Specifies if an item is scrap or gear.\nThis decides whether an item can be sold to the company for credits.", itemObj.isScrap);
                    //if (overridedSettings["Override Is Scrap?"])
                    //    itemEntry.AddField("Override Is Scrap?", "Override the scrap? when on a certain moon, interior, or tag.\nDenoted MOON/TAG/INTERIOR:New Value, separated with commas.", "");
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
                    itemEntry.AddField("Display Name", "Specifies the name that appears when scanning the item.", itemObj.itemName);
                    //if (overridedSettings["Override Display Name"])
                    //    itemEntry.AddField("Override Display Name", "Override the display name when on a certain moon, interior, or tag.\nDenoted MOON/TAG/INTERIOR:New Value, separated with commas.", "");
                    itemEntry.AddField("Minimum Value", "The minimum scrap value and item can have.\nTypically multiplied by 0.4, setting not applicable to non-scrap.\nDoes not work on items like Apparatus and items from enemies (Hives, Double-barrel).", itemObj.minValue);
                    if (overridedSettings["Override Minimum Value"])
                        itemEntry.AddField("Override Minimum Value", "Override the minimum value when on a certain moon, interior, or tag.\nDenoted MOON/TAG/INTERIOR:New Value, separated with commas.", "");
                    itemEntry.AddField("Maximum Value", "The maximum scrap value and item can have.\nTypically multiplied by 0.4, setting not applicable to non-scrap.\nDoes not work on items like Apparatus and items from enemies (Hives, Double-barrel).", itemObj.maxValue);
                    if (overridedSettings["Override Maximum Value"])
                        itemEntry.AddField("Override Maximum Value", "Override the maximum value when on a certain moon, interior, or tag.\nDenoted MOON/TAG/INTERIOR:New Value, separated with commas.", "");
                    itemEntry.AddField("Weight", "Specifies the weight of an item.\nCalculated with: (x - 1) * 105 = weight in pounds.", itemObj.weight);
                    //if (overridedSettings["Override Weight"])
                    //    itemEntry.AddField("Override Weight", "Override the weight when on a certain moon, interior, or tag.\nDenoted MOON/TAG/INTERIOR:New Value, separated with commas.", "");
                    itemEntry.AddField("Conductivity", "Specifies whether an item is conductive.", itemObj.isConductiveMetal);
                    //if (overridedSettings["Override Conductivity"])
                    //    itemEntry.AddField("Override Conductivity", "Override the conductivity when on a certain moon, interior, or tag.\nDenoted MOON/TAG/INTERIOR:New Value, separated with commas.", "");
                    itemEntry.AddField("Two-Handed", "Specifies whether an item is two-handed.", itemObj.twoHanded);
                    //if (overridedSettings["Override Two-Handed"])
                    //    itemEntry.AddField("Override Two-Handed", "Override the two-handedness when on a certain moon, interior, or tag.\nDenoted MOON/TAG/INTERIOR:New Value, separated with commas.", "");
                    itemEntry.AddField("Is Scrap?", "Specifies if an item is scrap or gear.\nThis decides whether an item can be sold to the company for credits.", itemObj.isScrap);
                    //if (overridedSettings["Override Is Scrap?"])
                    //    itemEntry.AddField("Override Is Scrap?", "Override the scrap? when on a certain moon, interior, or tag.\nDenoted MOON/TAG/INTERIOR:New Value, separated with commas.", "");
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
                    itemEntry.AddField("Display Name", "Specifies the name that appears when scanning the item.", itemObj.itemName);
                    //if (overridedSettings["Override Display Name"])
                    //    itemEntry.AddField("Override Display Name", "Override the display name when on a certain moon, interior, or tag.\nDenoted MOON/TAG/INTERIOR:New Value, separated with commas.", "");
                    itemEntry.AddField("Minimum Value", "The minimum scrap value and item can have.\nTypically multiplied by 0.4, setting not applicable to non-scrap.\nDoes not work on items like Apparatus and items from enemies (Hives, Double-barrel).", itemObj.minValue);
                    if (overridedSettings["Override Minimum Value"])
                        itemEntry.AddField("Override Minimum Value", "Override the minimum value when on a certain moon, interior, or tag.\nDenoted MOON/TAG/INTERIOR:New Value, separated with commas.", "");
                    itemEntry.AddField("Maximum Value", "The maximum scrap value and item can have.\nTypically multiplied by 0.4, setting not applicable to non-scrap.\nDoes not work on items like Apparatus and items from enemies (Hives, Double-barrel).", itemObj.maxValue);
                    if (overridedSettings["Override Maximum Value"])
                        itemEntry.AddField("Override Maximum Value", "Override the maximum value when on a certain moon, interior, or tag.\nDenoted MOON/TAG/INTERIOR:New Value, separated with commas.", "");
                    itemEntry.AddField("Weight", "Specifies the weight of an item.\nCalculated with: (x - 1) * 105 = weight in pounds.", itemObj.weight);
                    //if (overridedSettings["Override Weight"])
                    //    itemEntry.AddField("Override Weight", "Override the weight when on a certain moon, interior, or tag.\nDenoted MOON/TAG/INTERIOR:New Value, separated with commas.", "");
                    itemEntry.AddField("Conductivity", "Specifies whether an item is conductive.", itemObj.isConductiveMetal);
                    //if (overridedSettings["Override Conductivity"])
                    //    itemEntry.AddField("Override Conductivity", "Override the conductivity when on a certain moon, interior, or tag.\nDenoted MOON/TAG/INTERIOR:New Value, separated with commas.", "");
                    itemEntry.AddField("Two-Handed", "Specifies whether an item is two-handed.", itemObj.twoHanded);
                    //if (overridedSettings["Override Two-Handed"])
                    //    itemEntry.AddField("Override Two-Handed", "Override the two-handedness when on a certain moon, interior, or tag.\nDenoted MOON/TAG/INTERIOR:New Value, separated with commas.", "");
                    itemEntry.AddField("Is Scrap?", "Specifies if an item is scrap or gear.\nThis decides whether an item can be sold to the company for credits.", itemObj.isScrap);
                    //if (overridedSettings["Override Is Scrap?"])
                    //    itemEntry.AddField("Override Is Scrap?", "Override the scrap? when on a certain moon, interior, or tag.\nDenoted MOON/TAG/INTERIOR:New Value, separated with commas.", "");
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
                    itemEntry.AddField("Display Name", "Specifies the name that appears when scanning the item.", itemObj.itemName);
                    //if (overridedSettings["Override Display Name"])
                    //    itemEntry.AddField("Override Display Name", "Override the display name when on a certain moon, interior, or tag.\nDenoted MOON/TAG/INTERIOR:New Value, separated with commas.", "");
                    itemEntry.AddField("Minimum Value", "The minimum scrap value and item can have.\nTypically multiplied by 0.4, setting not applicable to non-scrap.\nDoes not work on items like Apparatus and items from enemies (Hives, Double-barrel).", itemObj.minValue);
                    if (overridedSettings["Override Minimum Value"])
                        itemEntry.AddField("Override Minimum Value", "Override the minimum value when on a certain moon, interior, or tag.\nDenoted MOON/TAG/INTERIOR:New Value, separated with commas.", "");
                    itemEntry.AddField("Maximum Value", "The maximum scrap value and item can have.\nTypically multiplied by 0.4, setting not applicable to non-scrap.\nDoes not work on items like Apparatus and items from enemies (Hives, Double-barrel).", itemObj.maxValue);
                    if (overridedSettings["Override Maximum Value"])
                        itemEntry.AddField("Override Maximum Value", "Override the maximum value when on a certain moon, interior, or tag.\nDenoted MOON/TAG/INTERIOR:New Value, separated with commas.", "");
                    itemEntry.AddField("Weight", "Specifies the weight of an item.\nCalculated with: (x - 1) * 105 = weight in pounds.", itemObj.weight);
                    //if (overridedSettings["Override Weight"])
                    //    itemEntry.AddField("Override Weight", "Override the weight when on a certain moon, interior, or tag.\nDenoted MOON/TAG/INTERIOR:New Value, separated with commas.", "");
                    itemEntry.AddField("Conductivity", "Specifies whether an item is conductive.", itemObj.isConductiveMetal);
                    //if (overridedSettings["Override Conductivity"])
                    //    itemEntry.AddField("Override Conductivity", "Override the conductivity when on a certain moon, interior, or tag.\nDenoted MOON/TAG/INTERIOR:New Value, separated with commas.", "");
                    itemEntry.AddField("Two-Handed", "Specifies whether an item is two-handed.", itemObj.twoHanded);
                    //if (overridedSettings["Override Two-Handed"])
                    //    itemEntry.AddField("Override Two-Handed", "Override the two-handedness when on a certain moon, interior, or tag.\nDenoted MOON/TAG/INTERIOR:New Value, separated with commas.", "");
                    itemEntry.AddField("Is Scrap?", "Specifies if an item is scrap or gear.\nThis decides whether an item can be sold to the company for credits.", itemObj.isScrap);
                    //if (overridedSettings["Override Is Scrap?"])
                    //    itemEntry.AddField("Override Is Scrap?", "Override the scrap? when on a certain moon, interior, or tag.\nDenoted MOON/TAG/INTERIOR:New Value, separated with commas.", "");
                    MiniLogger.LogInfo($"Recorded {itemObj.name}");
                    registeredItems.Add(itemUUID);
                }
            }

            itemFile.file.Save();
            itemFile.file.SaveOnConfigSet = true;
        }
        
        public void InitEnemies()
        {
            LunarConfigFile enemyFile = AddFile(LunarConfig.ENEMY_FILE, LunarConfig.ENEMY_FILE_NAME);
            enemyFile.file.SaveOnConfigSet = false;

            LunarConfigEntry overEnemyEntry = files[LunarConfig.CENTRAL_FILE_NAME].entries["Enemy Overrides"];
            Dictionary<string, bool> overridedSettings = new Dictionary<string, bool>();

            foreach (var entry in overEnemyEntry.fields.Keys)
            {
                overridedSettings[entry] = overEnemyEntry.GetValue<bool>(entry);
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

                    enemyEntry.AddField("Display Name", "Specifies the name of the enemy.", enemy.EnemyDisplayName);
                    if (overridedSettings["Override Display Name"])
                        enemyEntry.AddField("Override Display Name", "Override the display name when on a certain moon, interior, or tag.\nDenoted MOON/TAG/INTERIOR:New Value, separated with commas.", "");
                    enemyEntry.AddField("Can See Through Fog?", "Specifies if an enemy can see through fog in foggy weather.", enemyObj.canSeeThroughFog);
                    if (overridedSettings["Override Can See Through Fog?"])
                        enemyEntry.AddField("Override Can See Through Foh?", "Override whether an enemy can see through fog when on a certain moon, interior, or tag.\nDenoted MOON/TAG/INTERIOR:New Value, separated with commas.", "");
                    enemyEntry.AddField("Door Speed Multiplier", "Decides the speed at which enemies can open doors.\nCalculated with: 1 / x = time to open door in seconds.", enemyObj.doorSpeedMultiplier);
                    if (overridedSettings["Override Door Speed Multiplier"])
                        enemyEntry.AddField("Override Door Speed Multiplier", "Override the door speed multiplier when on a certain moon, interior, or tag.\nDenoted MOON/TAG/INTERIOR:New Value, separated with commas.", "");
                    enemyEntry.AddField("Is Daytime Enemy?", "Whether an enemy is a daytime enemy.", enemyObj.isDaytimeEnemy);
                    if (overridedSettings["Override Is Daytime Enemy?"])
                        enemyEntry.AddField("Override Is Daytime Enemy?", "Override whether an enemy is a daytime enemy when on a certain moon, interior, or tag.\nDenoted MOON/TAG/INTERIOR:New Value, separated with commas.", "");
                    enemyEntry.AddField("Is Outdoor Enemy?", "Whether an enemy is a outdoor enemy.", enemyObj.isOutsideEnemy);
                    if (overridedSettings["Override Is Outdoor Enemy?"])
                        enemyEntry.AddField("Override Is Outdoor Enemy?", "Override whether an enemy is a outdoor enemy when on a certain moon, interior, or tag.\nDenoted MOON/TAG/INTERIOR:New Value, separated with commas.", "");
                    enemyEntry.AddField("Loudness Multiplier", "Multiplies the volume of an enemy's sounds.", enemyObj.loudnessMultiplier);
                    if (overridedSettings["Override Loudness Multiplier"])
                        enemyEntry.AddField("Override Loudness Multiplier", "Override the loudness multiplier when on a certain moon, interior, or tag.\nDenoted MOON/TAG/INTERIOR:New Value, separated with commas.", "");
                    enemyEntry.AddField("Max Count", "The maximum amount of an enemy that can be alive.", enemyObj.MaxCount);
                    if (overridedSettings["Override Max Count"])
                        enemyEntry.AddField("Override Max Count", "Override the max count when on a certain moon, interior, or tag.\nDenoted MOON/TAG/INTERIOR:New Value, separated with commas.", "");
                    enemyEntry.AddField("Power Level", "The power level an enemy occupies.", enemyObj.PowerLevel);
                    if (overridedSettings["Override Power Level"])
                        enemyEntry.AddField("Override Power Level", "Override the power level when on a certain moon, interior, or tag.\nDenoted MOON/TAG/INTERIOR:New Value, separated with commas.", "");
                    enemyEntry.AddField("Probability Curve", "Multiplies enemy spawn weight depending on time of day.\nKeyframes represented by x,y and separated by semicolons.", CurveToString(enemyObj.probabilityCurve));
                    if (overridedSettings["Override Probability Curve"])
                        enemyEntry.AddField("Override Probability Curve", "Override the probability curve when on a certain moon, interior, or tag.\nDenoted MOON/TAG/INTERIOR:New Value, separated with commas.", "");
                    enemyEntry.AddField("Use Falloff?", "Whether or not to use the falloff curve.", enemyObj.useNumberSpawnedFalloff);
                    if (overridedSettings["Override Use Falloff?"])
                        enemyEntry.AddField("Override Use Falloff?", "Override whether an enemy uses the falloff curve when on a certain moon, interior, or tag.\nDenoted MOON/TAG/INTERIOR:New Value, separated with commas.", "");
                    enemyEntry.AddField("Falloff Curve", "Multiplier to enemy spawn weight depending on how many are already spawned.\nKeyframes represented by x,y and separated by semicolons.", CurveToString(enemyObj.numberSpawnedFalloff));
                    if (overridedSettings["Override Falloff Curve"])
                        enemyEntry.AddField("Override Falloff Curve", "Override the falloff curve when on a certain moon, interior, or tag.\nDenoted MOON/TAG/INTERIOR:New Value, separated with commas.", "");
                    enemyEntry.AddField("Enemy HP", "The amount of HP an enemy has.", enemyObj.enemyPrefab.GetComponent<EnemyAI>().enemyHP);
                    if (overridedSettings["Override Enemy HP"])
                        enemyEntry.AddField("Override Enemy HP", "Override the enemy HP when on a certain moon, interior, or tag.\nDenoted MOON/TAG/INTERIOR:New Value, separated with commas.", "");
                    enemyEntry.AddField("Can Die?", "Whether or not an enemy can die.", enemyObj.canDie);
                    if (overridedSettings["Override Can Die?"])
                        enemyEntry.AddField("Override Can Die?", "Override the can die setting when on a certain moon, interior, or tag.\nDenoted MOON/TAG/INTERIOR:New Value, separated with commas.", "");
                    enemyEntry.AddField("Destroy On Death?", "Whether or not an enemy is destroyed on death.", enemyObj.destroyOnDeath);
                    if (overridedSettings["Override Destroy On Death?"])
                        enemyEntry.AddField("Override Destroy On Death?", "Override the destroy on death setting when on a certain moon, interior, or tag.\nDenoted MOON/TAG/INTERIOR:New Value, separated with commas.", "");
                    enemyEntry.AddField("Can Destroy?", "Whether or not an enemy can be destroyed.", enemyObj.canBeDestroyed);
                    if (overridedSettings["Override Can Destroy?"])
                        enemyEntry.AddField("Override Can Destroy?", "Override the can destroy setting when on a certain moon, interior, or tag.\nDenoted MOON/TAG/INTERIOR:New Value, separated with commas.", "");
                    enemyEntry.AddField("Can Stun?", "Whether or not an enemy can be stunned.", enemyObj.canBeStunned);
                    if (overridedSettings["Override Can Stun?"])
                        enemyEntry.AddField("Override Can Stun?", "Override the can stun setting when on a certain moon, interior, or tag.\nDenoted MOON/TAG/INTERIOR:New Value, separated with commas.", "");
                    enemyEntry.AddField("Stun Difficulty", "I don't really know.", enemyObj.stunGameDifficultyMultiplier);
                    if (overridedSettings["Override Stun Difficulty"])
                        enemyEntry.AddField("Override Stun Difficulty", "Override the stun difficulty when on a certain moon, interior, or tag.\nDenoted MOON/TAG/INTERIOR:New Value, separated with commas.", "");
                    enemyEntry.AddField("Stun Time", "I don't really know.", enemyObj.stunTimeMultiplier);
                    if (overridedSettings["Override Stun Time"])
                        enemyEntry.AddField("Override Stun Time", "Override the stun time when on a certain moon, interior, or tag.\nDenoted MOON/TAG/INTERIOR:New Value, separated with commas.", "");
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

                    enemyEntry.AddField("Display Name", "Specifies the name of the enemy.", enemyObj.enemyName);
                    if (overridedSettings["Override Display Name"])
                        enemyEntry.AddField("Override Display Name", "Override the display name when on a certain moon, interior, or tag.\nDenoted MOON/TAG/INTERIOR:New Value, separated with commas.", "");
                    enemyEntry.AddField("Can See Through Fog?", "Specifies if an enemy can see through fog in foggy weather.", enemyObj.canSeeThroughFog);
                    if (overridedSettings["Override Can See Through Fog?"])
                        enemyEntry.AddField("Override Can See Through Foh?", "Override whether an enemy can see through fog when on a certain moon, interior, or tag.\nDenoted MOON/TAG/INTERIOR:New Value, separated with commas.", "");
                    enemyEntry.AddField("Door Speed Multiplier", "Decides the speed at which enemies can open doors.\nCalculated with: 1 / x = time to open door in seconds.", enemyObj.doorSpeedMultiplier);
                    if (overridedSettings["Override Door Speed Multiplier"])
                        enemyEntry.AddField("Override Door Speed Multiplier", "Override the door speed multiplier when on a certain moon, interior, or tag.\nDenoted MOON/TAG/INTERIOR:New Value, separated with commas.", "");
                    enemyEntry.AddField("Is Daytime Enemy?", "Whether an enemy is a daytime enemy.", enemyObj.isDaytimeEnemy);
                    if (overridedSettings["Override Is Daytime Enemy?"])
                        enemyEntry.AddField("Override Is Daytime Enemy?", "Override whether an enemy is a daytime enemy when on a certain moon, interior, or tag.\nDenoted MOON/TAG/INTERIOR:New Value, separated with commas.", "");
                    enemyEntry.AddField("Is Outdoor Enemy?", "Whether an enemy is a outdoor enemy.", enemyObj.isOutsideEnemy);
                    if (overridedSettings["Override Is Outdoor Enemy?"])
                        enemyEntry.AddField("Override Is Outdoor Enemy?", "Override whether an enemy is a outdoor enemy when on a certain moon, interior, or tag.\nDenoted MOON/TAG/INTERIOR:New Value, separated with commas.", "");
                    enemyEntry.AddField("Loudness Multiplier", "Multiplies the volume of an enemy's sounds.", enemyObj.loudnessMultiplier);
                    if (overridedSettings["Override Loudness Multiplier"])
                        enemyEntry.AddField("Override Loudness Multiplier", "Override the loudness multiplier when on a certain moon, interior, or tag.\nDenoted MOON/TAG/INTERIOR:New Value, separated with commas.", "");
                    enemyEntry.AddField("Max Count", "The maximum amount of an enemy that can be alive.", enemyObj.MaxCount);
                    if (overridedSettings["Override Max Count"])
                        enemyEntry.AddField("Override Max Count", "Override the max count when on a certain moon, interior, or tag.\nDenoted MOON/TAG/INTERIOR:New Value, separated with commas.", "");
                    enemyEntry.AddField("Power Level", "The power level an enemy occupies.", enemyObj.PowerLevel);
                    if (overridedSettings["Override Power Level"])
                        enemyEntry.AddField("Override Power Level", "Override the power level when on a certain moon, interior, or tag.\nDenoted MOON/TAG/INTERIOR:New Value, separated with commas.", "");
                    enemyEntry.AddField("Probability Curve", "Multiplies enemy spawn weight depending on time of day.\nKeyframes represented by x,y and separated by semicolons.", CurveToString(enemyObj.probabilityCurve));
                    if (overridedSettings["Override Probability Curve"])
                        enemyEntry.AddField("Override Probability Curve", "Override the probability curve when on a certain moon, interior, or tag.\nDenoted MOON/TAG/INTERIOR:New Value, separated with commas.", "");
                    enemyEntry.AddField("Use Falloff?", "Whether or not to use the falloff curve.", enemyObj.useNumberSpawnedFalloff);
                    if (overridedSettings["Override Use Falloff?"])
                        enemyEntry.AddField("Override Use Falloff?", "Override whether an enemy uses the falloff curve when on a certain moon, interior, or tag.\nDenoted MOON/TAG/INTERIOR:New Value, separated with commas.", "");
                    enemyEntry.AddField("Falloff Curve", "Multiplier to enemy spawn weight depending on how many are already spawned.\nKeyframes represented by x,y and separated by semicolons.", CurveToString(enemyObj.numberSpawnedFalloff));
                    if (overridedSettings["Override Falloff Curve"])
                        enemyEntry.AddField("Override Falloff Curve", "Override the falloff curve when on a certain moon, interior, or tag.\nDenoted MOON/TAG/INTERIOR:New Value, separated with commas.", "");
                    enemyEntry.AddField("Enemy HP", "The amount of HP an enemy has.", enemyObj.enemyPrefab.GetComponent<EnemyAI>().enemyHP);
                    if (overridedSettings["Override Enemy HP"])
                        enemyEntry.AddField("Override Enemy HP", "Override the enemy HP when on a certain moon, interior, or tag.\nDenoted MOON/TAG/INTERIOR:New Value, separated with commas.", "");
                    enemyEntry.AddField("Can Die?", "Whether or not an enemy can die.", enemyObj.canDie);
                    if (overridedSettings["Override Can Die?"])
                        enemyEntry.AddField("Override Can Die?", "Override the can die setting when on a certain moon, interior, or tag.\nDenoted MOON/TAG/INTERIOR:New Value, separated with commas.", "");
                    enemyEntry.AddField("Destroy On Death?", "Whether or not an enemy is destroyed on death.", enemyObj.destroyOnDeath);
                    if (overridedSettings["Override Destroy On Death?"])
                        enemyEntry.AddField("Override Destroy On Death?", "Override the destroy on death setting when on a certain moon, interior, or tag.\nDenoted MOON/TAG/INTERIOR:New Value, separated with commas.", "");
                    enemyEntry.AddField("Can Destroy?", "Whether or not an enemy can be destroyed.", enemyObj.canBeDestroyed);
                    if (overridedSettings["Override Can Destroy?"])
                        enemyEntry.AddField("Override Can Destroy?", "Override the can destroy setting when on a certain moon, interior, or tag.\nDenoted MOON/TAG/INTERIOR:New Value, separated with commas.", "");
                    enemyEntry.AddField("Can Stun?", "Whether or not an enemy can be stunned.", enemyObj.canBeStunned);
                    if (overridedSettings["Override Can Stun?"])
                        enemyEntry.AddField("Override Can Stun?", "Override the can stun setting when on a certain moon, interior, or tag.\nDenoted MOON/TAG/INTERIOR:New Value, separated with commas.", "");
                    enemyEntry.AddField("Stun Difficulty", "I don't really know.", enemyObj.stunGameDifficultyMultiplier);
                    if (overridedSettings["Override Stun Difficulty"])
                        enemyEntry.AddField("Override Stun Difficulty", "Override the stun difficulty when on a certain moon, interior, or tag.\nDenoted MOON/TAG/INTERIOR:New Value, separated with commas.", "");
                    enemyEntry.AddField("Stun Time", "I don't really know.", enemyObj.stunTimeMultiplier);
                    if (overridedSettings["Override Stun Time"])
                        enemyEntry.AddField("Override Stun Time", "Override the stun time when on a certain moon, interior, or tag.\nDenoted MOON/TAG/INTERIOR:New Value, separated with commas.", "");
                    MiniLogger.LogInfo($"Recorded {enemyObj.name}");
                }
            }

            enemyFile.file.Save();
            enemyFile.file.SaveOnConfigSet = true;
        }

        public void InitMoons()
        {
            LunarConfigFile moonFile = AddFile(LunarConfig.MOON_FILE, LunarConfig.MOON_FILE_NAME);
            moonFile.file.SaveOnConfigSet = false;

            LunarConfigEntry overMoonEntry = files[LunarConfig.CENTRAL_FILE_NAME].entries["Moon Overrides"];
            Dictionary<string, bool> overridedSettings = new Dictionary<string, bool>();

            foreach (var entry in overMoonEntry.fields.Keys)
            {
                overridedSettings[entry] = overMoonEntry.GetValue<bool>(entry);
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

                    moonEntry.AddField("Display Name", "Changes the name of the moon.\nDoes not modify terminal commands/nodes.", moonObj.PlanetName);
                    moonEntry.AddField("Risk Level", "Changes the risk level of the moon.\nThis setting is only cosmetic.", moonObj.riskLevel);
                    moonEntry.AddField("Description", "The description given to the moon.\nNew lines are represented by semi-colons.\nDoes not modify terminal commands/nodes.", moonObj.LevelDescription.Replace("\n", ";"));
                    
                    moonEntry.AddField("Route Price", "Changes the price to route to the moon.", moon.RoutePrice);
                    if (overridedSettings["Override Route Price"])
                        moonEntry.AddField("Override Route Price", "Override the route price when on a certain moon, weather, interior, or tag.\nDenoted Key:New Value, separated with commas.", "");
                    moonEntry.AddField("Is Hidden?", "Changes if the moon is hidden in the terminal.", moon.IsRouteHidden);
                    if (overridedSettings["Override Is Hidden?"])
                        moonEntry.AddField("Override Is Hidden?", "Override whether the moon is hidden on a certain moon, weather, interior, or tag.\nDenoted Key:New Value, separated with commas.", "");
                    moonEntry.AddField("Is Locked?", "Changes if the moon is locked in the terminal.", moon.IsRouteLocked);
                    if (overridedSettings["Override Is Locked?"])
                        moonEntry.AddField("Override Is Locked?", "Override whether the moon is locked on a certain moon, weather, interior, or tag.\nDenoted Key:New Value, separated with commas.", "");

                    moonEntry.AddField("Can Be Challenge Moon?", "Defines whether or not a moon can be selected for the weekly challenge moon.", moonObj.planetHasTime);
                    moonEntry.AddField("Has Time?", "Defines whether a moon has time.", moonObj.planetHasTime);
                    if (overridedSettings["Override Has Time?"])
                        moonEntry.AddField("Override Has Time?", "Override whether the moon has time on a certain moon, weather, interior, or tag.\nDenoted Key:New Value, separated with commas.", "");
                    moonEntry.AddField("Time Multiplier", "Multiplies the speed at which time progresses on a moon.", moonObj.DaySpeedMultiplier);
                    if (overridedSettings["Override Time Multiplier"])
                        moonEntry.AddField("Override Time Multiplier", "Override the time multiplier when on a certain moon, weather, interior, or tag.\nDenoted Key:New Value, separated with commas.", "");

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

                    moonEntry.AddField("Daytime Probability Range", "The amount of daytime enemies spawned that can differ from the curve.\nFor instance, if this value is 3, and at the current time and spawn cycle 2 daytime enemies should spawn, anywhere between 0 and 5 can spawn.", moonObj.daytimeEnemiesProbabilityRange);
                    if (overridedSettings["Override Daytime Probability Range"])
                        moonEntry.AddField("Override Daytime Probability Range", "Override the daytime probability range when on a certain moon, weather, interior, or tag.\nDenoted Key:New Value, separated with commas.", "");
                    moonEntry.AddField("Daytime Curve", "Decides the amount of daytime enemies that spawn as the day progresses.\nKeyframes represented by x,y and separated by semicolons.", CurveToString(moonObj.daytimeEnemySpawnChanceThroughDay));
                    if (overridedSettings["Override Daytime Curve"])
                        moonEntry.AddField("Override Daytime Curve", "Override the daytime curve when on a certain moon, weather, interior, or tag.\nDenoted Key:New Value, separated with commas.", "");
                    moonEntry.AddField("Max Daytime Power", "The amount of daytime power capacity that a moon has.", moonObj.maxDaytimeEnemyPowerCount);
                    if (overridedSettings["Override Max Daytime Power"])
                        moonEntry.AddField("Override Max Daytime Power", "Override the max daytime power when on a certain moon, weather, interior, or tag.\nDenoted Key:New Value, separated with commas.", "");
                    moonEntry.AddField("Spawnable Daytime Enemies", "The base daytime enemies that can spawn on the moon.\nDenoted with NAME:RARITY, separated with commas.", defaultDaytimeEnemies);
                    if (overridedSettings["Override Spawnable Daytime Enemies"])
                        moonEntry.AddField("Override Spawnable Daytime Enemies", "Override the spawnable daytime enemies when on a certain moon, weather, interior, or tag.\nDenoted Key:New Value, separated with commas.", "");

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

                    moonEntry.AddField("Interior Probability Range", "The amount of interior enemies spawned that can differ from the curve.\nFor instance, if this value is 3, and at the current time and spawn cycle 2 interior enemies should spawn, anywhere between 0 and 5 can spawn.", moonObj.spawnProbabilityRange);
                    if (overridedSettings["Override Interior Probability Range"])
                        moonEntry.AddField("Override Interior Probability Range", "Override the interior probability range when on a certain moon, weather, interior, or tag.\nDenoted Key:New Value, separated with commas.", "");
                    moonEntry.AddField("Interior Curve", "Decides the amount of interior enemies that spawn as the day progresses.\nKeyframes represented by x,y and separated by semicolons.", CurveToString(moonObj.enemySpawnChanceThroughoutDay));
                    if (overridedSettings["Override Interior Curve"])
                        moonEntry.AddField("Override Interior Curve", "Override the interior curve when on a certain moon, weather, interior, or tag.\nDenoted Key:New Value, separated with commas.", "");
                    moonEntry.AddField("Max Interior Power", "The amount of interior power capacity that a moon has.", moonObj.maxEnemyPowerCount);
                    if (overridedSettings["Override Max Interior Power"])
                        moonEntry.AddField("Override Max Interior Power", "Override the max interior power when on a certain moon, weather, interior, or tag.\nDenoted Key:New Value, separated with commas.", "");
                    moonEntry.AddField("Spawnable Interior Enemies", "The base interior enemies that can spawn on the moon.\nDenoted with NAME:RARITY, separated with commas.", defaultInsideEnemies);
                    if (overridedSettings["Override Spawnable Interior Enemies"])
                        moonEntry.AddField("Override Spawnable Interior Enemies", "Override the spawnable interior enemies when on a certain moon, weather, interior, or tag.\nDenoted Key:New Value, separated with commas.", "");

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

                    moonEntry.AddField("Outside Curve", "Decides the amount of outside enemies that spawn as the day progresses.\nKeyframes represented by x,y and separated by semicolons.", CurveToString(moonObj.outsideEnemySpawnChanceThroughDay));
                    if (overridedSettings["Override Outside Curve"])
                        moonEntry.AddField("Override Outside Curve", "Override the outside curve when on a certain moon, weather, interior, or tag.\nDenoted Key:New Value, separated with commas.", "");
                    moonEntry.AddField("Max Outside Power", "The amount of outside power capacity that a moon has.", moonObj.maxOutsideEnemyPowerCount);
                    if (overridedSettings["Override Max Outside Power"])
                        moonEntry.AddField("Override Max Outside Power", "Override the max outside power when on a certain moon, weather, interior, or tag.\nDenoted Key:New Value, separated with commas.", "");
                    moonEntry.AddField("Spawnable Outside Enemies", "The base outside enemies that can spawn on the moon.\nDenoted with NAME:RARITY, separated with commas.", defaultOutsideEnemies);
                    if (overridedSettings["Override Spawnable Outside Enemies"])
                        moonEntry.AddField("Override Spawnable Outside Enemies", "Override the spawnable outside enemies when on a certain moon, weather, interior, or tag.\nDenoted Key:New Value, separated with commas.", "");

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

                    moonEntry.AddField("Min Scrap", "The minimum amount of scrap items that can spawn on a moon.", moonObj.minScrap);
                    if (overridedSettings["Override Min Scrap"])
                        moonEntry.AddField("Override Min Scrap", "Override the min scrap when on a certain moon, weather, interior, or tag.\nDenoted Key:New Value, separated with commas.", "");
                    moonEntry.AddField("Max Scrap", "The maximum amount of scrap items that can spawn on a moon.", moonObj.maxScrap);
                    if (overridedSettings["Override Max Scrap"])
                        moonEntry.AddField("Override Max Scrap", "Override the max scrap when on a certain moon, weather, interior, or tag.\nDenoted Key:New Value, separated with commas.", "");
                    moonEntry.AddField("Value Multiplier", "The multiplier applied to the value of a moon's scrap.", 1f);
                    if (overridedSettings["Override Value Multiplier"])
                        moonEntry.AddField("Override Value Multiplier", "Override the value multiplier when on a certain moon, weather, interior, or tag.\nDenoted Key:New Value, separated with commas.", "");
                    moonEntry.AddField("Amount Multiplier", "The multiplier applied to the amount of scrap a moon has.", 1f);
                    if (overridedSettings["Override Amount Multiplier"])
                        moonEntry.AddField("Override Amount Multiplier", "Override the amount multiplier when on a certain moon, weather, interior, or tag.\nDenoted Key:New Value, separated with commas.", "");
                    moonEntry.AddField("Spawnable Scrap", "The base scrap that can spawn on the moon.\nDenoted with NAME:RARITY, separated with commas.", defaultScrap);
                    if (overridedSettings["Override Spawnable Scrap"])
                        moonEntry.AddField("Override Spawnable Scrap", "Override the spawnable scrap when on a certain moon, weather, interior, or tag.\nDenoted Key:New Value, separated with commas.", "");

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

                    moonEntry.AddField("Interior Multiplier", "Changes the size of the interior generated.", moonObj.factorySizeMultiplier);
                    if (overridedSettings["Override Interior Multiplier"])
                        moonEntry.AddField("Override Interior Multiplier", "Override the interior multiplier when on a certain moon, weather, interior, or tag.\nDenoted Key:New Value, separated with commas.", "");
                    moonEntry.AddField("Possible Interiors", "The base interiors that can spawn on the moon.\nDenoted with NAME:RARITY, separated with commas.", defaultDungeons);
                    if (overridedSettings["Override Possible Interiors"])
                        moonEntry.AddField("Override Possible Interiors", "Override the possible interiors when on a certain moon, weather, interior, or tag.\nDenoted Key:New Value, separated with commas.", "");

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

                    moonEntry.AddField("Tags", "Tags allocated to the moon.\nSeparate tags with commas.", defaultTags);
                    if (overridedSettings["Override Tags"])
                        moonEntry.AddField("Override Tags", "Override the tags when on a certain moon, weather, interior, or tag.\nDenoted Key:New Value, separated with commas.", "");
                    MiniLogger.LogInfo($"Recorded {moon.name}");
                    registeredMoons.Add(moonUUID);
                }
            }

            moonFile.file.Save();
            moonFile.file.SaveOnConfigSet = true;
        }

        public void InitDungeons()
        {
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
                    dungeonEntry.AddField("Configure Content", "Enable to change any of the settings below.", false);
                    dungeonEntry.AddField("Appropriate Aliases", "Changing this setting will do nothing, these are the names which LunarConfig will recognize as this object in other config options.\nThey are case-insensitve and do not regard whitespace.", $"{dungeon.DungeonName}, {dungeon.DungeonFlow.name}");
                    dungeonEntry.AddField("(Lunar Config) Tags", "Tags allocated to a dungeon.\nSeparate tags with commas.", "");
                    MiniLogger.LogInfo($"Recorded {dungeon.name}");
                    registeredDungeons.Add(dungeonUUID);
                }
            }

            dungeonFile.file.Save();
            dungeonFile.file.SaveOnConfigSet = true;
        }

        public void InitVehicles()
        {
            LunarConfigFile vehicleFile = AddFile(LunarConfig.VEHICLE_FILE, LunarConfig.VEHICLE_FILE_NAME);
            vehicleFile.file.SaveOnConfigSet = false;

            HashSet<string> registeredVehicles = new HashSet<string>();

            // LLL/Vanilla Content
            foreach (var vehicle in PatchedContent.ExtendedBuyableVehicles)
            {
                string vehicleUUID = UUIDify($"LLL - {vehicle.BuyableVehicle.vehicleDisplayName} ({vehicle.UniqueIdentificationName})");
                if (!registeredVehicles.Contains(vehicleUUID))
                {
                    BuyableVehicle vehicleObj = vehicle.BuyableVehicle;
                    LunarConfigEntry vehicleEntry = vehicleFile.AddEntry(vehicleUUID);
                    MiniLogger.LogInfo($"Recording {vehicle.name}...");
                    vehicleEntry.AddField("Configure Content", "Enable to change any of the settings below.", false);
                    vehicleEntry.AddField("Display Name", "Changes the name of the vehicle.\nDoes not modify terminal commands/nodes.", vehicleObj.vehicleDisplayName);
                    vehicleEntry.AddField("Credits Worth", "Changes the price of the vehicle.", vehicleObj.creditsWorth);
                    MiniLogger.LogInfo($"Recorded {vehicle.name}");
                    registeredVehicles.Add(vehicleUUID);
                }
            }

            vehicleFile.file.Save();
            vehicleFile.file.SaveOnConfigSet = true;
        }

        public void InitMapObjects()
        {
            LunarConfigFile mapObjectFile = AddFile(LunarConfig.MAP_OBJECT_FILE, LunarConfig.MAP_OBJECT_FILE_NAME);
            mapObjectFile.file.SaveOnConfigSet = false;

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
                        mapObjectEntry.AddField("Face Away From Wall?", "Whether or not the object should face away from walls.", mapObj.spawnFacingAwayFromWall);
                        mapObjectEntry.AddField("Face Towards Wall?", "Whether or not the object should face towards walls.", mapObj.spawnFacingWall);
                        mapObjectEntry.AddField("Disallow Near Entrance?", "Whether or not the object should not spawn near entrances.", mapObj.disallowSpawningNearEntrances);
                        mapObjectEntry.AddField("Require Distance Between Spawns?", "Whether or not the object should spawn away from others.", mapObj.requireDistanceBetweenSpawns);
                        mapObjectEntry.AddField("Flush Against Wall?", "Whether or not the object should spawn flush against walls.", mapObj.spawnWithBackFlushAgainstWall);
                        mapObjectEntry.AddField("Spawn Against Wall?", "Whether or not the object should spawn against walls.", mapObj.spawnWithBackToWall);
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
                            mapObjectEntry.AddField($"Level Curve - {level.NumberlessPlanetName}", $"The spawn curve for this trap on {level.NumberlessPlanetName}.", curve);
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
                        mapObjectEntry.AddField("Face Away From Wall?", "Whether or not the object should face away from walls.", mapObj.spawnFacingAwayFromWall);
                        mapObjectEntry.AddField("Face Towards Wall?", "Whether or not the object should face towards walls.", mapObj.spawnFacingWall);
                        mapObjectEntry.AddField("Disallow Near Entrance?", "Whether or not the object should not spawn near entrances.", mapObj.disallowSpawningNearEntrances);
                        mapObjectEntry.AddField("Require Distance Between Spawns?", "Whether or not the object should spawn away from others.", mapObj.requireDistanceBetweenSpawns);
                        mapObjectEntry.AddField("Flush Against Wall?", "Whether or not the object should spawn flush against walls.", mapObj.spawnWithBackFlushAgainstWall);
                        mapObjectEntry.AddField("Spawn Against Wall?", "Whether or not the object should spawn against walls.", mapObj.spawnWithBackToWall);
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
                            mapObjectEntry.AddField($"Level Curve - {level.NumberlessPlanetName}", $"The spawn curve for this trap on {level.NumberlessPlanetName}.", curve);
                        }

                        MiniLogger.LogInfo($"Recorded {mapObj.prefabToSpawn.name}");
                        registeredMapObjects.Add(mapObjectUUID);
                    }
                }
            }

            mapObjectFile.file.Save();
            mapObjectFile.file.SaveOnConfigSet = true;
        }

        public void InitOutsideMapObjects()
        {
            LunarConfigFile mapObjectFile = AddFile(LunarConfig.OUTSIDE_MAP_OBJECT_FILE, LunarConfig.OUTSIDE_MAP_OBJECT_FILE_NAME);
            mapObjectFile.file.SaveOnConfigSet = false;

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
                            mapObjectEntry.AddField($"Level Curve - {level.NumberlessPlanetName}", $"The spawn curve for this trap on {level.NumberlessPlanetName}.", CurveToString(mapObject.OutsideSpawnMechanics.CurveFunction(level.SelectableLevel)));
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
                            mapObjectEntry.AddField($"Level Curve - {level.NumberlessPlanetName}", $"The spawn curve for this trap on {level.NumberlessPlanetName}.", curve);
                        }

                        MiniLogger.LogInfo($"Recorded {mapObj.spawnableObject.name}");
                        registeredMapObjects.Add(mapObjectUUID);
                    }
                }
            }

            mapObjectFile.file.Save();
            mapObjectFile.file.SaveOnConfigSet = true;
        }

        public void InitTags()
        {
            LunarConfigFile tagFile = AddFile(LunarConfig.TAG_FILE, LunarConfig.TAG_FILE_NAME);
            tagFile.file.SaveOnConfigSet = false;

            List<string> toRemove = new List<string>();

            foreach (var tag in foundTags)
            {
                foreach (var other in foundTags)
                {
                    if (tag == other) continue;

                    if (other.StartsWith(tag) && other.Length > tag.Length)
                    {
                        toRemove.Add(tag);
                        break;
                    }
                }
            }

            foundTags.ExceptWith(toRemove);

            foreach (var tag in foundTags)
            {
                if (!tag.IsNullOrWhiteSpace())
                {
                    LunarConfigEntry tagEntry = tagFile.AddEntry(tag);
                    MiniLogger.LogInfo($"Recording {tag}...");
                    tagEntry.AddField("Configure Content", "Enable to change any of the settings below.", false);
                    MiniLogger.LogInfo($"Recorded {tag}");
                }
            }

            tagFile.file.Save();
            tagFile.file.SaveOnConfigSet = true;
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
