using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace DebugMenu.Scripts.Popups;

public class GameInfoPopup : CanvasWindow
{
	public override string PopupName => "Game Info";
	public override Vector2 Size => new(220, 500);

	public float updateInterval = 0.5F;
 
	private TMP_Text fpsLabel;
	private TMP_Text sceneLabel;
	
	private float lastInterval;
	private int frames = 0;
	private int fps;

	public override void CreateGUI()
	{
		base.CreateGUI();

		fpsLabel = Label("FPS: " + fps);
		sceneLabel = Label("Scenes: ");
	}

	public override void Update()
	{
		base.Update();
		++frames;
 
		float timeNow = Time.realtimeSinceStartup;
		if (timeNow > lastInterval + updateInterval)
		{
			fps = (int)(frames / (timeNow - lastInterval));
			frames = 0;
			lastInterval = timeNow;
		}
		fpsLabel.text = "FPS: " + fps;
		
		int sceneCount = SceneManager.sceneCount;		
		
		sceneLabel.text = $"Scenes ({sceneCount})";
		Scene activeScene = SceneManager.GetActiveScene();

		for (int i = 0; i < sceneCount; i++)
		{
			Scene scene = SceneManager.GetSceneAt(i);
			if (scene == activeScene)
			{
				sceneLabel.text += $"\n{i}: {scene.name} (Active)";
			}
			else
			{
				sceneLabel.text += $"\n{i}: {scene.name}";
			}
		}
	}
}