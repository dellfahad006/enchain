using UnityEngine;

// Developer menu. Press M in game to open or close it. Everything here changes live.
public class DevMenu : MonoBehaviour
{
	private bool open;

	private bool openedPause;

	private Vector2 scroll;

	private void Awake()
	{
		DevSettings.Load();
	}

	private void Update()
	{
		GameManager gm = GameManager.instance;
		if (gm == null || gm.menu.inMenu)
		{
			return;
		}
		if (Input.GetKeyDown(KeyCode.M))
		{
			SetOpen(!open);
		}
		else if (open && Input.GetKeyUp(KeyCode.Escape))
		{
			SetOpen(false);
		}
	}

	private void SetOpen(bool value)
	{
		if (open == value)
		{
			return;
		}
		CursorHideShow cursor = GameManager.instance.GetComponent<CursorHideShow>();
		open = value;
		DevSettings.menuOpen = value;
		if (value)
		{
			openedPause = false;
			if (cursor != null && !cursor.paused)
			{
				cursor.DevPause(true);
				openedPause = true;
			}
		}
		else
		{
			DevSettings.lastCloseFrame = Time.frameCount;
			DevSettings.Save();
			if (openedPause && cursor != null)
			{
				cursor.DevPause(false);
			}
			openedPause = false;
		}
	}

	private float Slider(string label, float value, float min, float max)
	{
		GUILayout.BeginHorizontal();
		GUILayout.Label(label, GUILayout.Width(210f));
		value = GUILayout.HorizontalSlider(value, min, max, GUILayout.Width(160f));
		GUILayout.Label(value.ToString("0.00"), GUILayout.Width(50f));
		GUILayout.EndHorizontal();
		return value;
	}

	private void OnGUI()
	{
		if (!open)
		{
			return;
		}
		float scale = Mathf.Max(1f, Screen.height / 720f);
		GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));
		float h = Screen.height / scale - 20f;
		GUILayout.BeginArea(new Rect(10f, 10f, 470f, h), GUI.skin.box);
		GUILayout.Label("DEV MENU  (press M or Esc to close)");
		scroll = GUILayout.BeginScrollView(scroll);

		GUILayout.Label("--- ENEMY SOUNDS (extra) ---");
		DevSettings.sfxMutter = GUILayout.Toggle(DevSettings.sfxMutter, "Idle mutters");
		DevSettings.sfxAlert = GUILayout.Toggle(DevSettings.sfxAlert, "Alert blips");
		DevSettings.sfxZap = GUILayout.Toggle(DevSettings.sfxZap, "Stun zap");

		GUILayout.Label("--- SHOTGUN ---");
		DevSettings.blastBrightness = Slider("Blast brightness", DevSettings.blastBrightness, 0f, 1f);
		DevSettings.blastLightBoost = Slider("Blast light boost", DevSettings.blastLightBoost, 0.5f, 4f);
		DevSettings.shotDamageMult = Slider("Hold-fire damage x", DevSettings.shotDamageMult, 0.5f, 4f);
		DevSettings.barDrainHold = Slider("Bar drain speed (hold)", DevSettings.barDrainHold, 1.5f, 3f);
		DevSettings.slowFireWhenEmpty = GUILayout.Toggle(DevSettings.slowFireWhenEmpty, "Slow fire when bar is empty (original)");
		DevSettings.showGunBar = GUILayout.Toggle(DevSettings.showGunBar, "Show gun bar");
		DevSettings.showCrosshair = GUILayout.Toggle(DevSettings.showCrosshair, "Show crosshair dot");

		GUILayout.Label("--- HIGHLIGHT ---");
		DevSettings.enemyHighlightRadius = Slider("Enemy/body highlight radius", DevSettings.enemyHighlightRadius, 0f, 1.5f);
		DevSettings.hookPointHighlightRadius = Slider("Hook point highlight radius", DevSettings.hookPointHighlightRadius, 0f, 3f);

		GUILayout.Label("--- CHAIN (right mouse) ---");
		DevSettings.hookCatchRadius = Slider("Catch radius (enemy/hook)", DevSettings.hookCatchRadius, 0f, 2.5f);
		DevSettings.hookWallColliderScale = Slider("Tip wall-collider size", DevSettings.hookWallColliderScale, 0.05f, 1.5f);
		DevSettings.wallGraceDistance = Slider("Wall-hit grace distance", DevSettings.wallGraceDistance, 0f, 8f);
		DevSettings.wallHitsCancelHook = GUILayout.Toggle(DevSettings.wallHitsCancelHook, "Wall/floor hit cancels chain");

		GUILayout.Label("--- SWING BOOST ---");
		DevSettings.swingBoost = Slider("Boost strength", DevSettings.swingBoost, 0f, 20f);
		DevSettings.swingBoostDuration = Slider("Boost duration (0 = instant)", DevSettings.swingBoostDuration, 0f, 1.5f);
		DevSettings.swingBoostLookMix = Slider("Direction: anchor <-> look", DevSettings.swingBoostLookMix, 0f, 1f);

		GUILayout.Label("--- BODIES / CARDS ---");
		DevSettings.discardHoldSeconds = Slider("Discard hold time (s)", DevSettings.discardHoldSeconds, 0.1f, 2f);
		DevSettings.maxBodies = Mathf.RoundToInt(Slider("Max carried bodies", DevSettings.maxBodies, 1f, 8f));
		DevSettings.ignorePadRequirements = GUILayout.Toggle(DevSettings.ignorePadRequirements, "Pads ignore enemy requirements");

		GUILayout.Label("--- RESPAWN / CHECKPOINTS ---");
		DevSettings.respawnDelay = Slider("Respawn delay (s)", DevSettings.respawnDelay, 0.2f, 4f);
		DevSettings.checkpointFadeDelay = Slider("Checkpoint fade delay (s)", DevSettings.checkpointFadeDelay, 0f, 4f);
		DevSettings.checkpointFadeTime = Slider("Checkpoint fade time (s)", DevSettings.checkpointFadeTime, 0.1f, 3f);

		GUILayout.Space(8f);
		if (GUILayout.Button("Reset everything to defaults"))
		{
			DevSettings.ResetDefaults();
		}
		if (GUILayout.Button("Close"))
		{
			SetOpen(false);
		}
		GUILayout.EndScrollView();
		GUILayout.EndArea();
	}
}
