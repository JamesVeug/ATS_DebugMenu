using DebugMenu.Scripts.Acts;
using DebugMenu.Scripts.Popups;
using UnityEngine;

namespace DebugMenu.Scripts.All;

public class AllGameModes : BaseGameMode
{
	private static bool blockAllInput = false;

	public static bool IsInputBlocked()
	{
		if(blockAllInput)
			return true;

		return Plugin.Instance.IsInputBlocked();
	}
	
	public AllGameModes(DebugWindow window) : base(window)
	{
	}

	public override void Update()
	{
		
	}
	
	public override void OnGUI()
	{
		// TODO
		// Window.Toggle("Block All Input", ref blockAllInput);

		using (Window.HorizontalScope(5))
		{
			Window.Label("<b>Time Scale:</b>");
			
			if (Window.Button("0x"))
			{
				SetTimeScale(0f);
			}

			if (Window.Button("1x"))
			{
				SetTimeScale(1f);
			}

			if (Window.Button("5x"))
			{
				SetTimeScale(5f);
			}

			if (Window.Button("10x"))
			{
				SetTimeScale(10f);
			}
		}

        if (Window.Button("Show Game Info"))
		{
			Plugin.Instance.ToggleWindow<GameInfoPopup>();
		}

		using (Window.HorizontalScope(4))
		{
			Window.Label("<b>Menu Scale:</b>");
			Window.Label($"{DrawableGUI.GetDisplayScalar()}x");

			int sizeAsInt = (int)Configs.WindowSize;
            if (Window.Button("-", disabled: () => new() { Disabled = sizeAsInt <= 0 }))
            {
                sizeAsInt--;
                Configs.WindowSize = (Configs.WindowSizes)sizeAsInt;
            }
            if (Window.Button("+", disabled: () => new() { Disabled = sizeAsInt > 6 }))
			{
				sizeAsInt++;
				Configs.WindowSize = (Configs.WindowSizes)sizeAsInt;
			}
		}
	}

	public override void OnGUIMinimal()
	{
		
	}

	public void SetTimeScale(float speed)
	{
		Time.timeScale = speed;
		Time.fixedDeltaTime = Plugin.StartingFixedDeltaTime * Time.timeScale;
	}

	public override void Restart()
	{
		// Nothing
	}

	public override void Reload()
	{
		// Nothing
	}
}