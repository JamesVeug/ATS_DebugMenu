using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using DebugMenu.Scripts.Utils;
using UnityEngine;

namespace DebugMenu.Scripts;

public abstract class ADrawable
{
	protected const float TopOffset = 20;

	public struct ButtonDisabledData
	{
		public bool Disabled;
		public string Reason;

		public ButtonDisabledData(string reason)
		{
			Disabled = true;
			Reason = reason;
		}
	}

	public struct LayoutScope : IDisposable
	{
		public readonly bool Horizontal => horizontal;
		public readonly int TotalElements => totalElements;
		public readonly Vector2 CurrentSize => currentSize;

		private readonly float originalX;
		private readonly Vector2 currentSize;
		private readonly int totalElements;
		private readonly bool horizontal;
		private readonly ADrawable scope;

		public LayoutScope(int totalElements, bool horizontal, ADrawable scope)
		{
			this.originalX = scope.X;
			this.totalElements = totalElements;
			this.horizontal = horizontal;
			this.scope = scope;
			this.currentSize = new Vector2(0, 0);
			scope.m_layoutScopes.Add(this);
		}
		
		public void Dispose()
		{
			scope.m_layoutScopes.Remove(this);
			if (horizontal)
			{
				scope.X = originalX;
				scope.Y += scope.RowHeight;
			}
		}
	}
	
	public float TotalWidth => Columns * ColumnWidth + ((Columns - 1) * ColumnPadding);
	public float Height => MaxHeight + RowHeight;
	
	protected float X = 0;
	protected float Y = 0;
	protected float ColumnWidth = 200;
	protected float RowHeight = 40;
	protected float ColumnPadding = 5;
	protected int Columns = 1;
	protected float MaxHeight = 1;

	protected readonly Dictionary<string, string> m_buttonGroups = new();
	protected readonly List<LayoutScope> m_layoutScopes = new();

    internal static float GetDisplayScalar()
    {
        return Configs.WindowSize switch
        {
            Configs.WindowSizes.OneQuarter => 0.25f,
            Configs.WindowSizes.Half => 0.5f,
            Configs.WindowSizes.ThreeQuarters => 0.75f,
            Configs.WindowSizes.OneAndAQuarter => 1.25f,
            Configs.WindowSizes.OneAndAHalf => 1.5f,
            Configs.WindowSizes.OneAndThreeQuarters => 1.75f,
            Configs.WindowSizes.Double => 2f,
            _ => 1f,
        };
    }

    public virtual void OnGUI()
	{
	}
	
	public virtual void Reset()
	{
		X = ColumnPadding;
		Y = TopOffset;
		MaxHeight = 0;
		Columns = 0;
	}

	public virtual void StartNewColumn()
	{
		X += ColumnWidth + ColumnPadding;
		Y = TopOffset;
		Columns++;
	}

	/// <returns>Returns true if the button was pressed</returns>
	public abstract bool Button(string text, Vector2? size = null, string buttonGroup = null,
		Func<ButtonDisabledData> disabled = null);

	/// <returns>Returns True if the value changed</returns>
	public abstract bool Toggle(string text, ref bool value, Vector2? size = null);
	public abstract bool Toggle(string text, ref ConfigEntry<bool> value, Vector2? size = null);
	public abstract void Label(string text, Vector2? size = null);
	public abstract void LabelHeader(string text, Vector2? size = null, bool leftAligned = false);
	public abstract void LabelBold(string text, Vector2? size = null);
	public abstract object InputField(object value, Type type, Vector2? size = null);
	public abstract string TextField(string text, Vector2? size = null);
	public abstract int IntField(int text, Vector2? size = null);
	public abstract float FloatField(float text, Vector2? size = null);
	public abstract void Padding(Vector2? size = null);
	public abstract (float X, float y, float w, float h) GetPosition(Vector2? size = null);

	public IDisposable HorizontalScope(int elementCount)
	{
		return new LayoutScope(elementCount, true, this);
	}
    public IDisposable VerticalScope(int elementCount)
    {
        return new LayoutScope(elementCount, false, this);
    }

    public abstract void Label(Sprite sprite, Vector2? size = null);
}