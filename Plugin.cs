using System;
using System.Collections.Generic;
using System.IO;
using ATS_API;
using ATS_API.Helpers;
using BepInEx;
using BepInEx.Logging;
using DebugMenu.Scripts.UIToolKit;
using Eremite;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;
using Image = UnityEngine.UI.Image;

namespace DebugMenu
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
	    public const string PluginGuid = "jamesgames.againstthestorm.debugmenu";
	    public const string PluginName = "Debug Menu";
	    public const string PluginVersion = "2.0.0";

	    public static Plugin Instance;
	    public static ManualLogSource Log;
	    public static InputActionAsset InputAsset;
	    
	    public static string PluginDirectory;
	    public static float StartingFixedDeltaTime;

	    public static List<CanvasWindow> AllWindows = new();
	    public static SaveData SaveData = null;
	    
	    private Harmony harmony;
	    private GameObject blockerParent = null;
	    private Canvas blockerParentCanvas = null;
	    private List<WindowBlocker> activeRectTransforms = new List<WindowBlocker>();
	    private List<WindowBlocker> rectTransformPool = new List<WindowBlocker>();

	    private Canvas windowCanvas;
	    private CanvasPrefabData canvasPrefabData = null;

	    private void Awake()
	    {
		    Instance = this;
		    Log = Logger;
		    StartingFixedDeltaTime = Time.fixedDeltaTime;
		    
		    gameObject.hideFlags = HideFlags.HideAndDontSave;
		    DontDestroyOnLoad(gameObject);

		    PluginDirectory = this.Info.Location.Replace("DebugMenu.dll", "");

		    blockerParent = new("DebugMenuWindowBlocker");
		    blockerParent.layer = LayerMask.NameToLayer("UI");
		    blockerParentCanvas = blockerParent.AddComponent<Canvas>();
		    blockerParentCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
		    blockerParentCanvas.sortingOrder = 32767;
		    blockerParent.AddComponent<CanvasScaler>();
		    blockerParent.AddComponent<GraphicRaycaster>();
		    blockerParent.transform.SetParent(transform);

		    
		    AssetBundle bundle = AssetBundle.LoadFromFile(Path.Combine(PluginDirectory, "Assets", "debugmenu"));
		    if (bundle != null)
		    {
			    canvasPrefabData = new CanvasPrefabData(); 
			    Log.LogInfo("Loading canvas");
			    canvasPrefabData.WindowParent = Instantiate(bundle.LoadAsset<GameObject>("Canvas"), transform).transform;
			    Assert(canvasPrefabData.WindowParent != null, "canvasPrefabData.WindowParent != null");
			    
			    Log.LogInfo("Loading row");
			    canvasPrefabData.RowPrefab = bundle.LoadAsset<GameObject>("Row").SafeGetComponent<RectTransform>();
			    Assert(canvasPrefabData.RowPrefab != null, "canvasPrefabData.RowPrefab != null");
			    
			    Log.LogInfo("Loading column");
			    canvasPrefabData.ColumnPrefab = bundle.LoadAsset<GameObject>("Column").SafeGetComponent<RectTransform>();
			    Assert(canvasPrefabData.ColumnPrefab != null, "canvasPrefabData.ColumnPrefab != null");
			    
			    Log.LogInfo("Loading Window");
			    canvasPrefabData.WindowPrefab = bundle.LoadAsset<GameObject>("Window");
			    Assert(canvasPrefabData.WindowPrefab != null, "canvasPrefabData.WindowPrefab != null");
			    
			    Log.LogInfo("Loading button");
			    canvasPrefabData.ButtonPrefab = bundle.LoadAsset<GameObject>("Button").SafeGetComponent<Button>();
			    Assert(canvasPrefabData.ButtonPrefab != null, "canvasPrefabData.ButtonPrefab != null");
			    
			    Log.LogInfo("Loading label");
			    canvasPrefabData.LabelPrefab = bundle.LoadAsset<GameObject>("Label").SafeGetComponent<TMP_Text>();
			    Assert(canvasPrefabData.LabelPrefab != null, "canvasPrefabData.LabelPrefab != null");
			    
			    Log.LogInfo("Loading label header");
			    canvasPrefabData.HeaderLabelPrefab = canvasPrefabData.LabelPrefab; // TODO:
			    
			    Log.LogInfo("Loading toggle");
			    canvasPrefabData.TogglePrefab = bundle.LoadAsset<GameObject>("Toggle").SafeGetComponent<Toggle>();
			    Assert(canvasPrefabData.TogglePrefab != null, "canvasPrefabData.TogglePrefab != null");
			    
			    Log.LogFatal("Done");
		    }
		    else
		    {
			    Log.LogError("Failed to load asset bundle");
			    return;
		    }
		    
		    Input.Initialize();

		    
		    // Load save data last so if it exceptions then the plugin still loads
		    try
		    {
			    SaveData = SaveData.Load();
		    } 
		    catch (Exception e)
		    {
			    Log.LogError($"Failed to load save data: {e}");
			    SaveData = new SaveData();
		    }
		    
		    
		    Hotkeys.RegisterKey(PluginGuid, "OpenWindow", "Open Debug Menu", [KeyCode.Keypad9], () =>
		    {
			    Logger.LogInfo("Opening Debug Menu");
			    ToggleWindow<DebugWindow>();
		    });
		    


		    harmony = Harmony.CreateAndPatchAll(typeof(Plugin).Assembly, PluginGuid);
		    Logger.LogInfo($"Loaded {PluginName}");
	    }

	    private void Update()
        {
	        if (Configs.ShowDebugMenu)
	        {
		        for (int i = 0; i < AllWindows.Count; i++)
		        {
			        if(AllWindows[i].IsVisible)
						AllWindows[i].Update();
		        }
	        }
        }

        public T ToggleWindow<T>() where T : CanvasWindow, new()
        {
	        return (T)ToggleWindow(typeof(T));
        }

        public CanvasWindow ToggleWindow(Type t)
        {
	        for (int i = 0; i < AllWindows.Count; i++)
	        {
		        CanvasWindow window = AllWindows[i];
		        if (window.GetType() == t)
		        {
			        if (window.IsVisible)
			        {
				        window.gameObject.Destroy();
				        AllWindows.RemoveAt(i);
				        return null;
			        }
			        else
			        {
				        window.SetVisible(!window.IsVisible);
			        }

			        return window;
		        }
	        }

	        return NewWindow(t);
        }

        public WindowBlocker CreateWindowBlocker()
        {
	        GameObject myGO = new("WindowBlocker", typeof(RectTransform), typeof(WindowBlocker));
	        myGO.transform.SetParent(blockerParent.transform);
	        myGO.layer = LayerMask.NameToLayer("UI");
		        
	        Image image = myGO.AddComponent<Image>();
	        Color color = Color.magenta;
	        color.a = 0; // hides the image
	        image.color = color;

			RectTransform blocker = myGO.GetComponent<RectTransform>();
	        blocker.sizeDelta = new Vector2(Screen.width / 4, Screen.height / 4);
	        blocker.anchoredPosition = Vector2.zero;
	        blocker.pivot = new Vector2(0f, 1f);
	        blocker.anchorMin = Vector2.zero;
	        blocker.anchorMax = Vector2.zero;

		        
	        WindowBlocker windowBlocker = myGO.GetComponent<WindowBlocker>();
	        activeRectTransforms.Add(windowBlocker);
	        return windowBlocker;
        }

        public bool IsInputBlocked()
        {
	        if (!Configs.ShowDebugMenu)
		        return false;
	        
	        foreach (WindowBlocker rectTransform in activeRectTransforms)
	        {
		        if (rectTransform.isHovered)
		        {
			        return true;
		        }
	        }

	        return false;
        }
        
        public CanvasWindow NewWindow(Type type)
        {
	        GameObject go = Instantiate(canvasPrefabData.WindowPrefab, canvasPrefabData.WindowParent).gameObject;
	        CanvasWindow window = go.AddComponent(type).GetComponent<CanvasWindow>();
	        window.Setup(canvasPrefabData);
	        window.CreateGUI();
	        
	        AllWindows.Add(window);
        
	        return window;
        }
        
        public void Assert(bool condition, string message)
		{
	        if (!condition)
	        {
		        Log.LogError(message);
	        }
	        else
	        {
		        Log.LogInfo("Condition passed: " + message);
	        }
		}
    }
}
