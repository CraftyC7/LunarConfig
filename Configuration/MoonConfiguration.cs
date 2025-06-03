using LunarConfig.Configuration.Entries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace LunarConfig.Configuration
{
    internal class MoonConfiguration
    {
        public string moonConfig { get; set; }

        public MoonConfiguration(List<MoonEntry> entries)
        {
            moonConfig = string.Join("\n\n", entries);
        }

        public MoonConfiguration(string entries)
        {
            moonConfig = entries;
        }

        public void AddEntry(MoonEntry entry)
        {
            moonConfig += entry.configString + "\n\n";
        }

        public void AddEntry(string entry)
        {
            moonConfig += "\n\n" + entry;
        }
    }

    internal static class parseMoonConfiguration
    {
        public static List<MoonEntry> parseConfiguration(string configText)
        {
            List<MoonEntry> moons = new List<MoonEntry>();
            foreach (var moon in Regex.Split(configText, @"(?=\[.*?\])"))
            {
                if (!string.IsNullOrWhiteSpace(moon))
                {
                    moons.Add(new MoonEntry(moon));
                }
            }
            return moons;
        }
    }
}
