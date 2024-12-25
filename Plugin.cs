using System;
using System.Collections.Generic;
using System.Reflection;
using ATS_API;
using BepInEx;
using BepInEx.Logging;
using DebugMenu.Scripts.Popups;
using HarmonyLib;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace DebugMenu
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
	    public const string PluginGuid = "jamesgames.againstthestorm.debugmenu";
	    public const string PluginName = "Debug Menu";
	    public const string PluginVersion = "2.0.3";

	    public static Plugin Instance;
	    public static ManualLogSource Log;
	    public static InputActionAsset InputAsset;
	    
	    public static string PluginDirectory;
	    public static float StartingFixedDeltaTime;

	    public static List<BaseWindow> AllWindows = new();
	    public static SaveData SaveData = null;
	    
	    private Harmony harmony;
	    private GameObject blockerParent = null;
	    private Canvas blockerParentCanvas = null;
	    private List<WindowBlocker> activeRectTransforms = new List<WindowBlocker>();
	    private List<WindowBlocker> rectTransformPool = new List<WindowBlocker>();

	    private void Awake()
	    {
		    Instance = this;
		    Log = Logger;
		    StartingFixedDeltaTime = Time.fixedDeltaTime;

		    PluginDirectory = this.Info.Location.Replace("DebugMenu.dll", "");

		    blockerParent = new("DebugMenuWindowBlocker");
		    blockerParent.layer = LayerMask.NameToLayer("UI");
		    blockerParentCanvas = blockerParent.AddComponent<Canvas>();
		    blockerParentCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
		    blockerParentCanvas.sortingOrder = 32767;
		    blockerParent.AddComponent<CanvasScaler>();
		    blockerParent.AddComponent<GraphicRaycaster>();
		    blockerParent.transform.SetParent(transform);

		    Input.Initialize();
		    harmony = Harmony.CreateAndPatchAll(typeof(Plugin).Assembly, PluginGuid);

		    // Get all types of BaseWindow, instantiate them and add them to allwindows
		    Type[] types = Assembly.GetExecutingAssembly().GetTypes();
		    for (int i = 0; i < types.Length; i++)
		    {
			    Type type = types[i];
			    if (type.IsSubclassOf(typeof(BaseWindow)))
			    {
				    AllWindows.Add((BaseWindow)Activator.CreateInstance(type));
			    }
		    }

		    DontDestroyOnLoad(gameObject);
		    
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
		    
		    Hotkeys.RegisterKey(PluginName, "toggleDebug", "Show/Hide Debug Menu", [KeyCode.Tilde], () =>
		    {
			    Configs.ShowDebugMenu = !Configs.ShowDebugMenu;
			    blockerParentCanvas.enabled = Configs.ShowDebugMenu;
		    });

		    Logger.LogInfo($"Loaded {PluginName}");
	    }

	    private void Update()
        {
	        if (Configs.ShowDebugMenu)
	        {
		        for (int i = 0; i < AllWindows.Count; i++)
		        {
			        if(AllWindows[i].IsActive)
						AllWindows[i].Update();
		        }
	        }
        }

        private void OnGUI()
        {
	        if (!Configs.ShowDebugMenu)
		        return;
	        
	        for (int i = 0; i < AllWindows.Count; i++)
	        {
		        if(AllWindows[i].IsActive)
					AllWindows[i].OnWindowGUI();
	        }
        }

        public T ToggleWindow<T>() where T : BaseWindow, new()
        {
	        return (T)ToggleWindow(typeof(T));
        }

        public BaseWindow ToggleWindow(Type t)
        {
	        for (int i = 0; i < AllWindows.Count; i++)
	        {
		        BaseWindow window = AllWindows[i];
		        if (window.GetType() == t)
		        {
			        window.IsActive = !window.IsActive;
			        return window;
		        }
	        }

	        return null;
        }
        
        public T GetWindow<T>() where T : BaseWindow, new()
		{
			return (T)GetWindow(typeof(T));
		}

        public BaseWindow GetWindow(Type t)
        {
            for (int i = 0; i < AllWindows.Count; i++)
            {
                BaseWindow window = AllWindows[i];
                if (window.GetType() == t)
                    return window;
            }

            return null;
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
    }
}
