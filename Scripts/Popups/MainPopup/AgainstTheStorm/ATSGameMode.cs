using DebugMenu;
using DebugMenu.Scripts.Acts;
using DebugMenu.Scripts.Utils;
using Eremite.Model.State;
using UnityEngine;

namespace GameMode1;

public class ATSGameMode : BaseGameMode
{
	public static bool SkipNextNode = false;
	public static bool ActivateAllMapNodesActive = false;

	public ATSGameMode(DebugWindow window) : base(window)
	{
		m_mapSequence = new ATSMapSequence(this);
		m_battleSequence = new ATSBattleSequence(window);
	}

	public override void OnGUI()
	{
		Window.LabelHeader("Against The Storm");
		
        // Window.StartNewColumn();

        OnGUICurrentNode();
    }

	public override void OnGUIMinimal()
	{
		OnGUICurrentNode();
	}

	private void OnGUICurrentNode()
	{
		if (Helpers.IsInGame)
		{
			// Show game related buttons
			m_battleSequence.OnGUI();
		}
		else
		{
			// Show main menu related buttons
			m_mapSequence.OnGUI();
		}
	}
	
	private void OnGUICardChoiceNodeSequence()
	{
		// CardSingleChoicesSequencer sequencer = Singleton<SpecialNodeHandler>.Instance.cardChoiceSequencer;
		// Window.Label("Sequencer: " + sequencer, new(0, 80));
		// if (Window.Button("Reroll choices"))
		// 	sequencer.OnRerollChoices();
	}

	private void OnGUIMap()
	{
		m_mapSequence.OnGUI();
	}

	public override void Restart()
	{
		
	}

	public override void Reload()
	{
		
	}
}