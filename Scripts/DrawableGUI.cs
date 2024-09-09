using System;
using BepInEx.Configuration;
using DebugMenu.Scripts.Utils;
using UnityEngine;

namespace DebugMenu.Scripts;

public abstract class DrawableGUI : ADrawable
{
	// these can only be set to the correct values from within OnGUI
	// since they reference GUI for their style
	private GUIStyle LabelHeaderStyle = GUIStyle.none;
    private GUIStyle LabelHeaderStyleLeft = GUIStyle.none;
    private GUIStyle LabelBoldStyle = GUIStyle.none;
    private GUIStyle ButtonStyle = GUIStyle.none;
    private GUIStyle ButtonDisabledStyle = GUIStyle.none;

    public override void OnGUI()
	{
		LabelHeaderStyleLeft = Helpers.HeaderLabelStyle();
        LabelHeaderStyle = new(GUI.skin.label)
        {
            fontSize = 17,
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold
        };
        LabelBoldStyle = new(GUI.skin.label)
        {
            fontStyle = FontStyle.Bold
        };
        ButtonStyle = new(GUI.skin.button)
        {
            wordWrap = true
        };
		ButtonDisabledStyle = Helpers.DisabledButtonStyle();

		Reset();
	}
	
	public override void Reset()
	{
		X = ColumnPadding;
		Y = TopOffset;
		MaxHeight = 0;
		Columns = 0;
	}

	public override void StartNewColumn()
	{
		X += ColumnWidth + ColumnPadding;
		Y = TopOffset;
		Columns++;
	}

    /// <returns>Returns true if the button was pressed</returns>
    public override bool Button(string text, Vector2? size = null, string buttonGroup = null, Func<ButtonDisabledData> disabled = null)
	{
		(float x, float y, float w, float h) = GetPosition(size);

		GUIStyle style = ButtonStyle;
		bool wasPressed = false;
		
		ButtonDisabledData disabledData = disabled?.Invoke() ?? new ButtonDisabledData();
		bool isDisabled = disabledData.Disabled;
		if (isDisabled)
		{
			if (!string.IsNullOrEmpty(disabledData.Reason))
				GUI.Label(new Rect(x,y,w,h), text + "\n(" + disabledData.Reason + ")", ButtonDisabledStyle);
			else
                GUI.Label(new Rect(x, y, w, h), text, ButtonDisabledStyle);
        }
		else if (buttonGroup == null)
		{
			wasPressed = GUI.Button(new Rect(x, y, w, h), text, ButtonStyle);
		}
		else
		{
			// create the button group if it doesn't exist
			if (!m_buttonGroups.TryGetValue(buttonGroup, out string selectedButton))
			{
				m_buttonGroups[buttonGroup] = text;
			}

			// grey-out the text if the current button has been selected
			if (selectedButton == text)
			{
				style = ButtonDisabledStyle;
			}

			wasPressed = GUI.Button(new Rect(x,y,w,h), text, style);
			if (wasPressed)
			{
				m_buttonGroups[buttonGroup] = text;
			}
		}


		return wasPressed;
	}
	
	/// <returns>Returns True if the value changed</returns>
	public override bool Toggle(string text, ref bool value, Vector2? size = null)
	{
		(float x, float y, float w, float h) = GetPosition(size);
		bool toggle = GUI.Toggle(new Rect(x,y,w,h), value, text);
		if (toggle != value)
		{
			value = toggle;
			return true;
		}
		return false;
	}
	
	public override bool Toggle(string text, ref ConfigEntry<bool> value, Vector2? size = null)
	{
		(float x, float y, float w, float h) = GetPosition(size);
		bool b = value.Value;
		bool toggle = GUI.Toggle(new Rect(x,y,w,h), b, text);
		if (toggle != b)
		{
			value.Value = toggle;
			return true;
		}
		return false;
	}

	public override void Label(string text, Vector2? size = null)
	{
		(float x, float y, float w, float h) = GetPosition(size);
		GUI.Label(new Rect(x, y,w,h), text);
	}

