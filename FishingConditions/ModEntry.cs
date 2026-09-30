using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace FishingConditions
{
	internal sealed class ModEntry : Mod
	{
		private const int TimeIndex = 5;
		private const int WeatherIndex = 7;

		public override void Entry(IModHelper helper)
		{
			helper.Events.Content.AssetRequested += this.OnAssetRequested;
		}

		private void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
		{
			if (!e.NameWithoutLocale.IsEquivalentTo("Data/Fish"))
				return;

			e.Edit(asset =>
			{
				IDictionary<string, string> data = asset.AsDictionary<string, string>().Data;
				foreach (string itemId in data.Keys.ToArray())
				{
					string? updated = RemoveTimeAndWeather(data[itemId]);
					if (updated != null)
						data[itemId] = updated;
				}
			}, AssetEditPriority.Late);
		}

		/// <summary>Clear the time and weather fields. Trap entries use a different format and are left alone.</summary>
		private static string? RemoveTimeAndWeather(string itemData)
		{
			string[] fields = itemData.Split('/');
			if (fields.Length <= WeatherIndex || fields[1] == "trap")
				return null;

			fields[TimeIndex] = "600 2600";
			fields[WeatherIndex] = "both";
			return string.Join("/", fields);
		}
	}
}
