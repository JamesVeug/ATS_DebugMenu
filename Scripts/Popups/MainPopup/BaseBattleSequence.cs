using DebugMenu.Scripts.Popups;
using UnityEngine.UI;

namespace DebugMenu.Scripts.Acts;

public abstract class BaseBattleSequence
{
	protected BaseWindow window;
	
	public BaseBattleSequence(BaseWindow window)
	{
		this.window = window;
	}
	
	public virtual void OnGUI()
	{
		
	}
}