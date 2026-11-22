using System;
using UnityEngine;

public class SoulData : MonoBehaviour
{
	public new string name = "Bob";

	public int enID = 1;

	public int color = 1;

	[NonSerialized]
	public Vector3 spawnPoint;

	[NonSerialized]
	public Quaternion spawnRot;

	[NonSerialized]
	public Vector3 spawnSca;

	private void Start()
	{
		spawnPoint = base.transform.position;
		spawnRot = base.transform.rotation;
		spawnSca = base.transform.localScale;
	}

	private void Update()
	{
	}
}
