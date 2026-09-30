using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Enchantments;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;

namespace BetterShaving
{
	internal sealed class ModEntry : Mod
	{
		internal static ModConfig Config = null!;
		private static IMonitor? MonitorRef;

		public override void Entry(IModHelper helper)
		{
			Config = helper.ReadConfig<ModConfig>();
			MonitorRef = this.Monitor;

			var harmony = new Harmony(this.ModManifest.UniqueID);
			HarmonyMethod transpiler = new(typeof(ModEntry), nameof(TranspileShaving));
			harmony.Patch(AccessTools.Method(typeof(Tree), nameof(Tree.performToolAction)), transpiler: transpiler);
			harmony.Patch(AccessTools.Method(typeof(FruitTree), nameof(FruitTree.performToolAction)), transpiler: transpiler);
			harmony.Patch(AccessTools.Method(typeof(ResourceClump), nameof(ResourceClump.performToolAction)), transpiler: transpiler);

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

			configMenu.AddNumberOption(
				mod: this.ModManifest,
				name: () => "Minimum extra wood",
				tooltip: () => "Minimum amount of extra wood to drop per hit",
				getValue: () => Config.MinWood,
				setValue: value => Config.MinWood = value,
				min: 0,
				max: 25
			);

			configMenu.AddNumberOption(
				mod: this.ModManifest,
				name: () => "Maximum extra wood",
				tooltip: () => "Maximum amount of extra wood to drop per hit",
				getValue: () => Config.MaxWood,
				setValue: value => Config.MaxWood = value,
				min: 0,
				max: 25
			);
		}

		/// <summary>
		/// Vanilla Shaving rolls once per axe hit, then adds one bonus debris. Drop the extra wood on that same hit.
		/// </summary>
		private static IEnumerable<CodeInstruction> TranspileShaving(IEnumerable<CodeInstruction> instructions, MethodBase original)
		{
			List<CodeInstruction> codes = instructions.ToList();
			MethodInfo? enchantment = AccessTools.Method(typeof(Tool), nameof(Tool.hasEnchantmentOfType), generics: new[] { typeof(ShavingEnchantment) });
			int enchantIndex = enchantment == null ? -1 : codes.FindIndex(code => code.Calls(enchantment));
			int addIndex = enchantIndex < 0 ? -1 : FindShavingDrop(codes, enchantIndex);

			if (addIndex < 0)
			{
				MonitorRef?.Log($"Couldn't find the Shaving drop in {original.DeclaringType?.Name}.{original.Name}.", LogLevel.Warn);
				return codes;
			}

			codes.InsertRange(addIndex + 1, new[]
			{
				new CodeInstruction(OpCodes.Ldarg_0),
				new CodeInstruction(OpCodes.Ldarg_3),
				new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(AddExtraWood))),
			});
			return codes;
		}

		private static int FindShavingDrop(List<CodeInstruction> codes, int enchantIndex)
		{
			int branchIndex = -1;
			for (int i = enchantIndex + 1; i < codes.Count && i <= enchantIndex + 4; i++)
			{
				if (codes[i].opcode == OpCodes.Brfalse || codes[i].opcode == OpCodes.Brfalse_S)
				{
					branchIndex = i;
					break;
				}
			}

			int skipIndex = branchIndex < 0 ? -1 : IndexOfBranchTarget(codes, codes[branchIndex].operand);
			if (skipIndex <= enchantIndex)
				return -1;

			int addIndex = -1;
			for (int i = enchantIndex; i < skipIndex; i++)
			{
				if (codes[i].operand is MethodInfo method && method.Name == "Add")
					addIndex = i;
			}

			return addIndex;
		}

		private static int IndexOfBranchTarget(List<CodeInstruction> codes, object? operand)
		{
			if (operand is Label label)
				return codes.FindIndex(code => code.labels.Contains(label));

			if (operand is CodeInstruction target)
			{
				for (int i = 0; i < codes.Count; i++)
				{
					if (ReferenceEquals(codes[i], target))
						return i;
				}
			}

			return -1;
		}

		public static void AddExtraWood(TerrainFeature feature, Vector2 tile)
		{
			int min = Math.Max(0, Config.MinWood);
			int max = Math.Max(0, Config.MaxWood);
			if (min > max)
				(min, max) = (max, min);
			if (max == 0)
				return;

			int amount = Game1.random.Next(min, max + 1);
			if (amount <= 0)
				return;

			GameLocation? location = feature.Location ?? Game1.currentLocation;
			if (location == null)
				return;

			Game1.createMultipleObjectDebris("(O)388", (int)tile.X, (int)tile.Y, amount, location);
		}
	}

	public sealed class ModConfig
	{
		public int MinWood { get; set; } = 1;
		public int MaxWood { get; set; } = 3;
	}
}
