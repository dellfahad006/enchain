using System;
using UnityEngine;

public class CollisionCheck : MonoBehaviour
{
	public GameObject receiver;

	public bool dealsDamage;

	public bool dieOnCollision;

	public bool dealSelfDam;

	public bool heals;

	public GameObject hitObj;

	public GameObject damageObj;

	public int baseDamage = 5;

	public float damageByForce;

	public float applyForce;

	public bool useVelocity;

	public bool doIfTrigger;

	public bool useAccel;

	public string[] damageTags = new string[2] { "Enemy", "Prop" };

	private bool willSelfDamage;

	public int damageType;

	public string hitSound = string.Empty;

	private Rigidbody rb;

	private void Start()
	{
		rb = GetComponent<Rigidbody>();
	}

	private void Update()
	{
	}

	private void OnCollisionEnter(Collision collision)
	{
		if ((bool)receiver)
		{
			SendCollision(receiver, collision.collider, false);
		}
		CollChecks(collision.collider, collision.contacts[0].point, collision.contacts[0].normal, false, false);
	}

	private void OnCollisionStay(Collision collision)
	{
		if (base.transform.tag == "Enemy" && collision.transform.tag == "Player" && applyForce > 0f && (bool)collision.transform.GetComponent<Rigidbody>() && applyForce > 0f)
		{
			Rigidbody component = collision.transform.GetComponent<Rigidbody>();
			component.velocity = Vector3.zero;
			Vector3 vector = collision.transform.position - base.transform.position;
			component.AddForce(Vector3.Scale(vector.normalized, new Vector3(1f, 0f, 1f)) * applyForce);
			if (component.gameObject.tag == "Player")
			{
				component.AddForce(Vector3.Scale(vector.normalized, new Vector3(1f, 0f, 1f)) * applyForce / 2f);
			}
		}
		if (base.transform.tag == "Enemy")
		{
			CollChecks(collision.collider, collision.contacts[0].point, collision.contacts[0].normal, false, true);
		}
	}

	private void OnTriggerStay(Collider other)
	{
		if ((bool)receiver && other.gameObject.layer != 2)
		{
			SendCollision(receiver, other, true);
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.layer != 2)
		{
			CollChecks(other, base.transform.position, Vector3.up, true, false);
		}
	}

	private void SendCollision(GameObject go, Collider coll, bool trigger)
	{
		if ((bool)go.GetComponent<PlayerScript>())
		{
			go.GetComponent<PlayerScript>().GrappleCollide(coll, trigger);
		}
	}

	private void CollChecks(Collider coll, Vector3 point, Vector3 collNorm, bool trigger, bool colStay)
	{
		if ((bool)coll.transform.GetComponent<Rigidbody>() && applyForce > 0f && (!trigger || doIfTrigger) && !colStay)
		{
			Rigidbody component = coll.transform.GetComponent<Rigidbody>();
			component.velocity = Vector3.zero;
			Vector3 vector = (component.transform.position - base.transform.position).normalized;
			if ((base.transform.tag == "Enemy" && component.transform.tag == "Enemy") || ((bool)rb && rb.velocity.magnitude == 0f))
			{
				vector = Vector3.zero;
			}
			if ((bool)rb && useVelocity)
			{
				vector = rb.velocity.normalized;
			}
			ForceMode mode = ForceMode.Force;
			if (useAccel)
			{
				mode = ForceMode.Acceleration;
			}
			component.AddForce(vector * applyForce, mode);
			if (component.gameObject.tag == "Player")
			{
				component.AddForce(Vector3.up * applyForce / 2f);
			}
		}
		if (!trigger && !colStay && hitSound != string.Empty)
		{
			SoundManager.instance.PlaySingle(hitSound, base.transform.position, true);
		}
		int num = baseDamage;
		if (damageByForce > 0f && !colStay)
		{
			float num2 = Vector3.Magnitude(coll.attachedRigidbody.velocity - rb.velocity);
			num *= Mathf.RoundToInt(num2 * damageByForce / 10f);
		}
		if (dealsDamage && !colStay && (bool)coll.transform.GetComponent<Health>() && Array.IndexOf(damageTags, coll.gameObject.tag) != -1 && (!GetComponent<Health>() || !GetComponent<Health>().stunned))
		{
			int type = damageType;
			Health component2 = coll.transform.GetComponent<Health>();
			component2.TakeDamage(num, type);
			if ((bool)damageObj && !coll.transform.GetComponent<Health>().stunned)
			{
				DoHitFX(point, collNorm, base.gameObject, damageObj, false);
			}
		}
		if (dealSelfDam && (!coll.transform.GetComponent<CollisionCheck>() || willSelfDamage) && !trigger && (bool)GetComponent<Health>() && GetComponent<Health>().stunned && willSelfDamage && (bool)GetComponent<Health>() && Array.IndexOf(damageTags, coll.gameObject.tag) != -1)
		{
			if (coll.gameObject.tag == "Enemy")
			{
				Health component3 = coll.transform.GetComponent<Health>();
				component3.TakeDamage(num * 2, damageType);
			}
			int type2 = 1;
			Health component4 = GetComponent<Health>();
			int stunHP = component4.stunHP;
			component4.TakeDamage(stunHP, type2);
		}
		if (heals && coll.gameObject.tag == "Player" && (bool)coll.GetComponent<Health>())
		{
			SoundManager.instance.PlaySingle("confirm1", base.transform.position, true);
			coll.GetComponent<Health>().HealAmt(baseDamage);
			if (dieOnCollision)
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}
		if ((bool)hitObj && !colStay)
		{
			DoHitFX(point, -rb.velocity.normalized, base.gameObject, hitObj, dieOnCollision);
		}
	}

