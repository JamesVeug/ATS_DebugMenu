// using DebugMenu.Scripts.All;
// using DebugMenu.Scripts.Popups;
// using UnityEngine;
//
// namespace DebugMenu.Scripts.Acts;
//
// public class DebugWindowOld : BaseWindow
// {
// 	public override string PopupName => "Debug Menu";
// 	public override Vector2 Size => new(650, 420);
// 	public override bool ClosableWindow => false;
// 	public BaseGameMode CurrentGameMode => currentGameMode;
// 	public AllGameModes AllGameModes => allGameModes;
//
// 	private BaseGameMode currentGameMode = null;
// 	private ToggleStates currentState = ToggleStates.Off;
//
// 	private readonly AllGameModes allGameModes;
// 	private readonly GameMode1.ATSGameMode _atsGameMode;
// 	private Vector2 position;
//
// 	public DebugWindowOld() : base()
// 	{
// 		allGameModes = new AllGameModes(this);
// 		_atsGameMode = new GameMode1.ATSGameMode(this);
// 		IsActive = true;
// 		isOpen = false;
// 	}
//
// 	public override void Update()
// 	{
// 		// Stubbed in case ATS gets a DLC or separate game mode
// 		currentGameMode = _atsGameMode;
// 		_atsGameMode?.Update();
//     }
//
// 	public override void OnGUI()
// 	{
// 		float scrollAreaWidth = Mathf.Max(TotalWidth, windowRect.width);
// 		float scrollAreaHeight = Mathf.Max(Height, windowRect.y);
// 		Rect contentSize = new(new Vector2(0, 0), new Vector2(scrollAreaWidth, scrollAreaHeight));
// 		Rect viewportSize = new(new Vector2(0, 0), Size - new Vector2(10, 0));
// 		position = GUI.BeginScrollView(viewportSize, position, contentSize);
// 		
// 		DrawToggleButtons();
// 		if (currentState > ToggleStates.Off)
// 		{
// 			base.OnGUI();
// 			if (currentState == ToggleStates.All || currentGameMode == null)
// 			{
// 				allGameModes.OnGUI();
// 				StartNewColumn();
// 			}
//
// 			if (currentGameMode != null)
// 			{
// 				if (currentState == ToggleStates.Minimal)
// 					currentGameMode.OnGUIMinimal();
// 				else
// 					currentGameMode.OnGUI();
// 			}
// 		}
//
// 		GUI.EndScrollView();
// 	}
//
// 	private void DrawToggleButtons()
// 	{
// 		if (GUI.Button(new Rect(5f, 0f, 20f, 20f), "-"))
// 		{
// 			currentState = ToggleStates.Off;
// 			position = Vector2.zero;
// 		}
//
// 		if (GUI.Button(new Rect(25f, 0f, 20f, 20f), "+"))
// 		{
// 			currentState = ToggleStates.Minimal;
// 		}
//
// 		if (GUI.Button(new Rect(45F, 0f, 25f, 20f), "X"))
// 		{
// 			currentState = ToggleStates.All;
// 		}
// 	}
//
// 	protected override bool OnToggleWindowDraw()
// 	{
// 		switch (currentState)
// 		{
// 			case ToggleStates.Off:
// 				windowRect.Set(windowRect.x, windowRect.y, 100, 60);
// 				break;
// 			case ToggleStates.Minimal:
// 				windowRect.Set(windowRect.x, windowRect.y, ColumnWidth+40, Size.y);
// 				break;
// 			case ToggleStates.All:
// 				windowRect.Set(windowRect.x, windowRect.y, Size.x, Size.y);
// 				break;
// 		}
// 		BeginDrawingGUI();
// 		return false;
// 	}
// }