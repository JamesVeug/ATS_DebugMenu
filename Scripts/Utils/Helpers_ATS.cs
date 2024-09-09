using Eremite;
using Eremite.Controller;
using HarmonyLib;

namespace DebugMenu.Scripts.Utils;

public static partial class Helpers
{
    public static bool IsInGame => GameController.IsGameActive;
}