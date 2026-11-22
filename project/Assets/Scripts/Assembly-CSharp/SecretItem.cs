using UnityEngine;

public class SecretItem : MonoBehaviour
{
	public int ID = -1;

	private void Start()
	{
	}

	private void Update()
	{
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.transform.tag == "Player")
		{
			GameManager.instance.secretGet(ID);
		}
	}
}
