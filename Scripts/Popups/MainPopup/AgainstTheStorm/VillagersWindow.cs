using System.Collections.Generic;
using ATS_API.Helpers;
using DebugMenu.Scripts.Popups;
using Eremite;
using Eremite.Characters.Villagers;
using Eremite.Model;
using Eremite.Services;
using UnityEngine;

namespace DebugMenu.Scripts.Acts;

public class VillagersWindow : CanvasWindow
{
	public override string PopupName => "Villagers";
	public override Vector2 Size => new(650, 420);
	public override bool ClosableWindow => true;
	
	public override void CreateGUI()
	{
		base.CreateGUI();
		var raceNames = new List<string>(Serviceable.VillagersService.Races.Keys);
		
		// Longer rows
		ColumnWidth = 400;
		
		int allRaceCount = raceNames.Count; // 20
		int rows = Mathf.Max(Mathf.FloorToInt(Size.y / RowHeight) - 2, 1); // 600 / 40 = 15 
		
		int j = 0;
		for (int i = 0; i < allRaceCount; i++)
		{
			string raceName = raceNames[i];
			List<Villager> villagers = Serviceable.VillagersService.Races[raceName];
			int raceCount = villagers.Count;
			RaceModel raceModel = raceName.ToRaceModel();

			using (HorizontalScope())
			{
				Image(raceModel.icon);
				Label($"{raceCount}x\n{raceName}");

				Button("+1", ()=>
				{
					SO.VillagersService.SpawnNewVillager(raceName.ToRaceModel());
				});

				Button("1 Leaves", () =>
				{
					if (raceCount > 0)
					{
						villagers[raceCount - 1].Die(VillagerLossType.Leave, "DebugMenu", false);
					}
				});

				Button("1 Dies", () =>
				{
					if (raceCount > 0)
					{
						villagers[raceCount - 1].Die(VillagerLossType.Death, "DebugMenu", true);
					}
				});
			}
		
			j++;
			if (j >= rows)
			{
				StartNewColumn();
				j = 0;
			}
		}
	}
}