using System.Collections.Generic;
using System.Linq;
using DebugMenu.Scripts.Popups;
using DebugMenu.Scripts.Utils;
using Eremite;
using Eremite.Model;
using Eremite.Services;
using UnityEngine;

namespace DebugMenu.Scripts.Acts;

public class InventoryWindow : CanvasWindow
{
	public override string PopupName => "Inventory";
	public override Vector2 Size => new(1000, 800);
	public override bool ClosableWindow => true;
	
	private string filterText = "";
	private int amountToAdd = 1;
	private bool showEatables = true;
	private bool showFuels = true;
	private bool showOther = true;
	private bool[] favourites = new bool[]{true,true};
	private bool[] ownershipType = new bool[]{true,true};
	private bool hideEffectsWithMissingKeys = true;
	private List<GoodModel> allGoods = null;

	public override void CreateGUI()
	{
		base.CreateGUI();

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
		// position = GUI.BeginScrollView(scrollViewSize, position, scrollableAreaSize);
		
		Label("Amount to Give/Remove");
		IntField(amountToAdd, i => amountToAdd = i); 

		Label("Filter");
		TextField(filterText, s => filterText = s);

		Label("Favourite");
		RadialButtons(favourites, new[] { "Favourited", "NotFavourited" });

		Label("Ownership");
		RadialButtons(ownershipType, new[] { "Owned", "NotOwned" });
		
		Label("Categories");
		Toggle("Eatables", showEatables, b => showEatables = b);
		Toggle("Fuels", showFuels, b => showFuels = b);
		Toggle("Other", showOther, b => showOther = b);
		
		Label("Other");
		Toggle("Hide Missing Keys", hideEffectsWithMissingKeys, b => hideEffectsWithMissingKeys = b);
		
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
			using (HorizontalScope())
			{
				Image(good.icon);
				Button(isFavourited ? "\u2713" : "X", () =>
				{
					if (isFavourited)
						Plugin.SaveData.favouritedGoods.Remove(good.name);
					else
						Plugin.SaveData.favouritedGoods.Add(good.name);
					SaveDataChanged();
				});

				Button("-", () =>
				{
					storage.Remove(good.name, amountToAdd);
				});

				Button("+", () =>
				{
					storage.Add(good.name, amountToAdd);
				});
				
				Label($"{amount}x\n{good.displayName}");
			}
		
			j++;
			if (j >= rows)
			{
				StartNewColumn();
				j = 0;
			}
		}
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

	private void RadialButtons(bool[] type, string[] buttons)
	{
		using (HorizontalScope())
		{
			for (int i = 0; i < buttons.Length; i++)
			{
				bool o = type[i];
				int index = i;
				Toggle(buttons[i], o, b =>
				{
					type[index] = b;
				});
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