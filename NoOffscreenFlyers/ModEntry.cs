using System;
using System.Threading;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;

namespace NoOffscreenFlyers
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
               original: AccessTools.Method(typeof(StardewValley.Locations.MineShaft), nameof(StardewValley.Locations.MineShaft.spawnFlyingMonsterOffScreen)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Mines_SpawnOffscreenFlyingMonsters_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(StardewValley.Locations.VolcanoDungeon), nameof(StardewValley.Locations.VolcanoDungeon.spawnFlyingMonsterOffScreen)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Volcano_SpawnOffscreenFlyingMonsters_Prefix))
            );


        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {

            // get Generic Mod Config Menu's API (if it's installed)
            var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null)
                return;

            // register mod
            configMenu.Register(
                mod: this.ModManifest,
                reset: () => Config = new ModConfig(),
                save: () => Helper.WriteConfig(Config)
            );

            // add some config options
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Prevent Spawning in Mines",
                tooltip: () => "You may want to turn it off to kill more bats or something",
                getValue: () => Config.MinesEnabled,
                setValue: value => Config.MinesEnabled = value
            );

            // add some config options
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Prevent Spawning in Volcano",
                tooltip: () => "You may want to turn it off to kill more fireballs or something",
                getValue: () => Config.MinesEnabled,
                setValue: value => Config.MinesEnabled = value
            );

        }


        public static bool Mines_SpawnOffscreenFlyingMonsters_Prefix()
        {
            try
            {
                if (Config.MinesEnabled)
                {
                    return false;
                }
                else
                { 
                    return true; 
                }
            }
            catch (Exception ex)
            {
                SMonitor.Log($"Failed in {nameof(Mines_SpawnOffscreenFlyingMonsters_Prefix)}:\n{ex}", LogLevel.Error);
                return true;
            }
        }

        public static bool Volcano_SpawnOffscreenFlyingMonsters_Prefix()
        {
            try
            {
                if (Config.VolcanoEnabled)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                SMonitor.Log($"Failed in {nameof(Volcano_SpawnOffscreenFlyingMonsters_Prefix)}:\n{ex}", LogLevel.Error);
                return true;
            }
        }
    }

    public class ModConfig
    {
        public bool MinesEnabled { get; set; } = true;
        public bool VolcanoEnabled { get; set; } = true;
    }
}
