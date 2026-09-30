using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley.Locations;
using StardewValley.Menus;

namespace OneItemBundles
{
	internal sealed class ModEntry : Mod
	{
		public override void Entry(IModHelper helper)
		{
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
		}

		/// <summary>
		/// The community center note draws one deposit slot per <see cref="Bundle.numberOfIngredientSlots"/>
		/// and still lists every accepted item. One slot means any single listed item completes the bundle.
		/// </summary>
		private static void AfterBundleCreated(Bundle __instance, JunimoNoteMenu menu)
		{
			if (menu.whichArea is < CommunityCenter.AREA_Pantry or > CommunityCenter.AREA_Bulletin)
				return;

			if (__instance.ingredients.Count == 0 || __instance.numberOfIngredientSlots <= 1)
				return;

			__instance.numberOfIngredientSlots = 1;
		}
	}
}
