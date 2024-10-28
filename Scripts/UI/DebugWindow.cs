using DebugMenu.Scripts.Acts;
using DebugMenu.Scripts.All;
using UnityEngine;

namespace DebugMenu.Scripts.UIToolKit;

public class DebugWindow : CanvasWindow
{
    public override string PopupName => "Debug Menu";
    public override Vector2 Size => new(650, 420);
    public override bool ClosableWindow => false;

    public BaseGameMode CurrentGameMode => currentGameMode;
    public AllGameModes AllGameModes => allGameModes;

    private BaseGameMode currentGameMode = null;

    private AllGameModes allGameModes;
    private GameMode1.ATSGameMode _atsGameMode;
    
    public override void CreateGUI()
    {
        Plugin.Log.LogInfo("Creating Debug Window");
        base.CreateGUI();
        
        // allGameModes = new AllGameModes(this);
        // allGameModes.CreateGUI();
        //
        // _atsGameMode = new GameMode1.ATSGameMode(this);
        // _atsGameMode.CreateGUI();
    }
    
    public override void Update()
    {
        base.Update();
        
        // Stubbed in case ATS gets a DLC or separate game mode
        // currentGameMode = _atsGameMode;
        // _atsGameMode?.Update();
    }
}