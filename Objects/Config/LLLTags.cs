using BepInEx;
using Dawn;
using LethalLevelLoader;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LunarConfig.Objects.Config
{
    internal class LLLTags
    {
        public static LunarCentral central = LunarConfig.central;

        public static void Init()
        {
            if (LunarCentral.configureMoons)
            {
                LunarConfigFile moonFile = central.files[LunarConfig.MOON_FILE_NAME];
                moonFile.file.SaveOnConfigSet = false;

                foreach (var moon in LethalContent.Moons)
                {
                    string uuid = LunarCentral.UUIDify(moon.Key.ToString());

                    try
                    {
                        string niceUUID = LunarCentral.NiceifyDawnUUID(moon.Key.Key);
                        DawnMoonInfo dawnMoon = moon.Value;
                        LunarConfigEntry moonEntry = moonFile.AddEntry($"{niceUUID} - {uuid}");
                        SelectableLevel moonObj = dawnMoon.Level;
                        ExtendedLevel extMoon;
                        LevelManager.TryGetExtendedLevel(moonObj, out extMoon);

                        // GETTING VALUES (for config)
                        if (extMoon != null)
                        {
                            String tagString = "";

                            foreach (ContentTag tag in extMoon.ContentTags)
                            {
                                if (tagString != "")
                                {
                                    tagString += ", ";
                                }
                                else if (!tag.contentTagName.IsNullOrWhiteSpace())
                                {
                                    tagString += tag.contentTagName;
                                }
                            }

                            moonEntry.TryAddField(LunarCentral.enabledMoonSettings, "LLL Tags", "Tags allocated to the moon by LLL.\nSeparate tags with commas.", tagString);

                            // SETTING VALUES
                            if (moonEntry.GetValue<bool>("Configure Content"))
                            {
                                if (LunarCentral.enabledMoonSettings.Contains("LLL Tags"))
                                {
                                    List<ContentTag> newTags = new List<ContentTag>();

                                    foreach (string tag in LunarCentral.RemoveWhitespace(moonEntry.GetValue<string>("LLL Tags")).ToLower().Split(","))
                                    {
                                        if (!tag.IsNullOrWhiteSpace())
                                            newTags.Add(ContentTag.Create(tag));
                                    }

                                    extMoon.ContentTags = newTags;
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        MiniLogger.LogError($"LunarConfig encountered an issue while configuring {uuid}, please report this!\n{e}");
                    }
                }

                moonFile.file.Save();
                moonFile.file.SaveOnConfigSet = true;
            }
        }
    }
}
