using UnityEngine;

public class LookAt : MonoBehaviour
{
	public Vector3 axis = Vector3.one;

	public bool atPlayer;

	public Transform target;

	public bool look = true;

	public float rate = 1f;

	private void Start()
	{
		if (atPlayer)
		{
			target = GameObject.FindGameObjectWithTag("Player").transform;
		}
	}

	private void Update()
	{
		if ((bool)target && look)
		{
			Vector3 eulerAngles = Quaternion.LookRotation((target.position - base.transform.position).normalized, Vector3.up).eulerAngles;
			Quaternion b = Quaternion.Euler(new Vector3(eulerAngles.x * axis.x, eulerAngles.y * axis.y, eulerAngles.z * axis.z));
			base.transform.rotation = Quaternion.Slerp(base.transform.rotation, b, Time.deltaTime * rate);
		}
	}
}
