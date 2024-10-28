using System.Linq;
using DebugMenu;
using DebugMenu.Scripts.Acts;
using Eremite.Model;
using Eremite.Services;
using HarmonyLib;

namespace GameMode1;

[HarmonyPatch]
public class ATSMapSequence : BaseMapSequence
{
	public static bool SkipNextNode = false;
	public static bool UnlockAllDifficulties = false;
	public static bool ActivateAllMapNodesActive = false;
	
	public static bool RegionOverride = false;
	public static string RegionNameOverride = "No region selected";  
	
	private readonly ATSGameMode GameMode = null;
	private readonly DebugWindow Window = null;

	public ATSMapSequence(ATSGameMode gameMode)
	{
		this.GameMode = gameMode;
		this.Window = gameMode.Window;
	}

	public override void OnGUI()
	{
		Window.LabelHeader("WorldMap");
		
		Window.Toggle("Unlock all Difficulties", ref UnlockAllDifficulties);

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

	public override void ToggleSkipNextNode()
	{
		SkipNextNode = !SkipNextNode;
	}

	public override void ToggleAllNodes()
	{
		ActivateAllMapNodesActive = !ActivateAllMapNodesActive;
	}

	[HarmonyPatch(typeof(MetaConditionsService), nameof(MetaConditionsService.GetMaxUnlockedDifficulty))]
	[HarmonyPrefix]
	private static bool ShowAllDifficulties(ref DifficultyModel __result)
	{
		if (UnlockAllDifficulties)
		{
			DifficultyModel difficulties = Serviceable.Settings.difficulties.
				Where(a=>a.canBePicked).
				OrderByDescending(a=>a.index).
				FirstOrDefault();
			
			__result = difficulties;
			return false;
		}
		return true;
	}
}