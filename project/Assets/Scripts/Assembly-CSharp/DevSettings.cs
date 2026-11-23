using System;
using UnityEngine;

// All live-tweakable values. Edited from the dev menu (press M in game) and saved in PlayerPrefs.
public static class DevSettings
{
	public static bool menuOpen;

	public static int lastCloseFrame = -10;

	// ---- extra enemy sounds (all off by default)
	public static bool sfxMutter;

	public static bool sfxAlert;

	public static bool sfxZap;

	// ---- shotgun
	public static float blastBrightness = 0.65f;

	public static float blastLightBoost = 1.6f;

	public static float shotDamageMult = 1.5f;

	public static float barDrainHold = 1.714f;

	public static bool slowFireWhenEmpty;

	public static bool showGunBar;

	public static bool showCrosshair;

	// ---- highlight
	public static float enemyHighlightRadius = 0.15f;

	public static float hookPointHighlightRadius = 1.4f;

	// ---- chain / hook
	public static float hookCatchRadius = 0.6f;

	public static float hookWallColliderScale = 0.6f;

	public static float wallGraceDistance = 2.5f;

	public static bool wallHitsCancelHook = true;

	// ---- swing boost
	public static float swingBoost = 4f;

	public static float swingBoostDuration = 0.25f;

	public static float swingBoostLookMix = 0.5f;

	// ---- bodies / cards
	public static float discardHoldSeconds = 0.8f;

	public static int maxBodies = 4;

	public static bool ignorePadRequirements = true;

	// ---- respawn / checkpoints
	public static float respawnDelay = 1f;

	public static float checkpointFadeDelay = 1f;

	public static float checkpointFadeTime = 0.8f;

	private const string Prefix = "dev_";

	private static float F(string key, float def)
	{
		return PlayerPrefs.GetFloat(Prefix + key, def);
	}

	private static bool B(string key, bool def)
	{
		return PlayerPrefs.GetInt(Prefix + key, def ? 1 : 0) == 1;
	}

	public static void Load()
	{
		sfxMutter = B("sfxMutter", false);
		sfxAlert = B("sfxAlert", false);
		sfxZap = B("sfxZap", false);
		blastBrightness = F("blastBrightness", 0.65f);
		blastLightBoost = F("blastLightBoost", 1.6f);
		shotDamageMult = F("shotDamageMult", 1.5f);
		barDrainHold = F("barDrainHold", 1.714f);
		slowFireWhenEmpty = B("slowFireWhenEmpty", false);
		showGunBar = B("showGunBar", false);
		showCrosshair = B("showCrosshair", false);
		enemyHighlightRadius = F("enemyHighlightRadius", 0.15f);
		hookPointHighlightRadius = F("hookPointHighlightRadius", 1.4f);
		hookCatchRadius = F("hookCatchRadius", 0.6f);
		hookWallColliderScale = F("hookWallColliderScale", 0.6f);
		wallGraceDistance = F("wallGraceDistance", 2.5f);
		wallHitsCancelHook = B("wallHitsCancelHook", true);
		swingBoost = F("swingBoost", 4f);
		swingBoostDuration = F("swingBoostDuration", 0.25f);
		swingBoostLookMix = F("swingBoostLookMix", 0.5f);
		discardHoldSeconds = F("discardHoldSeconds", 0.8f);
		maxBodies = Mathf.RoundToInt(F("maxBodies", 4f));
		ignorePadRequirements = B("ignorePadRequirements", true);
		respawnDelay = F("respawnDelay", 1f);
		checkpointFadeDelay = F("checkpointFadeDelay", 1f);
		checkpointFadeTime = F("checkpointFadeTime", 0.8f);
	}

