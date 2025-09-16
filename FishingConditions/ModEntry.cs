using System;
using System.Threading;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.GameData.Objects;

namespace FishingConditions
{
	internal sealed class ModEntry : Mod
	{
		public static ModEntry context;
		public static IMonitor SMonitor;
		public static IModHelper SHelper;

		public override void Entry(IModHelper helper)
		{
			context = this;
			SMonitor = Monitor;
			SHelper = helper;
			helper.Events.Content.AssetRequested += this.OnAssetRequested;
		}

		private void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
		{
			if (e.NameWithoutLocale.IsEquivalentTo("Data/Fish"))
			{
				e.Edit(asset =>
				{
					var data = asset.AsDictionary<string, string>().Data;
					var newData = new Dictionary<string, string>();
					foreach ((string itemID, string itemData) in data)
					{
						var splitData = itemData.Split("/");
						if (splitData[1] != "trap")
						{
							splitData[5] = "600 2600";
							splitData[7] = "both";
							var joined = string.Join("/", splitData);
							newData.Add(itemID, joined);
						}
					}
					foreach ((var itemid, var itemData) in newData)
					{
						data[itemid] = itemData;
					}
				});
			}
		}
	}
}
