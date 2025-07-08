using BepInEx.Configuration;
using LethalLevelLoader;
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

        public LunarCentral() { }

        public string UUIDify(string uuid)
        {
            return uuid.Replace("=", "").Replace("\n", "").Replace("\t", "").Replace("\\", "").Replace("\"", "").Replace("\'", "").Replace("[", "").Replace("]", "");
        }

        public string CurveToString(AnimationCurve curve)
        {
            return string.Join(";", curve.keys.Select(kf => $"{kf.time},{kf.value}"));
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
            InitItems();
            InitEnemies();
            InitMoons();
        }

        public void InitItems()
        {
            LunarConfigFile itemFile = AddFile(LunarConfig.ITEM_FILE, LunarConfig.ITEM_FILE_NAME);
            itemFile.file.SaveOnConfigSet = false;

            HashSet<string> registeredItems = new HashSet<string>();
            
            foreach (var item in PatchedContent.ExtendedItems)
            {
                string itemUUID = UUIDify($"{item.Item.itemName} ({item.UniqueIdentificationName})");
                if (!registeredItems.Contains(itemUUID))
                {
                    Item itemObj = item.Item;
                    LunarConfigEntry itemEntry = itemFile.AddEntry(itemUUID);
                    MiniLogger.LogInfo($"Recording {item.name}...");
                    itemEntry.AddField("Display Name", "Specifies the name that appears when scanning the item.", itemObj.itemName);
                    itemEntry.AddField("Minimum Value", "The minimum scrap value and item can have.\nTypically multiplied by 0.4, setting not applicable to non-scrap.\nDoes not work on items like Apparatus and items from enemies (Hives, Double-barrel).", itemObj.minValue);
                    itemEntry.AddField("Maximum Value", "The maximum scrap value and item can have.\nTypically multiplied by 0.4, setting not applicable to non-scrap.\nDoes not work on items like Apparatus and items from enemies (Hives, Double-barrel).", itemObj.maxValue);
                    itemEntry.AddField("Weight", "Specifies the weight of an item.\nCalculated with: (x - 1) * 105 = weight in pounds.", itemObj.weight);
                    itemEntry.AddField("Conductivity", "Specifies whether an item is conductive.", itemObj.isConductiveMetal);
                    itemEntry.AddField("Two-Handed", "Specifies whether an item is two-handed.", itemObj.twoHanded);
                    itemEntry.AddField("Is Scrap?", "Specifies if an item is scrap or gear.\nThis decides whether an item can be sold to the company for credits.", itemObj.isScrap);
                    itemEntry.AddField("(Lunar Config) Tags", "Tags allocated to an item\nSeparate tags with commas.", "");
                    MiniLogger.LogInfo($"Recorded {item.name}");
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

            foreach (var enemy in PatchedContent.ExtendedEnemyTypes)
            {
                string enemyUUID = UUIDify($"{enemy.EnemyType.enemyName} ({enemy.UniqueIdentificationName})");
                if (!registeredEnemies.Contains(enemyUUID))
                {
                    EnemyType enemyObj = enemy.EnemyType;
                    LunarConfigEntry enemyEntry = enemyFile.AddEntry(enemyUUID);
                    MiniLogger.LogInfo($"Recording {enemy.name}...");
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
                    MiniLogger.LogInfo($"Recorded {enemy.name}");
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

            foreach (var moon in PatchedContent.ExtendedLevels)
            {
                string moonUUID = UUIDify($"{moon.SelectableLevel.PlanetName} ({moon.UniqueIdentificationName})");
                if (!registeredMoons.Contains(moonUUID))
                {
                    SelectableLevel moonObj = moon.SelectableLevel;
                    LunarConfigEntry moonEntry = moonFile.AddEntry(moonUUID);
                    MiniLogger.LogInfo($"Recording {moon.name}...");
                    moonEntry.AddField("Display Name", "Changes the name of the moon.\nDoes not modify terminal commands/nodes.", moonObj.PlanetName);
                    moonEntry.AddField("Risk Level", "Changes the risk level of the moon.\nThis setting is only cosmetic.", moonObj.riskLevel);
                    moonEntry.AddField("Description", "The description given to the moon.\nNew lines are represented by semi-colons.\nDoes not modify terminal commands/nodes.", moonObj.LevelDescription.Replace("\n", ";"));
                    moonEntry.AddField("Route Price", "Changes the price to route to the moon.", moon.RoutePrice);
                    moonEntry.AddField("Is Hidden?", "Changes if the moon is hidden in the terminal.", moon.IsRouteHidden);
                    moonEntry.AddField("Is Locked?", "Changes if the moon is locked in the terminal.", moon.IsRouteLocked);
                    moonEntry.AddField("Has Time?", "Defines whether a moon has time.", moonObj.planetHasTime);
                    moonEntry.AddField("Time Multiplier", "Multiplies the speed at which time progresses on a moon.", moonObj.DaySpeedMultiplier);
                    moonEntry.AddField("Daytime Probability Range", "The amount of daytime enemies spawned that can differ from the curve.\nFor instance, if this value is 3, and at the current time and spawn cycle 2 daytime enemies should spawn, anywhere between 0 and 5 can spawn.", moonObj.daytimeEnemiesProbabilityRange);
                    moonEntry.AddField("Daytime Curve", "Decides the amount of daytime enemies that spawn as the day progresses.\nKeyframes represented by x,y and separated by semicolons.", CurveToString(moonObj.daytimeEnemySpawnChanceThroughDay));
                    moonEntry.AddField("Max Daytime Power", "The amount of daytime power capacity that a moon has.", moonObj.maxDaytimeEnemyPowerCount);
                    moonEntry.AddField("Interior Probability Range", "The amount of interior enemies spawned that can differ from the curve.\nFor instance, if this value is 3, and at the current time and spawn cycle 2 interior enemies should spawn, anywhere between 0 and 5 can spawn.", moonObj.spawnProbabilityRange);
                    moonEntry.AddField("Interior Curve", "Decides the amount of interior enemies that spawn as the day progresses.\nKeyframes represented by x,y and separated by semicolons.", CurveToString(moonObj.enemySpawnChanceThroughoutDay));
                    moonEntry.AddField("Max Interior Power", "The amount of interior power capacity that a moon has.", moonObj.maxEnemyPowerCount);
                    moonEntry.AddField("Outside Curve", "Decides the amount of outside enemies that spawn as the day progresses.\nKeyframes represented by x,y and separated by semicolons.", CurveToString(moonObj.outsideEnemySpawnChanceThroughDay));
                    moonEntry.AddField("Max Outside Power", "The amount of outside power capacity that a moon has.", moonObj.maxOutsideEnemyPowerCount);
                    moonEntry.AddField("Min Scrap", "The minimum amount of scrap items that can spawn on a moon.", moonObj.minScrap);
                    moonEntry.AddField("Max Scrap", "The maximum amount of scrap items that can spawn on a moon.", moonObj.maxScrap);
                    moonEntry.AddField("Value Multiplier", "The multiplier applied to the value of a moon's scrap.", 0.4f);
                    moonEntry.AddField("Amount Multiplier", "The multiplier applied to the amount of scrap a moon has.", 1f);
                    moonEntry.AddField("Interior Multiplier", "Changes the size of the interior generated.", moonObj.factorySizeMultiplier);
                    moonEntry.AddField("Tags", "Tags allocated to the moon.\nSeparate tags with commas.", "");
                    MiniLogger.LogInfo($"Recorded {moon.name}");
                    registeredMoons.Add(moonUUID);
                }
            }

            moonFile.file.Save();
            moonFile.file.SaveOnConfigSet = true;
        }

        public LunarConfigFile AddFile(string path, string name)
        {
            LunarConfigFile file = new LunarConfigFile(path);
            files[name] = file;
            return file;
        }
    }
}
