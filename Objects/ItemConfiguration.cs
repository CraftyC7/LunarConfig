using LunarConfig.Config_Entries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace LunarConfig.Objects
{
    internal class ItemConfiguration
    {
        public String itemConfig { get; set; }

        public ItemConfiguration(List<ItemEntry> entries) 
        {
            itemConfig = string.Join("\n\n", entries);
        }

        public ItemConfiguration(String entries)
        {
            itemConfig = entries;
        }

        public void AddEntry(ItemEntry entry)
        {
            itemConfig += entry.configString + "\n\n";
        }

        public void AddEntry(String entry)
        {
            itemConfig += "\n\n" + entry;
        }
    }

    internal static class parseConfiguration
    {
        public static List<ItemEntry> parseItemConfiguration(String configText) 
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
