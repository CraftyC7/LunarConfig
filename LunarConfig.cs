using BepInEx;
using BepInEx.Logging;
using Dawn;
using HarmonyLib;
using LunarConfig.Objects.Config;
using LunarConfig.Patches;
using System.IO;

namespace LunarConfig
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class LunarConfig : BaseUnityPlugin
    {
        internal static readonly string EXPORT_DIRECTORY = Path.Combine(Paths.ConfigPath, MyPluginInfo.PLUGIN_NAME);

        internal static readonly string ITEM_FILE_NAME = "LunarConfigItems.cfg";
        internal static readonly string ITEM_FILE = Path.Combine(EXPORT_DIRECTORY, ITEM_FILE_NAME);

        internal static readonly string ENEMY_FILE_NAME = "LunarConfigEnemies.cfg";
        internal static readonly string ENEMY_FILE = Path.Combine(EXPORT_DIRECTORY, ENEMY_FILE_NAME);

        internal static readonly string MOON_FILE_NAME = "LunarConfigMoons.cfg";
        internal static readonly string MOON_FILE = Path.Combine(EXPORT_DIRECTORY, MOON_FILE_NAME);

        internal static readonly string MAP_OBJECT_FILE_NAME = "LunarConfigMapObjects.cfg";
        internal static readonly string MAP_OBJECT_FILE = Path.Combine(EXPORT_DIRECTORY, MAP_OBJECT_FILE_NAME);

        internal static readonly string DUNGEON_FILE_NAME = "LunarConfigDungeons.cfg";
        internal static readonly string DUNGEON_FILE = Path.Combine(EXPORT_DIRECTORY, DUNGEON_FILE_NAME);

        internal static readonly string BUYABLE_FILE_NAME = "LunarConfigUnlockables.cfg";
        internal static readonly string BUYABLE_FILE = Path.Combine(EXPORT_DIRECTORY, BUYABLE_FILE_NAME);

        internal static readonly string CENTRAL_FILE_NAME = "LunarConfigCentral.cfg";
        internal static readonly string CENTRAL_FILE = Path.Combine(Paths.ConfigPath, CENTRAL_FILE_NAME);

        public static LunarConfig Instance { get; private set; } = null!;
        internal new static ManualLogSource Logger { get; private set; } = null!;
        internal static Harmony? Harmony { get; set; }

        public static LunarCentral central { get; set; } = new LunarCentral();

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
            Harmony.PatchAll(typeof(StartOfRoundPatch));

            LethalContent.Items.OnFreeze += central.InitItems;
            LethalContent.Enemies.OnFreeze += central.InitEnemies;
            LethalContent.Dungeons.OnFreeze += central.InitDungeons;
            LethalContent.Moons.OnFreeze += central.InitMoons;
            LethalContent.MapObjects.OnFreeze += central.InitMapObjects;
            //LethalContent.Unlockables.OnFreeze += central.InitUnlockables;

            Logger.LogDebug("Finished patching!");
        }

        internal static void Unpatch()
        {
            Logger.LogDebug("Unpatching...");

            Harmony?.UnpatchSelf();

            Logger.LogDebug("Finished unpatching!");
        }
    }

    // MiniLogger from LethalQuantities <3
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