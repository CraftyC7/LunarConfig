using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace LunarConfig.Objects.Config
{
    public class LunarConfigFile
    {
        public readonly string path;
        public ConfigFile file;
        public Dictionary<string, LunarConfigEntry> entries = new Dictionary<string, LunarConfigEntry>();

        public LunarConfigFile(string path)
        {
            this.path = path;
            file = new ConfigFile(path, true);
        }

        public LunarConfigEntry AddEntry(string name)
        {
            if (entries.ContainsKey(name))
            {
                return entries[name];
            }
            else
            {
                LunarConfigEntry entry = new LunarConfigEntry(file, name);
                entries[name] = entry;
                return entry;
            }
        }
    }
}