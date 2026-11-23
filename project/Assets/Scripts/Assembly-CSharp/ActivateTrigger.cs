using System;
using UnityEngine;

public class ActivateTrigger : MonoBehaviour
{
	public enum Mode
	{
		Trigger = 0,
		Replace = 1,
		Activate = 2,
		Enable = 3,
		Animate = 4,
		Deactivate = 5
	}

	public Mode action = Mode.Activate;

	public UnityEngine.Object target;

	public bool gameMan;

	public string method = "DoActivateTrigger";

	public string methodVal = string.Empty;

	public string[] sounds = new string[2]
	{
		string.Empty,
		string.Empty
	};

	public string activator = "Player";

	public int dataNeeded;

	public int dataNeededAmt = 1;

	public GameObject source;

	public int triggerCount = 1;

	public bool toggles;

	public bool repeatTrigger;

	[NonSerialized]
	public bool triggered;


	private void Start()
	{
		if (gameMan)
		{
			target = GameManager.instance;
		}
		if (DevSettings.ignorePadRequirements && dataNeeded != 0)
		{
			foreach (Transform child in base.transform)
			{
				if (child.name.StartsWith("Plane"))
				{
					child.gameObject.SetActive(false);
				}
			}
		}
	}

	private void DoActivateTrigger()
	{
		triggerCount--;
		if (triggerCount != 0 && !repeatTrigger)
		{
			return;
		}
		triggered = true;
		SoundManager.instance.PlaySingle(sounds[0], base.transform.position, true);
		UnityEngine.Object obj = ((!(target != null)) ? base.gameObject : target);
		Behaviour behaviour = obj as Behaviour;
		GameObject gameObject = obj as GameObject;
		if (behaviour != null)
		{
			gameObject = behaviour.gameObject;
		}
		switch (action)
		{
		case Mode.Trigger:
		{
			object parameter = base.gameObject;
			if (methodVal != string.Empty)
			{
				parameter = methodVal;
			}
			gameObject.BroadcastMessage(method, parameter);
			break;
		}
		case Mode.Replace:
			if (source != null)
			{
				UnityEngine.Object.Instantiate(source, gameObject.transform.position, gameObject.transform.rotation);
				UnityEngine.Object.DestroyObject(gameObject);
			}
			break;
		case Mode.Activate:
			gameObject.SetActive(true);
			break;
		case Mode.Enable:
			if (behaviour != null)
			{
				behaviour.enabled = true;
			}
			break;
		case Mode.Animate:
			gameObject.GetComponent<Animation>().Play();
			break;
		case Mode.Deactivate:
			gameObject.SetActive(false);
			break;
		}
	}

	private void DeActivateTrigger()
	{
		if (triggerCount != 0 && !repeatTrigger)
		{
			return;
		}
		SoundManager.instance.PlaySingle(sounds[1], base.transform.position, true);
		triggered = false;
		UnityEngine.Object obj = ((!(target != null)) ? base.gameObject : target);
		Behaviour behaviour = obj as Behaviour;
		GameObject gameObject = obj as GameObject;
		if (behaviour != null)
		{
			gameObject = behaviour.gameObject;
		}
		switch (action)
		{
		case Mode.Trigger:
			gameObject.BroadcastMessage(method, string.Empty);
			break;
		case Mode.Replace:
			if (source != null)
			{
				UnityEngine.Object.Instantiate(source, gameObject.transform.position, gameObject.transform.rotation);
				UnityEngine.Object.DestroyObject(gameObject);
			}
			break;
		case Mode.Activate:
			gameObject.SetActive(false);
			break;
		case Mode.Enable:
			if (behaviour != null)
			{
				behaviour.enabled = false;
			}
			break;
		case Mode.Animate:
			gameObject.GetComponent<Animation>().Play();
			break;
		case Mode.Deactivate:
			gameObject.SetActive(true);
			break;
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.tag == activator && !triggered && (DevSettings.ignorePadRequirements || dataNeeded == 0 || dataNeededAmt == 0 || GameManager.instance.SoulDataContainsAmt(dataNeeded) >= dataNeededAmt))
		{
			DoActivateTrigger();
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.tag == activator && toggles && triggered)
		{
			DeActivateTrigger();
		}
		else if (!toggles && repeatTrigger)
		{
			triggered = false;
		}
	}
}
