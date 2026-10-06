using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley.Locations;
using StardewValley.Menus;

namespace OneItemBundles
{
	internal sealed class ModEntry : Mod
	{
		internal static ModConfig Config = null!;

		public override void Entry(IModHelper helper)
		{
			Config = helper.ReadConfig<ModConfig>();

			var harmony = new Harmony(this.ModManifest.UniqueID);
			harmony.Patch(
				original: AccessTools.Constructor(typeof(Bundle), new[]
				{
					typeof(int),
					typeof(string),
					typeof(bool[]),
					typeof(Point),
					typeof(string),
					typeof(JunimoNoteMenu),
				}),
				postfix: new HarmonyMethod(typeof(ModEntry), nameof(AfterBundleCreated))
			);

			helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
		}

		private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
		{
			var configMenu = this.Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
			if (configMenu is null)
				return;

			configMenu.Register(
				mod: this.ModManifest,
				reset: () => Config = new ModConfig(),
				save: () => this.Helper.WriteConfig(Config)
			);

			configMenu.AddBoolOption(
				mod: this.ModManifest,
				name: () => "Missing Bundle",
				tooltip: () => "Whether the Missing Bundle (Abandoned JojaMart) requires only one item.",
				getValue: () => Config.SecretBundle,
				setValue: value => Config.SecretBundle = value
			);
		}

		/// <summary>
		/// The community center note draws one deposit slot per <see cref="Bundle.numberOfIngredientSlots"/>
		/// and still lists every accepted item. One slot means any single listed item completes the bundle.
		/// </summary>
		private static void AfterBundleCreated(Bundle __instance, JunimoNoteMenu menu)
		{
			bool isCommunityCenter = menu.whichArea is >= CommunityCenter.AREA_Pantry and <= CommunityCenter.AREA_Bulletin;
			bool isSecretBundle = menu.whichArea == CommunityCenter.AREA_AbandonedJojaMart;

			if (!isCommunityCenter && (!isSecretBundle || !Config.SecretBundle))
				return;

			if (__instance.complete || __instance.ingredients.Count == 0 || __instance.numberOfIngredientSlots <= 1)
				return;

			int completedCount = __instance.ingredients.Count(i => i.completed);
			if (completedCount >= __instance.numberOfIngredientSlots)
				return;

			__instance.numberOfIngredientSlots = completedCount + 1;
		}
	}

	public sealed class ModConfig
	{
		public bool SecretBundle { get; set; } = false;
	}
}
