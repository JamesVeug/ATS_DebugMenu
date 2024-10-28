using DebugMenu;
using DebugMenu.Scripts.Acts;
using Eremite;
using Eremite.Services;
using TMPro;
using UnityEngine;

namespace GameMode1;

public class ATSBattleSequence : BaseBattleSequence
{
	private TMP_Text resoluteLabel;
	private TMP_Text impatienceLabel;
	
    public ATSBattleSequence(CanvasWindow window) : base(window)
	{
		
	}

	public override void CreateGUI(RectTransform scopeContainer)
	{
		base.CreateGUI(scopeContainer);
		
		using (window.HorizontalScope())
		{
			// Reputation
			using (window.VerticalScope())
			{
				resoluteLabel = window.Label("Reputation: ");
				using (window.HorizontalScope())
				{
					window.Button("+1", () =>
					{
						ReputationService reputationService = (ReputationService)SO.ReputationService;
						reputationService.AddReputationPoints(1, ReputationChangeSource.Other);
					});

					window.Button("-1", () =>
					{
						ReputationService reputationService = (ReputationService)SO.ReputationService;
						reputationService.AddReputationPoints(-1, ReputationChangeSource.Other);
					}, disabled: () => (true, "Not working"));
				}
			}

			// Impatience
			using (window.VerticalScope())
			{
				impatienceLabel = window.Label($"Impatience: ");
				using (window.HorizontalScope())
				{
					window.Button("+1", () =>
					{
						ReputationService reputationService = (ReputationService)SO.ReputationService;
						reputationService.AddReputationPenalty(1, ReputationChangeSource.Order, false);
					});

					window.Button("-1", () =>
					{
						ReputationService reputationService = (ReputationService)SO.ReputationService;
						reputationService.AddReputationPenalty(-1, ReputationChangeSource.Other, false);
					});
				}
			}
		}

		window.Padding();

		window.Button("Villagers", () =>
		{
			Plugin.Instance.ToggleWindow<VillagersWindow>();
		});

		window.Button("Inventory", () =>
		{
			Plugin.Instance.ToggleWindow<InventoryWindow>();
		});

		window.Button("Perks", () =>
		{
			Plugin.Instance.ToggleWindow<PerksWindow>();
		});

		window.Padding();

		using (window.HorizontalScope())
		{
			window.Button("Win Now", () =>
			{
				ReputationService reputationService = (ReputationService)SO.ReputationService;
				reputationService.ForceGameWin();
			});

			window.Button("Lose Now", () =>
			{
				ReputationService reputationService = (ReputationService)SO.ReputationService;
				if (!reputationService.State.finished)
				{
					reputationService.GameLost();
				}
			});
		}
	}

	public override void Update()
	{
		ReputationService reputationService = (ReputationService)SO.ReputationService;
		resoluteLabel.text = "Reputation: " + reputationService.State.reputation + "/" + reputationService.GetReputationToWin();
		
		float impatience = reputationService.State.reputationPenalty;
		int maxImpatience = reputationService.GetReputationPenaltyToLoose();
		impatienceLabel.text = $"Impatience: {impatience:F1}/{maxImpatience:F1}";
	}
}