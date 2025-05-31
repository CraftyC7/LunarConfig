using LunarConfig.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using static UnityEngine.InputSystem.InputControlScheme.MatchResult;

namespace LunarConfig.Config_Entries
{
    internal class ItemEntry
    {
        public string configString { get; set; }

        public ItemEntry(ItemInfo info)
        {
            configString =
                $"[{info.itemID}]\n" +
                "## Specifies the name that appears when scanning the item.\n" +
                "# Setting type: String\n" +
                $"Display Name = {info.displayName}\n\n" +
                "## The minimum scrap value the item can have.\n" +
                "## Typically multiplied by 0.4, setting not applicable to non-scrap.\n" +
                "# Setting type: Integer\n" +
                $"Minimum Value = {info.minValue}\n\n" +
                "## The maximum scrap value the item can have.\n" +
                "## Typically multiplied by 0.4, setting not applicable to non-scrap.\n" +
                "# Setting type: Integer\n" +
                $"Maximum Value = {info.maxValue}\n\n" +
                "## Specifies the weight of an item.\n" +
                "## Calculated with: (x - 1) * 105 = weight in pounds.\n" +
                "# Setting type: Float\n" +
                $"Weight = {info.weight}\n\n" +
                "## Specifies whether an item is conductive.\n" +
                "# Setting type: Boolean\n" +
                $"Conductivity = {info.conductive.ToString().ToLower()}\n\n" +
                "## Specifies whether an item is two-handed.\n" +
                "# Setting type: Boolean\n" +
                $"Two-Handed = {info.twoHanded.ToString().ToLower()}\n\n" +
                "## Tags allocated to the item.\n" +
                "## Separate tags with commas.\n" +
                "# Setting type: String\n" +
                $"Tags = {string.Join(", ", info.tags)}\n\n" +
                "## Specifies if an item is scrap or gear.\n" +
                "## This decides whether an item can be sold to the company for credits.\n" +
                "# Setting type: Boolean\n" +
                $"Is Scrap? = {info.isScrap.ToString().ToLower()}\n\n";
        }

        public ItemEntry(String info)
        {
            configString = info;
        }
    }

    internal static class parseItemEntry
    {
        public static ItemInfo parseEntry(String entry)
        {
            string GetValue(string key)
            {
                var match = Regex.Match(entry, $@"{key}\s*=\s*(.+)");
                return match.Success ? match.Groups[1].Value.Trim() : "";
            }

            ItemInfo info = new ItemInfo(
                Regex.Match(entry, @"\[(.*?)\]").Groups[1].Value,
                GetValue("Display Name"),
                int.Parse(GetValue("Minimum Value")),
                int.Parse(GetValue("Maximum Value")),
                float.Parse(GetValue("Weight")),
                bool.Parse(GetValue("Conductivity").ToLower()),
                bool.Parse(GetValue("Two-Handed").ToLower()),
                bool.Parse(GetValue(@"Is Scrap\?").ToLower()),
                Regex.Split(GetValue("Tags"), @"[\s,]+").Where(tag => !string.IsNullOrWhiteSpace(tag)).ToList()
                );

            return info;
        }
    }
}