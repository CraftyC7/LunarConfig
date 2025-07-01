using LunarConfig.Objects.Entries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace LunarConfig.Objects.Configuration
{
    internal class TagConfiguration
    {
        public string tagConfig { get; set; }

        public TagConfiguration(List<TagEntry> entries)
        {
            tagConfig = string.Join("\n\n", entries);
        }

        public TagConfiguration(string entries)
        {
            tagConfig = entries;
        }

        public void AddEntry(TagEntry entry)
        {
            tagConfig += entry.configString + "\n\n";
        }

        public void AddEntry(string entry)
        {
            tagConfig += "\n\n" + entry;
        }
    }

    internal static class parseTagConfiguration
    {
        public static List<TagEntry> parseConfiguration(string configText)
        {
            List<TagEntry> tags = new List<TagEntry>();
            foreach (var tag in Regex.Split(configText, @"(?=\[.*?\])"))
            {
                if (!string.IsNullOrWhiteSpace(tag))
                {
                    tags.Add(new TagEntry(tag));
                }
            }
            return tags;
        }
    }
}
