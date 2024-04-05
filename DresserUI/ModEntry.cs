using System;
using System.Threading;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Objects;

namespace DresserUI
{
	internal sealed class ModEntry : Mod
	{
		internal static IModHelper? helper;
		internal static IMonitor? monitor;
		internal static IManifest? modManifest;
		internal List<Furniture> locationDressers = new List<Furniture>();
		internal StorageFurniture? currentDresser = null;

		public override void Entry(IModHelper helper)
		{
			ModEntry.helper = Helper;
			monitor = Monitor;
			modManifest = ModManifest;

			helper.Events.Input.ButtonPressed += OnButtonPressed;
			helper.Events.Player.Warped += OnWarped;
			helper.Events.GameLoop.DayStarted += OnDayStarted;
		}

		private void OnDayStarted(object? sender, DayStartedEventArgs e)
		{
			UpdateListDressers(Game1.currentLocation);
		}

		private void OnWarped(object? sender, WarpedEventArgs e)
		{
			UpdateListDressers(e.NewLocation);
		}

		private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
		{
			if (e.Button != SButton.MouseRight) return;
			if (locationDressers == null || locationDressers.Count == 0) return;

			if (IsTileDresser(e.Cursor.Tile))
			{
				Monitor.Log("Dresser interaction detected!", LogLevel.Debug);
				Helper.Input.Suppress(e.Button);
				List<Item> dresserItems = currentDresser.heldItems.ToList();
				foreach(var item in dresserItems)
					Monitor.Log($"{item.Name} {item.Stack} {item.Category} {item.TypeDefinitionId}", LogLevel.Debug);
				//List<Item> outfitItems = dresserItems.Where(item => item is Clothing || item is Ring || item is Hat || item is Boots).ToList();
				Game1.activeClickableMenu = new CustomPopup(Helper, "Outfits", dresserItems);
			}
		}
		private void UpdateListDressers(GameLocation location)
		{
			if (location.Name != "FarmHouse") return;

			locationDressers.Clear();
			foreach (var furniture in location.furniture)
			{
				if (furniture.ItemId.ToLower().Contains("dresser"))
					locationDressers.Add(furniture);
			}
			/*Monitor.Log("dresser's list:", LogLevel.Debug);
			foreach(var dresser in locationDressers)
			{
				Monitor.Log($"id {dresser.itemId} name {dresser.Name} tile {dresser.TileLocation} wide {dresser.getTilesWide()} high {dresser.getTilesHigh()}", LogLevel.Debug);
			}*/
		}
		private bool IsTileDresser(Vector2 tile)
		{
			/*
			var location = Game1.currentLocation;
			foreach (var ob in location.furniture)
			{
				Monitor.Log($"itemId {ob.ItemId} Name {ob.Name} location {ob.TileLocation}", LogLevel.Debug);
			}*/
			foreach (var dresser in locationDressers)
			{
				int[] rangeX = new int[dresser.getTilesWide()];
				for(int i = 0; i < rangeX.Length; i++)
				{
					rangeX[i] = (int)dresser.TileLocation.X + i;
				}
				int[] rangeY = new int[dresser.getTilesHigh()];
				for (int i = 0; i < rangeY.Length; i++)
				{
					rangeY[i] = (int)dresser.TileLocation.Y + i;
				}

				for(int i = 0; i < rangeX.Length; i++) 
				{
					for (int j = 0; j < rangeY.Length; j++)
					{
						//Monitor.Log($"tile pressed {tile} vs tile {dresser.ItemId} {new Vector2(rangeX[i], rangeY[j])}", LogLevel.Debug);
						if (new Vector2(rangeX[i], rangeY[j]) == tile)
						{
							currentDresser = (StorageFurniture)dresser;
							return true;
						}
					}
				}
			}
			return false;
		}
	}
}
