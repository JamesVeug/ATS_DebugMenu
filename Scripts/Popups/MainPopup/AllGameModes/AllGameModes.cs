using DebugMenu.Scripts.Acts;
using DebugMenu.Scripts.Popups;
using DebugMenu.Scripts.UIToolKit;
using UnityEngine;

namespace DebugMenu.Scripts.All;

public class AllGameModes : BaseGameMode
{
	public AllGameModes(DebugWindow window) : base(window)
	{
	}

	public override void Update()
	{
		
	}
	
	public override void CreateGUI()
	{
		Input.DrawToggleBlockInput(Window);

		using (Window.HorizontalScope())
		{
			Window.Label("<b>Time\nScale:</b>");

			Window.Button("0x", () =>
			{
				SetTimeScale(0f);
			});

			Window.Button("1x", () =>
			{
				SetTimeScale(1f);
			});

			Window.Button("5x", () =>
			{
				SetTimeScale(5f);
			});

			Window.Button("10x", () =>
			{
				SetTimeScale(10f);
			});
		}

		Window.Button("Show Game Info", () =>
		{
			Plugin.Instance.ToggleWindow<GameInfoPopup>();
		});

		using (Window.HorizontalScope())
		{
			Window.Label("<b>Menu Scale:</b>");
			Window.Label($"{DrawableGUI.GetDisplayScalar()}x");

			Window.Button("-", () =>
		    {
			    Configs.WindowSize = (Configs.WindowSizes)((int)Configs.WindowSize - 1);
			    
		    }, disabled: () => ((int)Configs.WindowSize <= 0,""));
			
            Window.Button("+", ()=>
			{
				Configs.WindowSize = (Configs.WindowSizes)((int)Configs.WindowSize + 1);
			}, disabled: () => ((int)Configs.WindowSize > 6,""));
		}
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