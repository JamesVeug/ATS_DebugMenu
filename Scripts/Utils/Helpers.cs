using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Eremite;
using UnityEngine;

namespace DebugMenu.Scripts.Utils;

public static partial class Helpers
{
	public static bool ContainsText(this string text, string substring, bool caseSensitive = true)
	{
		if (string.IsNullOrEmpty(text))
		{
			if (string.IsNullOrEmpty(substring))
			{
				// null.ContainsText(null)
				// "".ContainsText("")
				return true;
			}
			
			// null.ContainsText("Hello)
			// "".ContainsText("Hello")
			return false;
		}
		else if (string.IsNullOrEmpty(substring))
		{
			// "Hello".ContainsText(null)
			// "Hello".ContainsText("")
			return false;
		}

		return text.IndexOf(substring, caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase) >= 0;
	}

	public static string DumpAllInfoAsJSONUsingReflection(object o)
	{
		if (o == null)
		{
			return "null";
		}

		Dictionary<string, string> data = new();

		Type type = o.GetType();
		foreach (FieldInfo field in type.GetFields())
		{
			data[field.Name] = SerializeData(field.GetValue(o));
		}
		foreach (PropertyInfo property in type.GetProperties())
		{
			if (property.CanRead)
			{
				data[property.Name] = SerializeData(property.GetValue(o));
			}
		}

		return JSON.ToJson(data);
	}

	private static string SerializeData(object o)
	{
		if (o == null)
		{
			return "null";
		}
		
		// if o is of type primitive then return the value
		if (o.GetType().IsPrimitive || o is string)
		{
			return o.ToString();
		}
		
		return JSON.ToJson(o);
	}
	
	public static object GetDefaultValue(Type type)
	{
		// Validate parameters.
		if (type == null) throw new ArgumentNullException("type");

		// We want an Func<object> which returns the default.
		// Create that expression here.
		Expression<Func<object>> e = Expression.Lambda<Func<object>>(
			// Have to convert to object.
			Expression.Convert(
				// The default value, always get what the *code* tells us.
				Expression.Default(type), typeof(object)
			)
		);

		// Compile and return the value.
		return e.Compile()();
	}
	
	public static string ToLiteral(string input) 
	{
		StringBuilder literal = new(input.Length + 2);
		literal.Append("\"");
		foreach (var c in input) {
			switch (c) {
				case '\"': literal.Append("\\\""); break;
				case '\\': literal.Append(@"\\"); break;
				case '\0': literal.Append(@"\0"); break;
				case '\a': literal.Append(@"\a"); break;
				case '\b': literal.Append(@"\b"); break;
				case '\f': literal.Append(@"\f"); break;
				case '\n': literal.Append(@"\n"); break;
				case '\r': literal.Append(@"\r"); break;
				case '\t': literal.Append(@"\t"); break;
				case '\v': literal.Append(@"\v"); break;
				default:
					// ASCII printable character
					if (c >= 0x20 && c <= 0x7e) {
						literal.Append(c);
						// As UTF16 escaped character
					} else {
						literal.Append(@"\u");
						literal.Append(((int)c).ToString("x4"));
					}
					break;
			}
		}
		literal.Append("\"");
		return literal.ToString();
	}

	public static GUIStyle ButtonWidth(float width, GUIStyle original = null)
	{
		GUIStyle retval = original ?? new(GUI.skin.button);
		retval.fixedWidth = width;
		retval.stretchWidth = true;
		return retval;
	}
    public static GUIStyle DisabledButtonStyle()
    {
        GUIStyle style = new(GUI.skin.button)
        {
            fontStyle = FontStyle.Bold,
            wordWrap = true
        };
        style.normal.background = style.active.background;
        style.hover.background = style.active.background;
        style.onNormal.background = style.active.background;
        style.onHover.background = style.active.background;
        style.onActive.background = style.active.background;
        style.onFocused.background = style.active.background;
        style.normal.textColor = Color.black;
        return style;
    }
    public static GUIStyle HeaderLabelStyle()
    {
        GUIStyle style = new(GUI.skin.label)
        {
            fontSize = 17,
            fontStyle = FontStyle.Bold
        };
        return style;
    }
    public static GUIStyle HeaderLabelStyleRight()
    {
        GUIStyle style = new(GUI.skin.label)
        {
            fontSize = 17,
            fontStyle = FontStyle.Bold,
			alignment = TextAnchor.MiddleRight
        };
        return style;
    }
}

public static class KeyCodeExtensions
{
	public static string Serialize(this KeyCode keyCode)
	{
		return keyCode.ToString();
	}
	
	public static string Serialize(this IEnumerable<KeyCode> keyCode, string separator = ",", bool includeUnassigned = false)
	{
		string serialized = "";
		if (keyCode != null)
		{
			serialized = string.Join(separator, keyCode.Select((a)=>a.Serialize()));
		}

		if (includeUnassigned && string.IsNullOrEmpty(separator))
		{
			serialized = "Unassigned";
		}
		return serialized;
	}
}