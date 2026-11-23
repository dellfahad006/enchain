using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyScript : MonoBehaviour
{
	public int enemyClass;

	public bool lookOnSight = true;

	public bool followOnSight = true;

	public bool returnToStart = true;

	public bool canDash;

	private float dashTimer;

	private float dashTime = 4f;

	private bool dashed;

	private bool preDashed;

	public Transform target;

	public LayerMask sightMask;

	private float shootTimer;

	public string crySound = "cry0";

	private float cryCooldown = 8f;

	private float hurtCooldown = 4f;

	private bool dead;

	public float moveForce = 1f;

	public float maxSpeed = 2f;

	public float maxRange = 8f;

	public float shootCool = 2f;

	private bool wasInSight;

	private float sightTime;

	public string[] animStates = new string[4] { "Idle", "Walk", "Spot", "Stun" };

	public Transform mixingBone;

	public TrailRenderer swingLR;

	private float swingCooldown;

	private bool swingDo;

	private Vector3 initPos;

	private Transform player;

	private Rigidbody rb;

	private Animation anim;

	// Project recovery renamed duplicate clip names to Idle_0, Idle_1, ...
	// Register each such clip again under its original name so the code finds it.
	private void AliasSuffixedClips()
	{
		if (anim == null)
		{
			return;
		}
		List<AnimationState> states = new List<AnimationState>();
		foreach (AnimationState state in anim)
		{
			states.Add(state);
		}
		foreach (AnimationState state2 in states)
		{
			string stateName = state2.name;
			int underscore = stateName.LastIndexOf('_');
			if (underscore <= 0 || underscore == stateName.Length - 1)
			{
				continue;
			}
			bool allDigits = true;
			for (int i = underscore + 1; i < stateName.Length; i++)
			{
				if (!char.IsDigit(stateName[i]))
				{
					allDigits = false;
					break;
				}
			}
			if (!allDigits)
			{
				continue;
			}
			string baseName = stateName.Substring(0, underscore);
			if (anim.GetClip(baseName) == null && state2.clip != null)
			{
				anim.AddClip(state2.clip, baseName);
			}
		}
	}

	private float mutterTimer = -1f;

	private bool stunSfxPlayed;

	// extra procedural enemy sounds: idle mutters and a stun zap
	private void UpdateExtraSounds()
	{
		if (dead || SoundManager.instance == null)
		{
			return;
		}
		Health health = GetComponent<Health>();
		bool stunnedNow = health != null && health.stunned;
		if (stunnedNow && !stunSfxPlayed)
		{
			stunSfxPlayed = true;
			SoundManager.instance.PlayClip(ProcSfx.Get("zap"), base.transform.position, 0.9f, true);
		}
		else if (!stunnedNow)
		{
			stunSfxPlayed = false;
		}
		if (stunnedNow || player == null)
		{
			return;
		}
		if (mutterTimer < 0f)
		{
			mutterTimer = Random.Range(2f, 8f);
		}
		if (Vector3.Distance(base.transform.position, player.position) > 20f)
		{
			return;
		}
		mutterTimer -= Time.deltaTime;
		if (mutterTimer <= 0f)
		{
			mutterTimer = Random.Range(5f, 11f);
			SoundManager.instance.PlayClip(ProcSfx.Get("mutter"), base.transform.position, 0.4f, true);
		}
	}

	private bool HasClip(string clipName)
	{
		return anim != null && anim.GetClip(clipName) != null;
	}

	private HashSet<string> warnedClips = new HashSet<string>();

	private void WarnMissing(string clipName)
	{
		if (warnedClips.Add(clipName))
		{
			string have = "";
			if (anim != null)
			{
				foreach (AnimationState state in anim)
				{
					have += state.name + ", ";
				}
			}
			Debug.LogWarning("[Enchain] '" + name + "' (class " + enemyClass + ") has no clip '" + clipName + "'. Has: " + have);
		}
	}

	private void SafePlay(string clipName)
	{
		if (HasClip(clipName)) { anim.Play(clipName); } else { WarnMissing(clipName); }
	}

	private void SafeCrossFade(string clipName, float fade)
	{
		if (HasClip(clipName)) { anim.CrossFade(clipName, fade); } else { WarnMissing(clipName); }
	}

	private void SafeBlend(string clipName)
	{
		if (HasClip(clipName)) { anim.Blend(clipName); } else { WarnMissing(clipName); }
	}

	private void Start()
	{
		player = GameObject.FindGameObjectWithTag("Player").transform;
		rb = GetComponent<Rigidbody>();
		initPos = base.transform.position;
		dashTime = Random.Range(3.5f, 5f);
		if ((bool)GetComponentInChildren<Animation>())
		{
			anim = GetComponentInChildren<Animation>();
			AliasSuffixedClips();
			if (HasClip(animStates[0])) { anim[animStates[0]].speed = Random.Range(0.95f, 1.05f); }
			if ((bool)mixingBone)
			{
				if (HasClip("Swing1")) { anim["Swing1"].AddMixingTransform(mixingBone); anim["Swing1"].layer = 1; }
				if (HasClip("Cry")) { anim["Cry"].AddMixingTransform(mixingBone); anim["Cry"].layer = 1; }
				if (HasClip("Hurt1")) { anim["Hurt1"].AddMixingTransform(mixingBone); anim["Hurt1"].layer = 1; }
				if (HasClip("Hurt2")) { anim["Hurt2"].AddMixingTransform(mixingBone); anim["Hurt2"].layer = 1; }
			}
		}
	}

	private void Update()
	{
		UpdateExtraSounds();
		if (enemyClass == 0 || enemyClass == 1)
		{
			if (lookOnSight && (bool)player && InSight(player) && !GetComponent<Health>().stunned)
			{
				Vector3 vector = player.position + Vector3.up * 0.5f;
				dashTimer += Time.deltaTime;
				if (InSight(player) && !wasInSight)
				{
					wasInSight = true;
					shootTimer = 0f;
					if ((bool)anim)
					{
						SafePlay("Spot");
					}
					SoundManager.instance.PlaySingle(crySound, base.transform.position, true);
					if (Random.value < 0.5f)
					{
						SoundManager.instance.PlayClip(ProcSfx.Get("alert"), base.transform.position, 0.8f, true);
					}
				}
				sightTime = 0f;
				LookToggle(true);
				if (InSight(player) && followOnSight)
				{
					Vector3 normalized = (vector - base.transform.position).normalized;
					if (rb.velocity.magnitude < maxSpeed && !preDashed)
					{
						rb.AddForce(normalized * moveForce * Time.deltaTime * 10f);
					}
					if (canDash && dashTimer >= dashTime - 0.5f)
					{
						LookToggle(!preDashed);
						Transform transform = null;
						if (!preDashed)
						{
							preDashed = true;
							SoundManager.instance.PlaySingle(crySound, base.transform.position, true);
							transform = base.transform.Find("chargeGlow");
							if ((bool)transform)
							{
								transform.GetComponent<ParticleSystem>().Play();
							}
						}
						if (dashTimer >= dashTime)
						{
							if (!dashed)
							{
								SafePlay("Dash");
								dashed = true;
								SoundManager.instance.PlaySingle("dashSound", base.transform.position, true);
								rb.AddForce(normalized * moveForce * 60f);
							}
							if (dashTimer >= dashTime + 1.75f)
							{
								dashTimer = 0f;
								dashed = false;
								preDashed = false;
								dashTime = Random.Range(3.5f, 5f);
								if ((bool)transform)
								{
									transform.GetComponent<ParticleSystem>().Stop(false, ParticleSystemStopBehavior.StopEmitting);
								}
							}
						}
					}
				}
			}
			else
			{
				LookToggle(false);
				dashTimer = 0f;
				dashed = false;
				preDashed = false;
				sightTime += Time.deltaTime;
				if (sightTime > 2f)
				{
					wasInSight = false;
				}
				if (!wasInSight && !GetComponent<Health>().stunned && returnToStart)
				{
					Vector3 normalized2 = (initPos - base.transform.position).normalized;
					float num = Vector3.Distance(initPos, base.transform.position);
					if (rb.velocity.magnitude < maxSpeed && num > 1f)
					{
						rb.AddForce(normalized2 * moveForce * Time.deltaTime * 5f);
					}
				}
			}
			if ((bool)anim && !anim.IsPlaying("Spot") && !anim.IsPlaying("Stun") && !anim.IsPlaying("Dash"))
			{
				SafePlay(animStates[0]);
			}
			if (GetComponent<Health>().stunned && (bool)anim)
			{
				SafePlay("Stun");
			}
		}
		if (enemyClass == 2 && !dead)
		{
			NavMeshAgent component = GetComponent<NavMeshAgent>();
			cryCooldown -= Time.deltaTime;
			hurtCooldown -= Time.deltaTime;
			if (component.velocity.magnitude > 1f)
			{
				if (HasClip(animStates[1])) { anim[animStates[1]].speed = component.velocity.magnitude / component.speed; }
				SafeCrossFade(animStates[1], 0.5f);
			}
			if ((bool)player && InSight(player) && !anim.IsPlaying("Attack1") && Vector3.Distance(base.transform.position, player.position) < 7.5f)
			{
				swingLR.time = 0.3f;
				SafeBlend("Swing1");
				cryCooldown = 2f;
			}
			if ((bool)player && InSight(player) && (Vector3.Distance(base.transform.position, player.position) > 16f || anim.IsPlaying("Attack1")) && swingCooldown < 0f)
			{
				swingLR.time = 0.3f;
				SafeCrossFade("Attack1", 0.2f);
				if (HasClip("Attack1") && anim["Attack1"].time >= 2f)
				{
					swingCooldown = 6f;
				}
				component.velocity = Vector3.zero;
			}
			if ((bool)player && InSight(player) && cryCooldown < 0f && !anim.IsPlaying("Attack1") && !anim.IsPlaying("Swing1"))
			{
				SafeBlend("Cry");
				if ((bool)SoundManager.instance)
				{
					SoundManager.instance.PlaySingle("cryB2", base.transform.position, true);
				}
				cryCooldown = 10f;
			}
			if ((bool)anim && !anim.IsPlaying("Attack1") && component.velocity.magnitude < 1f)
			{
				SafeCrossFade(animStates[0], 0.6f);
			}
			if (!anim.IsPlaying("Attack1") && !anim.IsPlaying("Swing1"))
			{
				swingLR.time = 0f;
			}
			if ((bool)player && InSight(player) && (!GetComponent<Health>() || !GetComponent<Health>().stunned))
			{
				swingCooldown -= Time.deltaTime;
				Vector3 vector2 = player.position + Vector3.up * 0.5f;
				if (lookOnSight && InSight(player) && !wasInSight)
				{
					wasInSight = true;
					shootTimer = 0f;
				}
				if (followOnSight && component.enabled)
				{
					component.destination = player.position;
				}
				sightTime = 0f;
				if (lookOnSight)
				{
					LookToggle(true);
				}
			}
			else
			{
				LookToggle(false);
				sightTime += Time.deltaTime;
				if (sightTime > 2f)
				{
					wasInSight = false;
				}
				if (!wasInSight && (!GetComponent<Health>() || !GetComponent<Health>().stunned) && component.enabled)
				{
					component.destination = initPos;
				}
			}
		}
		if (enemyClass == 1 && (bool)player && InSight(player) && !GetComponent<Health>().stunned)
		{
			shootTimer += Time.deltaTime;
			if (shootTimer >= shootCool)
			{
				Vector3 forward = base.transform.forward;
				shootTimer = 0f;
				GetComponent<GunScript>().FireProjectile(0, 100f, 1f, base.transform.position, forward, string.Empty);
				SoundManager.instance.PlaySingle("shootShort1", base.transform.position, true);
			}
		}
	}

	private bool InSight(Transform tr)
	{
		Vector3 normalized = (tr.position - base.transform.position).normalized;
		RaycastHit hitInfo;
		if (Physics.Raycast(base.transform.position, normalized, out hitInfo, maxRange, sightMask))
		{
			Transform transform = hitInfo.transform;
			float num = Vector3.Distance(base.transform.position, tr.position);
			if (tr == transform && num <= maxRange)
			{
				if (tr.tag == "Player" && (bool)tr.GetComponent<Health>() && tr.GetComponent<Health>().dead)
				{
					return false;
				}
				return true;
			}
		}
		return false;
	}

	private void LookToggle(bool a)
	{
		float effect = 0f;
		if (a)
		{
			effect = 1f;
		}
		if ((bool)GetComponent<LookAt>())
		{
			GetComponent<LookAt>().look = a;
		}
		if ((bool)GetComponent<HeadLookController>())
		{
			GetComponent<HeadLookController>().effect = effect;
			if (a)
			{
				GetComponent<HeadLookController>().targetObj = player;
			}
		}
	}

	public void FootstepEvent(string fName)
	{
		if ((bool)SoundManager.instance)
		{
			SoundManager.instance.PlaySingle(fName, base.transform.position, true);
		}
	}

	public void SwingEvent(GameObject obj)
	{
		GameObject gameObject = Object.Instantiate(obj, base.transform.position, base.transform.rotation);
		if ((bool)GameObject.Find("_effects"))
		{
			gameObject.transform.parent = GameObject.Find("_effects").transform;
		}
	}

	public void HurtAnim()
	{
		if ((bool)anim && !anim.IsPlaying("Attack1") && !anim.IsPlaying("Swing1") && !anim.IsPlaying("Cry") && hurtCooldown <= 0f)
		{
			SafeBlend("Hurt" + Random.Range(1, 3));
			if ((bool)SoundManager.instance)
			{
				SoundManager.instance.PlaySingle("cryHurt", base.transform.position, true);
			}
			hurtCooldown = 1.3f;
		}
	}

	public void DeathAnim(int a)
	{
		if (a == 1)
		{
			base.transform.Find("boom").GetComponent<ParticleSystem>().Emit(30);
			GameManager.instance.DropSouls();
			return;
		}
		swingLR.time = 0f;
		GetComponent<Health>().enabled = false;
		GetComponent<NavMeshAgent>().enabled = false;
		LookToggle(false);
		GetComponent<Collider>().enabled = false;
		GetComponent<Rigidbody>().useGravity = false;
		GetComponent<Rigidbody>().isKinematic = true;
		Material[] sharedMaterials = new Material[2]
		{
			GetComponent<Health>().stunMat,
			GetComponent<Health>().stunMat
		};
		base.transform.Find("skullBoss").GetComponent<SkinnedMeshRenderer>().sharedMaterials = sharedMaterials;
		if ((bool)SoundManager.instance)
		{
			SoundManager.instance.PlaySingle("cryDie", base.transform.position, true);
		}
		SafeCrossFade("Death", 0.1f);
		dead = true;
	}
}
