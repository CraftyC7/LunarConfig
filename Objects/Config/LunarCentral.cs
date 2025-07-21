using BepInEx;
using BepInEx.Configuration;
using CodeRebirthLib;
using CodeRebirthLib.ContentManagement.Enemies;
using CodeRebirthLib.ContentManagement.Items;
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

namespace LunarConfig.Objects.Config
{
    public class LunarCentral
    {
        public Dictionary<string, LunarConfigFile> files = new Dictionary<string, LunarConfigFile>();

        public HashSet<string> foundTags = new HashSet<string>();
        public bool useLLLTags = false;

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

        public void InitConfig()
        {
            InitCentral();

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

            centralFile.file.Save();
            centralFile.file.SaveOnConfigSet = true;
        }

        public void InitItems()
        {
            LunarConfigFile itemFile = AddFile(LunarConfig.ITEM_FILE, LunarConfig.ITEM_FILE_NAME);
            itemFile.file.SaveOnConfigSet = false;

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
                    itemEntry.AddField("Display Name", "Specifies the name that appears when scanning the item.", itemObj.itemName);
                    itemEntry.AddField("Minimum Value", "The minimum scrap value and item can have.\nTypically multiplied by 0.4, setting not applicable to non-scrap.\nDoes not work on items like Apparatus and items from enemies (Hives, Double-barrel).", itemObj.minValue);
                    itemEntry.AddField("Maximum Value", "The maximum scrap value and item can have.\nTypically multiplied by 0.4, setting not applicable to non-scrap.\nDoes not work on items like Apparatus and items from enemies (Hives, Double-barrel).", itemObj.maxValue);
                    itemEntry.AddField("Weight", "Specifies the weight of an item.\nCalculated with: (x - 1) * 105 = weight in pounds.", itemObj.weight);
                    itemEntry.AddField("Conductivity", "Specifies whether an item is conductive.", itemObj.isConductiveMetal);
                    itemEntry.AddField("Two-Handed", "Specifies whether an item is two-handed.", itemObj.twoHanded);
                    itemEntry.AddField("Is Scrap?", "Specifies if an item is scrap or gear.\nThis decides whether an item can be sold to the company for credits.", itemObj.isScrap);
                    itemEntry.AddField("(Lunar Config) Tags", "Tags allocated to an item.\nSeparate tags with commas.", "");
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
                    itemEntry.AddField("Display Name", "Specifies the name that appears when scanning the item.", itemObj.itemName);
                    itemEntry.AddField("Minimum Value", "The minimum scrap value and item can have.\nTypically multiplied by 0.4, setting not applicable to non-scrap.\nDoes not work on items like Apparatus and items from enemies (Hives, Double-barrel).", itemObj.minValue);
                    itemEntry.AddField("Maximum Value", "The maximum scrap value and item can have.\nTypically multiplied by 0.4, setting not applicable to non-scrap.\nDoes not work on items like Apparatus and items from enemies (Hives, Double-barrel).", itemObj.maxValue);
                    itemEntry.AddField("Weight", "Specifies the weight of an item.\nCalculated with: (x - 1) * 105 = weight in pounds.", itemObj.weight);
                    itemEntry.AddField("Conductivity", "Specifies whether an item is conductive.", itemObj.isConductiveMetal);
                    itemEntry.AddField("Two-Handed", "Specifies whether an item is two-handed.", itemObj.twoHanded);
                    itemEntry.AddField("Is Scrap?", "Specifies if an item is scrap or gear.\nThis decides whether an item can be sold to the company for credits.", itemObj.isScrap);
                    itemEntry.AddField("(Lunar Config) Tags", "Tags allocated to an item.\nSeparate tags with commas.", "");
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
                    itemEntry.AddField("Display Name", "Specifies the name that appears when scanning the item.", itemObj.itemName);
                    itemEntry.AddField("Minimum Value", "The minimum scrap value and item can have.\nTypically multiplied by 0.4, setting not applicable to non-scrap.\nDoes not work on items like Apparatus and items from enemies (Hives, Double-barrel).", itemObj.minValue);
                    itemEntry.AddField("Maximum Value", "The maximum scrap value and item can have.\nTypically multiplied by 0.4, setting not applicable to non-scrap.\nDoes not work on items like Apparatus and items from enemies (Hives, Double-barrel).", itemObj.maxValue);
                    itemEntry.AddField("Weight", "Specifies the weight of an item.\nCalculated with: (x - 1) * 105 = weight in pounds.", itemObj.weight);
                    itemEntry.AddField("Conductivity", "Specifies whether an item is conductive.", itemObj.isConductiveMetal);
                    itemEntry.AddField("Two-Handed", "Specifies whether an item is two-handed.", itemObj.twoHanded);
                    itemEntry.AddField("Is Scrap?", "Specifies if an item is scrap or gear.\nThis decides whether an item can be sold to the company for credits.", itemObj.isScrap);
                    itemEntry.AddField("(Lunar Config) Tags", "Tags allocated to an item.\nSeparate tags with commas.", "");
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
                    itemEntry.AddField("Display Name", "Specifies the name that appears when scanning the item.", itemObj.itemName);
                    itemEntry.AddField("Minimum Value", "The minimum scrap value and item can have.\nTypically multiplied by 0.4, setting not applicable to non-scrap.\nDoes not work on items like Apparatus and items from enemies (Hives, Double-barrel).", itemObj.minValue);
                    itemEntry.AddField("Maximum Value", "The maximum scrap value and item can have.\nTypically multiplied by 0.4, setting not applicable to non-scrap.\nDoes not work on items like Apparatus and items from enemies (Hives, Double-barrel).", itemObj.maxValue);
                    itemEntry.AddField("Weight", "Specifies the weight of an item.\nCalculated with: (x - 1) * 105 = weight in pounds.", itemObj.weight);
                    itemEntry.AddField("Conductivity", "Specifies whether an item is conductive.", itemObj.isConductiveMetal);
                    itemEntry.AddField("Two-Handed", "Specifies whether an item is two-handed.", itemObj.twoHanded);
                    itemEntry.AddField("Is Scrap?", "Specifies if an item is scrap or gear.\nThis decides whether an item can be sold to the company for credits.", itemObj.isScrap);
                    itemEntry.AddField("(Lunar Config) Tags", "Tags allocated to an item.\nSeparate tags with commas.", "");
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
                    enemyEntry.AddField("Configure Content", "Enable to change any of the settings below.", false);
                    enemyEntry.AddField("Display Name", "Specifies the name of the enemy.", enemy.EnemyDisplayName);
                    enemyEntry.AddField("Can See Through Fog?", "Specifies if an enemy can see through fog in foggy weather.", enemyObj.canSeeThroughFog);
                    enemyEntry.AddField("Door Speed Multiplier", "Decides the speed at which enemies can open doors.\nCalculated with: 1 / x = time to open door in seconds.", enemyObj.doorSpeedMultiplier);
                    enemyEntry.AddField("Is Daytime Enemy?", "Whether an enemy is a daytime enemy.", enemyObj.isDaytimeEnemy);
                    enemyEntry.AddField("Is Outdoor Enemy?", "Whether an enemy is a outdoor enemy.", enemyObj.isOutsideEnemy);
                    enemyEntry.AddField("Loudness Multiplier", "Multiplies the volume of an enemy's sounds.", enemyObj.loudnessMultiplier);
                    enemyEntry.AddField("Max Count", "The maximum amount of an enemy that can be alive.", enemyObj.MaxCount);
                    enemyEntry.AddField("Power Level", "The power level an enemy occupies.", enemyObj.PowerLevel);
                    enemyEntry.AddField("Probability Curve", "Multiplies enemy spawn weight depending on time of day.\nKeyframes represented by x,y and separated by semicolons.", CurveToString(enemyObj.probabilityCurve));
                    enemyEntry.AddField("Use Falloff?", "Whether or not to use the falloff curve.", enemyObj.useNumberSpawnedFalloff);
                    enemyEntry.AddField("Falloff Curve", "Multiplier to enemy spawn weight depending on how many are already spawned.\nKeyframes represented by x,y and separated by semicolons.", CurveToString(enemyObj.numberSpawnedFalloff));
                    enemyEntry.AddField("Enemy HP", "The amount of HP an enemy has.", enemyObj.enemyPrefab.GetComponent<EnemyAI>().enemyHP);
                    enemyEntry.AddField("Can Die?", "Whether or not an enemy can die.", enemyObj.canDie);
                    enemyEntry.AddField("Destroy On Death?", "Whether or not an enemy is destroyed on death.", enemyObj.destroyOnDeath);
                    enemyEntry.AddField("Can Destroy?", "Whether or not an enemy can be destroyed.", enemyObj.canBeDestroyed);
                    enemyEntry.AddField("Can Stun?", "Whether or not an enemy can be stunned.", enemyObj.canBeStunned);
                    enemyEntry.AddField("Stun Difficulty", "I don't really know.", enemyObj.stunGameDifficultyMultiplier);
                    enemyEntry.AddField("Stun Time", "I don't really know.", enemyObj.stunTimeMultiplier);
                    enemyEntry.AddField("(Lunar Config) Daytime Tags", "Tags allocated to the enemy spawning as a daytime enemy.\nSeparate tags with commas.", "");
                    enemyEntry.AddField("(Lunar Config) Outside Tags", "Tags allocated to the enemy spawning as an outside enemy.\nSeparate tags with commas.", "");
                    enemyEntry.AddField("(Lunar Config) Interior Tags", "Tags allocated to the enemy spawning as an interior enemy.\nSeparate tags with commas.", "");
                    enemyEntry.AddField("(Lunar Config) Blacklist Daytime Tags", "Tags the enemy is blacklisted from.\nSeparate tags with commas.", "");
                    enemyEntry.AddField("(Lunar Config) Blacklist Outside Tags", "Tags the enemy is blacklisted from.\nSeparate tags with commas.", "");
                    enemyEntry.AddField("(Lunar Config) Blacklist Interior Tags", "Tags the enemy is blacklisted from.\nSeparate tags with commas.", "");
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
                    enemyEntry.AddField("Configure Content", "Enable to change any of the settings below.", false);
                    enemyEntry.AddField("Display Name", "Specifies the name of the enemy.", enemyObj.enemyName);
                    enemyEntry.AddField("Can See Through Fog?", "Specifies if an enemy can see through fog in foggy weather.", enemyObj.canSeeThroughFog);
                    enemyEntry.AddField("Door Speed Multiplier", "Decides the speed at which enemies can open doors.\nCalculated with: 1 / x = time to open door in seconds.", enemyObj.doorSpeedMultiplier);
                    enemyEntry.AddField("Is Daytime Enemy?", "Whether an enemy is a daytime enemy.", enemyObj.isDaytimeEnemy);
                    enemyEntry.AddField("Is Outdoor Enemy?", "Whether an enemy is a outdoor enemy.", enemyObj.isOutsideEnemy);
                    enemyEntry.AddField("Loudness Multiplier", "Multiplies the volume of an enemy's sounds.", enemyObj.loudnessMultiplier);
                    enemyEntry.AddField("Max Count", "The maximum amount of an enemy that can be alive.", enemyObj.MaxCount);
                    enemyEntry.AddField("Power Level", "The power level an enemy occupies.", enemyObj.PowerLevel);
                    enemyEntry.AddField("Probability Curve", "Multiplies enemy spawn weight depending on time of day.\nKeyframes represented by x,y and separated by semicolons.", CurveToString(enemyObj.probabilityCurve));
                    enemyEntry.AddField("Use Falloff?", "Whether or not to use the falloff curve.", enemyObj.useNumberSpawnedFalloff);
                    enemyEntry.AddField("Falloff Curve", "Multiplier to enemy spawn weight depending on how many are already spawned.\nKeyframes represented by x,y and separated by semicolons.", CurveToString(enemyObj.numberSpawnedFalloff));
                    enemyEntry.AddField("Enemy HP", "The amount of HP an enemy has.", enemyObj.enemyPrefab.GetComponent<EnemyAI>().enemyHP);
                    enemyEntry.AddField("Can Die?", "Whether or not an enemy can die.", enemyObj.canDie);
                    enemyEntry.AddField("Destroy On Death?", "Whether or not an enemy is destroyed on death.", enemyObj.destroyOnDeath);
                    enemyEntry.AddField("Can Destroy?", "Whether or not an enemy can be destroyed.", enemyObj.canBeDestroyed);
                    enemyEntry.AddField("Can Stun?", "Whether or not an enemy can be stunned.", enemyObj.canBeStunned);
                    enemyEntry.AddField("Stun Difficulty", "I don't really know.", enemyObj.stunGameDifficultyMultiplier);
                    enemyEntry.AddField("Stun Time", "I don't really know.", enemyObj.stunTimeMultiplier);
                    enemyEntry.AddField("(Lunar Config) Daytime Tags", "Tags allocated to the enemy spawning as a daytime enemy.\nSeparate tags with commas.", "");
                    enemyEntry.AddField("(Lunar Config) Outside Tags", "Tags allocated to the enemy spawning as an outside enemy.\nSeparate tags with commas.", "");
                    enemyEntry.AddField("(Lunar Config) Interior Tags", "Tags allocated to the enemy spawning as an interior enemy.\nSeparate tags with commas.", "");
                    enemyEntry.AddField("(Lunar Config) Blacklist Daytime Tags", "Tags the enemy is blacklisted from.\nSeparate tags with commas.", "");
                    enemyEntry.AddField("(Lunar Config) Blacklist Outside Tags", "Tags the enemy is blacklisted from.\nSeparate tags with commas.", "");
                    enemyEntry.AddField("(Lunar Config) Blacklist Interior Tags", "Tags the enemy is blacklisted from.\nSeparate tags with commas.", "");
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

                    moonEntry.AddField("Display Name", "Changes the name of the moon.\nDoes not modify terminal commands/nodes.", moonObj.PlanetName);
                    moonEntry.AddField("Risk Level", "Changes the risk level of the moon.\nThis setting is only cosmetic.", moonObj.riskLevel);
                    moonEntry.AddField("Description", "The description given to the moon.\nNew lines are represented by semi-colons.\nDoes not modify terminal commands/nodes.", moonObj.LevelDescription.Replace("\n", ";"));
                    
                    moonEntry.AddField("Route Price", "Changes the price to route to the moon.", moon.RoutePrice);
                    moonEntry.AddField("Is Hidden?", "Changes if the moon is hidden in the terminal.", moon.IsRouteHidden);
                    moonEntry.AddField("Is Locked?", "Changes if the moon is locked in the terminal.", moon.IsRouteLocked);

                    moonEntry.AddField("Has Time?", "Defines whether a moon has time.", moonObj.planetHasTime);
                    moonEntry.AddField("Time Multiplier", "Multiplies the speed at which time progresses on a moon.", moonObj.DaySpeedMultiplier);

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
                    moonEntry.AddField("Daytime Curve", "Decides the amount of daytime enemies that spawn as the day progresses.\nKeyframes represented by x,y and separated by semicolons.", CurveToString(moonObj.daytimeEnemySpawnChanceThroughDay));
                    moonEntry.AddField("Max Daytime Power", "The amount of daytime power capacity that a moon has.", moonObj.maxDaytimeEnemyPowerCount);
                    moonEntry.AddField("Spawnable Daytime Enemies", "The base daytime enemies that can spawn on the moon.\nDenoted with NAME:RARITY, separated with commas.", defaultDaytimeEnemies);

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
                    moonEntry.AddField("Interior Curve", "Decides the amount of interior enemies that spawn as the day progresses.\nKeyframes represented by x,y and separated by semicolons.", CurveToString(moonObj.enemySpawnChanceThroughoutDay));
                    moonEntry.AddField("Max Interior Power", "The amount of interior power capacity that a moon has.", moonObj.maxEnemyPowerCount);
                    moonEntry.AddField("Spawnable Interior Enemies", "The base interior enemies that can spawn on the moon.\nDenoted with NAME:RARITY, separated with commas.", defaultInsideEnemies);

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
                    moonEntry.AddField("Max Outside Power", "The amount of outside power capacity that a moon has.", moonObj.maxOutsideEnemyPowerCount);
                    moonEntry.AddField("Spawnable Outside Enemies", "The base outside enemies that can spawn on the moon.\nDenoted with NAME:RARITY, separated with commas.", defaultOutsideEnemies);

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
                    moonEntry.AddField("Max Scrap", "The maximum amount of scrap items that can spawn on a moon.", moonObj.maxScrap);
                    moonEntry.AddField("Value Multiplier", "The multiplier applied to the value of a moon's scrap.", 0.4f);
                    moonEntry.AddField("Amount Multiplier", "The multiplier applied to the amount of scrap a moon has.", 1f);
                    moonEntry.AddField("Spawnable Scrap", "The base scrap that can spawn on the moon.\nDenoted with NAME:RARITY, separated with commas.", defaultScrap);

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
                    moonEntry.AddField("Possible Interiors", "The base interiors that can spawn on the moon.\nDenoted with NAME:RARITY, separated with commas.", defaultDungeons);

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
                    dungeonEntry.AddField("(Lunar Config) Tags", "Tags allocated to a dungeon.\nSeparate tags with commas.", "");
                    MiniLogger.LogInfo($"Recorded {dungeon.name}");
                    registeredDungeons.Add(dungeonUUID);
                }
            }

            // LL/CRLib Content
            foreach (var dungeon in Dungeon.customDungeons)
            {
                string dungeonUUID = UUIDify($"LL - {dungeon.dungeonFlow.name}");
                if (!registeredDungeons.Contains(dungeonUUID))
                {
                    DungeonFlow dungeonObj = dungeon.dungeonFlow;
                    LunarConfigEntry dungeonEntry = dungeonFile.AddEntry(dungeonUUID);
                    MiniLogger.LogInfo($"Recording {dungeonObj.name}...");
                    dungeonEntry.AddField("Configure Content", "Enable to change any of the settings below.", false);
                    dungeonEntry.AddField("(Lunar Config) Tags", "Tags allocated to a dungeon.\nSeparate tags with commas.", "");
                    MiniLogger.LogInfo($"Recorded {dungeonObj.name}");
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
