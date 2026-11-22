using System;
using System.Collections;
using UnityEngine;

public class Health : MonoBehaviour
{
	public int hitPoints = 10;

	public float hitstun;

	private float hitstunTimer;

	public int currHP;

	public int stunHP;

	public float stunFor = 5f;

	public bool invin;

	public bool hurtAnim;

	public bool dieAnim;

	private float stunnedTime;

	[NonSerialized]
	public bool grappled;

	public int[] damagedBy = new int[2] { 0, 1 };

	public int[] stunDamagedBy = new int[1] { 1 };

	[NonSerialized]
	public bool stunned;

	private Material startMat;

	public Material stunMat;

	public Renderer rendStun;

	public bool givesSoul;

	public string[] hpSounds = new string[3]
	{
		string.Empty,
		string.Empty,
		string.Empty
	};

	public GameObject[] deathObjects;

	[NonSerialized]
	public bool dead;

	[NonSerialized]
	public bool willDie;

	[NonSerialized]
	public bool souled;

	private bool player;

	private float dragCache;

	private void Start()
	{
		if (currHP == 0)
		{
			currHP = hitPoints;
		}
		if ((bool)rendStun)
		{
			startMat = rendStun.material;
		}
		if (base.gameObject.tag == "Player")
		{
			player = true;
		}
	}

	private void Update()
	{
		hitstunTimer += Time.deltaTime;
		if (hitstunTimer >= hitstun && player && !dead)
		{
			GameManager.instance.playerHurt(false, false);
		}
		if (stunned && !grappled && !willDie)
		{
			stunnedTime += Time.deltaTime;
		}
		else
		{
			stunnedTime = 0f;
		}
		if (base.gameObject.tag == "Player")
		{
			GameManager.instance.playerMaxHP = hitPoints;
			GameManager.instance.playerHP = currHP;
		}
		if (stunnedTime >= stunFor && stunned && !willDie)
		{
			ResetHealth(2f);
		}
	}

	public void TakeDamage(int damage, int type)
	{
		if (hitstunTimer < hitstun || souled || dead || invin)
		{
			return;
		}
		hitstunTimer = 0f;
		GameManager.instance.playerHurt(true, false);
		if ((Array.IndexOf(damagedBy, type) != -1 && !stunned) || (stunned && Array.IndexOf(stunDamagedBy, type) != -1))
		{
			currHP -= damage;
			SoundManager.instance.PlaySingle(hpSounds[2], base.transform.position, true);
		}
		if (hurtAnim && (bool)GetComponent<EnemyScript>())
		{
			GetComponent<EnemyScript>().HurtAnim();
		}
		if (currHP > 0 || dead)
		{
			return;
		}
		currHP = 0;
		if (stunHP > 0 && stunned)
		{
			Kill();
		}
		else
		{
			if (stunned)
			{
				return;
			}
			if (stunHP > 0)
			{
				SoundManager.instance.PlaySingle(hpSounds[1], base.transform.position, true);
				stunned = true;
				dragCache = GetComponent<Rigidbody>().drag;
				GetComponent<Rigidbody>().drag *= 2f;
				if ((bool)GetComponent<GravityCustom>())
				{
					GetComponent<GravityCustom>().gravityScale = 0.35f;
				}
				if ((bool)stunMat)
				{
					rendStun.sharedMaterial = stunMat;
				}
				currHP = stunHP;
			}
			else
			{
				Kill();
			}
		}
	}

	public void HealAmt(int amt)
	{
		if (!dead)
		{
			currHP = Mathf.Clamp(currHP + amt, 0, hitPoints);
		}
	}

	private void Kill()
	{
		if (deathObjects.Length > 0)
		{
			SoundManager.instance.PlaySingle(hpSounds[0], base.transform.position, true);
			CollisionCheck.DoHitFX(base.transform.position, Vector3.up, base.gameObject, deathObjects, false);
		}
		if (player)
		{
			PlayerDie(base.gameObject);
		}
		else if (dieAnim && (bool)GetComponent<EnemyScript>())
		{
			GetComponent<EnemyScript>().DeathAnim(0);
		}
		else if (!givesSoul)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
		else
		{
			GiveSoul();
		}
	}

	private void GiveSoul()
	{
		if (GameManager.instance.soulDatas.Count < 5)
		{
			souled = true;
			GameManager.instance.SoulTake(base.gameObject);
		}
	}

	public void ResetHealth(float divBy)
	{
		stunned = false;
		GetComponent<Rigidbody>().drag = dragCache;
		if ((bool)GetComponent<GravityCustom>() && base.transform.tag == "Enemy")
		{
			GetComponent<GravityCustom>().gravityScale = 0f;
		}
		dead = false;
		souled = false;
		willDie = false;
		hitstunTimer = 0f;
		currHP = Mathf.CeilToInt((float)hitPoints * 1f / divBy);
		if ((bool)startMat)
		{
			rendStun.sharedMaterial = startMat;
		}
		if ((bool)GetComponent<CollisionCheck>() && base.transform.tag == "Enemy")
		{
			GetComponent<CollisionCheck>().DidThrow(false);
		}
	}

	public void PlayerDie(GameObject go)
	{
		dead = true;
		currHP = 0;
		GameManager.instance.playerHurt(true, true);
		if ((bool)GetComponent<PhysicsFPSWalker>().windLoop)
		{
			GetComponent<PhysicsFPSWalker>().windLoop.enabled = false;
		}
		GetComponent<PhysicsFPSWalker>().enabled = false;
		GetComponentInChildren<DragView>().gameObject.SetActive(false);
		GetComponent<PlayerScript>().enabled = false;
		GetComponent<Rigidbody>().velocity = Vector3.zero;
		GetComponent<Rigidbody>().drag = 2f;
		if (base.transform.position.y <= 40f)
		{
			GetComponent<Rigidbody>().isKinematic = true;
		}
		GameObject.FindGameObjectWithTag("MainCamera").GetComponent<MouseLook>().enabled = false;
		GameObject.FindGameObjectWithTag("MainCamera").transform.localEulerAngles = Vector3.Scale(GameObject.FindGameObjectWithTag("MainCamera").transform.localEulerAngles, Vector3.up);
		GameObject.FindGameObjectWithTag("MainCamera").transform.localPosition = new Vector3(0f, -0.7f, 0f);
		SoundManager.instance.PlaySingle("death1", base.transform.position - Vector3.up, true);
	}

	public void StartCountdown(float sec)
	{
		willDie = true;
		StartCoroutine(countdown(sec));
	}

	private IEnumerator countdown(float s)
	{
		stunnedTime = 0f;
		yield return new WaitForSeconds(s);
		if (willDie)
		{
			Kill();
			willDie = false;
		}
		else
		{
			ResetHealth(1f);
		}
	}
}
