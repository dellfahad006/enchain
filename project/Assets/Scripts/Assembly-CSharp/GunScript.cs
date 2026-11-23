using UnityEngine;

public class GunScript : MonoBehaviour
{
	public GameObject[] projectiles;

	public Vector3 localOrigin;

	public bool overrideOrigin;

	private void Start()
	{
	}

	private void Update()
	{
	}

	public void FireProjectile(int item, float force, float scale, Vector3 origin, Vector3 dir, string sound, float damageMult = 1f)
	{
		Vector3 position = origin + base.transform.TransformDirection(localOrigin);
		if (overrideOrigin)
		{
			position = origin;
		}
		Quaternion rotation = Quaternion.LookRotation(dir);
		GameObject gameObject = Object.Instantiate(projectiles[item], position, rotation);
		if ((bool)GameObject.Find("_effects"))
		{
			gameObject.transform.parent = GameObject.Find("_effects").transform;
		}
		gameObject.transform.localScale *= scale;
		if (damageMult != 1f)
		{
			CollisionCheck hitCheck = gameObject.GetComponentInChildren<CollisionCheck>();
			if (hitCheck != null)
			{
				hitCheck.baseDamage = Mathf.Max(1, Mathf.RoundToInt((float)hitCheck.baseDamage * damageMult));
			}
		}
		Physics.IgnoreCollision(gameObject.GetComponent<Collider>(), base.gameObject.GetComponent<Collider>(), true);
		Rigidbody component = gameObject.GetComponent<Rigidbody>();
		component.velocity = Vector3.zero;
		component.AddForce(dir * force);
	}

	public void FireRaycast()
	{
	}
}