	public override void Label(Sprite sprite, Vector2? size = null)
	{
		if (!size.HasValue)
		{
			float minSize = Mathf.Min(RowHeight, ColumnWidth);
			size = new Vector2(minSize, minSize);
		}
		(float x, float y, float w, float h) = GetPosition(size);
		Rect rect = new(x, y, w, h);
		Rect spriteRect = sprite != null ? sprite.GetTextureRect() : new Rect(0, 0, Texture2D.normalTexture.width, Texture2D.normalTexture.height);
		Texture2D tex = sprite != null ? sprite.texture : Texture2D.normalTexture;
		
		spriteRect.x /= tex.width;
		spriteRect.y /= tex.height;
		spriteRect.width /= tex.width;
		spriteRect.height /= tex.height;
		GUI.DrawTextureWithTexCoords(rect, tex, spriteRect);
	}

	public override void LabelHeader(string text, Vector2? size = null, bool leftAligned = false)
	{
		(float x, float y, float w, float h) = GetPosition(size);
		GUI.Label(new Rect(x,y,w,h), text, leftAligned ? LabelHeaderStyleLeft : LabelHeaderStyle);
	}
	
    public override void LabelBold(string text, Vector2? size = null)
    {
        (float x, float y, float w, float h) = GetPosition(size);
        GUI.Label(new Rect(x, y, w, h), text, LabelBoldStyle);
    }

    public override object InputField(object value, Type type, Vector2? size = null)
	{
		if (type == typeof(int))
		{
			return IntField((int)value, size);
		}
		else if (type == typeof(float))
		{
			return FloatField((float)value, size);
		}
		else if (type == typeof(string))
		{
			return TextField((string)value, size);
		}
		else if (type == typeof(string))
		{
			bool t = (bool)value;
			Toggle("", ref t, size);
			return t;
		}
		else
		{
			Label("Unsupported type: " + type);
			return value;
		}
	}

	public override string TextField(string text, Vector2? size = null)
	{
		(float x, float y, float w, float h) = GetPosition(size);
		return GUI.TextField(new Rect(x, y, w, h), text);
	}

	public override int IntField(int text, Vector2? size = null)
	{
		(float x, float y, float w, float h) = GetPosition(size);

		string textField = GUI.TextField(new Rect(x, y, w, h), text.ToString());
		if (!int.TryParse(textField, out int result)) 
			return text;
		
		return result;
	}

	public override float FloatField(float text, Vector2? size = null)
	{
		(float x, float y, float w, float h) = GetPosition(size);

		string textField = GUI.TextField(new Rect(x, y, w, h), text.ToString());
		if (!float.TryParse(textField, out float result)) 
			return text;
		
		return result;
	}

	public override void Padding(Vector2? size = null)
	{
		float w = size.HasValue && size.Value.x != 0 ? size.Value.x : ColumnWidth;
		float h = size.HasValue && size.Value.y != 0 ? size.Value.y : RowHeight;
		float y = Y;
		Y += h;
		MaxHeight = Mathf.Max(MaxHeight, Y);
		GUI.Label(new Rect(X, y, w, h), "");
	}

	public override (float X, float y, float w, float h) GetPosition(Vector2? size = null)
	{
		float x = X;
		float y = Y;
		float h = size.HasValue && size.Value.y != 0 ? size.Value.y : RowHeight;
		float w = size.HasValue && size.Value.x != 0 ? size.Value.x : ColumnWidth;
		
		bool verticallyAligned = m_layoutScopes.Count == 0 || !m_layoutScopes[m_layoutScopes.Count - 1].Horizontal;
		if (verticallyAligned)
		{
			Y += h;
		}
		else
		{
			if (!size.HasValue)
			{
				w = ColumnWidth / m_layoutScopes[m_layoutScopes.Count - 1].TotalElements;
			}

			X += w;
		}
		MaxHeight = Mathf.Max(MaxHeight, Y);
		
		return (x, y, w, h);
	}

	public IDisposable HorizontalScope(int elementCount)
	{
		return new LayoutScope(elementCount, true, this);
	}
    public IDisposable VerticalScope(int elementCount)
    {
        return new LayoutScope(elementCount, false, this);
    }
}