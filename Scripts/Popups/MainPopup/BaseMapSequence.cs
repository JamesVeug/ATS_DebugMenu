using UnityEngine;
using UnityEngine.UIElements;

namespace DebugMenu.Scripts.Acts;

public abstract class BaseMapSequence
{
	public RectTransform Container;
	
	public virtual void CreateGUI(RectTransform scopeContainer)
	{
		Container = scopeContainer;
	}
	
	public abstract void Update();
	
	public abstract void ToggleSkipNextNode();
	public abstract void ToggleAllNodes();

	public void ToggleVisible(bool visible)
	{
		Container.gameObject.SetActive(visible);
	}
}