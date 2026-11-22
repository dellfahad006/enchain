using System;
using UnityEngine;

public class DragView : MonoBehaviour
{
	public GameObject objectToDrag;

	public bool canDrag = true;

	public float dragAmount = 1f;

	public float dragSpeed = 2f;

	public float dragLimit = 3f;

	private float dragOnX;

	private float dragOnY;

	[NonSerialized]
	public float dragValX;

	[NonSerialized]
	public float dragValY;

	private Vector3 truePos;

	private Vector3 defaultPos;

	private Vector3 newPos;

	[NonSerialized]
	public float addOnZ;

	public float rayInFront = 2f;

	public LayerMask lm;

	private Camera cam;

	private void Start()
	{
		truePos = objectToDrag.transform.localPosition;
		defaultPos = truePos;
		cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
	}

	private void Update()
	{
		if (rayInFront > 0f)
		{
			float num = CheckInFront(rayInFront);
			defaultPos = new Vector3(truePos.x, truePos.y, truePos.z - num);
		}
		dragOnX = -1f * Input.GetAxis("Mouse X") * Time.deltaTime * dragAmount;
		dragOnY = -1f * Input.GetAxis("Mouse Y") * Time.deltaTime * dragAmount;
		if (!canDrag)
		{
			objectToDrag.transform.localPosition = Vector3.Lerp(objectToDrag.transform.localPosition, defaultPos, dragSpeed * Time.deltaTime);
			return;
		}
		newPos = new Vector3(defaultPos.x + Mathf.Clamp(dragOnX, defaultPos.x - dragLimit, defaultPos.x + dragLimit), defaultPos.y + Mathf.Clamp(dragOnY, defaultPos.y - dragLimit, defaultPos.y + dragLimit), defaultPos.z + addOnZ);
		objectToDrag.transform.localPosition = Vector3.Lerp(objectToDrag.transform.localPosition, newPos, dragSpeed * Time.deltaTime);
		dragValX = objectToDrag.transform.localPosition.x;
		dragValY = objectToDrag.transform.localPosition.y;
	}

	private float CheckInFront(float amt)
	{
		float result = 0f;
		Vector3 vector = cam.transform.TransformPoint(-(Vector3.up / 3f) + Vector3.right / 2f);
		Vector3 forward = cam.transform.forward;
		RaycastHit hitInfo;
		if (Physics.Raycast(vector, forward, out hitInfo, amt, lm))
		{
			Vector3 point = hitInfo.point;
			float num = Vector3.Distance(vector + forward * amt, point);
			result = num;
		}
		return result;
	}
}
