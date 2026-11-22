using UnityEngine;

public class SelfDestruct : MonoBehaviour
{
	public float duration = 1f;

	private float timer;

	private void Start()
	{
	}

	private void Update()
	{
		timer += Time.deltaTime;
		if (timer >= duration)
		{
			Object.Destroy(base.gameObject);
		}
	}

	public void setTo(float dur)
	{
		timer = 0f;
		duration = dur;
	}
}
