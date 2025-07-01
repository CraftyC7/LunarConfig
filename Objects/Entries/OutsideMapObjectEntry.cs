using LunarConfig.Objects.Info;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using static UnityEngine.InputSystem.InputControlScheme.MatchResult;

namespace LunarConfig.Objects.Entries
{
    internal class OutsideMapObjectEntry
    {
        public string configString { get; set; }

        public OutsideMapObjectEntry(OutsideMapObjectInfo info)
        {
            configString =
                $"[{info.objID}]\n" +
                "## The width of an object.\n" +
                "# Setting type: Integer\n" +
                $"Object Width = {info.objWidth}\n\n" +
                "## Specifies whether the object will spawn facing away from a wall.\n" +
                "# Setting type: Boolean\n" +
                $"Face Away From Wall = {info.faceAwayWall.ToString().ToLower()}\n\n" +
                "## The base animation curve of an object.\n" +
                "## Keyframes x,y separated by semicolons.\n" +
                "# Setting type: String\n" +
                $"(LunarConfig) Base Curve = {string.Join("; ", info.baseCurve.keys.Select(k => $"{k.time},{k.value}"))}\n\n";
        }

        public OutsideMapObjectEntry(string info)
        {
            configString = info;
        }
    }

    internal static class parseOutsideMapObjectEntry
    {
        public static OutsideMapObjectInfo parseEntry(string entry)
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

            OutsideMapObjectInfo info = new OutsideMapObjectInfo(
                Regex.Match(entry, @"\[(.*?)\]").Groups[1].Value,
                int.Parse(GetValue("Object Width")),
                bool.Parse(GetValue(@"Face Away From Wall").ToLower()),
                ParseCurve(GetValue("Base Curve"))
                );

            return info;
        }
    }
}