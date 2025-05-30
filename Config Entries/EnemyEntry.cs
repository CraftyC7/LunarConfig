using LunarConfig.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using static UnityEngine.InputSystem.InputControlScheme.MatchResult;

namespace LunarConfig.Config_Entries
{
    internal class EnemyEntry
    {
        public string configString { get; set; }

        public EnemyEntry(EnemyInfo info)
        {
            configString =
                $"[{info.enemyID}]\n" +
                "## Specifies the name that appears when scanning the enemy.\n" +
                "# Setting type: String\n" +
                $"Display Name = {info.displayName}\n\n" +
                "## Specifies if an enemy can see through fog in foggy weather.\n" +
                "# Setting type: Boolean\n" +
                $"Can See Through Fog? = {info.canSeeThroughFog.ToString().ToLower()}\n\n" +
                "## Decides the speed at which enemies can open doors.\n" +

                "# Setting type: Float\n" +
                $"Door Speed Multiplier = {info.doorSpeedMultiplier}\n\n" +
                "## Whether an enemy is a daytime enemy.\n" +
                "# Setting type: Boolean\n" +
                $"Is Daytime Enemy? = {info.isDaytimeEnemy.ToString().ToLower()}\n\n" +
                "## Whether an enemy is a outdoor enemy.\n" +
                "# Setting type: Boolean\n" +
                $"Is Outdoor Enemy? = {info.isOutsideEnemy.ToString().ToLower()}\n\n" +
                "## Multiplies the volume of an enemy's sounds.\n" +
                "# Setting type: Float\n" +
                $"Loudness Multiplier = {info.loudnessMultiplier}\n\n" +
                "## The maximum amount of an enemy that can be alive.\n" +
                "# Setting type: Integer\n" +
                $"Max Count = {info.maxCount}\n\n" +
                "## The power level an enemy occupies.\n" +
                "# Setting type: Float\n" +
                $"Power Level = {info.powerLevel}\n\n" +
                "## Decides how an enemy's weight changes after it's been spawned.\n" +
                "## Keyframes x,y separated by semicolons.\n" +
                "# Setting type: String\n" +
                $"Probability Curve = {string.Join("; ", info.probabilityCurve.keys.Select(k => $"{k.time},{k.value}"))}\n\n" +
                "## The amount of HP an enemy has.\n" +
                "# Setting type: Integer\n" +
                $"Enemy HP = {info.enemyHP}\n\n" +
                "## Tags allocated to the enemy.\n" +
                "## Separate tags with commas.\n" +
                "# Setting type: String\n" +
                $"Tags = {string.Join(", ", info.tags)}\n\n";
        }

        public EnemyEntry(String info)
        {
            configString = info;
        }
    }

    internal static class parseEnemyEntry
    {
        public static EnemyInfo parseEntry(String entry)
        {
            string GetValue(string key)
            {
                var match = Regex.Match(entry, $@"{key}\s*=\s*(.+)");
                return match.Success ? match.Groups[1].Value.Trim() : "";
            }

            AnimationCurve ParseCurve(string input)
            {
                var curve = new AnimationCurve();
                var keyframeStrings = Regex.Replace(input, @"\s+", "").Split(';');
                foreach (var kf in keyframeStrings)
                {
                    var parts = kf.Split(',');
                    if (parts.Length == 2 &&
                        float.TryParse(parts[0], out var time) &&
                        float.TryParse(parts[1], out var value))
                    {
                        curve.AddKey(time, value);
                    }
                }
                return curve;
            }

            EnemyInfo info = new EnemyInfo(
                Regex.Match(entry, @"\[(.*?)\]").Groups[1].Value,
                GetValue("Display Name"),
                bool.Parse(GetValue(@"Can See Through Fog\?").ToLower()),
                float.Parse(GetValue("Door Speed Multiplier")),
                bool.Parse(GetValue(@"Is Daytime Enemy\?").ToLower()),
                bool.Parse(GetValue(@"Is Outdoor Enemy\?").ToLower()),
                float.Parse(GetValue("Loudness Multiplier")),
                int.Parse(GetValue("Max Count")),
                float.Parse(GetValue("Power Level")),
                ParseCurve(GetValue("Probability Curve")),
                int.Parse(GetValue("Enemy HP")),
                Regex.Split(GetValue("Tags"), @"[\s,]+").Where(tag => !string.IsNullOrWhiteSpace(tag)).ToList()
                );

            return info;
        }
    }
}