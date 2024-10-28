using DebugMenu.Scripts.Acts;
using DebugMenu.Scripts.UIToolKit;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameMode1;

public class ATSMapSequence : BaseMapSequence
{
	public static bool RegionOverride = false;
	public static string RegionNameOverride = "No region selected";  
	
	private readonly ATSGameMode GameMode = null;
	private readonly DebugWindow Window = null;

	public ATSMapSequence(ATSGameMode gameMode)
	{
		this.GameMode = gameMode;
		this.Window = gameMode.Window;
	}

	public override void CreateGUI(RectTransform scopeContainer)
	{
		base.CreateGUI(scopeContainer);
		// TODO:
		// bool skipNextNode = ATSGameMode.SkipNextNode;
		// if (Window.Toggle("Skip next node", ref skipNextNode))
		// 	ToggleSkipNextNode();
		//
		// bool activateAllNodes = ATSGameMode.ActivateAllMapNodesActive;
		// if (Window.Toggle("Activate all Map nodes", ref activateAllNodes))
		// 	ToggleAllNodes();
  //
  //       Window.Toggle("Toggle Map Override", ref RegionOverride);
    }

	public override void Update()
	{
		
	}

	public override void ToggleSkipNextNode()
	{
		ATSGameMode.SkipNextNode = !ATSGameMode.SkipNextNode;
	}

	public override void ToggleAllNodes()
	{
		ATSGameMode.ActivateAllMapNodesActive = !ATSGameMode.ActivateAllMapNodesActive;
	}
}