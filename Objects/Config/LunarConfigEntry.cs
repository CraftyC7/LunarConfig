using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Xml.Schema;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace LunarConfig.Objects.Config
{
    public class LunarConfigEntry
    {
        public string name;
        public ConfigFile file;
        public Dictionary<string, ConfigEntryBase> fields = new Dictionary<string, ConfigEntryBase>();
        
        public LunarConfigEntry(ConfigFile file, string name) 
        {
            this.name = name;
            this.file = file;
        }

        public void AddField<T>(string key, string description, T defaultValue)
        {
            fields[key] = file.Bind(name, key, defaultValue, description);
        }

        public void AddFields(List<(string, string, object)> fieldList)
        {
            foreach (var field in fieldList)
            {
                fields[field.Item1] = file.Bind(name, field.Item1, field.Item3, field.Item2);
            }
        }

        public T GetValue<T>(string key)
        {
            ConfigEntry<T> entry = (ConfigEntry<T>)fields[key];
            return entry.Value;
        }

        public void SetValue<T>(string key, ref T obj)
        {
            T value = GetValue<T>(key);
            if (!EqualityComparer<T>.Default.Equals(obj, value))
            {
                obj = value;
            }
        }

        public void SetCurve(string key, ref AnimationCurve obj)
        {
            AnimationCurve value = LunarCentral.StringToCurve(GetValue<string>(key));
            if (obj != value)
            {
                obj = value;
            }
        }
    }
}
