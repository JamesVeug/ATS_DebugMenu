using System;
using System.Collections.Generic;
using System.Linq;
using DebugMenu.Scripts.Popups;
using DebugMenu.Scripts.Utils;
using Eremite;
using Eremite.Model;
using Eremite.Model.State;
using Eremite.Services;
using UnityEngine;
using ErrorEventArgs = Newtonsoft.Json.Serialization.ErrorEventArgs;

namespace DebugMenu.Scripts.Acts;

public class PerksWindow : BaseWindow
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
	private List<Effect> allEffects = null;
	
	private Vector2 position;
	private string filterText = "";
	private bool hideEffectsWithMissingKeys = true;
	private bool[] ownershipType = new bool[]{true,true};
	private bool[] benefitType = new bool[]{true,true};
	private bool[] effectType = new bool[]{false,false,true};
	private Dictionary<EffectRarity, bool> rarityFilter = new Dictionary<EffectRarity, bool>();

	public override void OnGUI()
	{
		base.OnGUI();

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
		int namesCount = allEffects.Count; // 20
		int rows = Mathf.Max(Mathf.FloorToInt((Size.y - TopOffset) / RowHeight) - 1, 1); // 600 / 40 = 15 
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
			EffectModel cornerStone = allEffects[i].model;
			if (!FilterEffect(cornerStone, allEffects, i, ownedPerks, out PerkState ownedPerk)) 
				continue;

			int stacks = ownedPerk == null ? 0 : ownedPerk.stacks;
			using (HorizontalScope(3))
			{
				Label(cornerStone.GetIcon());
				if(Button("-1", new Vector2(30, 0)))
				{
					cornerStone.Remove();
					
				}
				if(Button("+1", new Vector2(30, 0)))
				{
					cornerStone.Apply();
				}
				
				Vector2 nameSize = new Vector2(ColumnWidth - RowHeight*3, RowHeight);
				Label($"{stacks}x ({cornerStone.rarity})\n{cornerStone.DisplayName}", nameSize);
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
		Label("Filter", new(0, RowHeight / 2));
		filterText = TextField(filterText, new(0, RowHeight / 2));

		Label("Ownership", new(0, RowHeight / 2));
		RadialButtons(ownershipType, new[] { "Owned", "NotOwned" }, RowHeight / 2);
		
		Label("Benefit", new(0, RowHeight / 2));
		RadialButtons(benefitType, new[] { "Positive", "Negative" }, RowHeight / 2);

		Label("Type", new(0, RowHeight / 2));
		RadialButtons(effectType, Enum.GetNames(typeof(EffectType)).ToArray(), RowHeight / 2);
		
		Label("Rarity", new(0, RowHeight / 2));
		foreach (EffectRarity rarity in Enum.GetValues(typeof(EffectRarity)))
		{
			RarityToggle(rarity);
		}
		
		Label("Other", new(0, RowHeight / 2));
		Toggle("Hide Missing Keys", ref hideEffectsWithMissingKeys);
	}

	private void RarityToggle(EffectRarity rarity)
	{
		bool o = rarityFilter[rarity];
		if (Toggle(rarity.ToString(), ref o, new Vector2(ColumnWidth / 2, RowHeight / 2)))
		{
			rarityFilter[rarity] = !rarityFilter[rarity];
		}
	}

	private bool FilterEffect(EffectModel cornerStone, List<Effect> elements, int i, List<PerkState> ownedPerks, out PerkState perk)
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
			
		if (!cornerStone.IsPositive && !benefitType[1])
			return false;
		if (cornerStone.isPositive && !benefitType[0])
			return false;
		if (!effectType[(int)elements[i].type])
			return false;
		if (hideEffectsWithMissingKeys && string.IsNullOrEmpty(cornerStone.DisplayNameKey))
			return false;
		if(rarityFilter[cornerStone.rarity] == false)
			return false;
		
			
		perk = ownedPerks.FirstOrDefault(a=>a.name == cornerStone.Name);
		if (perk == null && !ownershipType[1])
			return false;
		if (perk != null && !ownershipType[0])
			return false;
		
		return true;
	}

	private static void Error(object sender, ErrorEventArgs e)
	{
		Plugin.Log.LogError(e.ErrorContext.Error.Message);
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

		// foreach (SimpleSeasonalEffectModel effect in SO.Settings.simpleSeasonalEffects)
		// {
		// 	yield return new Effect(EffectType.SeasonalEffect, effect.effect);
		// }
		
        
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
		
		allEffects = allEffectsLookup.Values
			.OrderBy((a)=>a.model.IsPositive)
			.ThenBy((a)=>a.model.DisplayName)
			.ToList();
		Plugin.Log.LogInfo("AllEffects count: " + allEffects.Count);
		// DumpPerksToJSON(allEffects);
		
		Enum.GetValues(typeof(EffectRarity)).Cast<EffectRarity>().ToList().ForEach(a=>rarityFilter[a] = true);
	}
}