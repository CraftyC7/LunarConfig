using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using LobbyCompatibility.Attributes;
using LobbyCompatibility.Enums;
using LunarConfig.Patches;
using System.IO;

namespace LunarConfig
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInDependency("BMX.LobbyCompatibility", BepInDependency.DependencyFlags.HardDependency)]
    [LobbyCompatibility(CompatibilityLevel.Everyone, VersionStrictness.None)]
    public class LunarConfig : BaseUnityPlugin
    {
        // Based off of LethalQuantities see NOTICE
        internal static readonly string EXPORT_DIRECTORY = Path.Combine(Paths.ConfigPath, MyPluginInfo.PLUGIN_NAME);

        internal static readonly string ITEM_FILE_NAME = "Items.cfg";
        internal static readonly string ITEM_FILE = Path.Combine(EXPORT_DIRECTORY, ITEM_FILE_NAME);

        internal static readonly string ENEMY_FILE_NAME = "Enemies.cfg";
        internal static readonly string ENEMY_FILE = Path.Combine(EXPORT_DIRECTORY, ENEMY_FILE_NAME);

        public static LunarConfig Instance { get; private set; } = null!;
        internal new static ManualLogSource Logger { get; private set; } = null!;
        internal static Harmony? Harmony { get; set; }

        private void Awake()
        {
            Logger = base.Logger;
            Instance = this;

            Patch();

            Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} has loaded!");
        }

        internal static void Patch()
        {
            Harmony ??= new Harmony(MyPluginInfo.PLUGIN_GUID);

            Logger.LogDebug("Patching...");

            Harmony.PatchAll(typeof(RoundManagerPatch));

            Logger.LogDebug("Finished patching!");
        }

        internal static void Unpatch()
        {
            Logger.LogDebug("Unpatching...");

            Harmony?.UnpatchSelf();

            Logger.LogDebug("Finished unpatching!");
        }
    }

    // MiniLogger from LethalQuantities <3 see NOTICE
    public static class MiniLogger
    {
        private static ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource(MyPluginInfo.PLUGIN_NAME);

        public static void LogInfo(string message)
        {
            logger.LogInfo(message);
        }

        public static void LogWarning(string message)
        {
            logger.LogWarning(message);
        }

        public static void LogError(string message)
        {
            logger.LogError(message);
        }
    }
}