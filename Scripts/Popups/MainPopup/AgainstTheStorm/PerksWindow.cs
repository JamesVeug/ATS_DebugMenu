using System;
using System.Collections.Generic;
using System.Linq;
using DebugMenu.Scripts.Utils;
using Eremite;
using Eremite.Model;
using Eremite.Model.State;
using Eremite.Services;
using UnityEngine;

namespace DebugMenu.Scripts.Acts;

public class PerksWindow : CanvasWindow
{
	private enum EffectType
	{
		Effect,
		Altar,
		Seasonal
	}

	private class Effect
	{
		public EffectModel model;
		public EffectType type;

		public Effect(EffectType effect, EffectModel effectModel)
		{
			type = effect;
			model = effectModel;
		}
	}
	
	public override string PopupName => "Perks";
	public override Vector2 Size => new(1000, 800);
	public override bool ClosableWindow => true;

	private Dictionary<string, Effect> allEffectsLookup = null;
	private List<Effect> orderedEffects = null;
	
	private Vector2 position;
	private string filterText = "";
	private bool hideEffectsWithMissingKeys = true;
	private bool[] favourites = new bool[]{true,true};
	private bool[] ownershipType = new bool[]{true,true};
	private bool[] benefitType = new bool[]{true,true};
	private bool[] effectType = new bool[]{false,false,true};
	private Dictionary<EffectRarity, bool> rarityFilter = new Dictionary<EffectRarity, bool>();

