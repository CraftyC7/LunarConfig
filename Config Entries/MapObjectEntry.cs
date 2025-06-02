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
    internal class MapObjectEntry
    {
        public string configString { get; set; }

        public MapObjectEntry(MapObjectInfo info)
        {
            configString =
                $"[{info.objID}]\n" +
                "## Specifies whether the object will spawn facing away from a wall.\n" +
                "# Setting type: Boolean\n" +
                $"Face Away From Wall = {info.faceAwayWall.ToString().ToLower()}\n\n" +
                "## Specifies whether the object will spawn facing towards a wall.\n" +
                "# Setting type: Boolean\n" +
                $"Face Towards Wall = {info.faceWall.ToString().ToLower()}\n\n" +
                "## Whether an object is disallowed spawning near entrances.\n" +
                "# Setting type: Boolean\n" +
                $"Disallow Near Entrances = {info.disallowNearEntrance.ToString().ToLower()}\n\n" +
                "## Whether distance is required between different object instances.\n" +
                "# Setting type: Boolean\n" +
                $"Require Distance Between Spawns = {info.requireDistanceBetweenSpawns.ToString().ToLower()}\n\n" +
                "## Specifies if an object should spawn flush against a wall.\n" +
                "# Setting type: Boolean\n" +
                $"Spawn Flush Against Wall = {info.spawnFlushAgainstWall.ToString().ToLower()}\n\n" +
                "## Specifies if an object should spawn against a wall.\n" +
                "# Setting type: Boolean\n" +
                $"Spawn Against Wall = {info.spawnAgainstWall.ToString().ToLower()}\n\n" +
                "## The base animation curve of an object.\n" +
                "## Keyframes x,y separated by semicolons.\n" +
                "# Setting type: String\n" +
                $"Base Curve = {string.Join("; ", info.baseCurve.keys.Select(k => $"{k.time},{k.value}"))}\n\n";
        }

        public MapObjectEntry(String info)
        {
            configString = info;
        }
    }

    internal static class parseMapObjectEntry
    {
        public static MapObjectInfo parseEntry(String entry)
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

            MapObjectInfo info = new MapObjectInfo(
                Regex.Match(entry, @"\[(.*?)\]").Groups[1].Value,
                bool.Parse(GetValue(@"Face Away From Wall").ToLower()),
                bool.Parse(GetValue(@"Face Towards Wall").ToLower()),
                bool.Parse(GetValue(@"Disallow Near Entrances").ToLower()),
                bool.Parse(GetValue(@"Require Distance Between Spawns").ToLower()),
                bool.Parse(GetValue(@"Spawn Flush Against Wall").ToLower()),
                bool.Parse(GetValue(@"Spawn Against Wall").ToLower()),
                ParseCurve(GetValue("Base Curve"))
                );

            return info;
        }
    }
}