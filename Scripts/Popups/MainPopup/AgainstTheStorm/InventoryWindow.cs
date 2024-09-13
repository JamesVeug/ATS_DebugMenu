using System.Collections.Generic;
using System.Linq;
using DebugMenu.Scripts.Popups;
using DebugMenu.Scripts.Utils;
using Eremite;
using Eremite.Model;
using Eremite.Services;
using UnityEngine;

namespace DebugMenu.Scripts.Acts;

public class InventoryWindow : BaseWindow
{
	public override string PopupName => "Inventory";
	public override Vector2 Size => new(1000, 800);
	public override bool ClosableWindow => true;
	
	private Vector2 position;
	private string filterText = "";
	private int amountToAdd = 1;
	private bool showEatables = true;
	private bool showFuels = true;
	private bool showOther = true;
	private bool[] favourites = new bool[]{true,true};
	private bool[] ownershipType = new bool[]{true,true};
	private bool hideEffectsWithMissingKeys = true;
	private List<GoodModel> allGoods = null;

	public override void OnGUI()
	{
		base.OnGUI();

		StorageService storageService = GameService.StorageService as StorageService;
		if (storageService == null)
		{
			Plugin.Log.LogError("EffectsService is null");
			return;
		}

		LockedGoodsCollection storage = storageService.Main.Goods;

		if (allGoods == null)
		{
			allGoods = Serviceable.Settings.Goods.ToList();
			SortGoods();
		}

		List<GoodModel> itemsInStorage = new List<GoodModel>(storage.goods.Keys.Select(a=>Serviceable.Settings.GetGood(a)));

		// Longer rows
		ColumnWidth = 200;
		
		int namesCount = allGoods.Count; // 20
		int rows = Mathf.Max(Mathf.FloorToInt(Size.y / RowHeight) - 2, 1); // 600 / 40 = 15 
		int columns = Mathf.CeilToInt((float)namesCount / rows) + 1; // 20 / 15 = 4
		Rect scrollableAreaSize = new(new Vector2(0, 0), new Vector2(columns *  ColumnWidth + (columns - 1) * 10, rows * RowHeight));
		Rect scrollViewSize = new(new Vector2(0, 0), Size - new Vector2(10, 25));
		position = GUI.BeginScrollView(scrollViewSize, position, scrollableAreaSize);
		
		Label("Amount to Give/Remove", new(0, RowHeight / 2));
		amountToAdd = IntField(amountToAdd, new(0, RowHeight / 2));

		Label("Filter", new(0, RowHeight / 2));
		filterText = TextField(filterText, new(0, RowHeight / 2));

		Label("Favourite", new(0, RowHeight / 2));
		RadialButtons(favourites, new[] { "Favourited", "NotFavourited" }, RowHeight / 2);

		Label("Ownership", new(0, RowHeight / 2));
		RadialButtons(ownershipType, new[] { "Owned", "NotOwned" }, RowHeight / 2);
		
		Label("Categories", new(0, RowHeight / 2));
		Toggle("Eatables", ref showEatables, new(0, RowHeight / 2));
		Toggle("Fuels", ref showFuels, new(0, RowHeight / 2));
		Toggle("Other", ref showOther, new(0, RowHeight / 2));
		
		Label("Other", new(0, RowHeight / 2));
		Toggle("Hide Missing Keys", ref hideEffectsWithMissingKeys);
		
		StartNewColumn();
		
		// Longer rows
		ColumnWidth = 200;
		
		int j = 0;
		for (int i = 0; i < namesCount; i++)
		{
			GoodModel good = allGoods[i];
			bool isFavourited = Plugin.SaveData.favouritedGoods.Contains(good.name);
			if (!IsFiltered(itemsInStorage, good, isFavourited)) 
				continue;

			storage.goods.TryGetValue(good.name, out int amount);
			using (HorizontalScope(4))
			{
				Label(good.icon);
				if(Button(isFavourited ? "\u2713" : "X", new Vector2(30, 0)))
				{
					if (isFavourited)
						Plugin.SaveData.favouritedGoods.Remove(good.name);
					else
						Plugin.SaveData.favouritedGoods.Add(good.name);
					SaveDataChanged();
				}
				
				if(Button("-", new Vector2(30, 0)))
				{
					storage.Remove(good.name, amountToAdd);
				}
				if(Button("+", new Vector2(30, 0)))
				{
					storage.Add(good.name, amountToAdd);
				}
				
				Vector2 nameSize = new Vector2(ColumnWidth - RowHeight*3, RowHeight);
				Label($"{amount}x\n{good.displayName}", nameSize);
			}
		
			j++;
			if (j >= rows)
			{
				StartNewColumn();
				j = 0;
			}
		}
		
		GUI.EndScrollView();
	}

	private bool IsFiltered(List<GoodModel> itemsInStorage, GoodModel good, bool favourited)
	{
		GoodModel ownedGood = itemsInStorage.FirstOrDefault(a=>a.name == good.Name);
		if (!favourited && !favourites[1])
			return false;
		if (favourited && !favourites[0])
			return false;
		if (ownedGood == null && !ownershipType[1])
			return false;
		if (ownedGood != null && !ownershipType[0])
			return false;
		if (hideEffectsWithMissingKeys && string.IsNullOrEmpty(good.displayName.key))
			return false;
		if (!showEatables && good.eatable)
			return false;
		if (!showFuels && good.canBeBurned)
			return false;
		if (!showOther && !good.canBeBurned && !good.eatable)
			return false;
			
		string displayName = good.displayName.GetText();
		string description = good.Description;
		if (!string.IsNullOrEmpty(filterText))
		{
			if (!displayName.ContainsText(filterText, false) &&
			    !description.ContainsText(filterText, false))
			{
				return false;
			}
		}

		return true;
	}

	private void RadialButtons(bool[] type, string[] buttons, float height)
	{
		Vector2 size = new Vector2(ColumnWidth / buttons.Length, height);

		using (HorizontalScope(buttons.Length))
		{
			for (int i = 0; i < buttons.Length; i++)
			{
				bool o = type[i];
				if (Toggle(buttons[i], ref o, size))
				{
					type[i] = !type[i];
				}
			}
		}
	}

	private void SortGoods()
	{
		allGoods.Sort(static (a, b) =>
		{
			bool aFav = Plugin.SaveData.favouritedGoods.Contains(a.name);
			bool bFav = Plugin.SaveData.favouritedGoods.Contains(b.name);
			if(aFav != bFav)
				return aFav ? -1 : 1;
			
			return a.displayName.GetText().CompareTo(b.displayName.GetText());
		});
	}

	private void SaveDataChanged()
	{
		Plugin.SaveData.Save();
		SortGoods();
	}
}