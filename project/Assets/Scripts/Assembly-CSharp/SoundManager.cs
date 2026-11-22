using UnityEngine;

public class SoundManager : MonoBehaviour
{
	public static SoundManager instance;

	public GameObject fxPrefab;

	public float fxVolume = 1f;

	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
		}
		else if (instance != this)
		{
			Object.Destroy(base.gameObject);
		}
		Object.DontDestroyOnLoad(base.gameObject);
	}

	private void Start()
	{
	}

	private void Update()
	{
	}

	public void PlaySingle(string soundName, Vector3 pos, bool randPitch)
	{
		if (!(soundName == string.Empty))
		{
			GameObject gameObject = Object.Instantiate(fxPrefab, pos, Quaternion.identity);
			if ((bool)GameObject.Find("_effects"))
			{
				gameObject.transform.parent = GameObject.Find("_effects").transform;
			}
			AudioSource component = gameObject.GetComponent<AudioSource>();
			AudioClip clip = (AudioClip)Resources.Load(soundName);
			component.clip = clip;
			gameObject.GetComponent<SelfDestruct>().duration = component.clip.length;
			float pitch = 1f;
			if (randPitch)
			{
				pitch = Random.Range(0.95f, 1.05f);
			}
			component.pitch = pitch;
			component.volume = fxVolume;
			component.Play();
		}
	}

	public void PlaySingle(string soundName)
	{
		if (!(soundName == string.Empty))
		{
			GameObject gameObject = Object.Instantiate(fxPrefab, Vector3.zero, Quaternion.identity);
			if ((bool)GameObject.Find("_effects"))
			{
				gameObject.transform.parent = GameObject.Find("_effects").transform;
			}
			AudioSource component = gameObject.GetComponent<AudioSource>();
			AudioClip clip = (AudioClip)Resources.Load(soundName);
			component.clip = clip;
			gameObject.GetComponent<SelfDestruct>().duration = component.clip.length;
			component.spatialBlend = 0f;
			component.volume = fxVolume * 0.7f;
			component.Play();
		}
	}
}
