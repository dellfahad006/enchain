using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Lets the title / level-select menus be used with W S or the arrow keys plus Enter or Space.
public class KeyboardMenu : MonoBehaviour
{
	private List<Button> buttons = new List<Button>();

	private int index;

	private bool hasSelection;

	private float ignoreActivateUntil;

	private int lastCount = -1;

	private Button lastSelected;

	private Vector3 lastScale = Vector3.one;

	private void Update()
	{
		GameManager gm = GameManager.instance;
		if (gm == null || !gm.menu.inMenu || gm.menu.menuState == 0)
		{
			ClearHighlight();
			hasSelection = false;
			lastCount = -1;
			return;
		}
		Refresh();
		if (buttons.Count == 0)
		{
			return;
		}
		bool down = Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow);
		bool up = Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow);
		if (down || up)
		{
			if (!hasSelection)
			{
				hasSelection = true;
				index = 0;
			}
			else
			{
				index = (index + (down ? 1 : buttons.Count - 1)) % buttons.Count;
			}
			Highlight();
			if (SoundManager.instance != null)
			{
				SoundManager.instance.PlaySingle("bump1");
			}
		}
		if (Time.unscaledTime < ignoreActivateUntil)
		{
			return;
		}
		if ((Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Space)) && buttons.Count > 0)
		{
			if (!hasSelection)
			{
				hasSelection = true;
				index = 0;
				Highlight();
			}
			Button button = buttons[Mathf.Clamp(index, 0, buttons.Count - 1)];
			if (button != null && button.interactable)
			{
				ClearHighlight();
				button.onClick.Invoke();
				hasSelection = false;
				lastCount = -1;
				ignoreActivateUntil = Time.unscaledTime + 0.25f;
			}
		}
	}

	private void Refresh()
	{
		Button[] all = Object.FindObjectsOfType<Button>();
		List<Button> found = new List<Button>();
		foreach (Button button in all)
		{
			if (button != null && button.gameObject.activeInHierarchy && button.interactable && button.GetComponent<RectTransform>() != null)
			{
				found.Add(button);
			}
		}
		found.Sort(delegate(Button a, Button b)
		{
			float ay = a.transform.position.y;
			float by = b.transform.position.y;
			if (Mathf.Abs(ay - by) > 0.01f)
			{
				return by.CompareTo(ay);
			}
			return a.transform.position.x.CompareTo(b.transform.position.x);
		});
		if (found.Count != lastCount)
		{
			ClearHighlight();
			hasSelection = false;
			index = 0;
			lastCount = found.Count;
			ignoreActivateUntil = Time.unscaledTime + 0.25f;
		}
		buttons = found;
	}

	private void Highlight()
	{
		ClearHighlight();
		if (buttons.Count == 0)
		{
			return;
		}
		index = Mathf.Clamp(index, 0, buttons.Count - 1);
		lastSelected = buttons[index];
		lastScale = lastSelected.transform.localScale;
		lastSelected.transform.localScale = lastScale * 1.12f;
		if (EventSystem.current != null)
		{
			EventSystem.current.SetSelectedGameObject(lastSelected.gameObject);
		}
	}

	private void ClearHighlight()
	{
		if (lastSelected != null)
		{
			lastSelected.transform.localScale = lastScale;
		}
		lastSelected = null;
	}
}
