using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class GravityCustom : MonoBehaviour
{
	public float gravityScale = 1f;

	public static float globalGravity = -9.81f;

	private Rigidbody m_rb;

	private void OnEnable()
	{
		m_rb = GetComponent<Rigidbody>();
		m_rb.useGravity = false;
	}

	private void FixedUpdate()
	{
		Vector3 force = globalGravity * gravityScale * Vector3.up;
		m_rb.AddForce(force, ForceMode.Acceleration);
	}
}
