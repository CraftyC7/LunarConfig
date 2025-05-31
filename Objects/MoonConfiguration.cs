using LunarConfig.Config_Entries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace LunarConfig.Objects
{
    internal class MoonConfiguration
    {
        public String moonConfig { get; set; }

        public MoonConfiguration(List<MoonEntry> entries) 
        {
            moonConfig = string.Join("\n\n", entries);
        }

        public MoonConfiguration(String entries)
        {
            moonConfig = entries;
        }

        public void AddEntry(MoonEntry entry)
        {
            moonConfig += entry.configString + "\n\n";
        }

        public void AddEntry(String entry)
        {
            moonConfig += "\n\n" + entry;
        }
    }

    internal static class parseMoonConfiguration
    {
        public static List<MoonEntry> parseConfiguration(String configText) 
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
