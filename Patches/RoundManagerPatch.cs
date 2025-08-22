using System;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Netcode;
using LunarConfig.Objects;
using System.IO;
using DunGen.Graph;
using LunarConfig.Objects.Info;
using LunarConfig.Objects.Entries;
using LunarConfig.Objects.Configuration;
using LethalLevelLoader;
using LunarConfig.Objects.Config;
using LethalLib.Modules;
using CodeRebirthLib;
using System.Linq.Expressions;
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
                LunarConfigEntry overrideItems = centralFile.entries["Item Overrides"];
                LunarConfigEntry overrideEnemies = centralFile.entries["Enemy Overrides"];
                LunarConfigEntry overrideMoons = centralFile.entries["Moon Overrides"];
                
                foreach (var val in overrideItems.fields.Keys)
                {
                    if (overrideItems.GetValue<bool>(val))
                    {
                        registeredOverrides["Item"].Add(val.Replace("Override ", ""));
                    }
                }

                foreach (var val in overrideEnemies.fields.Keys)
                {
                    if (overrideEnemies.GetValue<bool>(val))
                    {
                        registeredOverrides["Enemy"].Add(val.Replace("Override ", ""));
                    }
                }

                foreach (var val in overrideMoons.fields.Keys)
                {
                    if (overrideMoons.GetValue<bool>(val))
                    {
                        registeredOverrides["Moon"].Add(val.Replace("Override ", ""));
                    }
                }

                if (centralConfig.GetValue<bool>("Configure Items"))
                {
                    LunarConfigFile itemFile = lunarCentral.files[LunarConfig.ITEM_FILE_NAME];

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
                                configuredItem.SetValue("Display Name", ref item.itemName, overridenSettings.Contains("Display Name"));
                                configuredItem.SetValue("Minimum Value", ref item.minValue, overridenSettings.Contains("Minimum Value"));
                                configuredItem.SetValue("Maximum Value", ref item.maxValue, overridenSettings.Contains("Maximum Value"));
                                configuredItem.SetValue("Weight", ref item.weight, overridenSettings.Contains("Weight"));
                                configuredItem.SetValue("Conductivity", ref item.isConductiveMetal, overridenSettings.Contains("Conductivity"));
                                configuredItem.SetValue("Two-Handed", ref item.twoHanded, overridenSettings.Contains("Two-Handed"));
                                configuredItem.SetValue("Is Scrap?", ref item.isScrap, overridenSettings.Contains("Is Scrap?"));
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
                                configuredItem.SetValue("Display Name", ref item.itemName, overridenSettings.Contains("Display Name"));
                                configuredItem.SetValue("Minimum Value", ref item.minValue, overridenSettings.Contains("Minimum Value"));
                                configuredItem.SetValue("Maximum Value", ref item.maxValue, overridenSettings.Contains("Maximum Value"));
                                configuredItem.SetValue("Weight", ref item.weight, overridenSettings.Contains("Weight"));
                                configuredItem.SetValue("Conductivity", ref item.isConductiveMetal, overridenSettings.Contains("Conductivity"));
                                configuredItem.SetValue("Two-Handed", ref item.twoHanded, overridenSettings.Contains("Two-Handed"));
                                configuredItem.SetValue("Is Scrap?", ref item.isScrap, overridenSettings.Contains("Is Scrap?"));
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
                                configuredItem.SetValue("Display Name", ref item.itemName, overridenSettings.Contains("Display Name"));
                                configuredItem.SetValue("Minimum Value", ref item.minValue, overridenSettings.Contains("Minimum Value"));
                                configuredItem.SetValue("Maximum Value", ref item.maxValue, overridenSettings.Contains("Maximum Value"));
                                configuredItem.SetValue("Weight", ref item.weight, overridenSettings.Contains("Weight"));
                                configuredItem.SetValue("Conductivity", ref item.isConductiveMetal, overridenSettings.Contains("Conductivity"));
                                configuredItem.SetValue("Two-Handed", ref item.twoHanded, overridenSettings.Contains("Two-Handed"));
                                configuredItem.SetValue("Is Scrap?", ref item.isScrap, overridenSettings.Contains("Is Scrap?"));
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
                                configuredItem.SetValue("Display Name", ref item.itemName, overridenSettings.Contains("Display Name"));
                                configuredItem.SetValue("Minimum Value", ref item.minValue, overridenSettings.Contains("Minimum Value"));
                                configuredItem.SetValue("Maximum Value", ref item.maxValue, overridenSettings.Contains("Maximum Value"));
                                configuredItem.SetValue("Weight", ref item.weight, overridenSettings.Contains("Weight"));
                                configuredItem.SetValue("Conductivity", ref item.isConductiveMetal, overridenSettings.Contains("Conductivity"));
                                configuredItem.SetValue("Two-Handed", ref item.twoHanded, overridenSettings.Contains("Two-Handed"));
                                configuredItem.SetValue("Is Scrap?", ref item.isScrap, overridenSettings.Contains("Is Scrap?"));
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
                                extendedEnemy.EnemyDisplayName = configuredEnemy.GetValue<string>("Display Name", overridenSettings.Contains("Display Name"));
                                configuredEnemy.SetValue("Can See Through Fog?", ref enemy.canSeeThroughFog, overridenSettings.Contains("Can See Through Fog?"));
                                configuredEnemy.SetValue("Door Speed Multiplier", ref enemy.doorSpeedMultiplier, overridenSettings.Contains("Door Speed Multiplier"));
                                configuredEnemy.SetValue("Is Daytime Enemy?", ref enemy.isDaytimeEnemy, overridenSettings.Contains("Is Daytime Enemy?"));
                                configuredEnemy.SetValue("Is Outdoor Enemy?", ref enemy.isOutsideEnemy, overridenSettings.Contains("Is Outdoor Enemy?"));
                                configuredEnemy.SetValue("Loudness Multiplier", ref enemy.loudnessMultiplier, overridenSettings.Contains("Loudness Multiplier"));
                                configuredEnemy.SetValue("Max Count", ref enemy.MaxCount, overridenSettings.Contains("Max Count"));
                                configuredEnemy.SetValue("Power Level", ref enemy.PowerLevel, overridenSettings.Contains("Power Level"));
                                configuredEnemy.SetCurve("Probability Curve", ref enemy.probabilityCurve, overridenSettings.Contains("Probability Curve"));
                                configuredEnemy.SetValue("Use Falloff?", ref enemy.useNumberSpawnedFalloff, overridenSettings.Contains("Use Falloff?"));
                                configuredEnemy.SetCurve("Falloff Curve", ref enemy.numberSpawnedFalloff, overridenSettings.Contains("Falloff Curve"));
                                configuredEnemy.SetValue("Enemy HP", ref enemy.enemyPrefab.GetComponent<EnemyAI>().enemyHP, overridenSettings.Contains("Enemy HP"));
                                configuredEnemy.SetValue("Can Die?", ref enemy.canDie, overridenSettings.Contains("Can Die?"));
                                configuredEnemy.SetValue("Destroy On Death?", ref enemy.destroyOnDeath, overridenSettings.Contains("Destroy On Death?"));
                                configuredEnemy.SetValue("Can Destroy?", ref enemy.canBeDestroyed, overridenSettings.Contains("Can Destroy?"));
                                configuredEnemy.SetValue("Can Stun?", ref enemy.canBeStunned, overridenSettings.Contains("Can Stun?"));
                                configuredEnemy.SetValue("Stun Difficulty", ref enemy.stunGameDifficultyMultiplier, overridenSettings.Contains("Stun Difficulty"));
                                configuredEnemy.SetValue("Stun Time", ref enemy.stunTimeMultiplier, overridenSettings.Contains("Stun Time"));
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
                                configuredEnemy.SetValue("Display Name", ref enemy.enemyName, overridenSettings.Contains("Display Name"));
                                configuredEnemy.SetValue("Can See Through Fog?", ref enemy.canSeeThroughFog, overridenSettings.Contains("Can See Through Fog?"));
                                configuredEnemy.SetValue("Door Speed Multiplier", ref enemy.doorSpeedMultiplier, overridenSettings.Contains("Door Speed Multiplier"));
                                configuredEnemy.SetValue("Is Daytime Enemy?", ref enemy.isDaytimeEnemy, overridenSettings.Contains("Is Daytime Enemy?"));
                                configuredEnemy.SetValue("Is Outdoor Enemy?", ref enemy.isOutsideEnemy, overridenSettings.Contains("Is Outdoor Enemy?"));
                                configuredEnemy.SetValue("Loudness Multiplier", ref enemy.loudnessMultiplier, overridenSettings.Contains("Loudness Multiplier"));
                                configuredEnemy.SetValue("Max Count", ref enemy.MaxCount, overridenSettings.Contains("Max Count"));
                                configuredEnemy.SetValue("Power Level", ref enemy.PowerLevel, overridenSettings.Contains("Power Level"));
                                configuredEnemy.SetCurve("Probability Curve", ref enemy.probabilityCurve, overridenSettings.Contains("Probability Curve"));
                                configuredEnemy.SetValue("Use Falloff?", ref enemy.useNumberSpawnedFalloff, overridenSettings.Contains("Use Falloff?"));
                                configuredEnemy.SetCurve("Falloff Curve", ref enemy.numberSpawnedFalloff, overridenSettings.Contains("Falloff Curve"));
                                configuredEnemy.SetValue("Enemy HP", ref enemy.enemyPrefab.GetComponent<EnemyAI>().enemyHP, overridenSettings.Contains("Enemy HP"));
                                configuredEnemy.SetValue("Can Die?", ref enemy.canDie, overridenSettings.Contains("Can Die?"));
                                configuredEnemy.SetValue("Destroy On Death?", ref enemy.destroyOnDeath, overridenSettings.Contains("Destroy On Death?"));
                                configuredEnemy.SetValue("Can Destroy?", ref enemy.canBeDestroyed, overridenSettings.Contains("Can Destroy?"));
                                configuredEnemy.SetValue("Can Stun?", ref enemy.canBeStunned, overridenSettings.Contains("Can Stun?"));
                                configuredEnemy.SetValue("Stun Difficulty", ref enemy.stunGameDifficultyMultiplier, overridenSettings.Contains("Stun Difficulty"));
                                configuredEnemy.SetValue("Stun Time", ref enemy.stunTimeMultiplier, overridenSettings.Contains("Stun Time"));
                            }
                        }
                        catch (Exception e)
                        {
                            MiniLogger.LogError($"An error occured while setting enemy values, please report this!\n{e}");
                        }
                    }
                }

                if (centralConfig.GetValue<bool>("Configure Vehicles"))
                {
                    LunarConfigFile vehicleFile = lunarCentral.files[LunarConfig.VEHICLE_FILE_NAME];

                    // LLL/Vanilla Vehicles WIP
                    foreach (var extendedVehicle in PatchedContent.ExtendedBuyableVehicles)
                    {
                        try
                        {
                            BuyableVehicle vehicle = extendedVehicle.BuyableVehicle;
                            LunarConfigEntry configuredVehicle = vehicleFile.entries[lunarCentral.UUIDify($"LLL - {vehicle.vehicleDisplayName} ({extendedVehicle.UniqueIdentificationName})")];

                            if (configuredVehicle.GetValue<bool>("Configure Content"))
                            {
                                configuredVehicle.SetValue("Display Name", ref vehicle.vehicleDisplayName);
                                configuredVehicle.SetValue("Credits Worth", ref vehicle.creditsWorth);
                            }
                        }
                        catch (Exception e)
                        {
                            MiniLogger.LogError($"An error occured while setting vehicle values, please report this!\n{e}");
                        }
                    }
                }

                if (centralConfig.GetValue<bool>("Configure Map Objects"))
                {
                    LunarConfigFile mapObjectFile = lunarCentral.files[LunarConfig.MAP_OBJECT_FILE_NAME];
                    
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
                                    configuredMapObject.SetValue("Face Away From Wall?", ref mapObj.spawnFacingAwayFromWall);
                                    configuredMapObject.SetValue("Face Towards Wall?", ref mapObj.spawnFacingWall);
                                    configuredMapObject.SetValue("Disallow Near Entrance?", ref mapObj.disallowSpawningNearEntrances);
                                    configuredMapObject.SetValue("Require Distance Between Spawns?", ref mapObj.requireDistanceBetweenSpawns);
                                    configuredMapObject.SetValue("Flush Against Wall?", ref mapObj.spawnWithBackFlushAgainstWall);
                                    configuredMapObject.SetValue("Spawn Against Wall?", ref mapObj.spawnWithBackToWall);

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
                                        configuredMapObject.SetValue("Face Away From Wall?", ref obj.spawnFacingAwayFromWall);
                                        configuredMapObject.SetValue("Face Towards Wall?", ref obj.spawnFacingWall);
                                        configuredMapObject.SetValue("Disallow Near Entrance?", ref obj.disallowSpawningNearEntrances);
                                        configuredMapObject.SetValue("Require Distance Between Spawns?", ref obj.requireDistanceBetweenSpawns);
                                        configuredMapObject.SetValue("Flush Against Wall?", ref obj.spawnWithBackFlushAgainstWall);
                                        configuredMapObject.SetValue("Spawn Against Wall?", ref obj.spawnWithBackToWall);

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

                    List<string> overridenSettings = registeredOverrides["Moon"];

                    ExtendedLevel extendedMoon = LevelManager.CurrentExtendedLevel;
                    SelectableLevel moon = extendedMoon.SelectableLevel;
                    LunarConfigEntry configuredMoon = moonFile.entries[lunarCentral.UUIDify($"LLL - {extendedMoon.NumberlessPlanetName} ({extendedMoon.UniqueIdentificationName})")];

                    if (configuredMoon.GetValue<bool>("Configure Content"))
                    {
                        extendedMoon.ContentTags.Clear();

                        foreach (var tag in configuredMoon.GetValue<string>("Tags", overridenSettings.Contains("Tags"), true).Split(','))
                        {
                            string fixedTag = lunarCentral.UUIDify(tag).RemoveWhitespace();

                            extendedMoon.ContentTags.Add(ContentTag.Create(fixedTag));
                        }

                        LunarCentral.RefreshMatchers();

                        configuredMoon.SetValue("Interior Multiplier", ref moon.factorySizeMultiplier, overridenSettings.Contains("Interior Multiplier"));
                        configuredMoon.SetDungeons("Possible Interiors", lunarCentral, extendedMoon, overridenSettings.Contains("Possible Interiors"));
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

                if (centralConfig.GetValue<bool>("Configure Moons"))
                {
                    LunarConfigFile moonFile = lunarCentral.files[LunarConfig.MOON_FILE_NAME];

                    List<string> overridenSettings = registeredOverrides["Moon"];

                    try
                    {
                        ExtendedLevel extendedMoon = LevelManager.CurrentExtendedLevel;
                        SelectableLevel moon = extendedMoon.SelectableLevel;
                        LunarConfigEntry configuredMoon = moonFile.entries[lunarCentral.UUIDify($"LLL - {extendedMoon.NumberlessPlanetName} ({extendedMoon.UniqueIdentificationName})")];

                        if (configuredMoon.GetValue<bool>("Configure Content"))
                        {
                            extendedMoon.ContentTags.Clear();

                            foreach (var tag in configuredMoon.GetValue<string>("Tags", overridenSettings.Contains("Tags"), true).Split(','))
                            {
                                string fixedTag = lunarCentral.UUIDify(tag).RemoveWhitespace();

                                extendedMoon.ContentTags.Add(ContentTag.Create(fixedTag));
                            }

                            LunarCentral.RefreshMatchers();

                            configuredMoon.SetValue("Display Name", ref moon.PlanetName);
                            configuredMoon.SetValue("Risk Level", ref moon.riskLevel);
                            moon.LevelDescription = configuredMoon.GetValue<string>("Description").Replace(";", "\n");
                            configuredMoon.SetValue("Has Time?", ref moon.planetHasTime, overridenSettings.Contains("Has Time?"));
                            configuredMoon.SetValue("Time Multiplier", ref moon.DaySpeedMultiplier, overridenSettings.Contains("Time Multiplier"));
                            configuredMoon.SetValue("Daytime Probability Range", ref moon.daytimeEnemiesProbabilityRange, overridenSettings.Contains("Daytime Probability Range"));
                            configuredMoon.SetCurve("Daytime Curve", ref moon.daytimeEnemySpawnChanceThroughDay, overridenSettings.Contains("Daytime Curve"));
                            configuredMoon.SetValue("Max Daytime Power", ref moon.maxDaytimeEnemyPowerCount, overridenSettings.Contains("Max Daytime Power"));
                            configuredMoon.SetEnemies("Spawnable Daytime Enemies", lunarCentral, ref moon.DaytimeEnemies, overridenSettings.Contains("Spawnable Daytime Enemies"));
                            configuredMoon.SetValue("Interior Probability Range", ref moon.spawnProbabilityRange, overridenSettings.Contains("Interior Probability Range"));
                            configuredMoon.SetCurve("Interior Curve", ref moon.enemySpawnChanceThroughoutDay, overridenSettings.Contains("Interior Curve"));
                            configuredMoon.SetValue("Max Interior Power", ref moon.maxEnemyPowerCount, overridenSettings.Contains("Max Interior Power"));
                            configuredMoon.SetEnemies("Spawnable Interior Enemies", lunarCentral, ref moon.Enemies, overridenSettings.Contains("Spawnable Interior Enemies"));
                            configuredMoon.SetCurve("Outside Curve", ref moon.outsideEnemySpawnChanceThroughDay, overridenSettings.Contains("Outside Curve"));
                            configuredMoon.SetValue("Max Outside Power", ref moon.maxOutsideEnemyPowerCount, overridenSettings.Contains("Max Outside Power"));
                            configuredMoon.SetEnemies("Spawnable Outside Enemies", lunarCentral, ref moon.OutsideEnemies, overridenSettings.Contains("Spawnable Outside Enemies"));
                            configuredMoon.SetValue("Min Scrap", ref moon.minScrap, overridenSettings.Contains("Min Scrap"));
                            configuredMoon.SetValue("Max Scrap", ref moon.maxScrap, overridenSettings.Contains("Max Scrap"));
                            configuredMoon.SetItems("Spawnable Scrap", lunarCentral, ref moon.spawnableScrap, overridenSettings.Contains("Spawnable Scrap"));
                        }
                    }
                    catch (Exception e)
                    {
                        MiniLogger.LogError($"An error occured while setting moon values, please report this!\n{e}");
                    }
                }

                LunarCentral.RefreshMatchers();

                if (centralConfig.GetValue<bool>("Configure Items") && registeredOverrides["Item"].Count > 0)
                {
                    LunarConfigFile itemFile = lunarCentral.files[LunarConfig.ITEM_FILE_NAME];

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
                                configuredItem.SetValue("Minimum Value", ref item.minValue, overridenSettings.Contains("Minimum Value"));
                                configuredItem.SetValue("Maximum Value", ref item.maxValue, overridenSettings.Contains("Maximum Value"));
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
                                configuredItem.SetValue("Minimum Value", ref item.minValue, overridenSettings.Contains("Minimum Value"));
                                configuredItem.SetValue("Maximum Value", ref item.maxValue, overridenSettings.Contains("Maximum Value"));
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
                                configuredItem.SetValue("Minimum Value", ref item.minValue, overridenSettings.Contains("Minimum Value"));
                                configuredItem.SetValue("Maximum Value", ref item.maxValue, overridenSettings.Contains("Maximum Value"));
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
                                configuredItem.SetValue("Minimum Value", ref item.minValue, overridenSettings.Contains("Minimum Value"));
                                configuredItem.SetValue("Maximum Value", ref item.maxValue, overridenSettings.Contains("Maximum Value"));
                            }
                        }
                        catch (Exception e)
                        {
                            MiniLogger.LogError($"An error occured while setting item values, please report this!\n{e}");
                        }
                    }
                }

                if (centralConfig.GetValue<bool>("Configure Enemies") && registeredOverrides["Enemy"].Count > 0)
                {
                    LunarConfigFile enemyFile = lunarCentral.files[LunarConfig.ENEMY_FILE_NAME];

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
                                extendedEnemy.EnemyDisplayName = configuredEnemy.GetValue<string>("Display Name", overridenSettings.Contains("Display Name"));
                                configuredEnemy.SetValue("Can See Through Fog?", ref enemy.canSeeThroughFog, overridenSettings.Contains("Can See Through Fog?"));
                                configuredEnemy.SetValue("Door Speed Multiplier", ref enemy.doorSpeedMultiplier, overridenSettings.Contains("Door Speed Multiplier"));
                                configuredEnemy.SetValue("Is Daytime Enemy?", ref enemy.isDaytimeEnemy, overridenSettings.Contains("Is Daytime Enemy?"));
                                configuredEnemy.SetValue("Is Outdoor Enemy?", ref enemy.isOutsideEnemy, overridenSettings.Contains("Is Outdoor Enemy?"));
                                configuredEnemy.SetValue("Loudness Multiplier", ref enemy.loudnessMultiplier, overridenSettings.Contains("Loudness Multiplier"));
                                configuredEnemy.SetValue("Max Count", ref enemy.MaxCount, overridenSettings.Contains("Max Count"));
                                configuredEnemy.SetValue("Power Level", ref enemy.PowerLevel, overridenSettings.Contains("Power Level"));
                                configuredEnemy.SetCurve("Probability Curve", ref enemy.probabilityCurve, overridenSettings.Contains("Probability Curve"));
                                configuredEnemy.SetValue("Use Falloff?", ref enemy.useNumberSpawnedFalloff, overridenSettings.Contains("Use Falloff?"));
                                configuredEnemy.SetCurve("Falloff Curve", ref enemy.numberSpawnedFalloff, overridenSettings.Contains("Falloff Curve"));
                                configuredEnemy.SetValue("Enemy HP", ref enemy.enemyPrefab.GetComponent<EnemyAI>().enemyHP, overridenSettings.Contains("Enemy HP"));
                                configuredEnemy.SetValue("Can Die?", ref enemy.canDie, overridenSettings.Contains("Can Die?"));
                                configuredEnemy.SetValue("Destroy On Death?", ref enemy.destroyOnDeath, overridenSettings.Contains("Destroy On Death?"));
                                configuredEnemy.SetValue("Can Destroy?", ref enemy.canBeDestroyed, overridenSettings.Contains("Can Destroy?"));
                                configuredEnemy.SetValue("Can Stun?", ref enemy.canBeStunned, overridenSettings.Contains("Can Stun?"));
                                configuredEnemy.SetValue("Stun Difficulty", ref enemy.stunGameDifficultyMultiplier, overridenSettings.Contains("Stun Difficulty"));
                                configuredEnemy.SetValue("Stun Time", ref enemy.stunTimeMultiplier, overridenSettings.Contains("Stun Time"));
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
                                configuredEnemy.SetValue("Display Name", ref enemy.enemyName, overridenSettings.Contains("Display Name"));
                                configuredEnemy.SetValue("Can See Through Fog?", ref enemy.canSeeThroughFog, overridenSettings.Contains("Can See Through Fog?"));
                                configuredEnemy.SetValue("Door Speed Multiplier", ref enemy.doorSpeedMultiplier, overridenSettings.Contains("Door Speed Multiplier"));
                                configuredEnemy.SetValue("Is Daytime Enemy?", ref enemy.isDaytimeEnemy, overridenSettings.Contains("Is Daytime Enemy?"));
                                configuredEnemy.SetValue("Is Outdoor Enemy?", ref enemy.isOutsideEnemy, overridenSettings.Contains("Is Outdoor Enemy?"));
                                configuredEnemy.SetValue("Loudness Multiplier", ref enemy.loudnessMultiplier, overridenSettings.Contains("Loudness Multiplier"));
                                configuredEnemy.SetValue("Max Count", ref enemy.MaxCount, overridenSettings.Contains("Max Count"));
                                configuredEnemy.SetValue("Power Level", ref enemy.PowerLevel, overridenSettings.Contains("Power Level"));
                                configuredEnemy.SetCurve("Probability Curve", ref enemy.probabilityCurve, overridenSettings.Contains("Probability Curve"));
                                configuredEnemy.SetValue("Use Falloff?", ref enemy.useNumberSpawnedFalloff, overridenSettings.Contains("Use Falloff?"));
                                configuredEnemy.SetCurve("Falloff Curve", ref enemy.numberSpawnedFalloff, overridenSettings.Contains("Falloff Curve"));
                                configuredEnemy.SetValue("Enemy HP", ref enemy.enemyPrefab.GetComponent<EnemyAI>().enemyHP, overridenSettings.Contains("Enemy HP"));
                                configuredEnemy.SetValue("Can Die?", ref enemy.canDie, overridenSettings.Contains("Can Die?"));
                                configuredEnemy.SetValue("Destroy On Death?", ref enemy.destroyOnDeath, overridenSettings.Contains("Destroy On Death?"));
                                configuredEnemy.SetValue("Can Destroy?", ref enemy.canBeDestroyed, overridenSettings.Contains("Can Destroy?"));
                                configuredEnemy.SetValue("Can Stun?", ref enemy.canBeStunned, overridenSettings.Contains("Can Stun?"));
                                configuredEnemy.SetValue("Stun Difficulty", ref enemy.stunGameDifficultyMultiplier, overridenSettings.Contains("Stun Difficulty"));
                                configuredEnemy.SetValue("Stun Time", ref enemy.stunTimeMultiplier, overridenSettings.Contains("Stun Time"));
                            }
                        }
                        catch (Exception e)
                        {
                            MiniLogger.LogError($"An error occured while setting enemy values, please report this!\n{e}");
                        }
                    }
                }

                ExtendedLevel level = LevelManager.CurrentExtendedLevel;

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
                                __instance.scrapAmountMultiplier *= configuredMoon.GetValue<float>("Amount Multiplier");
                                __instance.scrapValueMultiplier *= configuredMoon.GetValue<float>("Value Multiplier");
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
            /*
            [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.LoadNewLevel))]
            [HarmonyPriority(0)]
            [HarmonyPrefix]
            private static void onLoadNewLevelPrefix(RoundManager __instance, ref SelectableLevel newLevel)
            {
                MoonInfo moonSettings = null;

                try
                {
                    MiniLogger.LogInfo("Fetching Moon Information...");

                    if (File.Exists(LunarConfig.MOON_FILE))
                    {
                        foreach (MoonEntry entry in parseMoonConfiguration.parseConfiguration(new MoonConfiguration(File.ReadAllText(LunarConfig.MOON_FILE)).moonConfig))
                        {
                            try
                            {
                                MoonInfo moon = parseMoonEntry.parseEntry(entry.configString);
                                if (moon.moonID == newLevel.name)
                                {
                                    moonSettings = moon;
                                }
                            }
                            catch (Exception e)
                            {
                                MiniLogger.LogError($"Moon Configuration File contains invalid entry, skipping entry!\n{e}");
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    MiniLogger.LogError($"An error occured while fetching level values!\nPlease report this: {e}");
                }

                if (moonSettings != null)
                {
                    try
                    {
                        MiniLogger.LogInfo("Setting Moon Information...");

                        newLevel.planetHasTime = moonSettings.hasTime;
                        newLevel.DaySpeedMultiplier = moonSettings.timeMultiplier;
                        newLevel.daytimeEnemiesProbabilityRange = moonSettings.daytimeProbabilityRange;
                        newLevel.daytimeEnemySpawnChanceThroughDay = moonSettings.daytimeCurve;
                        newLevel.maxDaytimeEnemyPowerCount = moonSettings.maxDaytimePower;
                        newLevel.spawnProbabilityRange = moonSettings.interiorProbabilityRange;
                        newLevel.enemySpawnChanceThroughoutDay = moonSettings.interiorCurve;
                        newLevel.maxEnemyPowerCount = moonSettings.maxInteriorPower;
                        newLevel.outsideEnemySpawnChanceThroughDay = moonSettings.outsideCurve;
                        newLevel.maxOutsideEnemyPowerCount = moonSettings.maxOutsidePower;
                        newLevel.minScrap = moonSettings.minScrap;
                        newLevel.maxScrap = moonSettings.maxScrap;
                        newLevel.factorySizeMultiplier = moonSettings.interiorSizeMultiplier;
                        __instance.scrapValueMultiplier = moonSettings.valueMultiplier;
                        __instance.scrapAmountMultiplier = moonSettings.amountMultiplier;

                        MiniLogger.LogInfo("Moon Information Defined!");
                    }
                    catch (Exception e)
                    {
                        MiniLogger.LogError($"An error occured while modifying level values!\nPlease report this: {e}");
                    }
                }
            }
            */

            /*
            [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.GenerateNewFloor))]
            [HarmonyPriority(250)]
            [HarmonyPrefix]
            private static void onGenerateNewFloorPrefix(RoundManager __instance)
            {
                CentralConfiguration central = null;
                MoonInfo moonSettings = null;
                SelectableLevel newLevel = __instance.currentLevel;
                shouldIncrement = true;

                Dictionary<string, DungeonInfo> registeredDungeons = new Dictionary<string, DungeonInfo>();
                Dictionary<string, TagInfo> registeredTags = new Dictionary<string, TagInfo>();
                MoonDifficultyInfo moonDifficultyInfo = null;

                try
                {
                    string save = GameNetworkManager.Instance.currentSaveFileName;
                    Dictionary<string, MoonDifficultyInfo> loadedData = ES3.Load("Lunar_Data", save, defaultValue: GetDefaultDictionary());

                    foreach (var data in loadedData)
                    {
                        MiniLogger.LogInfo($"{data.Key} : {data.Value.heat}");
                    }

                    if (loadedData.Keys.Contains(newLevel.name))
                    {
                        moonDifficultyInfo = loadedData[newLevel.name];
                    }
                    else
                    {
                        moonDifficultyInfo = new MoonDifficultyInfo();
                    }
                }
                catch (Exception e)
                {
                    MiniLogger.LogError($"Error occured while fetching moon difficulty values, please report this!\n{e}");
                }

                try
                {
                    if (File.Exists(LunarConfig.MOON_FILE))
                    {
                        if (File.Exists(LunarConfig.CENTRAL_FILE))
                        {
                            central = new CentralConfiguration(File.ReadAllText(LunarConfig.CENTRAL_FILE));
                        }

                        foreach (MoonEntry entry in parseMoonConfiguration.parseConfiguration(new MoonConfiguration(File.ReadAllText(LunarConfig.MOON_FILE)).moonConfig))
                        {
                            try
                            {
                                MoonInfo moon = parseMoonEntry.parseEntry(entry.configString);
                                if (moon.moonID == newLevel.name)
                                {
                                    moonSettings = moon;
                                }
                            }
                            catch (Exception e)
                            {
                                MiniLogger.LogError($"Moon Configuration File contains invalid entry, skipping entry!\n{e}");
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    MiniLogger.LogError($"An error occured while fetching level values!\nPlease report this: {e}");
                }

                if (moonSettings != null && central != null && moonDifficultyInfo != null)
                {
                    try
                    {
                        foreach (var entry in parseDungeonConfiguration.parseConfiguration(File.ReadAllText(LunarConfig.DUNGEON_FILE)))
                        {
                            DungeonInfo info = parseDungeonEntry.parseEntry(entry.configString);
                            registeredDungeons.Add(info.dungeonID, info);
                        }

                        foreach (var entry in parseTagConfiguration.parseConfiguration(File.ReadAllText(LunarConfig.TAG_FILE)))
                        {
                            TagInfo info = parseTagEntry.parseEntry(entry.configString);
                            registeredTags.Add(info.tagID, info);
                        }
                    }
                    catch (Exception e)
                    {
                        MiniLogger.LogError($"An error occured while fetching dungeon and tag values!\nPlease report this: {e}");
                    }

                    try
                    {
                        MiniLogger.LogInfo("Setting Dungeon Pools...");

                        var tags = moonSettings.tags;
                        var tagSet = new HashSet<string>(tags);

                        Dictionary<string, int> flowNameToIndex = new();
                        for (int i = 0; i < __instance.dungeonFlowTypes.Length; i++)
                        {
                            flowNameToIndex[__instance.dungeonFlowTypes[i].dungeonFlow.name] = i;
                        }

                        Dictionary<string, float> poolMultipliers = new();

                        foreach (var tag in tagSet)
                        {
                            foreach (var multi in registeredTags[tag].dungeonMultipliers)
                            {
                                if (poolMultipliers.Keys.Contains(multi.Key))
                                {
                                    poolMultipliers[multi.Key] *= multi.Value;
                                }
                                else
                                {
                                    poolMultipliers[multi.Key] = multi.Value;
                                }
                            }
                        }

                        Dictionary<int, int> newDungeonWeights = new();

                        foreach (var dungeon in registeredDungeons.Values)
                        {
                            var matchCount = dungeon.tags.Count(tag => tagSet.Contains(tag));
                            if (matchCount > 0 && flowNameToIndex.TryGetValue(dungeon.dungeonID, out int index))
                            {
                                newDungeonWeights[index] = matchCount * 100;
                            }
                        }

                        Dictionary<int, IntWithRarity> currentDungeonWeights = newLevel.dungeonFlowTypes.ToDictionary(k => k.id);

                        if (central.clearDungeons)
                        {
                            foreach (var dungeon in currentDungeonWeights.Values)
                                dungeon.rarity = 0;
                        }

                        foreach (var (id, weight) in newDungeonWeights)
                        {
                            if (!currentDungeonWeights.TryGetValue(id, out var rarity))
                            {
                                rarity = new IntWithRarity { id = id, rarity = 0 };
                                currentDungeonWeights[id] = rarity;
                            }
                            rarity.rarity += weight;
                        }

                        foreach (var dungeon in currentDungeonWeights.Values)
                        {
                            var flowName = __instance.dungeonFlowTypes[dungeon.id].dungeonFlow.name;
                            if (poolMultipliers.TryGetValue(flowName, out float mult))
                            {
                                dungeon.rarity = (int)Math.Ceiling(dungeon.rarity * mult);
                            }
                        }

                        if (central.logPools)
                        {
                            MiniLogger.LogInfo($"Starting {newLevel.name} with the following dungeon weights:");
                            foreach (var dungeon in currentDungeonWeights.Values)
                            {
                                MiniLogger.LogInfo($"{__instance.dungeonFlowTypes[dungeon.id].dungeonFlow.name}: {dungeon.rarity}");
                            }
                        }

                        newLevel.dungeonFlowTypes = currentDungeonWeights.Values.ToArray();

                        MiniLogger.LogInfo("Dungeon Pools Set!");
                    }
                    catch (Exception e)
                    {
                        MiniLogger.LogError($"An error occured while modifying dungeon weights!\nPlease report this: {e}");
                    }
                }
            }
            */

            /*
            [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.GenerateNewFloor))]
            [HarmonyPriority(800)]
            [HarmonyPostfix]
            private static void onGenerateNewFloorPostfix(RoundManager __instance)
            {
                NetworkManager manager = UnityEngine.Object.FindObjectOfType<NetworkManager>();
                SpawnableObjectKeeper objects = new SpawnableObjectKeeper(manager);

                string currentDungeon = __instance.dungeonFlowTypes[__instance.currentDungeonType].dungeonFlow.name;
                SelectableLevel level = __instance.currentLevel;
                lastLevel = level.name;

                Dictionary<string, ItemInfo> registeredItems = new Dictionary<string, ItemInfo>();
                Dictionary<string, EnemyInfo> registeredEnemies = new Dictionary<string, EnemyInfo>();
                Dictionary<string, MapObjectInfo> registeredMapObjects = new Dictionary<string, MapObjectInfo>();
                Dictionary<string, TagInfo> registeredTags = new Dictionary<string, TagInfo>();

                CentralConfiguration central = null;
                MoonInfo moonSettings = null;
                DungeonInfo dungeonInfo = null;
                MoonDifficultyInfo moonDifficultyInfo = null;

                try
                {
                    string save = GameNetworkManager.Instance.currentSaveFileName;
                    Dictionary<string, MoonDifficultyInfo> loadedData = ES3.Load("Lunar_Data", save, defaultValue: GetDefaultDictionary());

                    if (loadedData.Keys.Contains(level.name))
                    {
                        moonDifficultyInfo = loadedData[level.name];
                    }
                    else
                    {
                        moonDifficultyInfo = new MoonDifficultyInfo();
                    }
                }
                catch (Exception e)
                {
                    MiniLogger.LogError($"Error occured while fetching moon difficulty values, please report this!\n{e}");
                }

                if (File.Exists(LunarConfig.CENTRAL_FILE))
                {
                    central = new CentralConfiguration(File.ReadAllText(LunarConfig.CENTRAL_FILE));
                }

                if (File.Exists(LunarConfig.MOON_FILE))
                {
                    foreach (MoonEntry entry in parseMoonConfiguration.parseConfiguration(new MoonConfiguration(File.ReadAllText(LunarConfig.MOON_FILE)).moonConfig))
                    {
                        try
                        {
                            MoonInfo moon = parseMoonEntry.parseEntry(entry.configString);
                            if (moon.moonID == level.name)
                            {
                                moonSettings = moon;
                            }
                        }
                        catch (Exception e)
                        {
                            MiniLogger.LogError($"Moon Configuration File contains invalid entry, skipping entry!\n{e}");
                        }
                    }
                }

                if (File.Exists(LunarConfig.DUNGEON_FILE))
                {
                    foreach (DungeonEntry entry in parseDungeonConfiguration.parseConfiguration(new DungeonConfiguration(File.ReadAllText(LunarConfig.DUNGEON_FILE)).dungeonConfig))
                    {
                        try
                        {
                            DungeonInfo dungeon = parseDungeonEntry.parseEntry(entry.configString);
                            if (dungeon.dungeonID == currentDungeon)
                            {
                                dungeonInfo = dungeon;
                            }
                        }
                        catch (Exception e)
                        {
                            MiniLogger.LogError($"Dungeon Configuration File contains invalid entry, skipping entry!\n{e}");
                        }
                    }
                }

                if (moonSettings != null && dungeonInfo != null && central != null && moonDifficultyInfo != null)
                {
                    try
                    {
                        foreach (var entry in parseItemConfiguration.parseConfiguration(File.ReadAllText(LunarConfig.ITEM_FILE)))
                        {
                            ItemInfo info = parseItemEntry.parseEntry(entry.configString);
                            registeredItems.Add(info.itemID, info);
                        }

                        foreach (var entry in parseEnemyConfiguration.parseConfiguration(File.ReadAllText(LunarConfig.ENEMY_FILE)))
                        {
                            EnemyInfo info = parseEnemyEntry.parseEntry(entry.configString);
                            registeredEnemies.Add(info.enemyID, info);
                        }

                        foreach (var entry in parseMapObjectConfiguration.parseConfiguration(File.ReadAllText(LunarConfig.MAP_OBJECT_FILE)))
                        {
                            MapObjectInfo info = parseMapObjectEntry.parseEntry(entry.configString);
                            registeredMapObjects.Add(info.objID, info);
                        }

                        foreach (var entry in parseTagConfiguration.parseConfiguration(File.ReadAllText(LunarConfig.TAG_FILE)))
                        {
                            TagInfo info = parseTagEntry.parseEntry(entry.configString);
                            registeredTags.Add(info.tagID, info);
                        }
                    }
                    catch (Exception e)
                    {
                        MiniLogger.LogError($"An error occured while fetching pool values!\nPlease report this: {e}");
                    }

                    if (registeredTags.Any())
                    {
                        try
                        {
                            MiniLogger.LogInfo("Setting Item Pools...");

                            var tags = moonSettings.tags;
                            tags.Add(currentDungeon);
                            var tagSet = new HashSet<string>(tags);

                            Dictionary<string, float> poolMultipliers = new();

                            foreach (var tag in tagSet)
                            {
                                if (tag == currentDungeon)
                                { continue; }

                                foreach (var multi in registeredTags[tag].itemPoolMultipliers)
                                {
                                    if (poolMultipliers.Keys.Contains(multi.Key))
                                    {
                                        poolMultipliers[multi.Key] *= multi.Value;
                                    }
                                    else
                                    {
                                        poolMultipliers[multi.Key] = multi.Value;
                                    }
                                }
                            }

                            foreach (var pool in central.scrapDecayPools)
                            {
                                if (poolMultipliers.Keys.Contains(pool))
                                {
                                    poolMultipliers[pool] *= (float)Math.Pow(central.scrapDecayRate, Math.Max(moonDifficultyInfo.heat, 0));
                                }
                            }

                            List<(ItemInfo, int)> compatibleTagItems = new();

                            foreach (var item in registeredItems.Values)
                            {
                                List<(string, string)> splitTags = new();
                                List<string> matchTags = new();

                                foreach (var tag in item.tags)
                                {
                                    var parts = tag.Split("_");
                                    splitTags.Add((parts[0], parts[1]));
                                    matchTags.Add(parts[0]);

                                    if (tagSet.Contains(parts[0]))
                                    {
                                        compatibleTagItems.Add((item, (int)Math.Ceiling(100 * poolMultipliers[parts[1]])));
                                    }
                                }
                            }

                            Dictionary<string, SpawnableItemWithRarity> currentScrapWeights = new();

                            foreach (var k in level.spawnableScrap)
                            {
                                string name = k.spawnableItem.name;

                                if (currentScrapWeights.TryGetValue(name, out var existing))
                                {
                                    existing.rarity += k.rarity;
                                }
                                else
                                {
                                    currentScrapWeights[name] = new SpawnableItemWithRarity { spawnableItem = k.spawnableItem, rarity = k.rarity };
                                }
                            }

                            if (central.clearItems)
                            {
                                foreach (var item in currentScrapWeights.Values)
                                    item.rarity = 0;
                            }

                            foreach (var (item, weight) in compatibleTagItems)
                            {
                                if (currentScrapWeights.ContainsKey(item.itemID))
                                {
                                    currentScrapWeights[item.itemID].rarity += weight;
                                }
                                else
                                {
                                    currentScrapWeights[item.itemID] = new SpawnableItemWithRarity { spawnableItem = objects.items[item.itemID], rarity = weight };
                                }
                            }

                            if (central.logPools)
                            {
                                MiniLogger.LogInfo($"Starting {level} with the following scrap weights:");
                                foreach (var scrap in currentScrapWeights)
                                {
                                    MiniLogger.LogInfo($"{scrap.Key}: {scrap.Value.rarity}");
                                }
                            }

                            level.spawnableScrap = currentScrapWeights.Values.ToList();

                            MiniLogger.LogInfo("Item Pools Set!");

                            MiniLogger.LogInfo("Setting Enemy Pools...");

                            Dictionary<string, float> interiorEnemyPoolMultipliers = new();
                            Dictionary<string, float> exteriorEnemyPoolMultipliers = new();
                            Dictionary<string, float> daytimeEnemyPoolMultipliers = new();

                            foreach (var tag in tagSet)
                            {
                                if (tag == currentDungeon)
                                { continue; }

                                foreach (var multi in registeredTags[tag].interiorEnemyPoolMultipliers)
                                {
                                    if (interiorEnemyPoolMultipliers.Keys.Contains(multi.Key))
                                    {
                                        interiorEnemyPoolMultipliers[multi.Key] *= multi.Value;
                                    }
                                    else
                                    {
                                        interiorEnemyPoolMultipliers[multi.Key] = multi.Value;
                                    }
                                }

                                foreach (var multi in registeredTags[tag].exteriorEnemyPoolMultipliers)
                                {
                                    if (exteriorEnemyPoolMultipliers.Keys.Contains(multi.Key))
                                    {
                                        exteriorEnemyPoolMultipliers[multi.Key] *= multi.Value;
                                    }
                                    else
                                    {
                                        exteriorEnemyPoolMultipliers[multi.Key] = multi.Value;
                                    }
                                }

                                foreach (var multi in registeredTags[tag].daytimeEnemyPoolMultipliers)
                                {
                                    if (daytimeEnemyPoolMultipliers.Keys.Contains(multi.Key))
                                    {
                                        daytimeEnemyPoolMultipliers[multi.Key] *= multi.Value;
                                    }
                                    else
                                    {
                                        daytimeEnemyPoolMultipliers[multi.Key] = multi.Value;
                                    }
                                }
                            }

                            List<(EnemyInfo, int)> compatibleInteriorEnemies = new();
                            List<(EnemyInfo, int)> compatibleExteriorEnemies = new();
                            List<(EnemyInfo, int)> compatibleDaytimeEnemies = new();

                            foreach (var enemy in registeredEnemies.Values)
                            {
                                List<(string, string)> splitTags = new();
                                List<string> matchTags = new();

                                foreach (var tag in enemy.tags)
                                {
                                    var parts = tag.Split("_");
                                    splitTags.Add((parts[0], parts[1]));
                                    matchTags.Add(parts[0]);

                                    if (parts[0] == currentDungeon)
                                    {
                                        compatibleInteriorEnemies.Add((enemy, (int)Math.Ceiling(100 * interiorEnemyPoolMultipliers[parts[1]])));
                                    }
                                    else if (tagSet.Contains(parts[0]))
                                    {
                                        if (parts.Length > 2 && !enemy.blacklistTags.Intersect(tagSet).Any())
                                        {
                                            compatibleDaytimeEnemies.Add((enemy, (int)Math.Ceiling(100 * daytimeEnemyPoolMultipliers[parts[1]])));
                                        }
                                        else
                                        {
                                            compatibleExteriorEnemies.Add((enemy, (int)Math.Ceiling(100 * exteriorEnemyPoolMultipliers[parts[1]])));
                                        }
                                    }
                                }
                            }

                            Dictionary<string, SpawnableEnemyWithRarity> currentInteriorEnemyWeights = new();
                            Dictionary<string, SpawnableEnemyWithRarity> currentExteriorEnemyWeights = new();
                            Dictionary<string, SpawnableEnemyWithRarity> currentDaytimeEnemyWeights = new();

                            foreach (var k in level.Enemies)
                            {
                                string name = k.enemyType.name;

                                if (currentInteriorEnemyWeights.TryGetValue(name, out var existing))
                                {
                                    existing.rarity += k.rarity;
                                }
                                else
                                {
                                    currentInteriorEnemyWeights[name] = new SpawnableEnemyWithRarity { enemyType = k.enemyType, rarity = k.rarity };
                                }
                            }

                            foreach (var k in level.OutsideEnemies)
                            {
                                string name = k.enemyType.name;

                                if (currentExteriorEnemyWeights.TryGetValue(name, out var existing))
                                {
                                    existing.rarity += k.rarity;
                                }
                                else
                                {
                                    currentExteriorEnemyWeights[name] = new SpawnableEnemyWithRarity { enemyType = k.enemyType, rarity = k.rarity };
                                }
                            }

                            foreach (var k in level.DaytimeEnemies)
                            {
                                string name = k.enemyType.name;

                                if (currentDaytimeEnemyWeights.TryGetValue(name, out var existing))
                                {
                                    existing.rarity += k.rarity;
                                }
                                else
                                {
                                    currentDaytimeEnemyWeights[name] = new SpawnableEnemyWithRarity { enemyType = k.enemyType, rarity = k.rarity };
                                }
                            }

                            if (central.clearEnemies)
                            {
                                foreach (var enemy in currentInteriorEnemyWeights.Values)
                                    enemy.rarity = 0;

                                foreach (var enemy in currentExteriorEnemyWeights.Values)
                                    enemy.rarity = 0;

                                foreach (var enemy in currentDaytimeEnemyWeights.Values)
                                    enemy.rarity = 0;
                            }

                            foreach (var (enemy, weight) in compatibleInteriorEnemies)
                            {
                                if (currentInteriorEnemyWeights.ContainsKey(enemy.enemyID))
                                {
                                    currentInteriorEnemyWeights[enemy.enemyID].rarity += weight;
                                }
                                else
                                {
                                    currentInteriorEnemyWeights[enemy.enemyID] = new SpawnableEnemyWithRarity { enemyType = objects.enemies[enemy.enemyID], rarity = weight };
                                }
                            }

                            foreach (var (enemy, weight) in compatibleExteriorEnemies)
                            {
                                if (currentExteriorEnemyWeights.ContainsKey(enemy.enemyID))
                                {
                                    currentExteriorEnemyWeights[enemy.enemyID].rarity += weight;
                                }
                                else
                                {
                                    currentExteriorEnemyWeights[enemy.enemyID] = new SpawnableEnemyWithRarity { enemyType = objects.enemies[enemy.enemyID], rarity = weight };
                                }
                            }

                            foreach (var (enemy, weight) in compatibleDaytimeEnemies)
                            {
                                if (currentDaytimeEnemyWeights.ContainsKey(enemy.enemyID))
                                {
                                    currentDaytimeEnemyWeights[enemy.enemyID].rarity += weight;
                                }
                                else
                                {
                                    currentDaytimeEnemyWeights[enemy.enemyID] = new SpawnableEnemyWithRarity { enemyType = objects.enemies[enemy.enemyID], rarity = weight };
                                }
                            }

                            if (central.logPools)
                            {
                                MiniLogger.LogInfo($"Starting {level} with the following interior enemy weights:");
                                foreach (var enemy in currentInteriorEnemyWeights)
                                {
                                    MiniLogger.LogInfo($"{enemy.Key}: {enemy.Value.rarity}");
                                }

                                MiniLogger.LogInfo($"Starting {level} with the following exterior enemy weights:");
                                foreach (var enemy in currentExteriorEnemyWeights)
                                {
                                    MiniLogger.LogInfo($"{enemy.Key}: {enemy.Value.rarity}");
                                }

                                MiniLogger.LogInfo($"Starting {level} with the following daytime enemy weights:");
                                foreach (var enemy in currentDaytimeEnemyWeights)
                                {
                                    MiniLogger.LogInfo($"{enemy.Key}: {enemy.Value.rarity}");
                                }
                            }

                            level.Enemies = currentInteriorEnemyWeights.Values.ToList();
                            level.OutsideEnemies = currentExteriorEnemyWeights.Values.ToList();
                            level.DaytimeEnemies = currentDaytimeEnemyWeights.Values.ToList();

                            MiniLogger.LogInfo("Enemy Pools Set!");

                            MiniLogger.LogInfo("Setting Trap Curves...");

                            float trapDifficultyMultiplier = 1;
                            Dictionary<string, float> trapCurveMultipliers = new();

                            foreach (var tag in tagSet)
                            {
                                if (tag == currentDungeon)
                                { continue; }

                                trapDifficultyMultiplier *= registeredTags[tag].mapObjectPeakMultiplier;
                            }

                            Dictionary<string, SpawnableMapObject> currentMapObjects = level.spawnableMapObjects.ToDictionary(k => k.prefabToSpawn.name);
                            HashSet<string> presentTraps = currentMapObjects.Keys.ToHashSet();

                            foreach (var (id, trap) in registeredMapObjects)
                            {
                                if (!presentTraps.Contains(id) && objects.mapObjects.Keys.Contains(id))
                                {
                                    currentMapObjects.Add(id, objects.mapObjects[id]);
                                }
                            }

                            if (central.useTrapCurves)
                            {
                                foreach (var trap in currentMapObjects)
                                    trap.Value.numberToSpawn = registeredMapObjects[trap.Key].baseCurve;
                            }

                            foreach (var (id, trap) in currentMapObjects)
                            {
                                AnimationCurve curve = trap.numberToSpawn;

                                float multiplier = dungeonInfo.mapObjectMultipliers[id];

                                Keyframe[] newCurve = new Keyframe[curve.length];

                                for (int i = 0; i < curve.length; i++)
                                {
                                    Keyframe key = curve[i];
                                    key.value *= multiplier;
                                    key.inTangent *= multiplier;
                                    key.outTangent *= multiplier;
                                    if (i == curve.length - 1)
                                    {
                                        key.value *= trapDifficultyMultiplier;
                                        key.inTangent *= trapDifficultyMultiplier;
                                        key.outTangent *= trapDifficultyMultiplier;
                                    }
                                    newCurve[i] = key;
                                }

                                trap.numberToSpawn = new AnimationCurve(newCurve);
                            }

                            level.spawnableMapObjects = currentMapObjects.Values.ToArray();

                            MiniLogger.LogInfo("Trap Curves Set!");
                        }
                        catch (Exception e)
                        {
                            MiniLogger.LogError($"An error occured while modifying pool values!\nPlease report this: {e}");
                        }
                    }
                }
            }
            */
        }
}
