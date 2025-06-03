using LunarConfig.Configuration.Entries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace LunarConfig.Configuration
{
    internal class ItemConfiguration
    {
        public string itemConfig { get; set; }

        public ItemConfiguration(List<ItemEntry> entries)
        {
            itemConfig = string.Join("\n\n", entries);
        }

        public ItemConfiguration(string entries)
        {
            itemConfig = entries;
        }

        public void AddEntry(ItemEntry entry)
        {
            itemConfig += entry.configString + "\n\n";
        }

        public void AddEntry(string entry)
        {
            itemConfig += "\n\n" + entry;
        }
    }

    internal static class parseItemConfiguration
    {
        public static List<ItemEntry> parseConfiguration(string configText)
        {
            List<ItemEntry> items = new List<ItemEntry>();
            foreach (var item in Regex.Split(configText, @"(?=\[.*?\])"))
            {
                if (!string.IsNullOrWhiteSpace(item))
                {
                    items.Add(new ItemEntry(item));
                }
            }
            return items;
        }
    }
}