	public static void Save()
	{
		PlayerPrefs.SetInt(Prefix + "sfxMutter", sfxMutter ? 1 : 0);
		PlayerPrefs.SetInt(Prefix + "sfxAlert", sfxAlert ? 1 : 0);
		PlayerPrefs.SetInt(Prefix + "sfxZap", sfxZap ? 1 : 0);
		PlayerPrefs.SetFloat(Prefix + "blastBrightness", blastBrightness);
		PlayerPrefs.SetFloat(Prefix + "blastLightBoost", blastLightBoost);
		PlayerPrefs.SetFloat(Prefix + "shotDamageMult", shotDamageMult);
		PlayerPrefs.SetFloat(Prefix + "barDrainHold", barDrainHold);
		PlayerPrefs.SetInt(Prefix + "slowFireWhenEmpty", slowFireWhenEmpty ? 1 : 0);
		PlayerPrefs.SetInt(Prefix + "showGunBar", showGunBar ? 1 : 0);
		PlayerPrefs.SetInt(Prefix + "showCrosshair", showCrosshair ? 1 : 0);
		PlayerPrefs.SetFloat(Prefix + "enemyHighlightRadius", enemyHighlightRadius);
		PlayerPrefs.SetFloat(Prefix + "hookPointHighlightRadius", hookPointHighlightRadius);
		PlayerPrefs.SetFloat(Prefix + "hookCatchRadius", hookCatchRadius);
		PlayerPrefs.SetFloat(Prefix + "hookWallColliderScale", hookWallColliderScale);
		PlayerPrefs.SetFloat(Prefix + "wallGraceDistance", wallGraceDistance);
		PlayerPrefs.SetInt(Prefix + "wallHitsCancelHook", wallHitsCancelHook ? 1 : 0);
		PlayerPrefs.SetFloat(Prefix + "swingBoost", swingBoost);
		PlayerPrefs.SetFloat(Prefix + "swingBoostDuration", swingBoostDuration);
		PlayerPrefs.SetFloat(Prefix + "swingBoostLookMix", swingBoostLookMix);
		PlayerPrefs.SetFloat(Prefix + "discardHoldSeconds", discardHoldSeconds);
		PlayerPrefs.SetFloat(Prefix + "maxBodies", maxBodies);
		PlayerPrefs.SetInt(Prefix + "ignorePadRequirements", ignorePadRequirements ? 1 : 0);
		PlayerPrefs.SetFloat(Prefix + "respawnDelay", respawnDelay);
		PlayerPrefs.SetFloat(Prefix + "checkpointFadeDelay", checkpointFadeDelay);
		PlayerPrefs.SetFloat(Prefix + "checkpointFadeTime", checkpointFadeTime);
		PlayerPrefs.Save();
	}

	public static void ResetDefaults()
	{
		sfxMutter = false;
		sfxAlert = false;
		sfxZap = false;
		blastBrightness = 0.65f;
		blastLightBoost = 1.6f;
		shotDamageMult = 1.5f;
		barDrainHold = 1.714f;
		slowFireWhenEmpty = false;
		showGunBar = false;
		showCrosshair = false;
		enemyHighlightRadius = 0.15f;
		hookPointHighlightRadius = 1.4f;
		hookCatchRadius = 0.6f;
		hookWallColliderScale = 0.6f;
		wallGraceDistance = 2.5f;
		wallHitsCancelHook = true;
		swingBoost = 4f;
		swingBoostDuration = 0.25f;
		swingBoostLookMix = 0.5f;
		discardHoldSeconds = 0.8f;
		maxBodies = 4;
		ignorePadRequirements = true;
		respawnDelay = 1f;
		checkpointFadeDelay = 1f;
		checkpointFadeTime = 0.8f;
	}
}

// Text clean-up: removes the "[M] MUSIC" line and renames "devil" to "skull".
public static class TextFix
{
	public static string Clean(string s)
	{
		if (string.IsNullOrEmpty(s))
		{
			return s;
		}
		if (s.IndexOf("[M]", StringComparison.Ordinal) >= 0 && s.IndexOf("MUSIC", StringComparison.OrdinalIgnoreCase) >= 0)
		{
			string[] lines = s.Split('\n');
			string result = string.Empty;
			bool first = true;
			foreach (string line in lines)
			{
				if (line.IndexOf("[M]", StringComparison.Ordinal) >= 0 && line.IndexOf("MUSIC", StringComparison.OrdinalIgnoreCase) >= 0)
				{
					continue;
				}
				if (!first)
				{
					result += "\n";
				}
				result += line;
				first = false;
			}
			s = result;
		}
		if (s.IndexOf("evil", StringComparison.OrdinalIgnoreCase) >= 0)
		{
			s = s.Replace("DEVIL", "SKULL").Replace("Devil", "Skull").Replace("devil", "skull");
		}
		return s;
	}
}
