using UnityEngine;

public class PhysicsFPSWalker : MonoBehaviour
{
	public float maxSpeed = 7f;

	public float force = 8f;

	public float jumpSpeed = 5f;

	public float aControl = 0.8f;

	public float friction = 2f;

	public LayerMask floorLM;

	public AudioSource windLoop;

	private int state;

	private bool contact;

	private float jumpLimit;

	private float distToGround;

	private bool jump;

	private float stepTimer;

	public float stepRate = 0.4f;

	private Rigidbody rb;

	private float jumpMoment = 0.2f;

	private float jumpMomentTimer;

	public virtual float horizontal
	{
		get
		{
			return Input.GetAxis("Horizontal") * force;
		}
	}

	public virtual float vertical
	{
		get
		{
			return Input.GetAxis("Vertical") * force;
		}
	}

	private void Start()
	{
		GetComponent<Rigidbody>().freezeRotation = true;
		rb = GetComponent<Rigidbody>();
		distToGround = GetComponent<Collider>().bounds.extents.y;
	}

	private void OnCollisionEnter()
	{
		state++;
		if (state > 0)
		{
			contact = true;
		}
	}

	private void OnCollisionExit()
	{
		state--;
		if (state < 1)
		{
			contact = false;
			state = 0;
		}
	}

	public bool IsGrounded()
	{
		return Physics.Raycast(base.transform.position, -Vector3.up, distToGround + 0.15f, floorLM);
	}

	private RaycastHit IsGrounded(bool giveRay)
	{
		RaycastHit hitInfo;
		Physics.Raycast(base.transform.position, -Vector3.up, out hitInfo, distToGround + 0.1f);
		return hitInfo;
	}

	private void Update()
	{
		if (Input.GetButtonDown("Jump") && (IsGrounded() || jumpMomentTimer < jumpMoment))
		{
			jumpMomentTimer = jumpMoment;
			jump = true;
		}
		if (IsGrounded())
		{
			if (!jump)
			{
				jumpMomentTimer = 0f;
			}
			stepTimer += Time.deltaTime;
			if (stepTimer >= stepRate / rb.velocity.magnitude)
			{
				SoundManager.instance.PlaySingle("step1", base.transform.position - Vector3.up, true);
				stepTimer = 0f;
			}
		}
		else
		{
			jumpMomentTimer += Time.deltaTime;
			stepTimer = stepRate * 3f;
		}
		if (!windLoop)
		{
			return;
		}
		float volume = 0f;
		if (!IsGrounded() && !GameManager.instance.gamePaused)
		{
			float num = Mathf.Abs(rb.velocity.y);
			if (rb.velocity.y > 0f)
			{
				num /= 3f;
			}
			volume = Mathf.Clamp01((num - 15f) / 14f);
		}
		windLoop.volume = volume;
	}

	private void FixedUpdate()
	{
		float num = 1f;
		if (!contact)
		{
			num = aControl;
		}
		Vector2 normalized = new Vector2(horizontal, vertical).normalized;
		Vector3 vector = base.transform.InverseTransformDirection(rb.velocity);
		Vector2 normalized2 = new Vector2(vector.x, vector.z).normalized;
		float f = Mathf.Clamp(normalized.x + normalized2.x, -1f, 1f);
		float f2 = Mathf.Clamp(normalized.y + normalized2.y, -1f, 1f);
		float x = 1f - Mathf.Abs(f);
		float y = 1f - Mathf.Abs(f2);
		Vector2 vector2 = new Vector2(x, y);
		if (rb.velocity.magnitude > -1f)
		{
			float num2 = 1f;
			float num3 = 1f;
			if (rb.velocity.magnitude >= maxSpeed)
			{
				num2 = vector2.x;
				num3 = vector2.y;
			}
			float perfMult = GetComponent<PlayerScript>().perfMult;
			Vector3 vector3 = new Vector3((Vector3.right * (horizontal * num * (num2 / perfMult))).x, 0f, (Vector3.forward * (vertical * num * (num3 / perfMult))).z);
			vector3 = base.transform.rotation * vector3;
			if (!IsGrounded())
			{
				GetComponent<Collider>().material.staticFriction = 0f;
				GetComponent<Collider>().material.dynamicFriction = 0f;
			}
			if (IsGrounded())
			{
				Vector3 vector4 = vector3;
				RaycastHit raycastHit = IsGrounded(true);
				Vector3 lhs = Vector3.Cross(raycastHit.normal, new Vector3(1f, 0f, 1f));
				Vector3 b = new Vector3(1f, 0f, 1f);
				if (IsGrounded())
				{
					b = Vector3.Cross(lhs, raycastHit.normal);
				}
				Vector3 vector5 = Vector3.Scale(-rb.velocity, b);
				float num4 = 1f;
				GetComponent<Collider>().material.staticFriction = 0f;
				GetComponent<Collider>().material.dynamicFriction = 0f;
				if (normalized == Vector2.zero)
				{
					if (IsGrounded())
					{
						if (raycastHit.normal != Vector3.zero)
						{
							GetComponent<Collider>().material.dynamicFriction = 1f;
						}
						GetComponent<Collider>().material.staticFriction = 1.5f;
					}
					num4 = 3.2f;
				}
				rb.AddForce(vector5 * friction * num4);
				Debug.DrawLine(base.transform.position - Vector3.up, base.transform.position - Vector3.up + vector5 * friction * num4, Color.red);
				if (IsGrounded())
				{
					Debug.DrawLine(base.transform.position, raycastHit.point, Color.blue);
				}
				if (raycastHit.normal != Vector3.up)
				{
					vector4 = Vector3.Cross(raycastHit.normal, vector3);
					vector3 = Vector3.Cross(vector4, raycastHit.normal);
				}
			}
			rb.AddForce(vector3);
		}
		if (jumpLimit < 10f)
		{
			jumpLimit++;
		}
		if (jump && jumpLimit >= 10f)
		{
			GetComponent<Rigidbody>().velocity = GetComponent<Rigidbody>().velocity + Vector3.up * jumpSpeed / GetComponent<PlayerScript>().perfMult;
			SoundManager.instance.PlaySingle("step1", base.transform.position - Vector3.up, true);
			jumpLimit = 0f;
		}
		jump = false;
	}
}
