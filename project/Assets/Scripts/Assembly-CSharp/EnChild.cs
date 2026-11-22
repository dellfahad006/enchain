using UnityEngine;

public class EnChild : MonoBehaviour
{
	public Transform[] par;

	public Vector3 localOffset;

	public LineRenderer lr;

	public Transform chainTr;

	public float moveSpeed = 3f;

	public bool parent;

	public GameObject shield;

	private bool shieldUp = true;

	private float checkTimer = 1f;

	private Rigidbody rb;

	private Health h;

	private void Start()
	{
		rb = GetComponent<Rigidbody>();
		h = GetComponent<Health>();
		if (!parent)
		{
			Physics.IgnoreCollision(GetComponent<Collider>(), par[0].GetComponent<Collider>());
			if ((bool)base.transform.GetComponentInChildren<Collider>())
			{
				Physics.IgnoreCollision(base.transform.GetComponentInChildren<Collider>(), par[0].GetComponent<Collider>());
			}
		}
	}

	private void Update()
	{
		if ((bool)par[0] && !h.stunned && !parent)
		{
			Vector3 vector = par[0].TransformPoint(localOffset);
			Vector3 vector2 = vector - base.transform.position;
			rb.AddForce(vector2 * moveSpeed * Time.deltaTime * 10f, ForceMode.Acceleration);
			if ((bool)lr && (bool)chainTr)
			{
				lr.enabled = true;
				Vector3 position = base.transform.InverseTransformPoint(chainTr.transform.position);
				lr.SetPosition(1, position);
			}
		}
		else if (!parent && h.stunned)
		{
			lr.enabled = false;
		}
		checkTimer -= Time.deltaTime;
		if (!parent || !(checkTimer <= 0f))
		{
			return;
		}
		checkTimer = 1f;
		int num = 0;
		Transform[] array = par;
		foreach (Transform transform in array)
		{
			if (transform.GetComponent<Health>().stunned || transform.GetComponent<Health>().souled)
			{
				num++;
			}
		}
		if (num >= par.Length && (bool)shield && shield.activeSelf)
		{
			ToggleShield(false);
		}
		else if (num < par.Length && (bool)shield && !shield.activeSelf)
		{
			ToggleShield(true);
		}
	}

	private void ToggleShield(bool a)
	{
		shield.SetActive(a);
		GetComponent<Health>().invin = a;
		if (a)
		{
			SoundManager.instance.PlaySingle("shieldU", base.transform.position, true);
		}
		else
		{
			SoundManager.instance.PlaySingle("shieldD", base.transform.position, true);
		}
	}
}
