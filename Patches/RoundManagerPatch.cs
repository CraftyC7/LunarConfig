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

                LunarConfigEntry centralConfig = lunarCentral.files[LunarConfig.CENTRAL_FILE_NAME].entries["Configuration"];

                if (centralConfig.GetValue<bool>("Configure Items"))
                {
                    LunarConfigFile itemFile = lunarCentral.files[LunarConfig.ITEM_FILE_NAME];

                    // LLL/Vanilla Items
                    foreach (var extendedItem in PatchedContent.ExtendedItems)
                    {
                        try
                        {
                            Item item = extendedItem.Item;
                            LunarConfigEntry configuredItem = itemFile.entries[lunarCentral.UUIDify($"LLL - {item.itemName} ({extendedItem.UniqueIdentificationName})")];

                            if (configuredItem.GetValue<bool>("Configure Content"))
                            {
                                configuredItem.SetValue("Display Name", ref item.itemName);
                                configuredItem.SetValue("Minimum Value", ref item.minValue);
                                configuredItem.SetValue("Maximum Value", ref item.maxValue);
                                configuredItem.SetValue("Weight", ref item.weight);
                                configuredItem.SetValue("Conductivity", ref item.isConductiveMetal);
                                configuredItem.SetValue("Two-Handed", ref item.twoHanded);
                                configuredItem.SetValue("Is Scrap?", ref item.isScrap);
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
                                configuredItem.SetValue("Display Name", ref item.itemName);
                                configuredItem.SetValue("Minimum Value", ref item.minValue);
                                configuredItem.SetValue("Maximum Value", ref item.maxValue);
                                configuredItem.SetValue("Weight", ref item.weight);
                                configuredItem.SetValue("Conductivity", ref item.isConductiveMetal);
                                configuredItem.SetValue("Two-Handed", ref item.twoHanded);
                                configuredItem.SetValue("Is Scrap?", ref item.isScrap);
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
                                configuredItem.SetValue("Display Name", ref item.itemName);
                                configuredItem.SetValue("Minimum Value", ref item.minValue);
                                configuredItem.SetValue("Maximum Value", ref item.maxValue);
                                configuredItem.SetValue("Weight", ref item.weight);
                                configuredItem.SetValue("Conductivity", ref item.isConductiveMetal);
                                configuredItem.SetValue("Two-Handed", ref item.twoHanded);
                                configuredItem.SetValue("Is Scrap?", ref item.isScrap);
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
                                configuredItem.SetValue("Display Name", ref item.itemName);
                                configuredItem.SetValue("Minimum Value", ref item.minValue);
                                configuredItem.SetValue("Maximum Value", ref item.maxValue);
                                configuredItem.SetValue("Weight", ref item.weight);
                                configuredItem.SetValue("Conductivity", ref item.isConductiveMetal);
                                configuredItem.SetValue("Two-Handed", ref item.twoHanded);
                                configuredItem.SetValue("Is Scrap?", ref item.isScrap);
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

                    // LLL/Vanilla Enemies
                    foreach (var extendedEnemy in PatchedContent.ExtendedEnemyTypes)
                    {
                        try
                        {
                            EnemyType enemy = extendedEnemy.EnemyType;
                            LunarConfigEntry configuredEnemy = enemyFile.entries[lunarCentral.UUIDify($"LLL - {enemy.enemyName} ({extendedEnemy.UniqueIdentificationName})")];

                            if (configuredEnemy.GetValue<bool>("Configure Content"))
                            {
                                extendedEnemy.EnemyDisplayName = configuredEnemy.GetValue<string>("Display Name");
                                configuredEnemy.SetValue("Can See Through Fog?", ref enemy.canSeeThroughFog);
                                configuredEnemy.SetValue("Door Speed Multiplier", ref enemy.doorSpeedMultiplier);
                                configuredEnemy.SetValue("Is Daytime Enemy?", ref enemy.isDaytimeEnemy);
                                configuredEnemy.SetValue("Is Outdoor Enemy?", ref enemy.isOutsideEnemy);
                                configuredEnemy.SetValue("Loudness Multiplier", ref enemy.loudnessMultiplier);
                                configuredEnemy.SetValue("Max Count", ref enemy.MaxCount);
                                configuredEnemy.SetValue("Power Level", ref enemy.PowerLevel);
                                configuredEnemy.SetCurve("Probability Curve", ref enemy.probabilityCurve);
                                configuredEnemy.SetValue("Use Falloff?", ref enemy.useNumberSpawnedFalloff);
                                configuredEnemy.SetCurve("Falloff Curve", ref enemy.numberSpawnedFalloff);
                                configuredEnemy.SetValue("Enemy HP", ref enemy.enemyPrefab.GetComponent<EnemyAI>().enemyHP);
                                configuredEnemy.SetValue("Can Die?", ref enemy.canDie);
                                configuredEnemy.SetValue("Destroy On Death?", ref enemy.destroyOnDeath);
                                configuredEnemy.SetValue("Can Destroy?", ref enemy.canBeDestroyed);
                                configuredEnemy.SetValue("Can Stun?", ref enemy.canBeStunned);
                                configuredEnemy.SetValue("Stun Difficulty", ref enemy.stunGameDifficultyMultiplier);
                                configuredEnemy.SetValue("Stun Time", ref enemy.stunTimeMultiplier);
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
                                configuredEnemy.SetValue("Display Name", ref enemy.enemyName);
                                configuredEnemy.SetValue("Can See Through Fog?", ref enemy.canSeeThroughFog);
                                configuredEnemy.SetValue("Door Speed Multiplier", ref enemy.doorSpeedMultiplier);
                                configuredEnemy.SetValue("Is Daytime Enemy?", ref enemy.isDaytimeEnemy);
                                configuredEnemy.SetValue("Is Outdoor Enemy?", ref enemy.isOutsideEnemy);
                                configuredEnemy.SetValue("Loudness Multiplier", ref enemy.loudnessMultiplier);
                                configuredEnemy.SetValue("Max Count", ref enemy.MaxCount);
                                configuredEnemy.SetValue("Power Level", ref enemy.PowerLevel);
                                configuredEnemy.SetCurve("Probability Curve", ref enemy.probabilityCurve);
                                configuredEnemy.SetValue("Use Falloff?", ref enemy.useNumberSpawnedFalloff);
                                configuredEnemy.SetCurve("Falloff Curve", ref enemy.numberSpawnedFalloff);
                                configuredEnemy.SetValue("Enemy HP", ref enemy.enemyPrefab.GetComponent<EnemyAI>().enemyHP);
                                configuredEnemy.SetValue("Can Die?", ref enemy.canDie);
                                configuredEnemy.SetValue("Destroy On Death?", ref enemy.destroyOnDeath);
                                configuredEnemy.SetValue("Can Destroy?", ref enemy.canBeDestroyed);
                                configuredEnemy.SetValue("Can Stun?", ref enemy.canBeStunned);
                                configuredEnemy.SetValue("Stun Difficulty", ref enemy.stunGameDifficultyMultiplier);
                                configuredEnemy.SetValue("Stun Time", ref enemy.stunTimeMultiplier);
                            }
                        }
                        catch (Exception e)
                        {
                            MiniLogger.LogError($"An error occured while setting enemy values, please report this!\n{e}");
                        }
                    }
                }

                if (centralConfig.GetValue<bool>("Configure Moons"))
                {
                    LunarConfigFile moonFile = lunarCentral.files[LunarConfig.MOON_FILE_NAME];

                    // LLL/Vanilla Moons
                    foreach (var extendedMoon in PatchedContent.ExtendedLevels)
                    {
                        try
                        {
                            SelectableLevel moon = extendedMoon.SelectableLevel;
                            LunarConfigEntry configuredMoon = moonFile.entries[lunarCentral.UUIDify($"LLL - {extendedMoon.NumberlessPlanetName} ({extendedMoon.UniqueIdentificationName})")];

                            if (configuredMoon.GetValue<bool>("Configure Content"))
                            {
                                configuredMoon.SetValue("Display Name", ref moon.PlanetName);
                                configuredMoon.SetValue("Risk Level", ref moon.riskLevel);
                                moon.LevelDescription = configuredMoon.GetValue<string>("Description").Replace(";", "\n");
                                extendedMoon.RoutePrice = configuredMoon.GetValue<int>("Route Price");
                                extendedMoon.IsRouteHidden = configuredMoon.GetValue<bool>("Is Hidden?");
                                extendedMoon.IsRouteLocked = configuredMoon.GetValue<bool>("Is Locked?");
                                configuredMoon.SetValue("Has Time?", ref moon.planetHasTime);
                                configuredMoon.SetValue("Time Multiplier", ref moon.DaySpeedMultiplier);
                                configuredMoon.SetValue("Daytime Probability Range", ref moon.daytimeEnemiesProbabilityRange);
                                configuredMoon.SetCurve("Daytime Curve", ref moon.daytimeEnemySpawnChanceThroughDay);
                                configuredMoon.SetValue("Max Daytime Power", ref moon.maxDaytimeEnemyPowerCount);
                                configuredMoon.SetEnemies("Spawnable Daytime Enemies", ref moon.DaytimeEnemies);
                                configuredMoon.SetValue("Interior Probability Range", ref moon.spawnProbabilityRange);
                                configuredMoon.SetCurve("Interior Curve", ref moon.enemySpawnChanceThroughoutDay);
                                configuredMoon.SetValue("Max Interior Power", ref moon.maxEnemyPowerCount);
                                configuredMoon.SetEnemies("Spawnable Interior Enemies", ref moon.Enemies);
                                configuredMoon.SetCurve("Outside Curve", ref moon.outsideEnemySpawnChanceThroughDay);
                                configuredMoon.SetValue("Max Outside Power", ref moon.maxOutsideEnemyPowerCount);
                                configuredMoon.SetEnemies("Spawnable Outside Enemies", ref moon.OutsideEnemies);
                                configuredMoon.SetValue("Min Scrap", ref moon.minScrap);
                                configuredMoon.SetValue("Max Scrap", ref moon.maxScrap);
                                configuredMoon.SetItems("Spawnable Scrap", ref moon.spawnableScrap);
                                configuredMoon.SetValue("Interior Multiplier", ref moon.factorySizeMultiplier);
                                configuredMoon.SetDungeons("Possible Interiors", extendedMoon);

                                extendedMoon.ContentTags.Clear();

                                foreach (var tag in configuredMoon.GetValue<string>("Tags").Split(','))
                                {
                                    string fixedTag = lunarCentral.UUIDify(tag).RemoveWhitespace();

                                    extendedMoon.ContentTags.Add(ContentTag.Create(fixedTag));
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            MiniLogger.LogError($"An error occured while setting moon values, please report this!\n{e}");
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

                

                /*
                NetworkManager manager = UnityEngine.Object.FindObjectOfType<NetworkManager>();
                
                CentralConfiguration central;

                if (File.Exists(LunarConfig.CENTRAL_FILE))
                {
                    central = new CentralConfiguration(File.ReadAllText(LunarConfig.CENTRAL_FILE));
                }
                else
                {
                    central = new CentralConfiguration();
                    central.CreateConfiguration(central);
                }
                
                HashSet<string> registeredItems = new HashSet<string>();
                ItemConfiguration itemConfig;
                Dictionary<string, ItemInfo> configuredItems = new Dictionary<string, ItemInfo>();

                MiniLogger.LogInfo("Beginning Logging...");
                
                if (File.Exists(LunarConfig.ITEM_FILE))
                {
                    itemConfig = new ItemConfiguration(File.ReadAllText(LunarConfig.ITEM_FILE));
                    foreach (ItemEntry entry in parseItemConfiguration.parseConfiguration(itemConfig.itemConfig))
                    {
                        try
                        {
                            ItemInfo item = parseItemEntry.parseEntry(entry.configString);
                            registeredItems.Add(item.itemID);
                            configuredItems.Add(item.itemID, item);
                        }
                        catch (Exception e)
                        {
                            MiniLogger.LogError($"Item Configuration File contains invalid entry, skipping entry!\n{e}");
                        }
                    }
                }
                else
                {
                    itemConfig = new ItemConfiguration("");
                }

                foreach (var item in registeredItems)
                {
                    MiniLogger.LogInfo($"Registered : {item}");
                }

                foreach (var item in Resources.FindObjectsOfTypeAll<Item>())
                {
                    MiniLogger.LogInfo($"{item.name} {item.GetHashCode()}");
                    if (item.spawnPrefab == null)
                    {
                        //Item is missing prefab!
                    }
                    else if (!manager.NetworkConfig.Prefabs.Contains(item.spawnPrefab))
                    {
                        //Item not a real item!
                    }
                    else if (configuredItems.Keys.Contains(item.name))
                    {
                        ItemInfo configuredItem = configuredItems[item.name];
                        item.itemName = configuredItem.displayName;
                        item.minValue = configuredItem.minValue;
                        item.maxValue = configuredItem.maxValue;
                        item.weight = configuredItem.weight;
                        item.isConductiveMetal = configuredItem.conductive;
                        item.twoHanded = configuredItem.twoHanded;
                        item.isScrap = configuredItem.isScrap;
                    }
                    else
                    {
                        itemConfig.AddEntry(new ItemEntry(new ItemInfo(item)));
                        MiniLogger.LogInfo($"Recorded {item.name}");
                        registeredItems.Add(item.name);
                    }
                }

                HashSet<string> registeredEnemies = new HashSet<string>();
                EnemyConfiguration enemyConfig;
                Dictionary<string, EnemyInfo> configuredEnemies = new Dictionary<string, EnemyInfo>();

                if (File.Exists(LunarConfig.ENEMY_FILE))
                {
                    enemyConfig = new EnemyConfiguration(File.ReadAllText(LunarConfig.ENEMY_FILE));
                    foreach (EnemyEntry entry in parseEnemyConfiguration.parseConfiguration(enemyConfig.enemyConfig))
                    {
                        try
                        {
                            EnemyInfo enemy = parseEnemyEntry.parseEntry(entry.configString);
                            registeredEnemies.Add(enemy.enemyID);
                            configuredEnemies.Add(enemy.enemyID, enemy);
                        }
                        catch (Exception e)
                        {
                            MiniLogger.LogError($"Enemy Configuration File contains invalid entry, skipping entry!\n{e}");
                        }
                    }
                }
                else
                {
                    enemyConfig = new EnemyConfiguration("");
                }

                foreach (var enemy in Resources.FindObjectsOfTypeAll<EnemyType>())
                {
                    if (enemy.enemyPrefab == null)
                    {
                        //Enemy is missing prefab!
                    }
                    else if (!manager.NetworkConfig.Prefabs.Contains(enemy.enemyPrefab))
                    {
                        //Enemy not a real enemy!
                    }
                    else if (registeredEnemies.Contains(enemy.name))
                    {
                        EnemyInfo configuredEnemy = configuredEnemies[enemy.name];
                        enemy.enemyName = configuredEnemy.displayName;
                        enemy.canSeeThroughFog = configuredEnemy.canSeeThroughFog;
                        enemy.doorSpeedMultiplier = configuredEnemy.doorSpeedMultiplier;
                        enemy.isDaytimeEnemy = configuredEnemy.isDaytimeEnemy;
                        enemy.isOutsideEnemy = configuredEnemy.isOutsideEnemy;
                        enemy.loudnessMultiplier = configuredEnemy.loudnessMultiplier;
                        enemy.MaxCount = configuredEnemy.maxCount;
                        enemy.PowerLevel = configuredEnemy.powerLevel;
                        enemy.probabilityCurve = configuredEnemy.probabilityCurve;
                        enemy.enemyPrefab.GetComponent<EnemyAI>().enemyHP = configuredEnemy.enemyHP;
                    }
                    else
                    {
                        enemyConfig.AddEntry(new EnemyEntry(new EnemyInfo(enemy)));
                        MiniLogger.LogInfo($"Recorded {enemy.name}");
                        registeredEnemies.Add(enemy.name);
                    }
                }
                
                HashSet<string> registeredMoons = new HashSet<string>();
                HashSet<string> registeredMapObjects = new HashSet<string>();
                //HashSet<string> registeredOutsideMapObjects = new HashSet<string>();
                MoonConfiguration moonConfig;
                MapObjectConfiguration mapObjectConfig;
                //OutsideMapObjectConfiguration outsideMapObjectConfig;
                Dictionary<string, MoonInfo> configuredMoons = new Dictionary<string, MoonInfo>();
                Dictionary<string, MapObjectInfo> configuredMapObjects = new Dictionary<string, MapObjectInfo>();
                //Dictionary<string, OutsideMapObjectInfo> configuredOutsideMapObjects = new Dictionary<string, OutsideMapObjectInfo>();

                if (File.Exists(LunarConfig.MOON_FILE))
                {
                    moonConfig = new MoonConfiguration(File.ReadAllText(LunarConfig.MOON_FILE));
                    foreach (MoonEntry entry in parseMoonConfiguration.parseConfiguration(moonConfig.moonConfig))
                    {
                        try
                        {
                            MoonInfo moon = parseMoonEntry.parseEntry(entry.configString);
                            registeredMoons.Add(moon.moonID);
                            configuredMoons.Add(moon.moonID, moon);
                        }
                        catch (Exception e)
                        {
                            MiniLogger.LogError($"Moon Configuration File contains invalid entry, skipping entry!\n{e}");
                        }
                    }
                }
                else
                {
                    moonConfig = new MoonConfiguration("");
                }

                if (File.Exists(LunarConfig.MAP_OBJECT_FILE))
                {
                    mapObjectConfig = new MapObjectConfiguration(File.ReadAllText(LunarConfig.MAP_OBJECT_FILE));
                    foreach (MapObjectEntry entry in parseMapObjectConfiguration.parseConfiguration(mapObjectConfig.mapObjectConfig))
                    {
                        try
                        {
                            MapObjectInfo mapObject = parseMapObjectEntry.parseEntry(entry.configString);
                            registeredMapObjects.Add(mapObject.objID);
                            configuredMapObjects.Add(mapObject.objID, mapObject);
                        }
                        catch (Exception e)
                        {
                            MiniLogger.LogError($"Map Object Configuration File contains invalid entry, skipping entry!\n{e}");
                        }
                    }
                }
                else
                {
                    mapObjectConfig = new MapObjectConfiguration("");
                }
                */
                /*

                if (File.Exists(LunarConfig.OUTSIDE_MAP_OBJECT_FILE))
                {
                    outsideMapObjectConfig = new OutsideMapObjectConfiguration(File.ReadAllText(LunarConfig.OUTSIDE_MAP_OBJECT_FILE));
                    foreach (OutsideMapObjectEntry entry in Objects.parseOutsideMapObjectConfiguration.parseConfiguration(outsideMapObjectConfig.outsideMapObjectConfig))
                    {
                        try
                        {
                            OutsideMapObjectInfo mapObject = Config_Entries.parseOutsideMapObjectEntry.parseEntry(entry.configString);
                            registeredOutsideMapObjects.Add(mapObject.objID);
                            configuredOutsideMapObjects.Add(mapObject.objID, mapObject);
                        }
                        catch (Exception e)
                        {
                            MiniLogger.LogError($"Outside Map Object Configuration File contains invalid entry, skipping entry!\n{e}");
                        }
                    }
                }
                else
                {
                    outsideMapObjectConfig = new OutsideMapObjectConfiguration("");
                }

                */
                /*
                foreach (var moon in Resources.FindObjectsOfTypeAll<SelectableLevel>())
                {
                    if (registeredMoons.Contains(moon.name))
                    {
                        // Moon exists in config
                        MoonInfo configuredMoon = configuredMoons[moon.name];
                        moon.PlanetName = configuredMoon.displayName;
                        moon.riskLevel = configuredMoon.risk;
                        moon.LevelDescription = configuredMoon.description;
                        /*
                        moon.planetHasTime = configuredMoon.hasTime;
                        moon.DaySpeedMultiplier = configuredMoon.timeMultiplier;
                        moon.daytimeEnemiesProbabilityRange = configuredMoon.daytimeProbabilityRange;
                        moon.daytimeEnemySpawnChanceThroughDay = configuredMoon.daytimeCurve;
                        moon.maxDaytimeEnemyPowerCount = configuredMoon.maxDaytimePower;
                        moon.spawnProbabilityRange = configuredMoon.interiorProbabilityRange;
                        moon.enemySpawnChanceThroughoutDay = configuredMoon.interiorCurve;
                        moon.maxEnemyPowerCount = configuredMoon.maxInteriorPower;
                        moon.outsideEnemySpawnChanceThroughDay = configuredMoon.outsideCurve;
                        moon.maxOutsideEnemyPowerCount = configuredMoon.maxOutsidePower;
                        moon.minScrap = configuredMoon.minScrap;
                        moon.maxScrap = configuredMoon.maxScrap;
                        moon.factorySizeMultiplier = configuredMoon.interiorSizeMultiplier;
                        */
                /*
                    }
                    else
                    {
                        moonConfig.AddEntry(new MoonEntry(new MoonInfo(moon)));
                        MiniLogger.LogInfo($"Recorded {moon.name}");
                        registeredMoons.Add(moon.name);
                    }

                    foreach (SpawnableMapObject spawnableObject in moon.spawnableMapObjects)
                    {
                        GameObject prefab = spawnableObject.prefabToSpawn;
                        if (!manager.NetworkConfig.Prefabs.Contains(prefab))
                        {
                            // No object networked
                        }
                        else if (configuredMapObjects.Keys.Contains(prefab.name))
                        {
                            MapObjectInfo configuredMapObject = configuredMapObjects[prefab.name];
                            spawnableObject.spawnFacingAwayFromWall = configuredMapObject.faceAwayWall;
                            spawnableObject.spawnFacingWall = configuredMapObject.faceWall;
                            spawnableObject.disallowSpawningNearEntrances = configuredMapObject.disallowNearEntrance;
                            spawnableObject.requireDistanceBetweenSpawns = configuredMapObject.requireDistanceBetweenSpawns;
                            spawnableObject.spawnWithBackFlushAgainstWall = configuredMapObject.spawnFlushAgainstWall;
                            spawnableObject.spawnWithBackToWall = configuredMapObject.spawnAgainstWall;
                        }
                        else if (!registeredMapObjects.Contains(prefab.name))
                        {
                            registeredMapObjects.Add(prefab.name);
                            MiniLogger.LogInfo($"Recorded {prefab.name}");
                            mapObjectConfig.AddEntry(new MapObjectEntry(new MapObjectInfo(spawnableObject)));
                        }
                    }
                    */
                /*

                foreach (SpawnableOutsideObjectWithRarity spawnableObject in moon.spawnableOutsideObjects)
                {
                    GameObject prefab = spawnableObject.spawnableObject.prefabToSpawn;
                    if (configuredOutsideMapObjects.Keys.Contains(spawnableObject.spawnableObject.name))
                    {
                        OutsideMapObjectInfo configuredMapObject = configuredOutsideMapObjects[spawnableObject.spawnableObject.name];
                        spawnableObject.spawnableObject.objectWidth = configuredMapObject.objWidth;
                        spawnableObject.spawnableObject.spawnFacingAwayFromWall = configuredMapObject.faceAwayWall;
                    }
                    else if (!registeredOutsideMapObjects.Contains(spawnableObject.spawnableObject.name))
                    {
                        registeredOutsideMapObjects.Add(spawnableObject.spawnableObject.name);
                        MiniLogger.LogInfo($"Recorded {spawnableObject.spawnableObject.name}");
                        outsideMapObjectConfig.AddEntry(new OutsideMapObjectEntry(new OutsideMapObjectInfo(spawnableObject)));
                    }
                }

                */
                /*
            }



            HashSet<string> registeredDungeons = new HashSet<string>();
            DungeonConfiguration dungeonConfig;
            Dictionary<string, DungeonInfo> configuredDungeons = new Dictionary<string, DungeonInfo>();

            if (File.Exists(LunarConfig.DUNGEON_FILE))
            {
                dungeonConfig = new DungeonConfiguration(File.ReadAllText(LunarConfig.DUNGEON_FILE));
                string newDungeonConfigString = "";
                foreach (DungeonEntry entry in parseDungeonConfiguration.parseConfiguration(dungeonConfig.dungeonConfig))
                {
                    try
                    {
                        DungeonInfo dungeon = parseDungeonEntry.parseEntry(entry.configString);
                        List<string> mapObjects = dungeon.mapObjectMultipliers.Keys.ToList();

                        foreach (var obj in registeredMapObjects)
                        {
                            if (mapObjects.Contains(obj))
                            {
                                continue;
                            }
                            else
                            {
                                dungeon.mapObjectMultipliers.Add(obj, 1);
                            }
                        }

                        newDungeonConfigString += new DungeonEntry(dungeon).configString;
                        registeredDungeons.Add(dungeon.dungeonID);
                        configuredDungeons.Add(dungeon.dungeonID, dungeon);
                    }
                    catch (Exception e)
                    {
                        MiniLogger.LogError($"Dungeon Configuration File contains invalid entry, skipping entry!\n{e}");
                    }
                }

                if (newDungeonConfigString != dungeonConfig.dungeonConfig)
                {
                    dungeonConfig.dungeonConfig = newDungeonConfigString;
                    MiniLogger.LogInfo("Dungeon Configuration Updated!");
                }
            }
            else
            {
                dungeonConfig = new DungeonConfiguration("");
            }

            foreach (var dungeon in Resources.FindObjectsOfTypeAll<DungeonFlow>())
            {
                if (registeredDungeons.Contains(dungeon.name))
                {
                    // Dungeon already recorded
                }
                else
                {
                    dungeonConfig.AddEntry(new DungeonEntry(new DungeonInfo(dungeon, registeredMapObjects)));
                    MiniLogger.LogInfo($"Recorded {dungeon.name}");
                    registeredDungeons.Add(dungeon.name);
                }
            }

            HashSet<string> registeredTags = new HashSet<string>();
            TagConfiguration tagConfig;
            List<TagInfo> configuredTags = new List<TagInfo>();

            if (File.Exists(LunarConfig.TAG_FILE))
            {
                tagConfig = new TagConfiguration(File.ReadAllText(LunarConfig.TAG_FILE));
                string newTagConfigString = "";
                foreach (TagEntry entry in parseTagConfiguration.parseConfiguration(tagConfig.tagConfig))
                {
                    try
                    {
                        TagInfo tag = parseTagEntry.parseEntry(entry.configString);
                        //List<string> mapObjects = tag.mapObjectMultipliers.Keys.ToList();
                        List<string> itemPools = tag.itemPoolMultipliers.Keys.ToList();
                        List<string> interiorEnemyPools = tag.interiorEnemyPoolMultipliers.Keys.ToList();
                        List<string> exteriorEnemyPools = tag.exteriorEnemyPoolMultipliers.Keys.ToList();
                        List<string> daytimeEnemyPools = tag.daytimeEnemyPoolMultipliers.Keys.ToList();
                        List<string> dungeons = tag.dungeonMultipliers.Keys.ToList();
                        */
                /*
                foreach (var obj in registeredMapObjects)
                {
                    if (mapObjects.Contains(obj))
                    {
                        continue;
                    }
                    else
                    {
                        tag.mapObjectMultipliers.Add(obj, 1);
                    }
                }
                */
                /*
                foreach (var obj in central.itemPools)
                {
                    if (itemPools.Contains(obj))
                    {
                        continue;
                    }
                    else
                    {
                        tag.itemPoolMultipliers.Add(obj, 1);
                    }
                }

                foreach (var obj in central.interiorEnemyPools)
                {
                    if (interiorEnemyPools.Contains(obj))
                    {
                        continue;
                    }
                    else
                    {
                        tag.interiorEnemyPoolMultipliers.Add(obj, 1);
                    }
                }

                foreach (var obj in central.exteriorEnemyPools)
                {
                    if (exteriorEnemyPools.Contains(obj))
                    {
                        continue;
                    }
                    else
                    {
                        tag.exteriorEnemyPoolMultipliers.Add(obj, 1);
                    }
                }

                foreach (var obj in central.daytimeEnemyPools)
                {
                    if (daytimeEnemyPools.Contains(obj))
                    {
                        continue;
                    }
                    else
                    {
                        tag.daytimeEnemyPoolMultipliers.Add(obj, 1);
                    }
                }

                foreach (var obj in registeredDungeons)
                {
                    if (dungeons.Contains(obj))
                    {
                        continue;
                    }
                    else
                    {
                        tag.dungeonMultipliers.Add(obj, 1f);
                    }
                }

                newTagConfigString += new TagEntry(tag).configString;
                registeredTags.Add(tag.tagID);
                configuredTags.Add(tag);
            }
            catch (Exception e)
            {
                MiniLogger.LogError($"Tag Configuration File contains invalid entry, skipping entry!\n{e}");
            }
        }

        if (newTagConfigString != tagConfig.tagConfig) 
        {
            tagConfig.tagConfig = newTagConfigString;
            MiniLogger.LogInfo("Tag Configuration Updated!");
        }
    }
    else
    {
        tagConfig = new TagConfiguration("");
    }

    foreach (var tag in central.tags)
    {
        if (registeredTags.Contains(tag))
        {
            // Tag already recorded
        }
        else
        {
            TagEntry tagEntry = new TagEntry(new TagInfo(
                tag, 
                1, 
                //registeredMapObjects.ToDictionary(k => k, v => 1f),
                central.itemPools.ToDictionary(k => k, v => 1f),
                central.interiorEnemyPools.ToDictionary(k => k, v => 1f),
                central.exteriorEnemyPools.ToDictionary(k => k, v => 1f),
                central.daytimeEnemyPools.ToDictionary(k => k, v => 1f),
                registeredDungeons.ToDictionary(k => k, v => 1f)
                ));
            tagConfig.AddEntry(tagEntry);
            MiniLogger.LogInfo($"Recorded {tag}");
            registeredTags.Add(tag);
        }
    }

    Directory.CreateDirectory(Path.GetDirectoryName(LunarConfig.ITEM_FILE)!);
    File.WriteAllText(LunarConfig.ITEM_FILE, itemConfig.itemConfig);
    File.WriteAllText(LunarConfig.ENEMY_FILE, enemyConfig.enemyConfig);
    File.WriteAllText(LunarConfig.MOON_FILE, moonConfig.moonConfig);
    File.WriteAllText(LunarConfig.MAP_OBJECT_FILE, mapObjectConfig.mapObjectConfig);
    //File.WriteAllText(LunarConfig.OUTSIDE_MAP_OBJECT_FILE, outsideMapObjectConfig.outsideMapObjectConfig);
    File.WriteAllText(LunarConfig.DUNGEON_FILE, dungeonConfig.dungeonConfig);
    File.WriteAllText(LunarConfig.TAG_FILE, tagConfig.tagConfig);
    File.WriteAllText(LunarConfig.CENTRAL_FILE, central.CreateConfiguration(central));

    MiniLogger.LogInfo("Logging complete!");
    */
            }
            catch (Exception e) 
            {
                MiniLogger.LogError($"An error occured, please report this!\n{e}");
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

                ExtendedLevel level = null;

                foreach (var lev in PatchedContent.ExtendedLevels)
                {
                    if (lev.IsCurrentLevel)
                    {
                        foreach (var obj in lev.SelectableLevel.spawnableOutsideObjects)
                        {
                            MiniLogger.LogInfo(obj.spawnableObject.name);
                        }
                        level = lev;
                        break;
                    }
                }

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
