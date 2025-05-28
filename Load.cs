using HarmonyLib;
using UnityEngine;

[HarmonyPatch(typeof(StartOfRound), "Start")]
public class Patch_StartOfRound
{
    static void Postfix(StartOfRound __instance)
    {
        if (__instance.GetComponent<ObjectRegister>() == null)
        {
            __instance.gameObject.AddComponent<ObjectRegister>();
        }
    }
}