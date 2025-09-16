using System;
using System.Threading;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Tools;

namespace EmptyWateringCanSound
{
    /// <summary>The mod entry point.</summary>
    internal sealed class ModEntry : Mod
    {
        public static ModConfig Config;
        public static ModEntry context;
        public static IMonitor SMonitor;
        public static IModHelper SHelper;

        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {

            context = this;
            SMonitor = Monitor;
            SHelper = helper;
            Config = Helper.ReadConfig<ModConfig>();

            var harmony = new Harmony(ModManifest.UniqueID);

            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;

            harmony.Patch(
               original: AccessTools.Method(typeof(StardewValley.Tools.WateringCan), nameof(StardewValley.Tools.WateringCan.DoFunction)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.WateringCan_DoFunction_Prefix))
            );
        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {

            //get Generic Mod Config Menu's API (if it's installed)
            var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null)
                return;

            //register mod
            configMenu.Register(
                mod: this.ModManifest,
                reset: () => Config = new ModConfig(),
                save: () => Helper.WriteConfig(Config)
            );

            // add some config options
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Always play the last drop sound",
                tooltip: () => "Even if it isn't the last drop.",
                getValue: () => Config.AlwaysPlay,
                setValue: value => Config.AlwaysPlay = value
            );

            // add some config options
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "SoundID to play",
                tooltip: () => "",
                getValue: () => Config.SoundID,
                setValue: value => Config.SoundID = value
            );

            // add some config options
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Sound delay in MS",
                tooltip: () => "",
                getValue: () => Config.SoundDelay,
                setValue: value => Config.SoundDelay = value
            );

        }

        public static bool WateringCan_DoFunction_Prefix(WateringCan __instance, GameLocation location, int x, int y, int power, Farmer who)
        {
            int delay = Config.SoundDelay;
            string soundId = Config.SoundID;
            if (Game1.currentLocation.CanRefillWateringCanOnTile(x / 64, y / 64))
            {
                return true;
            }
            else if(((int)__instance.WaterLeft >= 0 && !who.hasWateringCanEnchantment && !__instance.isBottomless.Value) || Config.AlwaysPlay)
            {
                int newValue = __instance.WaterLeft - who.toolPower.Value + 1;
                //SMonitor.Log($"new value = {newValue}", LogLevel.Error);
                if (newValue <= 2 || Config.AlwaysPlay)
                {
                    DelayedAction.playSoundAfterDelay(soundId, delay, location, who.Tile);
                }
            }
            return true;
        }
    }

    public class ModConfig
    {
        public int SoundDelay { get; set; } = 500;
        public string SoundID { get; set; } = "cavedrip";
        public bool AlwaysPlay { get; set; } = true;
    }
}
