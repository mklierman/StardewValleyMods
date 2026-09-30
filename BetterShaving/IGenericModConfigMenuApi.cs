using StardewModdingAPI;

namespace BetterShaving
{
	/// <summary>The API which lets other mods add a config UI through Generic Mod Config Menu.</summary>
	public interface IGenericModConfigMenuApi
	{
		void Register(IManifest mod, Action reset, Action save, bool titleScreenOnly = false);

		void AddNumberOption(IManifest mod, Func<int> getValue, Action<int> setValue, Func<string> name, Func<string>? tooltip = null, int? min = null, int? max = null, int? interval = null, string? fieldId = null);
	}
}
