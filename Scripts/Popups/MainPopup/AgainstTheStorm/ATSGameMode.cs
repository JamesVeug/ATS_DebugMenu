using DebugMenu;
using DebugMenu.Scripts.Acts;
using DebugMenu.Scripts.Utils;
using Eremite.Model.State;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameMode1;

public class ATSGameMode : BaseGameMode
{
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
		string mapName = SceneManager.GetActiveScene().name;
		if (mapName == "Game")
		{
			// Show game related buttons
			m_battleSequence.OnGUI();
		}
		else if (mapName == "WorldMap")
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