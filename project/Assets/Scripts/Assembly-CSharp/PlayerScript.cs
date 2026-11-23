using System;
using System.Collections;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
	private Camera cam;

	public GameObject grabber;

	private Vector3 camOffsetHand = Vector3.zero;

	public GameObject hookshot;

	public float hsForce = 10f;

	public float hsThrow = 10f;

	private Vector3 hsFirePoint = Vector3.zero;

	private ConfigurableJoint cjA;

	private ConfigurableJoint cjB;

	public float maxRange = 15f;

	[NonSerialized]
	public float perfMult = 1f;

	private bool hooked;

	private Transform lastHL;

	public GameObject highlightObj;

	private Transform hookingTo;

	public LayerMask grappleMask;

	public float grappleRadius = 0.2f;

	private GameObject HLObj;

	private bool hookBreak;

	private bool hookReturn;

	private bool startRtrn;

	private float coolF1;

	private float coolF2;

	[NonSerialized]
	public float gunCharge = 1f;

	private bool charged = true;

	// --- gun rework state ---
	private bool fireLocked = true;

	private bool gunEmptied;

	private float reloadLock;

	// 1.5 = original drain while the bar is above 0.92
	private const float DrainStart = 1.5f;

	private float hookBaseRadius = -1f;

	private float swingBoostTimer;

	private Transform swingAnchor;

	[NonSerialized]
	public float sens = 10f;

	public Transform firepnt;

	public GameObject[] muzzleFX;

	private Vector3 initVmPoint;

	private Transform viewM;

	public Animation vmAnim;

	private void Start()
	{
		cam = GetComponentInChildren<Camera>();
		SphereCollider tipCollider = hookshot.GetComponent<SphereCollider>();
		if (tipCollider != null)
		{
			hookBaseRadius = tipCollider.radius;
		}
		cjA = GetComponent<ConfigurableJoint>();
		cjB = grabber.GetComponent<ConfigurableJoint>();
		Vector3 vector = grabber.transform.localPosition + grabber.GetComponent<LineRenderer>().GetPosition(0);
		camOffsetHand = vector;
		viewM = vmAnim.transform;
		initVmPoint = viewM.localPosition;
		vmAnim["IdleR"].AddMixingTransform(base.transform.Find("Main Camera/viewmodel/metarig/chest/upper_arm_R"));
		vmAnim["FireR"].AddMixingTransform(base.transform.Find("Main Camera/viewmodel/metarig/chest/upper_arm_R"));
		vmAnim["Fire2R"].AddMixingTransform(base.transform.Find("Main Camera/viewmodel/metarig/chest/upper_arm_R"));
		vmAnim["ChainedR"].AddMixingTransform(base.transform.Find("Main Camera/viewmodel/metarig/chest/upper_arm_R"));
		vmAnim["IdleL"].layer = 1;
		vmAnim["IdleL"].AddMixingTransform(base.transform.Find("Main Camera/viewmodel/metarig/chest/upper_arm_L"));
		vmAnim["FireL"].layer = 1;
		vmAnim["FireL"].AddMixingTransform(base.transform.Find("Main Camera/viewmodel/metarig/chest/upper_arm_L"));
		vmAnim["HoldL"].layer = 1;
		vmAnim["HoldL"].AddMixingTransform(base.transform.Find("Main Camera/viewmodel/metarig/chest/upper_arm_L"));
		vmAnim["ReturnL"].layer = 1;
		vmAnim["ReturnL"].AddMixingTransform(base.transform.Find("Main Camera/viewmodel/metarig/chest/upper_arm_L"));
	}

	private void Update()
	{
		coolF1 += Time.deltaTime;
		coolF2 += Time.deltaTime;
		gunCharge += Time.deltaTime;
		gunCharge = Mathf.Clamp01(gunCharge);
		bool flag = false;
		bool flag2 = false;
		if (hookReturn)
		{
			HookshotReturn();
		}
		if (hooked && (bool)hookingTo)
		{
			hookshot.transform.position = hookingTo.position;
		}
		if (sens > 0f)
		{
			GetComponent<MouseLook>().sensitivityX = sens;
			GetComponent<MouseLook>().sensitivityY = sens;
			cam.GetComponent<MouseLook>().sensitivityX = sens;
			cam.GetComponent<MouseLook>().sensitivityY = sens;
		}
		bool paused = GameManager.instance.gamePaused;
		if (paused)
		{
			fireLocked = true;
		}
		else if (fireLocked && !Input.GetButton("Fire1") && !Input.GetButtonUp("Fire1"))
		{
			fireLocked = false;
		}
		bool canFire = !paused && !fireLocked && !flag;
		reloadLock -= Time.deltaTime;
		bool reloading = reloadLock > 0f;
		if (!Input.GetButton("Fire1") && !Input.GetButtonUp("Fire1"))
		{
			gunEmptied = false;
		}
		if (canFire && Input.GetButtonUp("Fire1"))
		{
			if (gunEmptied)
			{
				gunEmptied = false;
				gunCharge = 0f;
				StartReload();
			}
			else if (gunCharge >= 0.92f && !reloading)
			{
				gunCharge = 0f;
				PlayerFire(0, 10, 0.07f, 1f);
				SoundManager.instance.PlaySingle("shootBig1", base.transform.position - Vector3.up, true);
				StartReload();
			}
		}
		if (canFire && !reloading && Input.GetButton("Fire1") && !gunEmptied)
		{
			gunCharge -= Time.deltaTime * ((gunCharge > 0.92f) ? DrainStart : DevSettings.barDrainHold);
			if (gunCharge <= 0f)
			{
				gunCharge = 0f;
				gunEmptied = true;
			}
			else if (coolF1 >= 0.1f && gunCharge <= 0.92f)
			{
				SoundManager.instance.PlaySingle("shootShort1", base.transform.position - Vector3.up, true);
				PlayerFire(0, 1, 0.03f, DevSettings.shotDamageMult);
				coolF1 = 0f;
				vmAnim.Stop("FireR");
				vmAnim.Play("FireR", PlayMode.StopSameLayer);
			}
		}
		if (canFire && !reloading && Input.GetButton("Fire1") && gunEmptied && DevSettings.slowFireWhenEmpty && coolF1 >= 0.2f)
		{
			SoundManager.instance.PlaySingle("shootShort1", base.transform.position - Vector3.up, true);
			PlayerFire(0, 1, 0.03f, DevSettings.shotDamageMult);
			coolF1 = 0f;
			vmAnim.Stop("FireR");
			vmAnim.Play("FireR", PlayMode.StopSameLayer);
		}
		if (flag)
		{
			vmAnim.Play("ChainedR");
			vmAnim.GetComponent<LineRenderer>().enabled = true;
		}
		else
		{
			vmAnim.GetComponent<LineRenderer>().enabled = false;
		}
		if (flag2)
		{
			vmAnim.Play("HoldL");
		}
		float speed = 1f + GetComponent<Rigidbody>().velocity.magnitude / 1.4f;
		vmAnim["IdleR"].speed = speed;
		vmAnim["IdleL"].speed = speed;
		if (!GetComponent<PhysicsFPSWalker>().IsGrounded())
		{
			vmAnim["IdleR"].speed = 0.5f;
			vmAnim["IdleL"].speed = 0.5f;
		}
		if (!Input.GetButton("Fire1") && !vmAnim.IsPlaying("IdleR") && !vmAnim.IsPlaying("FireR") && !vmAnim.IsPlaying("Fire2R") && !flag)
		{
			vmAnim.CrossFade("IdleR", 0.2f);
		}
		if (gunCharge >= 0.95f && !charged)
		{
			charged = true;
			SoundManager.instance.PlaySingle("load1", base.transform.position - Vector3.up, true);
		}
		else if (gunCharge < 0.5f)
		{
			charged = false;
		}
		gunCharge = Mathf.Clamp01(gunCharge);
		if (!hooked)
		{
			Transform obj = GetHighlightTarget();
			if (((bool)lastHL && (bool)HLObj && (obj == null || obj != lastHL)) || (!lastHL && (bool)HLObj))
			{
				UnityEngine.Object.Destroy(HLObj);
				HLObj = null;
				lastHL = null;
			}
			if (lastHL == null && CanGrapple(obj))
			{
				lastHL = obj;
				addHighlightObj(obj);
			}
		}
		if (hooked)
		{
			if (((bool)HLObj && (bool)hookingTo && (!lastHL || lastHL != hookingTo)) || ((!hookingTo || !hookingTo.gameObject.activeSelf) && (bool)HLObj))
			{
				if ((!hookingTo || !hookingTo.gameObject.activeSelf) && (bool)HLObj)
				{
					hooked = false;
					hookReturn = true;
					HookshotReturn();
				}
				UnityEngine.Object.Destroy(HLObj);
				HLObj = null;
				lastHL = null;
			}
			if ((bool)hookingTo && lastHL == null)
			{
				Transform transform = hookingTo;
				lastHL = hookingTo;
				addHighlightObj(hookingTo);
			}
		}
		if (!vmAnim.IsPlaying("FireL"))
		{
			if (hookReturn)
			{
				vmAnim.CrossFade("ReturnL", 0.2f);
			}
			else if (!hookshot.activeSelf)
			{
				vmAnim.CrossFade("IdleL", 0.5f);
			}
		}
		if (Input.GetButton("Fire2") && hooked && !vmAnim.IsPlaying("FireL"))
		{
			vmAnim.CrossFade("HoldL", 1f);
		}
		if (Input.GetButtonDown("Fire2") && !hooked && !hookReturn && !GameManager.instance.gamePaused && !flag2 && coolF2 > 0.9f)
		{
			vmAnim.CrossFade("FireL", 0.1f);
			HookshotFire(cam.transform.forward.normalized);
			coolF2 = 0f;
		}
		if (Input.GetButtonUp("Fire2") && !GameManager.instance.gamePaused)
		{
			if (hooked && (bool)hookingTo)
			{
				hookshot.transform.position = hookingTo.position;
			}
			if (hookshot.activeSelf || hooked)
			{
				hooked = false;
				hookReturn = true;
				HookshotReturn();
			}
		}
		if (!hooked)
		{
			cjA.connectedAnchor = base.transform.position;
			cjA.connectedBody = null;
			cjB.connectedBody = null;
			FreeMotion(cjA, true);
			grabber.SetActive(false);
			if (hookshot.activeSelf && Vector3.Distance(hookshot.transform.position, hsFirePoint) > maxRange)
			{
				hookshot.GetComponent<Rigidbody>().velocity = Vector3.zero;
			}
			if (hookshot.activeSelf)
			{
				float num2 = maxRange * 1.3f;
				float num3 = 5f;
				float num4 = Vector3.Distance(hookshot.transform.position, cam.transform.position);
				if (num4 > num2)
				{
					hooked = false;
					hookReturn = true;
					HookshotReturn();
				}
			}
		}
		float num5 = GameManager.instance.soulDatas.Count;
		perfMult = Mathf.Clamp((num5 + 1f) / 2.5f, 1f, 2f);
		if (perfMult == 0f)
		{
			perfMult = 1f;
		}
		if (!grabber.activeSelf && hooked && !GetComponent<LineRenderer>().enabled)
		{
			GetComponent<LineRenderer>().enabled = true;
		}
		UpdateHookForgiveness();
	}

	private void FixedUpdate()
	{
		if (swingBoostTimer > 0f)
		{
			if (!hooked || swingAnchor == null)
			{
				swingBoostTimer = 0f;
			}
			else
			{
				swingBoostTimer -= Time.fixedDeltaTime;
				GetComponent<Rigidbody>().AddForce(SwingBoostDir(swingAnchor.position) * (DevSettings.swingBoost / Mathf.Max(0.01f, DevSettings.swingBoostDuration)), ForceMode.Acceleration);
			}
		}
		Transform transform = base.transform;
		if ((bool)cjB.connectedBody)
		{
			transform = cjB.connectedBody.transform;
		}
		Vector3 position = base.transform.InverseTransformPoint(cjA.connectedAnchor);
		Vector3 position2 = grabber.transform.InverseTransformPoint(transform.position);
		Vector3 position3 = cam.transform.TransformPoint(camOffsetHand);
		GetComponent<LineRenderer>().SetPosition(0, base.transform.InverseTransformPoint(position3));
		if (hooked)
		{
			GetComponent<LineRenderer>().SetPosition(1, position);
		}
		else
		{
			GetComponent<LineRenderer>().SetPosition(1, base.transform.InverseTransformPoint(hookshot.transform.position));
		}
		grabber.GetComponent<LineRenderer>().SetPosition(1, position2);
	}

	private void PlayerFire(int type, int shotCount, float inacc, float damageMult)
	{
		for (int i = 0; i < shotCount; i++)
		{
			float num = UnityEngine.Random.Range(0f - inacc, inacc);
			float num2 = UnityEngine.Random.Range(0f - inacc, inacc);
			Vector3 vector = cam.transform.right * num;
			Vector3 vector2 = cam.transform.up * num2;
			Vector3 normalized = (cam.transform.forward.normalized + vector + vector2).normalized;
			Vector3 vector3 = cam.transform.TransformPoint(GetComponent<GunScript>().localOrigin);
			vector3 = firepnt.position;
			GetComponent<GunScript>().FireProjectile(type, 500f, 1f, vector3, normalized, string.Empty, damageMult);
		}
		if (muzzleFX.Length > 0)
		{
			int num3 = 0;
			if (shotCount > 1)
			{
				num3 = 1;
			}
			GameObject gameObject = UnityEngine.Object.Instantiate(muzzleFX[num3], firepnt.position, Quaternion.LookRotation(cam.transform.forward));
			gameObject.transform.parent = firepnt;
			if (shotCount > 1)
			{
				BrightenBlast(gameObject);
			}
		}
	}

	private Transform GetHighlightTarget()
	{
		Vector3 forward = cam.transform.forward;
		RaycastHit hitInfo;
		if (DevSettings.hookPointHighlightRadius > 0f && Physics.SphereCast(cam.transform.position, DevSettings.hookPointHighlightRadius, forward, out hitInfo, maxRange, grappleMask) && hitInfo.transform.tag == "Hook")
		{
			return hitInfo.transform;
		}
		float radius = DevSettings.enemyHighlightRadius;
		if (radius > 0f && Physics.SphereCast(cam.transform.position, radius, forward, out hitInfo, maxRange, grappleMask))
		{
			return hitInfo.transform;
		}
		if (radius <= 0f && Physics.Raycast(cam.transform.position, forward, out hitInfo, maxRange))
		{
			return hitInfo.transform;
		}
		return null;
	}

	private Transform GrabRoot(Collider coll)
	{
		Transform transform = coll.transform;
		if ((bool)transform.parent && transform.parent.tag == "Enemy")
		{
			transform = transform.parent;
		}
		return transform;
	}

	// makes the chain tip more forgiving: smaller wall collider, bigger catch radius
	private void UpdateHookForgiveness()
	{
		SphereCollider tip = hookshot.GetComponent<SphereCollider>();
		if (tip != null && hookBaseRadius > 0f)
		{
			tip.radius = hookBaseRadius * Mathf.Max(0.05f, DevSettings.hookWallColliderScale);
		}
		if (!hookshot.activeSelf || hooked || hookReturn || hookBreak || GameManager.instance.gamePaused)
		{
			return;
		}
		float radius = DevSettings.hookCatchRadius;
		if (radius <= 0f)
		{
			return;
		}
		Collider[] found = Physics.OverlapSphere(hookshot.transform.position, radius, grappleMask, QueryTriggerInteraction.Collide);
		foreach (Collider coll in found)
		{
			if (coll == null || !coll.gameObject.activeInHierarchy || coll.transform == hookshot.transform || coll.transform.IsChildOf(base.transform))
			{
				continue;
			}
			if (CanGrapple(GrabRoot(coll)))
			{
				HookToTransform(coll.transform);
				return;
			}
		}
	}

	private Vector3 SwingBoostDir(Vector3 anchorPos)
	{
		Vector3 toAnchor = (anchorPos - base.transform.position).normalized;
		Vector3 look = cam.transform.forward.normalized;
		Vector3 dir = Vector3.Lerp(toAnchor, look, DevSettings.swingBoostLookMix);
		if (dir.sqrMagnitude < 0.0001f)
		{
			return toAnchor;
		}
		return dir.normalized;
	}

	private void StartSwingBoost(Transform anchor)
	{
		if (DevSettings.swingBoost <= 0f)
		{
			return;
		}
		if (DevSettings.swingBoostDuration <= 0.01f)
		{
			Rigidbody body = GetComponent<Rigidbody>();
			body.velocity = body.velocity + SwingBoostDir(anchor.position) * DevSettings.swingBoost;
			swingBoostTimer = 0f;
		}
		else
		{
			swingBoostTimer = DevSettings.swingBoostDuration;
			swingAnchor = anchor;
		}
	}

	private void StartReload()
	{
		vmAnim.Stop("Fire2R");
		vmAnim.Play("Fire2R");
		reloadLock = Mathf.Clamp(vmAnim["Fire2R"].length, 0.3f, 1.5f);
	}

	private void BrightenBlast(GameObject fx)
	{
		ParticleSystem[] systems = fx.GetComponentsInChildren<ParticleSystem>(true);
		foreach (ParticleSystem ps in systems)
		{
			ParticleSystem.MainModule main = ps.main;
			ParticleSystem.MinMaxGradient startColor = main.startColor;
			if (startColor.mode == ParticleSystemGradientMode.Color)
			{
				main.startColor = BrightenColor(startColor.color);
			}
			else if (startColor.mode == ParticleSystemGradientMode.TwoColors)
			{
				main.startColor = new ParticleSystem.MinMaxGradient(BrightenColor(startColor.colorMin), BrightenColor(startColor.colorMax));
			}
		}
		Light[] lights = fx.GetComponentsInChildren<Light>(true);
		foreach (Light fxLight in lights)
		{
			fxLight.intensity *= DevSettings.blastLightBoost;
		}
	}

	private static Color BrightenColor(Color c)
	{
		Color result = Color.Lerp(c, new Color(1f, 0.93f, 0.7f), DevSettings.blastBrightness);
		result.a = Mathf.Clamp01(Mathf.Lerp(c.a, Mathf.Max(c.a, 0.9f), DevSettings.blastBrightness));
		return result;
	}

	private void LateUpdate()
	{
		// the second ability's chains are invisible; only the hook ball stays visible
		LineRenderer own = GetComponent<LineRenderer>();
		if (own != null)
		{
			own.enabled = false;
		}
		if (grabber != null)
		{
			LineRenderer grab = grabber.GetComponent<LineRenderer>();
			if (grab != null)
			{
				grab.enabled = false;
			}
		}
	}

	private void FreeMotion(ConfigurableJoint cj, bool free)
	{
		if (free)
		{
			cj.xMotion = ConfigurableJointMotion.Free;
			cj.yMotion = ConfigurableJointMotion.Free;
			cj.zMotion = ConfigurableJointMotion.Free;
		}
		else
		{
			cj.xMotion = ConfigurableJointMotion.Limited;
			cj.yMotion = ConfigurableJointMotion.Limited;
			cj.zMotion = ConfigurableJointMotion.Limited;
		}
	}

	private void addHighlightObj(Transform tr)
	{
		HLObj = UnityEngine.Object.Instantiate(highlightObj, tr.position, tr.rotation);
		if ((bool)GameObject.Find("_effects"))
		{
			HLObj.transform.parent = GameObject.Find("_effects").transform;
		}
		HLObj.transform.localScale = tr.localScale;
		HLObj.GetComponent<LockTransformScript>().lockTransform = tr;
		if ((bool)HLObj.GetComponent<MeshFilter>() && (bool)tr.GetComponent<MeshFilter>())
		{
			HLObj.GetComponent<MeshFilter>().mesh = tr.GetComponent<MeshFilter>().mesh;
		}
	}

	private Transform GetObj(float r)
	{
		Vector3 forward = cam.transform.forward;
		Vector3 vector = cam.transform.position + forward * maxRange;
		RaycastHit hitInfo;
		if (r > 0f && Physics.SphereCast(cam.transform.position, r, forward, out hitInfo, maxRange, grappleMask))
		{
			return hitInfo.transform;
		}
		if (r <= 0f && Physics.Raycast(cam.transform.position, forward, out hitInfo, maxRange))
		{
			return hitInfo.transform;
		}
		return null;
	}

	private bool CanGrapple(Transform tr)
	{
		bool result = false;
		if ((bool)tr && (tr.tag == "Prop" || tr.tag == "Enemy" || tr.tag == "Hook") && (!tr.GetComponent<Health>() || (tr.GetComponent<Health>().stunned && !tr.GetComponent<Health>().willDie)))
		{
			result = true;
		}
		return result;
	}

	public void SuperJump(string arg)
	{
		GetComponent<Rigidbody>().velocity = Vector3.Scale(GetComponent<Rigidbody>().velocity, new Vector3(1f, 0f, 1f));
		GetComponent<Rigidbody>().velocity = GetComponent<Rigidbody>().velocity + Vector3.up * GetComponent<PhysicsFPSWalker>().jumpSpeed * 5f;
		if (arg != string.Empty)
		{
			SoundManager.instance.PlaySingle("superjump", base.transform.position - Vector3.up, true);
		}
	}

	private void HookshotFire(Vector3 dir)
	{
		if (!hookBreak && !hookReturn)
		{
			GetComponent<LineRenderer>().enabled = true;
			float num = 0.2f;
			hookshot.transform.position = cam.transform.position + cam.transform.up * (0f - num) + cam.transform.right * (0f - num);
			hsFirePoint = hookshot.transform.position;
			hookshot.SetActive(true);
			hookshot.GetComponent<Collider>().isTrigger = false;
			hookshot.GetComponent<Rigidbody>().isKinematic = false;
			hookshot.GetComponent<Rigidbody>().velocity = Vector3.zero;
			hookshot.GetComponent<Rigidbody>().AddForce(dir * hsForce);
			SoundManager.instance.PlaySingle("hook1", base.transform.position - Vector3.up, true);
			PhysicsFPSWalker component = GetComponent<PhysicsFPSWalker>();
			if (!component.IsGrounded())
			{
				GetComponent<Rigidbody>().velocity = Vector3.Scale(GetComponent<Rigidbody>().velocity, new Vector3(1f, 0f, 1f));
				GetComponent<Rigidbody>().velocity = GetComponent<Rigidbody>().velocity + Vector3.up * component.jumpSpeed / 1.6f;
			}
		}
	}

	private void HookshotHide()
	{
		GetComponent<LineRenderer>().enabled = false;
		hookshot.SetActive(false);
		hookshot.GetComponent<Rigidbody>().isKinematic = true;
		hookshot.GetComponent<Rigidbody>().velocity = Vector3.zero;
	}

	private void HookshotReturn()
	{
		if (hookBreak)
		{
			return;
		}
		if (!hooked && hookReturn)
		{
			if (!startRtrn)
			{
				hookshot.GetComponent<Collider>().isTrigger = true;
				if ((bool)hookingTo)
				{
					if ((bool)hookingTo.GetComponent<Health>() && hookingTo.tag == "Enemy")
					{
						SoundManager.instance.PlaySingle("throw1", base.transform.position - Vector3.up, true);
						if ((bool)hookingTo.GetComponent<Rigidbody>())
						{
							hookingTo.GetComponent<Rigidbody>().drag = 0.01f;
						}
						if ((bool)hookingTo.GetComponent<GravityCustom>())
						{
							hookingTo.GetComponent<GravityCustom>().gravityScale = 0f;
						}
						hookingTo.GetComponent<Health>().grappled = false;
					}
					else
					{
						SoundManager.instance.PlaySingle("hook3", hookingTo.transform.position, true);
					}
					if ((bool)hookingTo.GetComponent<Rigidbody>())
					{
						hookingTo.GetComponent<Rigidbody>().velocity = Vector3.zero;
						hookingTo.GetComponent<Rigidbody>().AddForce(cam.transform.forward * hsThrow);
						if ((bool)hookingTo.GetComponent<CollisionCheck>())
						{
							hookingTo.GetComponent<CollisionCheck>().DidThrow(true);
						}
					}
					if ((bool)hookingTo)
					{
						Physics.IgnoreCollision(base.gameObject.GetComponent<Collider>(), hookingTo.GetComponent<Collider>(), false);
					}
					hookingTo = null;
				}
				startRtrn = true;
			}
			hookshot.SetActive(true);
			hookshot.GetComponent<Rigidbody>().isKinematic = false;
			hookshot.GetComponent<Rigidbody>().velocity = Vector3.zero;
			float num = 30f;
			float num2 = 0.2f;
			Vector3 vector = cam.transform.position + cam.transform.up * (0f - num2) + cam.transform.right * (0f - num2);
			Vector3 normalized = (vector - hookshot.transform.position).normalized;
			hookshot.transform.Translate(normalized * num * Time.deltaTime, Space.World);
			float num3 = Vector3.Distance(hookshot.transform.position, vector);
			if (!(num3 <= 1f))
			{
				return;
			}
			SoundManager.instance.PlaySingle("hook4", base.transform.position - Vector3.up, true);
			hookReturn = false;
		}
		GetComponent<LineRenderer>().enabled = false;
		hookshot.transform.position = cam.transform.position;
		hookshot.SetActive(false);
		hookshot.GetComponent<Collider>().isTrigger = false;
		hookshot.GetComponent<Rigidbody>().isKinematic = true;
		hookshot.GetComponent<Rigidbody>().velocity = Vector3.zero;
		hookReturn = false;
		startRtrn = false;
	}

	private void HookToTransform(Transform tr)
	{
		if (!hooked && (bool)tr.parent && tr.parent.transform.tag == "Enemy")
		{
			tr = tr.parent;
		}
		if (!hooked && CanGrapple(tr))
		{
			if ((bool)hookshot.GetComponent<CollisionCheck>().hitObj)
			{
				CollisionCheck.DoHitFX(tr.position, Vector3.up, hookshot, hookshot.GetComponent<CollisionCheck>().hitObj, false);
			}
			bool flag = true;
			if (tr.tag == "Prop" || tr.tag == "Enemy")
			{
				flag = false;
			}
			hooked = !hooked;
			hookReturn = false;
			HookshotHide();
			if (!hooked)
			{
				return;
			}
			FreeMotion(cjA, true);
			cjA.connectedAnchor = base.transform.position;
			GetComponent<LineRenderer>().enabled = false;
			grabber.SetActive(false);
			cjB.connectedBody = tr.GetComponent<Rigidbody>();
			if (flag)
			{
				GetComponent<LineRenderer>().enabled = true;
				cjA.connectedAnchor = tr.position;
				FreeMotion(cjA, false);
				StartSwingBoost(tr);
			}
			else
			{
				grabber.SetActive(true);
				if ((bool)tr.GetComponent<Health>() && tr.tag == "Enemy")
				{
					tr.GetComponent<Health>().grappled = true;
				}
			}
			SoundManager.instance.PlaySingle("hook2", tr.position, true);
			hookingTo = tr;
			Physics.IgnoreCollision(base.gameObject.GetComponent<Collider>(), hookingTo.GetComponent<Collider>(), true);
		}
		else if (hooked)
		{
			hooked = false;
		}
	}

	public void GrappleCollide(Collider coll, bool trigger)
	{
		if (!hookBreak)
		{
			if (trigger && !hooked && !hookReturn)
			{
				HookToTransform(coll.transform);
			}
			if (!trigger && !hookReturn)
			{
				if (!hooked && CanGrapple(GrabRoot(coll)))
				{
					HookToTransform(coll.transform);
					return;
				}
				if (!DevSettings.wallHitsCancelHook || Vector3.Distance(hookshot.transform.position, hsFirePoint) < DevSettings.wallGraceDistance)
				{
					return;
				}
				Vector3 normalized = (base.transform.position + Vector3.up * 4f - hookshot.transform.position).normalized;
				StartCoroutine(BreakHook(normalized));
			}
		}
	}

	private IEnumerator BreakHook(Vector3 dir)
	{
		hookBreak = true;
		hookshot.GetComponent<Rigidbody>().velocity = dir * 5f;
		yield return new WaitForSeconds(0.35f);
		hookBreak = false;
		hookReturn = true;
	}
}
