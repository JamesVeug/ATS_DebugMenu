using BepInEx.Logging;

namespace DebugMenu.Scripts.Acts;

public abstract class BaseGameMode
{
    public BaseBattleSequence BattleSequence => m_battleSequence;
    public BaseMapSequence MapSequence => m_mapSequence;

    public readonly ManualLogSource Logger;
    public readonly DebugWindow Window;

    protected BaseBattleSequence m_battleSequence;
    protected BaseMapSequence m_mapSequence;

    public BaseGameMode(DebugWindow window)
    {
        Window = window;
        Logger = Plugin.Log;
    }

    public virtual void Update() { }

    public abstract void OnGUI();
    public abstract void OnGUIMinimal();

    public abstract void Reload();
    public abstract void Restart();

    public void Log(string log) => Logger.LogInfo($"[{GetType().Name}] {log}");
    public void Warning(string log) => Logger.LogWarning($"[{GetType().Name}] {log}");
    public void Error(string log) => Logger.LogError($"[{GetType().Name}] {log}");

    public void DrawItemsGUI()
    {
        
    }

}