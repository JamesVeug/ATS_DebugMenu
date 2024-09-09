using System.Collections.Generic;
using ATS_API.Helpers;
using DebugMenu.Scripts.Popups;
using Eremite;
using Eremite.Characters.Villagers;
using Eremite.Model;
using Eremite.Services;
using UnityEngine;

namespace DebugMenu.Scripts.Acts;

public class VillagersWindow : BaseWindow
{
	public override string PopupName => "Villagers";
	public override Vector2 Size => new(650, 420);
	public override bool ClosableWindow => true;

	private Vector2 position;
	
	public override void OnGUI()
	{
		base.OnGUI();
		var raceNames = new List<string>(Serviceable.VillagersService.Races.Keys);
		
		// Longer rows
		ColumnWidth = 400;
		
		int allRaceCount = raceNames.Count; // 20
		int rows = Mathf.Max(Mathf.FloorToInt(Size.y / RowHeight) - 2, 1); // 600 / 40 = 15 
		int columns = Mathf.CeilToInt((float)allRaceCount / rows) + 1; // 20 / 15 = 4
		Rect scrollableAreaSize = new(new Vector2(0, 0), new Vector2(columns *  ColumnWidth + (columns - 1) * 10, rows * RowHeight));
		Rect scrollViewSize = new(new Vector2(0, 0), Size - new Vector2(10, 25));
		position = GUI.BeginScrollView(scrollViewSize, position, scrollableAreaSize);
		
		int j = 0;
		for (int i = 0; i < allRaceCount; i++)
		{
			string raceName = raceNames[i];
			List<Villager> villagers = Serviceable.VillagersService.Races[raceName];
			int raceCount = villagers.Count;
			RaceModel raceModel = raceName.ToRaceModel();

			using (HorizontalScope(5))
			{
				Label(raceModel.icon);
				Label($"{raceCount}x\n{raceName}");

				if (Button("+1"))
				{
					SO.VillagersService.SpawnNewVillager(raceName.ToRaceModel());
				}
				if (Button("1 Leaves") && raceCount > 0)
				{
					villagers[raceCount - 1].Die(VillagerLossType.Leave, "DebugMenu", false);
				}
				if (Button("1 Dies") && raceCount > 0)
				{
					villagers[raceCount - 1].Die(VillagerLossType.Death, "DebugMenu", true);
				}
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
}