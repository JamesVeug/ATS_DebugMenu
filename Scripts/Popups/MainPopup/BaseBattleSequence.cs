using DebugMenu.Scripts.Popups;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace DebugMenu.Scripts.Acts;

public abstract class BaseBattleSequence
{
	public RectTransform Container;
	protected CanvasWindow window;
	
	public BaseBattleSequence(CanvasWindow window)
	{
		this.window = window;
	}
	
	public virtual void CreateGUI(RectTransform scopeContainer)
	{
		Container = scopeContainer;
	}

	public abstract void Update();

	public void ToggleVisible(bool visible)
	{
		Container.gameObject.SetActive(visible);
	}
}