using System;
using BepInEx.Configuration;

namespace DebugMenu;

public static class Configs
{
    public enum WindowSizes
    {
        OneQuarter,
        Half,
        ThreeQuarters,
        Default,
        OneAndAQuarter,
        OneAndAHalf,
        OneAndThreeQuarters,
        Double
    }
	
	public static bool ShowDebugMenu
	{
		get => m_showDebugMenu.Value;
		set
		{
			m_showDebugMenu.Value = value;
			Plugin.Instance.Config.Save();
		}
	}

    public static WindowSizes WindowSize
    {
        get => m_windowSize.Value;
        set
        {
            m_windowSize.Value = value;
            Plugin.Instance.Config.Save();
        }
    }

    public static ConfigEntry<WindowSizes> m_windowSize = Bind("General", "Window Scale", WindowSizes.Default, "How big the menu windows should be.");
	public static ConfigEntry<bool> m_showDebugMenu = Bind("General", "Show Debug Menu", true, "Should the in-game debug menu window be shown?");

    private static ConfigEntry<T> Bind<T>(string section, string key, T defaultValue, string description)
	{
		return Plugin.Instance.Config.Bind(section, key, defaultValue, new ConfigDescription(description, null, Array.Empty<object>()));
	}
}