	public void DidThrow(bool val)
	{
		if (dealSelfDam && (bool)GetComponent<Health>() && GetComponent<Health>().stunned && val)
		{
			willSelfDamage = true;
			GetComponent<Health>().StartCountdown(2f);
			damageTags = new string[3] { "Untagged", "Enemy", "Prop" };
			damageType = 0;
		}
		if (!val)
		{
			willSelfDamage = false;
			damageType = 1;
			damageTags = new string[4] { "Untagged", "Enemy", "Prop", "Player" };
		}
	}

	public static void DoHitFX(Vector3 point, Vector3 normal, GameObject self, GameObject hitObj, bool destroy)
	{
		Quaternion rotation = Quaternion.identity;
		if (normal != Vector3.zero)
		{
			rotation = Quaternion.LookRotation(normal);
		}
		if ((bool)hitObj)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(hitObj, point, rotation);
			if ((bool)GameObject.Find("_effects"))
			{
				gameObject.transform.parent = GameObject.Find("_effects").transform;
			}
			if (destroy)
			{
				if ((bool)self && (bool)self.GetComponent<MeshRenderer>())
				{
					self.GetComponent<MeshRenderer>().enabled = false;
				}
				if ((bool)self && (bool)self.GetComponent<SelfDestruct>() && (bool)self.GetComponent<ParticleSystem>())
				{
					self.GetComponent<ParticleSystem>().Stop();
					self.GetComponent<Collider>().enabled = false;
					self.GetComponent<Rigidbody>().isKinematic = true;
					self.GetComponent<SelfDestruct>().setTo(0.9f);
				}
				else if (destroy)
				{
					UnityEngine.Object.Destroy(self);
				}
			}
		}
		else if ((bool)self && destroy)
		{
			self.GetComponent<Collider>().enabled = false;
			self.GetComponent<Rigidbody>().isKinematic = true;
			UnityEngine.Object.Destroy(self);
		}
	}

	public static void DoHitFX(Vector3 point, Vector3 normal, GameObject self, GameObject[] hitObjs, bool destroy)
	{
		Quaternion rotation = Quaternion.LookRotation(normal);
		if (hitObjs.Length > 0)
		{
			foreach (GameObject original in hitObjs)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(original, point, rotation);
				if ((bool)GameObject.Find("_effects"))
				{
					gameObject.transform.parent = GameObject.Find("_effects").transform;
				}
			}
			if (destroy)
			{
				if ((bool)self && (bool)self.GetComponent<MeshRenderer>())
				{
					self.GetComponent<MeshRenderer>().enabled = false;
				}
				if ((bool)self && (bool)self.GetComponent<SelfDestruct>() && (bool)self.GetComponent<ParticleSystem>())
				{
					self.GetComponent<ParticleSystem>().Stop();
					self.GetComponent<Collider>().enabled = false;
					self.GetComponent<Rigidbody>().isKinematic = true;
					self.GetComponent<SelfDestruct>().setTo(0.9f);
				}
				else if (destroy)
				{
					UnityEngine.Object.Destroy(self);
				}
			}
		}
		else if ((bool)self && destroy)
		{
			self.GetComponent<Collider>().enabled = false;
			self.GetComponent<Rigidbody>().isKinematic = true;
			UnityEngine.Object.Destroy(self);
		}
	}
}
