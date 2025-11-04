using Dawn;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace LunarConfig.Patches
{
    internal class DawnPatch
    {
        [HarmonyPatch(typeof(DawnBaseInfo<DawnItemInfo>), MethodType.Constructor)]
        private static void dawnBaseInfoPatch(DawnBaseInfo<DawnItemInfo> __instance)
        {
            __instance._tags.Add(DawnLibTags.LunarConfig);
        }

        [HarmonyPatch(typeof(DawnItemInfo), MethodType.Constructor)]
        private static void dawnItemInfoPatch(DawnItemInfo __instance)
        {
            __instance.
        }

        [HarmonyPatch(typeof(DawnScrapItemInfo), MethodType.Constructor, typeof(ProviderTable<int?, DawnMoonInfo>))]
        [HarmonyPrefix]
        private static void dawnScrapInfoPatch(DawnScrapItemInfo __instance, ref ProviderTable<int?, DawnMoonInfo> weights)
        {
            __instance.ParentInfo.Key
            weights = null;
        }
    }
}
