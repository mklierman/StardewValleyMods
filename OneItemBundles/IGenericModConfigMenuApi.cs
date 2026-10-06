using StardewModdingAPI;

namespace OneItemBundles
{
	/// <summary>The API which lets other mods add a config UI through Generic Mod Config Menu.</summary>
	public interface IGenericModConfigMenuApi
	{
		void Register(IManifest mod, Action reset, Action save, bool titleScreenOnly = false);

		void AddBoolOption(IManifest mod, Func<bool> getValue, Action<bool> setValue, Func<string> name, Func<string>? tooltip = null, string? fieldId = null);
	}
}
