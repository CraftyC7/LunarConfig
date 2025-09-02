using System;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using LethalLevelLoader;
using LunarConfig.Objects.Config;
using LethalLib.Modules;
using CodeRebirthLib;
using CodeRebirthLib.ContentManagement.MapObjects;

namespace LunarConfig.Patches
{
    internal class RoundManagerPatch
    {
        public static string lastLevel = "";
        public static bool shouldIncrement = false;

        public static Dictionary<string, List<string>> registeredOverrides = new Dictionary<string, List<string>>();

        public static Dictionary<string, SpawnableMapObject> configuredMapObjects = new Dictionary<string, SpawnableMapObject>();
        public static Dictionary<string, CRMapObjectDefinition> configuredCROutsideObjects = new Dictionary<string, CRMapObjectDefinition>();
        public static Dictionary<string, SpawnableOutsideObjectWithRarity> configuredOutsideObjects = new Dictionary<string, SpawnableOutsideObjectWithRarity>();

        [HarmonyPatch(typeof(RoundManager), "Awake")]
        [HarmonyPriority(400)]
        [HarmonyPostfix]
        private static void onStartPrefix(RoundManager __instance)
        {
            try
            {
                LunarCentral lunarCentral = LunarConfig.central;

                lunarCentral.InitConfig();

                registeredOverrides["Item"] = new List<string>();
                registeredOverrides["Enemy"] = new List<string>();
                registeredOverrides["Moon"] = new List<string>();

                LunarConfigFile centralFile = lunarCentral.files[LunarConfig.CENTRAL_FILE_NAME];
                LunarConfigEntry centralConfig = centralFile.entries["Configuration"];

                if (centralConfig.GetValue<bool>("Configure Items"))
                {
                    LunarConfigFile itemFile = lunarCentral.files[LunarConfig.ITEM_FILE_NAME];

                    LunarConfigEntry enabledEntry = centralFile.entries["Enabled Item Settings"];
                    HashSet<string> enabledSettings = new HashSet<string>();

                    foreach (var setting in enabledEntry.fields.Keys)
                    {
                        if (enabledEntry.GetValue<bool>(setting))
                        {
                            enabledSettings.Add(setting);
                        }
                    }

                    List<string> overridenSettings = registeredOverrides["Item"];

                    // LLL/Vanilla Items
                    foreach (var extendedItem in PatchedContent.ExtendedItems)
                    {
                        try
                        {
                            Item item = extendedItem.Item;
                            LunarConfigEntry configuredItem = itemFile.entries[lunarCentral.UUIDify($"LLL - {item.itemName} ({extendedItem.UniqueIdentificationName})")];

                            if (configuredItem.GetValue<bool>("Configure Content"))
                            {
                                if (enabledSettings.Contains("Display Name")) { configuredItem.SetValue("Display Name", ref item.itemName, overridenSettings.Contains("Display Name")); }
                                if (enabledSettings.Contains("Minimum Value")) { configuredItem.SetValue("Minimum Value", ref item.minValue, overridenSettings.Contains("Minimum Value")); }
                                if (enabledSettings.Contains("Maximum Value")) { configuredItem.SetValue("Maximum Value", ref item.maxValue, overridenSettings.Contains("Maximum Value")); }
                                if (enabledSettings.Contains("Weight")) { configuredItem.SetValue("Weight", ref item.weight, overridenSettings.Contains("Weight")); }
                                if (enabledSettings.Contains("Conductivity")) { configuredItem.SetValue("Conductivity", ref item.isConductiveMetal, overridenSettings.Contains("Conductivity")); }
                                if (enabledSettings.Contains("Two-Handed")) { configuredItem.SetValue("Two-Handed", ref item.twoHanded, overridenSettings.Contains("Two-Handed")); }
                                if (enabledSettings.Contains("Is Scrap?")) { configuredItem.SetValue("Is Scrap?", ref item.isScrap, overridenSettings.Contains("Is Scrap?")); }
                            }
                        }
                        catch (Exception e)
                        {
                            MiniLogger.LogError($"An error occured while setting item values, please report this!\n{e}");
                        }
                    }

                    // LL/CRLib Items
                    foreach (var spawnableItem in Items.scrapItems)
                    {
                        try
                        {
                            Item item = spawnableItem.item;
                            LunarConfigEntry configuredItem = itemFile.entries[lunarCentral.UUIDify($"LL - {item.itemName} ({spawnableItem.modName}.{item.name})")];

                            if (configuredItem.GetValue<bool>("Configure Content"))
                            {
                                if (enabledSettings.Contains("Display Name")) { configuredItem.SetValue("Display Name", ref item.itemName, overridenSettings.Contains("Display Name")); }
                                if (enabledSettings.Contains("Minimum Value")) { configuredItem.SetValue("Minimum Value", ref item.minValue, overridenSettings.Contains("Minimum Value")); }
                                if (enabledSettings.Contains("Maximum Value")) { configuredItem.SetValue("Maximum Value", ref item.maxValue, overridenSettings.Contains("Maximum Value")); }
                                if (enabledSettings.Contains("Weight")) { configuredItem.SetValue("Weight", ref item.weight, overridenSettings.Contains("Weight")); }
                                if (enabledSettings.Contains("Conductivity")) { configuredItem.SetValue("Conductivity", ref item.isConductiveMetal, overridenSettings.Contains("Conductivity")); }
                                if (enabledSettings.Contains("Two-Handed")) { configuredItem.SetValue("Two-Handed", ref item.twoHanded, overridenSettings.Contains("Two-Handed")); }
                                if (enabledSettings.Contains("Is Scrap?")) { configuredItem.SetValue("Is Scrap?", ref item.isScrap, overridenSettings.Contains("Is Scrap?")); }
                            }
                        }
                        catch (Exception e)
                        {
                            MiniLogger.LogError($"An error occured while setting item values, please report this!\n{e}");
                        }
                    }

                    foreach (var spawnableItem in Items.shopItems)
                    {
                        try
                        {
                            Item item = spawnableItem.item;
                            LunarConfigEntry configuredItem = itemFile.entries[lunarCentral.UUIDify($"LL - {item.itemName} ({spawnableItem.modName}.{item.name})")];

                            if (configuredItem.GetValue<bool>("Configure Content"))
                            {
                                if (enabledSettings.Contains("Display Name")) { configuredItem.SetValue("Display Name", ref item.itemName, overridenSettings.Contains("Display Name")); }
                                if (enabledSettings.Contains("Minimum Value")) { configuredItem.SetValue("Minimum Value", ref item.minValue, overridenSettings.Contains("Minimum Value")); }
                                if (enabledSettings.Contains("Maximum Value")) { configuredItem.SetValue("Maximum Value", ref item.maxValue, overridenSettings.Contains("Maximum Value")); }
                                if (enabledSettings.Contains("Weight")) { configuredItem.SetValue("Weight", ref item.weight, overridenSettings.Contains("Weight")); }
                                if (enabledSettings.Contains("Conductivity")) { configuredItem.SetValue("Conductivity", ref item.isConductiveMetal, overridenSettings.Contains("Conductivity")); }
                                if (enabledSettings.Contains("Two-Handed")) { configuredItem.SetValue("Two-Handed", ref item.twoHanded, overridenSettings.Contains("Two-Handed")); }
                                if (enabledSettings.Contains("Is Scrap?")) { configuredItem.SetValue("Is Scrap?", ref item.isScrap, overridenSettings.Contains("Is Scrap?")); }
                            }
                        }
                        catch (Exception e)
                        {
                            MiniLogger.LogError($"An error occured while setting item values, please report this!\n{e}");
                        }
                    }

                    foreach (var spawnableItem in Items.plainItems)
                    {
                        try
                        {
                            Item item = spawnableItem.item;
                            LunarConfigEntry configuredItem = itemFile.entries[lunarCentral.UUIDify($"LL - {item.itemName} ({spawnableItem.modName}.{item.name})")];

                            if (configuredItem.GetValue<bool>("Configure Content"))
                            {
                                if (enabledSettings.Contains("Display Name")) { configuredItem.SetValue("Display Name", ref item.itemName, overridenSettings.Contains("Display Name")); }
                                if (enabledSettings.Contains("Minimum Value")) { configuredItem.SetValue("Minimum Value", ref item.minValue, overridenSettings.Contains("Minimum Value")); }
                                if (enabledSettings.Contains("Maximum Value")) { configuredItem.SetValue("Maximum Value", ref item.maxValue, overridenSettings.Contains("Maximum Value")); }
                                if (enabledSettings.Contains("Weight")) { configuredItem.SetValue("Weight", ref item.weight, overridenSettings.Contains("Weight")); }
                                if (enabledSettings.Contains("Conductivity")) { configuredItem.SetValue("Conductivity", ref item.isConductiveMetal, overridenSettings.Contains("Conductivity")); }
                                if (enabledSettings.Contains("Two-Handed")) { configuredItem.SetValue("Two-Handed", ref item.twoHanded, overridenSettings.Contains("Two-Handed")); }
                                if (enabledSettings.Contains("Is Scrap?")) { configuredItem.SetValue("Is Scrap?", ref item.isScrap, overridenSettings.Contains("Is Scrap?")); }
                            }
                        }
                        catch (Exception e)
                        {
                            MiniLogger.LogError($"An error occured while setting item values, please report this!\n{e}");
                        }
                    }
                }

                if (centralConfig.GetValue<bool>("Configure Enemies"))
                {
                    LunarConfigFile enemyFile = lunarCentral.files[LunarConfig.ENEMY_FILE_NAME];

                    LunarConfigEntry enabledEntry = centralFile.entries["Enabled Enemy Settings"];
                    HashSet<string> enabledSettings = new HashSet<string>();

                    foreach (var setting in enabledEntry.fields.Keys)
                    {
                        if (enabledEntry.GetValue<bool>(setting))
                        {
                            enabledSettings.Add(setting);
                        }
                    }

                    List<string> overridenSettings = registeredOverrides["Enemy"];

                    // LLL/Vanilla Enemies
                    foreach (var extendedEnemy in PatchedContent.ExtendedEnemyTypes)
                    {
                        try
                        {
                            EnemyType enemy = extendedEnemy.EnemyType;
                            LunarConfigEntry configuredEnemy = enemyFile.entries[lunarCentral.UUIDify($"LLL - {enemy.enemyName} ({extendedEnemy.UniqueIdentificationName})")];

                            if (configuredEnemy.GetValue<bool>("Configure Content"))
                            {
                                if (enabledSettings.Contains("Display Name")) { extendedEnemy.EnemyDisplayName = configuredEnemy.GetValue<string>("Display Name"); }
                                if (enabledSettings.Contains("Can See Through Fog?")) { configuredEnemy.SetValue("Can See Through Fog?", ref enemy.canSeeThroughFog, overridenSettings.Contains("Can See Through Fog?")); }
                                if (enabledSettings.Contains("Door Speed Multiplier")) { configuredEnemy.SetValue("Door Speed Multiplier", ref enemy.doorSpeedMultiplier, overridenSettings.Contains("Door Speed Multiplier")); }
                                if (enabledSettings.Contains("Is Daytime Enemy?")) { configuredEnemy.SetValue("Is Daytime Enemy?", ref enemy.isDaytimeEnemy, overridenSettings.Contains("Is Daytime Enemy?")); }
                                if (enabledSettings.Contains("Is Outdoor Enemy?")) { configuredEnemy.SetValue("Is Outdoor Enemy?", ref enemy.isOutsideEnemy, overridenSettings.Contains("Is Outdoor Enemy?")); }
                                if (enabledSettings.Contains("Loudness Multiplier")) { configuredEnemy.SetValue("Loudness Multiplier", ref enemy.loudnessMultiplier, overridenSettings.Contains("Loudness Multiplier")); }
                                if (enabledSettings.Contains("Max Count")) { configuredEnemy.SetValue("Max Count", ref enemy.MaxCount, overridenSettings.Contains("Max Count")); }
                                if (enabledSettings.Contains("Power Level")) { configuredEnemy.SetValue("Power Level", ref enemy.PowerLevel, overridenSettings.Contains("Power Level")); }
                                if (enabledSettings.Contains("Probability Curve")) { configuredEnemy.SetCurve("Probability Curve", ref enemy.probabilityCurve, overridenSettings.Contains("Probability Curve")); }
                                if (enabledSettings.Contains("Use Falloff?")) { configuredEnemy.SetValue("Use Falloff?", ref enemy.useNumberSpawnedFalloff, overridenSettings.Contains("Use Falloff?")); }
                                if (enabledSettings.Contains("Falloff Curve")) { configuredEnemy.SetCurve("Falloff Curve", ref enemy.numberSpawnedFalloff, overridenSettings.Contains("Falloff Curve")); }
                                if (enabledSettings.Contains("Enemy HP")) { configuredEnemy.SetValue("Enemy HP", ref enemy.enemyPrefab.GetComponent<EnemyAI>().enemyHP, overridenSettings.Contains("Enemy HP")); }
                                if (enabledSettings.Contains("Can Die?")) { configuredEnemy.SetValue("Can Die?", ref enemy.canDie, overridenSettings.Contains("Can Die?")); }
                                if (enabledSettings.Contains("Destroy On Death?")) { configuredEnemy.SetValue("Destroy On Death?", ref enemy.destroyOnDeath, overridenSettings.Contains("Destroy On Death?")); }
                                if (enabledSettings.Contains("Can Destroy?")) { configuredEnemy.SetValue("Can Destroy?", ref enemy.canBeDestroyed, overridenSettings.Contains("Can Destroy?")); }
                                if (enabledSettings.Contains("Can Stun?")) { configuredEnemy.SetValue("Can Stun?", ref enemy.canBeStunned, overridenSettings.Contains("Can Stun?")); }
                                if (enabledSettings.Contains("Stun Difficulty")) { configuredEnemy.SetValue("Stun Difficulty", ref enemy.stunGameDifficultyMultiplier, overridenSettings.Contains("Stun Difficulty")); }
                                if (enabledSettings.Contains("Stun Time")) { configuredEnemy.SetValue("Stun Time", ref enemy.stunTimeMultiplier, overridenSettings.Contains("Stun Time")); }
                            }
                        }
                        catch (Exception e)
                        {
                            MiniLogger.LogError($"An error occured while setting enemy values, please report this!\n{e}");
                        }
                    }

                    // LL/CRLib Enemies
                    foreach (var spawnableEnemy in Enemies.spawnableEnemies)
                    {
                        try
                        {
                            EnemyType enemy = spawnableEnemy.enemy;
                            LunarConfigEntry configuredEnemy = enemyFile.entries[lunarCentral.UUIDify($"LL - {enemy.enemyName} ({spawnableEnemy.modName}.{enemy.name})")];

                            if (configuredEnemy.GetValue<bool>("Configure Content"))
                            {
                                if (enabledSettings.Contains("Display Name")) { enemy.enemyName = configuredEnemy.GetValue<string>("Display Name"); }
                                if (enabledSettings.Contains("Can See Through Fog?")) { configuredEnemy.SetValue("Can See Through Fog?", ref enemy.canSeeThroughFog, overridenSettings.Contains("Can See Through Fog?")); }
                                if (enabledSettings.Contains("Door Speed Multiplier")) { configuredEnemy.SetValue("Door Speed Multiplier", ref enemy.doorSpeedMultiplier, overridenSettings.Contains("Door Speed Multiplier")); }
                                if (enabledSettings.Contains("Is Daytime Enemy?")) { configuredEnemy.SetValue("Is Daytime Enemy?", ref enemy.isDaytimeEnemy, overridenSettings.Contains("Is Daytime Enemy?")); }
                                if (enabledSettings.Contains("Is Outdoor Enemy?")) { configuredEnemy.SetValue("Is Outdoor Enemy?", ref enemy.isOutsideEnemy, overridenSettings.Contains("Is Outdoor Enemy?")); }
                                if (enabledSettings.Contains("Loudness Multiplier")) { configuredEnemy.SetValue("Loudness Multiplier", ref enemy.loudnessMultiplier, overridenSettings.Contains("Loudness Multiplier")); }
                                if (enabledSettings.Contains("Max Count")) { configuredEnemy.SetValue("Max Count", ref enemy.MaxCount, overridenSettings.Contains("Max Count")); }
                                if (enabledSettings.Contains("Power Level")) { configuredEnemy.SetValue("Power Level", ref enemy.PowerLevel, overridenSettings.Contains("Power Level")); }
                                if (enabledSettings.Contains("Probability Curve")) { configuredEnemy.SetCurve("Probability Curve", ref enemy.probabilityCurve, overridenSettings.Contains("Probability Curve")); }
                                if (enabledSettings.Contains("Use Falloff?")) { configuredEnemy.SetValue("Use Falloff?", ref enemy.useNumberSpawnedFalloff, overridenSettings.Contains("Use Falloff?")); }
                                if (enabledSettings.Contains("Falloff Curve")) { configuredEnemy.SetCurve("Falloff Curve", ref enemy.numberSpawnedFalloff, overridenSettings.Contains("Falloff Curve")); }
                                if (enabledSettings.Contains("Enemy HP")) { configuredEnemy.SetValue("Enemy HP", ref enemy.enemyPrefab.GetComponent<EnemyAI>().enemyHP, overridenSettings.Contains("Enemy HP")); }
                                if (enabledSettings.Contains("Can Die?")) { configuredEnemy.SetValue("Can Die?", ref enemy.canDie, overridenSettings.Contains("Can Die?")); }
                                if (enabledSettings.Contains("Destroy On Death?")) { configuredEnemy.SetValue("Destroy On Death?", ref enemy.destroyOnDeath, overridenSettings.Contains("Destroy On Death?")); }
                                if (enabledSettings.Contains("Can Destroy?")) { configuredEnemy.SetValue("Can Destroy?", ref enemy.canBeDestroyed, overridenSettings.Contains("Can Destroy?")); }
                                if (enabledSettings.Contains("Can Stun?")) { configuredEnemy.SetValue("Can Stun?", ref enemy.canBeStunned, overridenSettings.Contains("Can Stun?")); }
                                if (enabledSettings.Contains("Stun Difficulty")) { configuredEnemy.SetValue("Stun Difficulty", ref enemy.stunGameDifficultyMultiplier, overridenSettings.Contains("Stun Difficulty")); }
                                if (enabledSettings.Contains("Stun Time")) { configuredEnemy.SetValue("Stun Time", ref enemy.stunTimeMultiplier, overridenSettings.Contains("Stun Time")); }
                            }
                        }
                        catch (Exception e)
                        {
                            MiniLogger.LogError($"An error occured while setting enemy values, please report this!\n{e}");
                        }
                    }
                }

                if (centralConfig.GetValue<bool>("Configure Map Objects"))
                {
                    LunarConfigFile mapObjectFile = lunarCentral.files[LunarConfig.MAP_OBJECT_FILE_NAME];

                    LunarConfigEntry enabledEntry = centralFile.entries["Enabled Map Object Settings"];
                    HashSet<string> enabledSettings = new HashSet<string>();

                    foreach (var setting in enabledEntry.fields.Keys)
                    {
                        if (enabledEntry.GetValue<bool>(setting))
                        {
                            enabledSettings.Add(setting);
                        }
                    }

                    HashSet<string> registeredObjects = new HashSet<string>();

                    // LL/CRLib MapObjects
                    foreach (var spawnableMapObject in MapObjects.mapObjects)
                    {
                        try
                        {
                            if (spawnableMapObject.mapObject != null)
                            {
                                SpawnableMapObject mapObj = spawnableMapObject.mapObject;
                                LunarConfigEntry configuredMapObject = mapObjectFile.entries[lunarCentral.UUIDify($"LL - {mapObj.prefabToSpawn.name}")];

                                if (configuredMapObject.GetValue<bool>("Configure Content"))
                                {
                                    if (enabledSettings.Contains("Face Away From Wall?")) { configuredMapObject.SetValue("Face Away From Wall?", ref mapObj.spawnFacingAwayFromWall); }
                                    if (enabledSettings.Contains("Face Towards Wall?")) { configuredMapObject.SetValue("Face Towards Wall?", ref mapObj.spawnFacingWall); }
                                    if (enabledSettings.Contains("Disallow Near Entrance?")) { configuredMapObject.SetValue("Disallow Near Entrance?", ref mapObj.disallowSpawningNearEntrances); }
                                    if (enabledSettings.Contains("Require Distance Between Spawns?")) { configuredMapObject.SetValue("Require Distance Between Spawns?", ref mapObj.requireDistanceBetweenSpawns); }
                                    if (enabledSettings.Contains("Flush Against Wall?")) { configuredMapObject.SetValue("Flush Against Wall?", ref mapObj.spawnWithBackFlushAgainstWall); }
                                    if (enabledSettings.Contains("Spawn Against Wall?")) { configuredMapObject.SetValue("Spawn Against Wall?", ref mapObj.spawnWithBackToWall); }

                                    configuredMapObjects[lunarCentral.UUIDify($"LL - {mapObj.prefabToSpawn.name}")] = mapObj;
                                }

                                registeredObjects.Add(mapObj.prefabToSpawn.name);
                            }
                        }
                        catch (Exception e)
                        {
                            MiniLogger.LogError($"An error occured while setting map object values, please report this!\n{e}");
                        }
                    }

                    // Vanilla MapObjects
                    foreach (var level in PatchedContent.ExtendedLevels)
                    {
                        foreach (var obj in level.SelectableLevel.spawnableMapObjects)
                        {
                            if (!registeredObjects.Contains(obj.prefabToSpawn.name))
                            {
                                try
                                {
                                    LunarConfigEntry configuredMapObject = mapObjectFile.entries[lunarCentral.UUIDify($"Vanilla - {obj.prefabToSpawn.name}")];

                                    if (configuredMapObject.GetValue<bool>("Configure Content"))
                                    {
                                        if (enabledSettings.Contains("Face Away From Wall?")) { configuredMapObject.SetValue("Face Away From Wall?", ref obj.spawnFacingAwayFromWall); }
                                        if (enabledSettings.Contains("Face Towards Wall?")) { configuredMapObject.SetValue("Face Towards Wall?", ref obj.spawnFacingWall); }
                                        if (enabledSettings.Contains("Disallow Near Entrance?")) { configuredMapObject.SetValue("Disallow Near Entrance?", ref obj.disallowSpawningNearEntrances); }
                                        if (enabledSettings.Contains("Require Distance Between Spawns?")) { configuredMapObject.SetValue("Require Distance Between Spawns?", ref obj.requireDistanceBetweenSpawns); }
                                        if (enabledSettings.Contains("Flush Against Wall?")) { configuredMapObject.SetValue("Flush Against Wall?", ref obj.spawnWithBackFlushAgainstWall); }
                                        if (enabledSettings.Contains("Spawn Against Wall?")) { configuredMapObject.SetValue("Spawn Against Wall?", ref obj.spawnWithBackToWall); }

                                        configuredMapObjects[lunarCentral.UUIDify($"Vanilla - {obj.prefabToSpawn.name}")] = obj;
                                    }

                                    registeredObjects.Add(obj.prefabToSpawn.name);
                                }
                                catch (Exception e)
                                {
                                    MiniLogger.LogError($"An error occured while setting map object values, please report this!\n{e}");
                                }
                            }
                        }
                    }
                }

                if (centralConfig.GetValue<bool>("Configure Outside Map Objects"))
                {
                    LunarConfigFile outsideObjectFile = lunarCentral.files[LunarConfig.OUTSIDE_MAP_OBJECT_FILE_NAME];

                    HashSet<string> registeredObjects = new HashSet<string>();

                    // CRLib MapObjects
                    foreach (var spawnableMapObject in CRMod.AllMapObjects())
                    {
                        try
                        {
                            if (spawnableMapObject.OutsideSpawnMechanics != null)
                            {
                                LunarConfigEntry configuredMapObject = outsideObjectFile.entries[lunarCentral.UUIDify($"CRLib - {spawnableMapObject.GameObject.name}")];

                                if (configuredMapObject.GetValue<bool>("Configure Content"))
                                {
                                    configuredCROutsideObjects[lunarCentral.UUIDify($"CRLib - {spawnableMapObject.GameObject.name}")] = spawnableMapObject;
                                }

                                registeredObjects.Add(spawnableMapObject.GameObject.name);
                            }
                        }
                        catch (Exception e)
                        {
                            MiniLogger.LogError($"An error occured while setting outside object values, please report this!\n{e}");
                        }
                    }

                    // Vanilla MapObjects
                    foreach (var level in PatchedContent.ExtendedLevels)
                    {
                        foreach (var obj in level.SelectableLevel.spawnableOutsideObjects)
                        {
                            if (!registeredObjects.Contains(obj.spawnableObject.name))
                            {
                                try
                                {
                                    LunarConfigEntry configuredMapObject = outsideObjectFile.entries[lunarCentral.UUIDify($"Vanilla - {obj.spawnableObject.name}")];

                                    if (configuredMapObject.GetValue<bool>("Configure Content"))
                                    {
                                        configuredOutsideObjects[lunarCentral.UUIDify($"Vanilla - {obj.spawnableObject.name}")] = obj;
                                    }

                                    registeredObjects.Add(obj.spawnableObject.name);
                                }
                                catch (Exception e)
                                {
                                    MiniLogger.LogError($"An error occured while setting outside object values, please report this!\n{e}");
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e) 
            {
                MiniLogger.LogError($"An error occured, please report this!\n{e}");
            }
        }

        [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.LoadNewLevel))]
        [HarmonyPriority(800)]
        [HarmonyPrefix]
        private static void onLoadNewLevelPrefix(RoundManager __instance)
        {
            try
            {
                LunarCentral lunarCentral = LunarConfig.central;
                LunarConfigEntry centralConfig = lunarCentral.files[LunarConfig.CENTRAL_FILE_NAME].entries["Configuration"];

                if (centralConfig.GetValue<bool>("Configure Moons"))
                {
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

                    List<string> overridenSettings = registeredOverrides["Moon"];

                    ExtendedLevel extendedMoon = LevelManager.CurrentExtendedLevel;
                    SelectableLevel moon = extendedMoon.SelectableLevel;
                    LunarConfigEntry configuredMoon = moonFile.entries[lunarCentral.UUIDify($"LLL - {extendedMoon.NumberlessPlanetName} ({extendedMoon.UniqueIdentificationName})")];

                    if (configuredMoon.GetValue<bool>("Configure Content"))
                    {
                        if (enabledSettings.Contains("Tags"))
                        {
                            extendedMoon.ContentTags.Clear();

                            foreach (var tag in configuredMoon.GetValue<string>("Tags").Split(','))
                            {
                                string fixedTag = lunarCentral.UUIDify(tag).RemoveWhitespace();

                                extendedMoon.ContentTags.Add(ContentTag.Create(fixedTag));
                            }
                        }

                        LunarCentral.RefreshMatchers();

                        if (enabledSettings.Contains("Interior Multiplier")) { configuredMoon.SetValue("Interior Multiplier", ref moon.factorySizeMultiplier, overridenSettings.Contains("Interior Multiplier")); }
                        if (enabledSettings.Contains("Possible Interiors")) { configuredMoon.SetDungeons("Possible Interiors", lunarCentral, extendedMoon, overridenSettings.Contains("Possible Interiors")); }
                    }
                }
            }
            catch (Exception e)
            {
                MiniLogger.LogError($"An error occured while setting interior values, please report this!\n{e}");
            }
        }

        [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.GenerateNewFloor))]
        [HarmonyPriority(800)]
        [HarmonyPostfix]
        private static void onGenerateNewFloorPostfix(RoundManager __instance)
        {
            try
            {
                LunarCentral lunarCentral = LunarConfig.central;

                LunarConfigFile mapObjectFile = lunarCentral.files[LunarConfig.MAP_OBJECT_FILE_NAME];
                LunarConfigFile outsideObjectFile = lunarCentral.files[LunarConfig.OUTSIDE_MAP_OBJECT_FILE_NAME];

                LunarConfigEntry centralConfig = lunarCentral.files[LunarConfig.CENTRAL_FILE_NAME].entries["Configuration"];

                LunarConfigEntry enabledEntry;
                HashSet<string> enabledSettings = new HashSet<string>();

                if (centralConfig.GetValue<bool>("Configure Moons"))
                {
                    LunarConfigFile moonFile = lunarCentral.files[LunarConfig.MOON_FILE_NAME];

                    enabledEntry = lunarCentral.files[LunarConfig.CENTRAL_FILE_NAME].entries["Enabled Moon Settings"];
                    enabledSettings = new HashSet<string>();

                    foreach (var setting in enabledEntry.fields.Keys)
                    {
                        if (enabledEntry.GetValue<bool>(setting))
                        {
                            enabledSettings.Add(setting);
                        }
                    }

                    List<string> overridenSettings = registeredOverrides["Moon"];

                    try
                    {
                        ExtendedLevel extendedMoon = LevelManager.CurrentExtendedLevel;
                        SelectableLevel moon = extendedMoon.SelectableLevel;
                        LunarConfigEntry configuredMoon = moonFile.entries[lunarCentral.UUIDify($"LLL - {extendedMoon.NumberlessPlanetName} ({extendedMoon.UniqueIdentificationName})")];

                        if (configuredMoon.GetValue<bool>("Configure Content"))
                        {
                            if (enabledSettings.Contains("Tags"))
                            {
                                extendedMoon.ContentTags.Clear();

                                foreach (var tag in configuredMoon.GetValue<string>("Tags").Split(','))
                                {
                                    string fixedTag = lunarCentral.UUIDify(tag).RemoveWhitespace();

                                    extendedMoon.ContentTags.Add(ContentTag.Create(fixedTag));
                                }
                            }

                            LunarCentral.RefreshMatchers();

                            if (enabledSettings.Contains("Display Name")) { configuredMoon.SetValue("Display Name", ref moon.PlanetName); }
                            if (enabledSettings.Contains("Risk Level")) { configuredMoon.SetValue("Risk Level", ref moon.riskLevel); }
                            if (enabledSettings.Contains("Description")) { moon.LevelDescription = configuredMoon.GetValue<string>("Description").Replace(";", "\n"); }
                            if (enabledSettings.Contains("Has Time?")) { configuredMoon.SetValue("Has Time?", ref moon.planetHasTime, overridenSettings.Contains("Has Time?")); }
                            if (enabledSettings.Contains("Time Multiplier")) { configuredMoon.SetValue("Time Multiplier", ref moon.DaySpeedMultiplier, overridenSettings.Contains("Time Multiplier")); }
                            if (enabledSettings.Contains("Daytime Probability Range")) { configuredMoon.SetValue("Daytime Probability Range", ref moon.daytimeEnemiesProbabilityRange, overridenSettings.Contains("Daytime Probability Range")); }
                            if (enabledSettings.Contains("Daytime Curve")) { configuredMoon.SetCurve("Daytime Curve", ref moon.daytimeEnemySpawnChanceThroughDay, overridenSettings.Contains("Daytime Curve")); }
                            if (enabledSettings.Contains("Max Daytime Power")) { configuredMoon.SetValue("Max Daytime Power", ref moon.maxDaytimeEnemyPowerCount, overridenSettings.Contains("Max Daytime Power")); }
                            if (enabledSettings.Contains("Spawnable Daytime Enemies")) { configuredMoon.SetEnemies("Spawnable Daytime Enemies", lunarCentral, ref moon.DaytimeEnemies, overridenSettings.Contains("Spawnable Daytime Enemies")); }
                            if (enabledSettings.Contains("Interior Probability Range")) { configuredMoon.SetValue("Interior Probability Range", ref moon.spawnProbabilityRange, overridenSettings.Contains("Interior Probability Range")); }
                            if (enabledSettings.Contains("Interior Curve")) { configuredMoon.SetCurve("Interior Curve", ref moon.enemySpawnChanceThroughoutDay, overridenSettings.Contains("Interior Curve")); }
                            if (enabledSettings.Contains("Max Interior Power")) { configuredMoon.SetValue("Max Interior Power", ref moon.maxEnemyPowerCount, overridenSettings.Contains("Max Interior Power")); }
                            if (enabledSettings.Contains("Spawnable Interior Enemies")) { configuredMoon.SetEnemies("Spawnable Interior Enemies", lunarCentral, ref moon.Enemies, overridenSettings.Contains("Spawnable Interior Enemies")); }
                            if (enabledSettings.Contains("Outside Curve")) { configuredMoon.SetCurve("Outside Curve", ref moon.outsideEnemySpawnChanceThroughDay, overridenSettings.Contains("Outside Curve")); }
                            if (enabledSettings.Contains("Max Outside Power")) { configuredMoon.SetValue("Max Outside Power", ref moon.maxOutsideEnemyPowerCount, overridenSettings.Contains("Max Outside Power")); }
                            if (enabledSettings.Contains("Spawnable Outside Enemies")) { configuredMoon.SetEnemies("Spawnable Outside Enemies", lunarCentral, ref moon.OutsideEnemies, overridenSettings.Contains("Spawnable Outside Enemies")); }
                            if (enabledSettings.Contains("Min Scrap")) { configuredMoon.SetValue("Min Scrap", ref moon.minScrap, overridenSettings.Contains("Min Scrap")); }
                            if (enabledSettings.Contains("Max Scrap")) { configuredMoon.SetValue("Max Scrap", ref moon.maxScrap, overridenSettings.Contains("Max Scrap")); }
                            if (enabledSettings.Contains("Spawnable Scrap")) { configuredMoon.SetItems("Spawnable Scrap", lunarCentral, ref moon.spawnableScrap, overridenSettings.Contains("Spawnable Scrap")); }
                        }
                    }
                    catch (Exception e)
                    {
                        MiniLogger.LogError($"An error occured while setting moon values, please report this!\n{e}");
                    }
                }

                LunarCentral.RefreshMatchers();

                ExtendedLevel level = LevelManager.CurrentExtendedLevel;

                enabledSettings.Clear();
                enabledEntry = lunarCentral.files[LunarConfig.CENTRAL_FILE_NAME].entries["Enabled Map Object Settings"];
                enabledSettings = new HashSet<string>();

                foreach (var setting in enabledEntry.fields.Keys)
                {
                    if (enabledEntry.GetValue<bool>(setting))
                    {
                        enabledSettings.Add(setting);
                    }
                }

                if (enabledSettings.Contains($"Level Curve - {level.NumberlessPlanetName}"))
                {
                    // Modifying Map Object Curves And Injecting Modified Objects
                    if (configuredMapObjects.Count > 0 && level != null)
                    {
                        foreach (var mapObject in configuredMapObjects)
                        {
                            List<SpawnableMapObject> objects = level.SelectableLevel.spawnableMapObjects.ToList();

                            objects.RemoveAll(obj => obj.prefabToSpawn.name == mapObject.Value.prefabToSpawn.name);

                            SpawnableMapObject objWithCurve = mapObject.Value;
                            LunarConfigEntry configuredMapObject = mapObjectFile.entries[mapObject.Key];
                            string stringCurve = configuredMapObject.GetValue<string>($"Level Curve - {level.NumberlessPlanetName}");
                            if (stringCurve.Trim() != "")
                            {
                                objWithCurve.numberToSpawn = LunarCentral.StringToCurve(stringCurve);
                            }
                            else
                            {
                                objWithCurve.numberToSpawn = LunarCentral.StringToCurve(configuredMapObject.GetValue<string>("Base Curve"));
                            }

                            objects.Add(objWithCurve);

                            level.SelectableLevel.spawnableMapObjects = objects.ToArray();
                        }
                    }
                }

                enabledSettings.Clear();
                enabledEntry = lunarCentral.files[LunarConfig.CENTRAL_FILE_NAME].entries["Enabled Outside Map Object Settings"];

                foreach (var setting in enabledEntry.fields.Keys)
                {
                    if (enabledEntry.GetValue<bool>(setting))
                    {
                        enabledSettings.Add(setting);
                    }
                }

                if (enabledSettings.Contains($"Level Curve - {level.NumberlessPlanetName}"))
                {
                    // Modifying Outside Vanilla Object Curves And Injecting Modified Objects
                    if (configuredOutsideObjects.Count > 0 && level != null)
                    {
                        foreach (var mapObject in configuredOutsideObjects)
                        {
                            List<SpawnableOutsideObjectWithRarity> objects = level.SelectableLevel.spawnableOutsideObjects.ToList();

                            objects.RemoveAll(obj => obj.spawnableObject.name == mapObject.Value.spawnableObject.name);

                            SpawnableOutsideObjectWithRarity objWithCurve = mapObject.Value;
                            LunarConfigEntry configuredMapObject = outsideObjectFile.entries[mapObject.Key];
                            string stringCurve = configuredMapObject.GetValue<string>($"Level Curve - {level.NumberlessPlanetName}");
                            if (stringCurve.Trim() != "")
                            {
                                objWithCurve.randomAmount = LunarCentral.StringToCurve(stringCurve);
                            }
                            else
                            {
                                objWithCurve.randomAmount = LunarCentral.StringToCurve(configuredMapObject.GetValue<string>("Base Curve"));
                            }

                            objects.Add(objWithCurve);

                            level.SelectableLevel.spawnableOutsideObjects = objects.ToArray();
                        }
                    }

                    // Modifying Outside CR Object Curves And Injecting Modified Objects
                    if (configuredCROutsideObjects.Count > 0 && level != null)
                    {
                        foreach (var mapObject in configuredCROutsideObjects)
                        {
                            MiniLogger.LogInfo($"Configuring {mapObject.Key}");

                            Enum.TryParse(level.SelectableLevel.name, true, out Levels.LevelTypes levelType);

                            LunarConfigEntry configuredMapObject = outsideObjectFile.entries[mapObject.Key];
                            string stringCurve = configuredMapObject.GetValue<string>($"Level Curve - {level.NumberlessPlanetName}");
                            if (stringCurve.Trim() == "")
                            {
                                stringCurve = configuredMapObject.GetValue<string>("Base Curve");
                            }
                            AnimationCurve configuredCurve = LunarCentral.StringToCurve(stringCurve);

                            if (levelType == Levels.LevelTypes.Modded)
                            {
                                AnimationCurve curve = mapObject.Value.OutsideSpawnMechanics.CurveFunction(level.SelectableLevel);

                                if (curve != configuredCurve)
                                {
                                    mapObject.Value.OutsideSpawnMechanics.CurvesByCustomLevelType[level.name] = configuredCurve;
                                }
                            }
                            else
                            {
                                AnimationCurve curve = mapObject.Value.OutsideSpawnMechanics.CurveFunction(level.SelectableLevel);

                                if (curve != configuredCurve)
                                {
                                    mapObject.Value.OutsideSpawnMechanics.CurvesByLevelType[levelType] = configuredCurve;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MiniLogger.LogError($"An issue occured when modifying spawn pools, please report this!\n{e}");
            }
        }

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

            LunarConfigEntry centralConfig = lunarCentral.files[LunarConfig.CENTRAL_FILE_NAME].entries["Configuration"];

            LunarConfigEntry enabledEntry = lunarCentral.files[LunarConfig.CENTRAL_FILE_NAME].entries["Enabled Moon Settings"];
            HashSet<string> enabledSettings = new HashSet<string>();

            foreach (var setting in enabledEntry.fields.Keys)
            {
                if (enabledEntry.GetValue<bool>(setting))
                {
                    enabledSettings.Add(setting);
                }
            }

            if (centralConfig.GetValue<bool>("Configure Moons"))
            {
                LunarConfigFile moonFile = lunarCentral.files[LunarConfig.MOON_FILE_NAME];

                List<string> overridenSettings = registeredOverrides["Moon"];

                // LLL/Vanilla Moons
                foreach (var extendedMoon in PatchedContent.ExtendedLevels)
                {
                    try
                    {
                        if (extendedMoon.IsCurrentLevel)
                        {
                            SelectableLevel moon = extendedMoon.SelectableLevel;
                            LunarConfigEntry configuredMoon = moonFile.entries[lunarCentral.UUIDify($"LLL - {extendedMoon.NumberlessPlanetName} ({extendedMoon.UniqueIdentificationName})")];

                            if (configuredMoon.GetValue<bool>("Configure Content"))
                            {
                                if (enabledSettings.Contains("Amount Multiplier")) { __instance.scrapAmountMultiplier *= configuredMoon.GetValue<float>("Amount Multiplier"); }
                                if (enabledSettings.Contains("Value Multiplier")) { __instance.scrapValueMultiplier *= configuredMoon.GetValue<float>("Value Multiplier"); }
                            }

                            break;
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
}
