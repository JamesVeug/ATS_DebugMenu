using System;
using System.Collections.Generic;
using DebugMenu.Scripts.Utils;
using UnityEngine;

namespace DebugMenu.Scripts.Popups;

public class ButtonListPopup : CanvasWindow
{
	public override string PopupName => popupNameOverride;
	public override Vector2 Size => new(630, 600);

	private string header = "";
	private string filterText = "";
	private List<string> buttonNames = new();
	private List<string> buttonValues = new();
	private string popupNameOverride = "Button List";
	private Action<int, string, string> callback;
	private Vector2 position;
	private string disableMatch;

	public override void CreateGUI()
	{
		base.CreateGUI();
		
		int namesCount = buttonNames.Count; // 20
		int rows = Mathf.Max(Mathf.FloorToInt(Size.y / 50) - 2, 1); // 600 / 40 = 15 
		// int columns = Mathf.CeilToInt((float)namesCount / rows) + 1; // 20 / 15 = 4
		// Rect scrollableAreaSize = new(new Vector2(0, 0), new Vector2(columns *  ColumnWidth + (columns - 1) * 10, rows * RowHeight));
		// Rect scrollViewSize = new(new Vector2(0, 0), Size - new Vector2(10, 25));
		// position = GUI.BeginScrollView(scrollViewSize, position, scrollableAreaSize);
		
		LabelHeader(header);

		Label("Filter");
		TextField(filterText, s => filterText = s);

		DrawExtraTools();

		StartNewColumn();
		
		int j = 0;
		for (int i = 0; i < buttonNames.Count; i++)
		{
			string buttonName = buttonNames[i];
			string buttonValue = buttonValues[i];
			if (!string.IsNullOrEmpty(filterText))
			{
				if (!buttonName.ContainsText(filterText, false) &&
				    !buttonValue.ContainsText(filterText, false))
				{
					continue;
				}
			}
			
			if(!IsFiltered(buttonName, buttonValue))
				continue;

			int index = i;
			Button(buttonName, () =>
			{
				callback(index, buttonValue, disableMatch);
				Plugin.Instance.ToggleWindow<ButtonListPopup>();
			}, disabled: () => (buttonValue == disableMatch, ""));

			j++;
			if (j >= rows)
			{
				StartNewColumn();
				j = 0;
			}
		}
	}

	public virtual bool IsFiltered(string buttonName, string buttonValue)
	{
		return true;
	}

	public virtual void DrawExtraTools()
	{
		
	}

	public static bool OnGUI(DrawableGUI gui, string buttonText, string headerText, Func<Tuple<List<string>, List<string>>> GetDataCallback, Action<int, string, string> OnChoseButtonCallback, string disableMatch = null)
	{
		if (gui.Button(buttonText))
		{
			Debug.Log("ButtonListPopup pressed " + buttonText);
			Tuple<List<string>, List<string>> data = GetDataCallback();

			ButtonListPopup buttonListPopup = Plugin.Instance.ToggleWindow<ButtonListPopup>();
			buttonListPopup.position = Vector2.zero;
			buttonListPopup.popupNameOverride = buttonText;
			buttonListPopup.callback = OnChoseButtonCallback;
			buttonListPopup.buttonNames = data.Item1;
			buttonListPopup.buttonValues = data.Item2;
			buttonListPopup.header = headerText;
			buttonListPopup.filterText = "";
			buttonListPopup.disableMatch = disableMatch;
			return true;
		}

		return false;
	}
}