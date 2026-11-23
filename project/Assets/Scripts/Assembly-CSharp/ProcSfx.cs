using System.Collections.Generic;
using UnityEngine;

// Small retro sound synthesiser. Builds extra enemy sounds at runtime so no audio files are needed.
public static class ProcSfx
{
	private const int Rate = 22050;

	private static Dictionary<string, AudioClip[]> bank = new Dictionary<string, AudioClip[]>();

	public static AudioClip Get(string kind)
	{
		AudioClip[] clips;
		if (!bank.TryGetValue(kind, out clips) || clips == null || clips.Length == 0 || clips[0] == null)
		{
			clips = Build(kind);
			bank[kind] = clips;
		}
		return clips[Random.Range(0, clips.Length)];
	}

	private static AudioClip[] Build(string kind)
	{
		System.Random rng = new System.Random(kind.GetHashCode() & 0x7fffffff);
		int count = 4;
		AudioClip[] result = new AudioClip[count];
		for (int i = 0; i < count; i++)
		{
			float[] data;
			if (kind == "mutter")
			{
				data = Mutter(rng);
			}
			else if (kind == "alert")
			{
				data = Alert(rng);
			}
			else
			{
				data = Zap(rng);
			}
			AudioClip clip = AudioClip.Create("proc_" + kind + "_" + i, data.Length, 1, Rate, false);
			clip.SetData(data, 0);
			result[i] = clip;
		}
		return result;
	}

	private static float Square(float phase)
	{
		return (phase - Mathf.Floor(phase)) < 0.5f ? 1f : -1f;
	}

	private static float Saw(float phase)
	{
		return (phase - Mathf.Floor(phase)) * 2f - 1f;
	}

	private static float Rand(System.Random rng, float min, float max)
	{
		return min + (float)rng.NextDouble() * (max - min);
	}

	// low wobbling growl, used as an idle sound
	private static float[] Mutter(System.Random rng)
	{
		float length = Rand(rng, 0.35f, 0.6f);
		int n = (int)(length * Rate);
		float[] d = new float[n];
		float baseHz = Rand(rng, 70f, 130f);
		float wobbleHz = Rand(rng, 6f, 14f);
		float phase = 0f;
		for (int i = 0; i < n; i++)
		{
			float t = (float)i / Rate;
			float u = (float)i / n;
			float hz = baseHz * (1f + 0.25f * Mathf.Sin(t * wobbleHz * 6.2832f)) * (1f - 0.3f * u);
			phase += hz / Rate;
			float env = Mathf.Sin(u * Mathf.PI);
			float s = Square(phase) * 0.6f + Saw(phase * 0.5f) * 0.4f;
			d[i] = Mathf.Round(s * env * 6f) / 6f * 0.35f;
		}
		return d;
	}

	// quick rising blips, used when an enemy notices the player
	private static float[] Alert(System.Random rng)
	{
		int steps = rng.Next(3, 5);
		float stepLen = Rand(rng, 0.05f, 0.08f);
		float startHz = Rand(rng, 260f, 420f);
		float ratio = Rand(rng, 1.25f, 1.5f);
		int n = (int)(steps * stepLen * Rate);
		float[] d = new float[n];
		float phase = 0f;
		for (int i = 0; i < n; i++)
		{
			float t = (float)i / Rate;
			int step = Mathf.Min(steps - 1, (int)(t / stepLen));
			float inStep = (t - step * stepLen) / stepLen;
			float hz = startHz * Mathf.Pow(ratio, step);
			phase += hz / Rate;
			float env = (1f - inStep * 0.6f) * Mathf.Min(1f, (n - i) / (0.02f * Rate));
			d[i] = Square(phase) * env * 0.3f;
		}
		return d;
	}

	// falling noisy buzz, used when an enemy is stunned
	private static float[] Zap(System.Random rng)
	{
		float length = Rand(rng, 0.25f, 0.4f);
		int n = (int)(length * Rate);
		float[] d = new float[n];
		float startHz = Rand(rng, 500f, 800f);
		float phase = 0f;
		for (int i = 0; i < n; i++)
		{
			float u = (float)i / n;
			float hz = Mathf.Lerp(startHz, 90f, u * u);
			phase += hz / Rate;
			float noise = (float)rng.NextDouble() * 2f - 1f;
			float env = 1f - u;
			d[i] = (Square(phase) * 0.6f + noise * 0.4f) * env * 0.35f;
		}
		return d;
	}
}
