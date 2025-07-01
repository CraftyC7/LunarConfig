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
    internal class MoonEntry
    {
        public string configString { get; set; }

        public MoonEntry(MoonInfo info)
        {
            configString =
                $"[{info.moonID}]\n" +
                "## Changes the name of the moon.\n" +
                "## Does not modify terminal commands/output.\n" +
                "# Setting type: String\n" +
                $"Display Name = {info.displayName}\n\n" +
                "## Changes the risk level of the moon.\n" +
                "## This setting is only cosmetic.\n" +
                "# Setting type: String\n" +
                $"Risk Level = {info.risk}\n\n" +
                "## The description given to a moon.\n" +
                "## New lines are represented by semi-colons.\n" +
                "## Does not modify terminal commands/output.\n" +
                "# Setting type: String\n" +
                $"Description = {info.description.Replace("\n", ";")}\n\n" +
                "## Defines whether a moon has time.\n" +
                "# Setting type: Boolean\n" +
                $"Has Time? = {info.hasTime.ToString().ToLower()}\n\n" +
                "## Multiplies the speed at which time progresses on a moon.\n" +
                "# Setting type: Float\n" +
                $"Time Multiplier = {info.timeMultiplier}\n\n" +
                "## The amount of daytime enemies spawned that can differ from the curve.\n" +
                "## For instance, if this value is 3, and at the current time and spawn cycle 2 daytime enemies should spawn, anywhere between 0 and 5 can spawn.\n" +
                "# Setting type: Float\n" +
                $"Daytime Probability Range = {info.daytimeProbabilityRange}\n\n" +
                "## Decides the amount of daytime enemies that spawn as the day progresses.\n" +
                "## Keyframes x,y separated by semicolons.\n" +
                "# Setting type: String\n" +
                $"Daytime Curve = {string.Join("; ", info.daytimeCurve.keys.Select(k => $"{k.time},{k.value}"))}\n\n" +
                "## The amount of daytime power capacity that a moon has.\n" +
                "# Setting type: Integer\n" +
                $"Max Daytime Power = {info.maxDaytimePower}\n\n" +
                "## The amount of interior enemies spawned that can differ from the curve.\n" +
                "## For instance, if this value is 3, and at the current time and spawn cycle 2 interior enemies should spawn, anywhere between 0 and 5 can spawn.\n" +
                "# Setting type: Float\n" +
                $"Interior Probability Range = {info.interiorProbabilityRange}\n\n" +
                "## Decides the amount of interior enemies that spawn as the day progresses.\n" +
                "## Keyframes x,y separated by semicolons.\n" +
                "# Setting type: String\n" +
                $"Interior Curve = {string.Join("; ", info.interiorCurve.keys.Select(k => $"{k.time},{k.value}"))}\n\n" +
                "## The amount of interior power capacity that a moon has.\n" +
                "# Setting type: Integer\n" +
                $"Max Interior Power = {info.maxInteriorPower}\n\n" +
                "## Decides the amount of outside enemies that spawn as the day progresses.\n" +
                "## Keyframes x,y separated by semicolons.\n" +
                "# Setting type: String\n" +
                $"Outside Curve = {string.Join("; ", info.outsideCurve.keys.Select(k => $"{k.time},{k.value}"))}\n\n" +
                "## The amount of outside power capacity that a moon has.\n" +
                "# Setting type: Integer\n" +
                $"Max Outside Power = {info.maxOutsidePower}\n\n" +
                "## The minimum amount of scrap items that can spawn on a moon.\n" +
                "# Setting type: Integer\n" +
                $"Min Scrap = {info.minScrap}\n\n" +
                "## The maximum amount of scrap items that can spawn on a moon.\n" +
                "# Setting type: Integer\n" +
                $"Max Scrap = {info.maxScrap}\n\n" +
                "## The multiplier applied to the value of a moon.\n" +
                "# Setting type: Float\n" +
                $"Value Multiplier = {info.valueMultiplier}\n\n" +
                "## The multiplier applied to the amount of scrap on a moon.\n" +
                "# Setting type: Float\n" +
                $"Amount Multiplier = {info.amountMultiplier}\n\n" +
                "## Changes the size of the interior generated.\n" +
                "# Setting type: Float\n" +
                $"Interior Multiplier = {info.interiorSizeMultiplier}\n\n" +
                "## Tags allocated to the moon.\n" +
                "## Separate tags with commas.\n" +
                "# Setting type: String\n" +
                $"Tags = {string.Join(", ", info.tags)}\n\n";
        }

        public MoonEntry(string info)
        {
            configString = info;
        }
    }

    internal static class parseMoonEntry
    {
        public static MoonInfo parseEntry(string entry)
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

            MoonInfo info = new MoonInfo(
                Regex.Match(entry, @"\[(.*?)\]").Groups[1].Value,
                GetValue("Display Name"),
                GetValue("Risk Level"),
                GetValue("Description").Replace(";", "\n"),
                bool.Parse(GetValue(@"Has Time\?").ToLower()),
                float.Parse(GetValue("Time Multiplier")),
                float.Parse(GetValue("Daytime Probability Range")),
                ParseCurve(GetValue("Daytime Curve")),
                int.Parse(GetValue("Max Daytime Power")),
                float.Parse(GetValue("Interior Probability Range")),
                ParseCurve(GetValue("Interior Curve")),
                int.Parse(GetValue("Max Interior Power")),
                ParseCurve(GetValue("Outside Curve")),
                int.Parse(GetValue("Max Outside Power")),
                int.Parse(GetValue("Min Scrap")),
                int.Parse(GetValue("Max Scrap")),
                float.Parse(GetValue("Value Multiplier")),
                float.Parse(GetValue("Amount Multiplier")),
                float.Parse(GetValue("Interior Multiplier")),
                Regex.Split(GetValue("Tags"), @"[\s,]+").Where(tag => !string.IsNullOrWhiteSpace(tag)).ToList()
                );

            return info;
        }
    }
}