using System.Collections.Generic;
using ATS_API.Helpers;
using DebugMenu;
using DebugMenu.Scripts;
using DebugMenu.Scripts.Acts;
using DebugMenu.Scripts.Popups;
using Eremite;
using Eremite.Characters.Villagers;
using Eremite.Services;

namespace GameMode1;

public class ATSBattleSequence : BaseBattleSequence
{
    public ATSBattleSequence(BaseWindow window) : base(window)
	{
		
	}

	public override void OnGUI()
	{
		ReputationService reputationService = (ReputationService)SO.ReputationService;
		using (window.HorizontalScope(2))
		{
			// Reputation
			using (window.VerticalScope(2))
			{
				window.Label("Reputation: " + reputationService.State.reputation + "/" +
				             reputationService.GetReputationToWin());
				using (window.HorizontalScope(2))
				{
					if (window.Button("+1"))
					{
						reputationService.AddReputationPoints(1, ReputationChangeSource.Other);
					}

					if (window.Button("-1", disabled: () => new ADrawable.ButtonDisabledData("Not working")))
					{
						reputationService.AddReputationPoints(-1, ReputationChangeSource.Other);
					}
				}
			}

			// Impatience
			using (window.VerticalScope(2))
			{
				float impatience = reputationService.State.reputationPenalty;
				int maxImpatience = reputationService.GetReputationPenaltyToLoose();
				window.Label($"Impatience: {impatience:F1}/{maxImpatience:F1}");
				using (window.HorizontalScope(2))
				{
					if (window.Button("+1"))
					{
						reputationService.AddReputationPenalty(1, ReputationChangeSource.Order, false);
					}

					if (window.Button("-1"))
					{
						reputationService.AddReputationPenalty(-1, ReputationChangeSource.Other, false);
					}
				}
			}
		}

		window.Padding();
		
		if (window.Button("Villagers"))
		{
			Plugin.Instance.ToggleWindow<VillagersWindow>();
		}
		
		if (window.Button("Inventory"))
		{
			Plugin.Instance.ToggleWindow<InventoryWindow>();
		}
		
		if (window.Button("Perks"))
		{
			Plugin.Instance.ToggleWindow<PerksWindow>();
		}

		window.Padding();

		using (window.HorizontalScope(2))
		{
			if (window.Button("Win Now"))
			{
				reputationService.ForceGameWin();
			}

			if (window.Button("Lose Now"))
			{
				if (!reputationService.State.finished)
				{
					reputationService.GameLost();
				}
			}
		}
		
		base.OnGUI();
	}
}