	public override void CreateGUI()
	{
		base.CreateGUI();

		EffectsService effectsService = GameService.EffectsService as EffectsService;
		if (effectsService == null)
		{
			Plugin.Log.LogError("EffectsService is null");
			return;
		}
		
		PerksService perksService = GameMB.PerksService as PerksService;
		if (perksService == null)
		{
			Plugin.Log.LogError("PerksService is null");
			return;
		}

		if (allEffectsLookup == null)
		{
			SetupEffectLookup();
		}

		List<PerkState> ownedPerks = perksService.SortedPerks;

		// Longer rows
		ColumnWidth = 200;

		const int scrollableColumnWidth = 300;
		int namesCount = orderedEffects.Count; // 20
		int rows = Mathf.Max(Mathf.FloorToInt((Size.y) / RowHeight) - 1, 1); // 600 / 40 = 15 
		int columns = Mathf.CeilToInt((float)namesCount / rows) + 1; // 20 / 15 = 4
		Rect scrollableAreaSize = new(new Vector2(0, 0), new Vector2(columns *  scrollableColumnWidth + (columns - 1) * 10, rows * RowHeight));
		Rect scrollViewSize = new(new Vector2(0, 0), Size - new Vector2(10, 25));
		position = GUI.BeginScrollView(scrollViewSize, position, scrollableAreaSize);

		DrawFilters();

		StartNewColumn();
		
		// Longer rows
		ColumnWidth = scrollableColumnWidth;
		
		int j = 0;
		for (int i = 0; i < namesCount; i++)
		{
			EffectModel cornerStone = orderedEffects[i].model;
			bool isFavourited = Plugin.SaveData.favouritedPerks.Contains(cornerStone.name);
			if (!FilterEffect(cornerStone, orderedEffects, i, ownedPerks, isFavourited, out PerkState ownedPerk)) 
				continue;

			int stacks = ownedPerk == null ? 0 : ownedPerk.stacks;
			using (HorizontalScope())
			{
				Image(cornerStone.GetIcon());
				Button(isFavourited ? "\u2713" : "X", () =>
				{
					if (isFavourited)
						Plugin.SaveData.favouritedPerks.Remove(cornerStone.name);
					else
						Plugin.SaveData.favouritedPerks.Add(cornerStone.name);
					SaveDataChanged();
				});

				Button("-1", () =>
				{
					cornerStone.Remove();
				});

				Button("+1", () =>
				{
					cornerStone.Apply();
				});
				
				Label($"{stacks}x ({cornerStone.rarity})\n{cornerStone.DisplayName}");
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

	private void DrawFilters()
	{
		Label("Filter");
		TextField(filterText, s => filterText = s);

		Label("Favourite");
		RadialButtons(favourites, new[] { "Favourited", "NotFavourited" });
		
		Label("Ownership");
		RadialButtons(ownershipType, new[] { "Owned", "NotOwned" });
		
		Label("Benefit");
		RadialButtons(benefitType, new[] { "Positive", "Negative" });

		Label("Type");
		RadialButtons(effectType, Enum.GetNames(typeof(EffectType)).ToArray());
		
		Label("Rarity");
		foreach (EffectRarity rarity in Enum.GetValues(typeof(EffectRarity)))
		{
			RarityToggle(rarity);
		}
		
		Label("Other");
		Toggle("Hide Missing Keys", hideEffectsWithMissingKeys, (a)=>hideEffectsWithMissingKeys=a);
	}

	private void RarityToggle(EffectRarity rarity)
	{
		bool o = rarityFilter[rarity];
		Toggle(rarity.ToString(), o, (b) =>
		{
			rarityFilter[rarity] = b;
		});
	}

	private bool FilterEffect(EffectModel cornerStone, List<Effect> elements, int i, List<PerkState> ownedPerks, bool favourited,
		out PerkState perk)
	{
		perk = null;
		if (!string.IsNullOrEmpty(filterText))
		{
			if (!cornerStone.DisplayName.ContainsText(filterText, false) &&
			    !cornerStone.Description.ContainsText(filterText, false))
			{
				return false;
			}
		}
			
		if (!favourited && !favourites[1])
			return false;
		if (favourited && !favourites[0])
			return false;
		if (!cornerStone.IsPositive && !benefitType[1])
			return false;
		if (cornerStone.isPositive && !benefitType[0])
			return false;
		if (!effectType[(int)elements[i].type])
			return false;
		if (hideEffectsWithMissingKeys && string.IsNullOrEmpty(cornerStone.DisplayNameKey))
			return false;
		if (rarityFilter[cornerStone.rarity] == false)
			return false;
		
			
		perk = ownedPerks.FirstOrDefault(a=>a.name == cornerStone.Name);
		if (perk == null && !ownershipType[1])
			return false;
		if (perk != null && !ownershipType[0])
			return false;
		
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
				Toggle(buttons[i], o, (a) =>
				{
					type[index] = a;
				});
			}
		}
	}

	private IEnumerable<Effect> GetAllEffects()
	{
		foreach (EffectModel effect in SO.Settings.effects)
		{
			yield return new Effect(EffectType.Effect, effect);
		}
		foreach (AltarEffectModel effect in SO.Settings.altarEffects)
		{
			yield return new Effect(EffectType.Altar, effect.upgradedEffect);
			yield return new Effect(EffectType.Altar, effect.regularEffect);
		}

		var effectModels = (from e in Serviceable.StateService.Conditions.earlyEffects
				.Concat(Serviceable.StateService.Conditions.lateEffects)
				.Concat(Serviceable.WorldStateService.Cycle.activeEventsEffects)
			select Serviceable.Settings.GetEffect(e));
		
		var modifiers = Serviceable.BiomeService.Difficulty.modifiers;
		
		IEnumerable<EffectModel> models = (from e in effectModels
			.Concat(Serviceable.Biome.effects)
			.Concat(Serviceable.Biome.seasons.SeasonRewards.SelectMany((SeasonRewardModel m) =>
				m.effectsTable.GetAllEffects()))
			.Concat(Serviceable.Biome.earlyEffects)
			orderby e.IsPositive, e.DisplayName
			select e).Concat(from e in modifiers select e.effect);
		foreach (var effect in models)
		{
			yield return new Effect(EffectType.Seasonal, effect);
		}
	}

	private void SetupEffectLookup()
	{
		Plugin.Log.LogInfo("Finding all effects...");
		allEffectsLookup = new Dictionary<string, Effect>();
		foreach (Effect effect in GetAllEffects())
		{
			if (effect.model != null)
			{
				allEffectsLookup[effect.model.Name] = effect;
			}
		}
		
		orderedEffects = allEffectsLookup.Values.ToList();
		SortEffects();
		Plugin.Log.LogInfo("AllEffects count: " + orderedEffects.Count);
		
		// Setup rarity filter
		foreach (EffectRarity a in Enum.GetValues(typeof(EffectRarity)))
		{
			rarityFilter[a] = true;
		}
	}

	private void SortEffects()
	{
		orderedEffects.Sort(static (a, b) =>
		{
			bool aFavourited = Plugin.SaveData.favouritedPerks.Contains(a.model.Name);
			bool bFavourited = Plugin.SaveData.favouritedPerks.Contains(b.model.Name);
			if(aFavourited != bFavourited)
				return aFavourited ? -1 : 1;
			
			if(a.model.IsPositive != b.model.IsPositive)
				return a.model.IsPositive ? -1 : 1;
			
			return a.model.DisplayName.CompareTo(b.model.DisplayName);
		});
	}

	private void SaveDataChanged()
	{
		Plugin.SaveData.Save();
		SortEffects();
	}
}