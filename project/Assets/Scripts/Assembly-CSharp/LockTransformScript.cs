using UnityEngine;

public class LockTransformScript : MonoBehaviour
{
	public Transform lockTransform;

	private void Start()
	{
	}

	private void Update()
	{
		if ((bool)lockTransform)
		{
			lockAll(base.transform, lockTransform);
		}
	}

	private static void lockAll(Transform t, Transform lockTarget)
	{
		t.position = lockTarget.position;
		t.rotation = lockTarget.rotation;
		t.localScale = lockTarget.localScale;
	}
}
