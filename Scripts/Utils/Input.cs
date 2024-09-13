using DebugMenu;
using DebugMenu.Scripts.Acts;
using Eremite;
using Eremite.Services;
using Eremite.View.Cameras;
using HarmonyLib;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[HarmonyPatch]
public static class Input
{
    private static bool blockAllInput = false;
    
    public static InputActionAsset InputAsset;
    private static GameObject blockerParent;

    public static void Initialize()
    {
        blockerParent = new("DebugMenuFullScreenBlocker");
        blockerParent.layer = LayerMask.NameToLayer("UI");
        var blockerParentCanvas = blockerParent.AddComponent<Canvas>();
        blockerParentCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        blockerParentCanvas.sortingOrder = 32766;
        blockerParent.AddComponent<CanvasScaler>();
        blockerParent.AddComponent<GraphicRaycaster>();
        blockerParent.AddComponent<Image>().color = new Color(0,0,1,0.0f);
        blockerParent.transform.SetParent(Plugin.Instance.transform);
        blockerParent.SetActive(false);
    }
    
    public static void DrawToggleBlockInput(DebugWindow window)
    {
        if (window.Toggle("Block All Input", ref blockAllInput))
        {
            Plugin.Log.LogInfo($"Block all input: {blockAllInput}");
            blockerParent.SetActive(blockAllInput);
        }
    }
    
    [HarmonyPatch(typeof(InputConfig), MethodType.Constructor)]
    [HarmonyPostfix]
    private static void HookMainControllerSetup(InputConfig __instance)
    {
        InputAsset = __instance.asset;
    }
    
    [HarmonyPatch(typeof(InputService), nameof(InputService.IsTriggering))]
    [HarmonyPrefix]
    private static bool InputService_IsTriggering()
    {
        return !blockAllInput; // Don't allow any input if blockAllInput is true
    }
    
    [HarmonyPatch(typeof(InputService), nameof(InputService.WasTriggered))]
    [HarmonyPrefix]
    private static bool InputService_WasTriggered()
    {
        return !blockAllInput; // Don't allow any input if blockAllInput is true
    }
    
    [HarmonyPatch(typeof(InputService), nameof(InputService.WasTriggeredAny))]
    [HarmonyPrefix]
    private static bool InputService_WasTriggeredAny()
    {
        return !blockAllInput; // Don't allow any input if blockAllInput is true
    }
    
    [HarmonyPatch(typeof(InputService), nameof(InputService.WasCanceled))]
    [HarmonyPrefix]
    private static bool InputService_WasCanceled()
    {
        return !blockAllInput; // Don't allow any input if blockAllInput is true
    }
    
    [HarmonyPatch(typeof(InputService), nameof(InputService.IsOverUI))]
    [HarmonyPrefix]
    private static bool InputService_IsOverUI()
    {
        return !blockAllInput; // Don't allow any input if blockAllInput is true
    }
    
    [HarmonyPatch(typeof(InputService), nameof(InputService.GetKey))]
    [HarmonyPrefix]
    private static bool InputService_GetKey()
    {
        return !blockAllInput; // Don't allow any input if blockAllInput is true
    }
    
    [HarmonyPatch(typeof(InputService), nameof(InputService.GetKeyDown))]
    [HarmonyPrefix]
    private static bool InputService_GetMouseButton()
    {
        return !blockAllInput; // Don't allow any input if blockAllInput is true
    }
    
    [HarmonyPatch(typeof(InputService), nameof(InputService.IsValidBind))]
    [HarmonyPrefix]
    private static bool InputService_IsValidBind()
    {
        return !blockAllInput; // Don't allow any input if blockAllInput is true
    }
    
    [HarmonyPatch(typeof(InputService), nameof(InputService.GetMouseButtonRaw))]
    [HarmonyPrefix]
    private static bool InputService_GetMouseButtonRaw()
    {
        return !blockAllInput; // Don't allow any input if blockAllInput is true
    }
    
    [HarmonyPatch(typeof(InputService), nameof(InputService.GetMouseButtonDownRaw))]
    [HarmonyPrefix]
    private static bool InputService_GetMouseButtonDownRaw()
    {
        return !blockAllInput; // Don't allow any input if blockAllInput is true
    }
    
    [HarmonyPatch(typeof(InputService), nameof(InputService.IsWorldDragging))]
    [HarmonyPrefix]
    private static bool InputService_IsWorldDragging()
    {
        return !blockAllInput; // Don't allow any input if blockAllInput is true
    }
    
    [HarmonyPatch(typeof(InputService), nameof(InputService.IsLocked))]
    [HarmonyPrefix]
    private static bool InputService_IsLocked(ref bool __result)
    {
        if (blockAllInput)
        {
            __result = true;
            return false;
        }
        return true; // Don't allow any input if blockAllInput is true
    }
    
    [HarmonyPatch(typeof(CameraController), nameof(CameraController.EdgeScrollingEnabled))]
    [HarmonyPrefix]
    private static bool CameraController_EdgeScrollingEnabled()
    {
        return !blockAllInput; // Don't allow any input if blockAllInput is true
    }
}