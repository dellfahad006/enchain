using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
	[Serializable]
	public class menuData
	{
		public bool inMenu = true;

		public int menuState;

		public GameObject[] title;

		public GameObject titleOverlay;

		public GameObject[] menuWindows;
	}

	public static GameManager instance;

	public RectTransform[] cardParents;

	public RectTransform cardPanel;

	public GameObject cardPrefab;

	public GameObject soulPrefab;

	private Vector3 panelPos;

	private float panelSpeed = 6f;

	private GameObject player;

	public RectTransform[] hpBars;

	public RectTransform bossHp;

	private float bossStartW;

	private Health boss;

	private Vector2 hpBarScale;

	private Vector2 cBarScale;

	public GameObject hurtOverlay;

	public GameObject dieText;

	[NonSerialized]
	public int playerMaxHP;

	[NonSerialized]
	public int playerHP;

	[NonSerialized]
	public bool gamePaused;

	private float[] relfloats = new float[5];

	private float relTime = 1f;

	private GameObject soulRet;

	private GameObject soulledRet;

	private List<GameObject[]> soulReturnWait = new List<GameObject[]>();

	private List<GameObject> cards = new List<GameObject>();

	private List<GameObject> souls = new List<GameObject>();

	private List<GameObject> soulledObjs = new List<GameObject>();

	public List<int> soulDatas = new List<int>();

	private GameObject checkpointObj;

	private Vector3 checkpntPos = Vector3.zero;

	private float cpYrot;

	public Material[] checkpntMats;

	public GameObject PARObj;

	public GameObject quitButton;

	public GameObject lvlDoneMenu;

	public GameObject lvlEndMenu;

	public Text timerText;

	public Text feedText;

	public Text sensText;

	public Text helpText;

	public Text tipBox;

	public string[] tipBoxResponses;

	private float feedTime;

	private bool photoBool;

	public Text lvlText;

	private float lvlColTime;

	private string[] lvlNames = new string[5] { "ENTRANCE", "PYRAMID", "BULWARK", "PILLARS", "END" };

	private int[] lvlData = new int[3] { 1, 1, 1 };

	private int[,] secretsData = new int[3, 3];

	public Transform lvlParUI;

	public Material secMat;

	private GameObject[] secretGOs = new GameObject[3];

	public menuData menu;

	private float sensTextTimer;

	private float sensVal = 6f;

	private Vector3 sensPos;

	private float lvlTimeStart;

	private float lvlTimer;

	private float lvlCheckpntTime;

	private float lastCheckpntTime;

	private bool playerDead;

	private bool resumeCheck;

	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
		}
		else if (instance != this)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
	}

	private void Start()
	{
		player = GameObject.FindGameObjectWithTag("Player");
		sensPos = sensText.rectTransform.anchoredPosition;
		panelPos = cardParents[0].anchoredPosition;
		hpBarScale = hpBars[0].sizeDelta;
		cBarScale = hpBars[2].sizeDelta;
		PARObj.SetActive(false);
		if (PlayerPrefs.HasKey("Secrets"))
		{
			secretsData = PlayerPrefsX.Int1Dto2D(PlayerPrefsX.GetIntArray("Secrets"), secretsData.GetUpperBound(0) + 1, secretsData.GetUpperBound(1) + 1);
		}
		if (PlayerPrefs.HasKey("Levels"))
		{
			lvlData = PlayerPrefsX.GetIntArray("Levels");
		}
		secretUpdate();
		lvlUpdate();
		if ((bool)bossHp)
		{
			bossStartW = bossHp.sizeDelta.x;
		}
		if ((bool)quitButton && (Application.platform == RuntimePlatform.WebGLPlayer || Application.platform == RuntimePlatform.WindowsEditor))
		{
			quitButton.SetActive(false);
		}
	}

	private void OnLevelWasLoaded(int level)
	{
		player = GameObject.FindGameObjectWithTag("Player");
		if (player == null)
		{
			PlayerScript ps = UnityEngine.Object.FindObjectOfType<PlayerScript>();
			if (ps != null) player = ps.gameObject;
		}
		Canvas canvas = UnityEngine.Object.FindObjectOfType<Canvas>();
		GameObject mainCamObj = GameObject.FindGameObjectWithTag("MainCamera");
		Camera cam = mainCamObj != null ? mainCamObj.GetComponent<Camera>() : Camera.main;
		if (cam == null) cam = UnityEngine.Object.FindObjectOfType<Camera>();
		if (canvas != null && cam != null)
		{
			canvas.worldCamera = cam;
		}
		if ((bool)player)
		{
			if (SoundManager.instance != null)
			{
				SoundManager.instance.PlaySingle("startLvl", player.transform.position, true);
			}
			lastCheckpntTime = lvlCheckpntTime;
			player.transform.position = checkpntPos;
			if (player.GetComponent<Rigidbody>() != null)
			{
				player.GetComponent<Rigidbody>().velocity = Vector3.zero;
			}
			if (checkpntPos != Vector3.zero)
			{
				player.transform.eulerAngles = new Vector3(0f, cpYrot, 0f);
			}
			if (player.GetComponent<PlayerScript>() != null)
			{
				player.GetComponent<PlayerScript>().sens = sensVal;
			}
		}
		if ((bool)GameObject.Find("en_skullBoss"))
		{
			SetBoss(GameObject.Find("en_skullBoss"));
		}
		else if (bossHp != null)
		{
			boss = null;
			bossHp.transform.parent.gameObject.SetActive(false);
		}
		secretSetLvl();
	}


	private void Update()
	{
		if (menu.inMenu)
		{
			if (Input.GetKeyDown(KeyCode.Space) && menu.menuState == 0)
			{
				MenuState(1);
			}
			return;
		}
		lvlTimer = Time.time - lvlTimeStart - lastCheckpntTime;
		timerText.text = string.Format("{0:0}:{1:00}", Mathf.Floor(lvlTimer / 60f), lvlTimer % 60f);
		gamePaused = GetComponent<CursorHideShow>().paused;
		if (gamePaused)
		{
		}
		lvlColTime -= Time.deltaTime;
		Color white = Color.white;
		white.a = Mathf.Clamp01(-0.05f * Mathf.Pow(lvlColTime - 4f, 2f) + 1.2f);
		lvlText.color = white;
		feedTime -= Time.deltaTime;
		if (feedTime < 0f)
		{
			feedText.text = string.Empty;
		}
		if ((bool)bossHp && (bool)boss)
		{
			bossHp.sizeDelta = new Vector2((float)boss.currHP * 1f / (float)boss.hitPoints * 1f * bossStartW, bossHp.sizeDelta.y);
		}
		if (Input.GetKeyDown(KeyCode.M))
		{
			SoundManager.instance.PlaySingle("bump1");
			GetComponent<AudioSource>().mute = !GetComponent<AudioSource>().mute;
		}
		if (Input.GetKeyDown(KeyCode.O))
		{
		}
		if (PARObj.activeSelf && (bool)player && !gamePaused)
		{
			sensTextTimer -= Time.deltaTime;
			sensText.text = "Sensitivity: " + player.GetComponent<PlayerScript>().sens + "\nUSE [-] [+]";
			if (Input.GetKeyDown(KeyCode.Minus))
			{
				SoundManager.instance.PlaySingle("bump1");
				sensTextTimer = 2f;
				sensVal = Mathf.Clamp(sensVal - 1f, 1f, 15f);
				player.GetComponent<PlayerScript>().sens = sensVal;
			}
			if (Input.GetKeyDown(KeyCode.Equals))
			{
				SoundManager.instance.PlaySingle("bump1");
				sensTextTimer = 2f;
				sensVal = Mathf.Clamp(sensVal + 1f, 1f, 15f);
				player.GetComponent<PlayerScript>().sens = sensVal;
			}
			if (sensTextTimer <= 0f)
			{
				Vector3 b = sensPos + Vector3.up * 30f;
				sensText.rectTransform.anchoredPosition = Vector3.Lerp(sensText.rectTransform.anchoredPosition, b, Time.deltaTime * panelSpeed / 2f);
			}
			else
			{
				Vector3 b2 = sensPos;
				sensText.rectTransform.anchoredPosition = Vector3.Lerp(sensText.rectTransform.anchoredPosition, b2, Time.deltaTime * panelSpeed / 2f);
			}
		}
		if ((bool)soulRet)
		{
			ActiveReturn();
		}
		if (soulReturnWait.Count > 0 && soulRet == null)
		{
			soulRet = soulReturnWait[0][0];
			soulledRet = soulReturnWait[0][1];
			soulReturnWait.RemoveAt(0);
		}
		if (Input.GetButton("Burden"))
		{
			sensTextTimer = 0.1f;
			if ((bool)cardParents[0])
			{
				RectTransform[] array = cardParents;
				foreach (RectTransform rectTransform in array)
				{
					Vector3 b3 = panelPos;
					rectTransform.anchoredPosition = Vector3.Lerp(rectTransform.anchoredPosition, b3, Time.deltaTime * panelSpeed);
				}
			}
		}
		else if ((bool)cardParents[0])
		{
			RectTransform[] array2 = cardParents;
			foreach (RectTransform rectTransform2 in array2)
			{
				Vector3 b4 = panelPos - Vector3.up * 50f;
				rectTransform2.anchoredPosition = Vector3.Lerp(rectTransform2.anchoredPosition, b4, Time.deltaTime * panelSpeed);
			}
		}
		if (playerHP >= 0)
		{
			hpBars[0].anchoredPosition = new Vector2(-1.5f, hpBars[0].anchoredPosition.y);
			hpBars[0].sizeDelta = new Vector2(hpBarScale.x * ((float)playerHP * 1f / (float)playerMaxHP * 1f), hpBarScale.y);
			hpBars[1].GetComponent<Text>().text = playerHP + string.Empty;
		}
		if ((bool)player && player.GetComponent<PlayerScript>().gunCharge < 1f)
		{
			hpBars[2].transform.parent.gameObject.SetActive(true);
			float gunCharge = player.GetComponent<PlayerScript>().gunCharge;
			hpBars[2].sizeDelta = new Vector2(cBarScale.x * Mathf.Clamp01(gunCharge * 1f / 0.9f), cBarScale.y);
		}
		else
		{
			hpBars[2].transform.parent.gameObject.SetActive(false);
		}
		if (Input.GetKey(KeyCode.Alpha1) && cards.Count >= 1)
		{
			ReleaseSoul(1, true);
		}
		else if (cards.Count > 0)
		{
			ReleaseSoul(1, false);
		}
		if (Input.GetKey(KeyCode.Alpha2) && cards.Count >= 2)
		{
			ReleaseSoul(2, true);
		}
		else if (cards.Count > 1)
		{
			ReleaseSoul(2, false);
		}
		if (Input.GetKey(KeyCode.Alpha3) && cards.Count >= 3)
		{
			ReleaseSoul(3, true);
		}
		else if (cards.Count > 2)
		{
			ReleaseSoul(3, false);
		}
		if (Input.GetKey(KeyCode.Alpha4) && cards.Count >= 4)
		{
			ReleaseSoul(4, true);
		}
		else if (cards.Count > 3)
		{
			ReleaseSoul(4, false);
		}
		if (Input.GetKey(KeyCode.Alpha5) && cards.Count >= 5)
		{
			ReleaseSoul(5, true);
		}
		else if (cards.Count > 4)
		{
			ReleaseSoul(5, false);
		}
		foreach (GameObject soul in souls)
		{
			if ((bool)soul && (bool)player)
			{
				Vector3 position = soul.transform.InverseTransformPoint(player.transform.position);
				soul.GetComponent<LineRenderer>().SetPosition(0, position);
				float num = Vector3.Distance(soul.transform.position, player.transform.position);
				Vector3 vector = player.transform.position - soul.transform.position;
				if (num > 5f)
				{
					soul.GetComponent<Rigidbody>().AddForce(vector.normalized * 2f * (num / 3f) * Time.deltaTime * 100f);
				}
			}
		}
		if ((gamePaused || playerDead) && Input.GetKeyDown(KeyCode.R))
		{
			if (!gamePaused)
			{
				GetComponent<CursorHideShow>().DOpause(true);
			}
			RestartScene();
		}
		if (resumeCheck && gamePaused && Input.GetMouseButtonDown(0))
		{
			resumeCheck = false;
			GetComponent<CursorHideShow>().Unpause(true);
			SoundManager.instance.PlaySingle("bump1", player.transform.position, true);
		}
	}

	public int SoulDataContainsAmt(int data)
	{
		int num = 0;
		foreach (int soulData in soulDatas)
		{
			if (soulData == data)
			{
				num++;
			}
		}
		return num;
	}

	private void MenuState(int state)
	{
		if (state == 0)
		{
		}
		if (state == 1)
		{
			PARObj.SetActive(false);
			GetComponent<CursorHideShow>().Unpause(false);
			if ((bool)menu.title[0])
			{
				menu.title[0].SetActive(true);
			}
			menu.title[1].SetActive(false);
			SoundManager.instance.PlaySingle("shootBig1", Vector3.zero, true);
			GetComponent<CursorHideShow>().enabled = false;
			menu.titleOverlay.SetActive(true);
			secretUpdate();
			lvlUpdate();
			GameObject[] menuWindows = menu.menuWindows;
			foreach (GameObject gameObject in menuWindows)
			{
				gameObject.SetActive(false);
			}
			menu.inMenu = true;
			GetComponent<AudioSource>().Stop();
		}
		if (state == 2)
		{
			GetComponent<CursorHideShow>().enabled = true;
			GetComponent<CursorHideShow>().Unpause(true);
			PARObj.SetActive(true);
			if ((bool)menu.title[0])
			{
				menu.title[0].SetActive(false);
			}
			menu.titleOverlay.SetActive(false);
			menu.inMenu = false;
			GetComponent<AudioSource>().Play();
		}
		menu.menuState = state;
	}

	private void TogglePhoto(bool a)
	{
		if (a)
		{
			Time.timeScale = 0.2f;
			PARObj.SetActive(false);
			if ((bool)player)
			{
				player.transform.Find("Main Camera/viewmodel/arms").gameObject.SetActive(false);
				player.transform.Find("Main Camera/viewmodel/gun1").gameObject.SetActive(false);
			}
		}
		if (!a)
		{
			Time.timeScale = 1f;
			PARObj.SetActive(true);
			if ((bool)player)
			{
				player.transform.Find("Main Camera/viewmodel/arms").gameObject.SetActive(true);
				player.transform.Find("Main Camera/viewmodel/gun1").gameObject.SetActive(true);
			}
		}
	}

	public void StartLevel(int num)
	{
		NextScene(num.ToString());
		MenuState(2);
	}

	public void EndLevel(string nextLvl)
	{
		GetComponent<CursorHideShow>().Unpause(false);
		lvlDoneMenu.SetActive(true);
		if (int.Parse(nextLvl) < 0)
		{
			lvlDoneMenu.SetActive(false);
			lvlEndMenu.SetActive(true);
		}
		player.transform.Find("Main Camera").GetComponent<MouseLook>().enabled = false;
		player.GetComponent<MouseLook>().enabled = false;
		player.GetComponent<Rigidbody>().isKinematic = true;
		player.GetComponent<PhysicsFPSWalker>().enabled = false;
		player.GetComponentInChildren<DragView>().gameObject.SetActive(false);
		player.GetComponent<PlayerScript>().enabled = false;
		lvlDone(SceneManager.GetActiveScene().buildIndex);
		PlayerPrefs.Save();
		Analytics.CustomEvent("LevelCompleted", new Dictionary<string, object> { 
		{
			"level",
			SceneManager.GetActiveScene().buildIndex - 1
		} });
	}

	public void NextLVL()
	{
		GetComponent<CursorHideShow>().Unpause(true);
		NextScene(SceneManager.GetActiveScene().buildIndex + 1 + string.Empty);
	}

	public void openUrl(string url)
	{
		Application.OpenURL(url);
	}

	public void NextScene(string sceneNum)
	{
		relfloats = new float[5];
		relTime = 1f;
		soulRet = null;
		soulledRet = null;
		soulReturnWait = new List<GameObject[]>();
		soulDatas = new List<int>();
		lvlDoneMenu.SetActive(false);
		lvlEndMenu.SetActive(false);
		checkpointObj = null;
		checkpntPos = Vector3.zero;
		lastCheckpntTime = 0f;
		lvlCheckpntTime = 0f;
		playerHurt(false, true);
		foreach (GameObject card in cards)
		{
			UnityEngine.Object.Destroy(card);
		}
		cards = new List<GameObject>();
		souls = new List<GameObject>();
		soulledObjs = new List<GameObject>();
		playerDead = false;
		SceneManager.LoadScene(int.Parse(sceneNum));
		PlayerPrefs.Save();
		SetLvlNameText(int.Parse(sceneNum));
	}

	public void BackToMenu()
	{
		NextScene("0");
		MenuState(1);
	}

	public void ResumeGame(bool can)
	{
		resumeCheck = can;
	}

	public void ExitGame()
	{
		Application.Quit();
	}

	public void GiveHelp(string helpTxt)
	{
		if (helpTxt != string.Empty)
		{
			helpText.gameObject.SetActive(true);
			helpText.text = helpTxt;
		}
		else
		{
			helpText.gameObject.SetActive(false);
		}
	}

	public void GiveEnd(string helpTxt)
	{
		if (helpTxt != string.Empty)
		{
			helpText.gameObject.SetActive(true);
			helpText.text = helpTxt;
			helpText.color = Color.red;
		}
		else
		{
			helpText.gameObject.SetActive(false);
		}
	}

	public void GiveTip()
	{
		tipBox.text = "TIP: " + tipBoxResponses[UnityEngine.Random.Range(0, tipBoxResponses.Length)];
	}

	public void RestartScene()
	{
		relfloats = new float[5];
		relTime = 1f;
		soulRet = null;
		soulledRet = null;
		soulReturnWait = new List<GameObject[]>();
		soulDatas = new List<int>();
		checkpointObj = null;
		playerHurt(false, true);
		foreach (GameObject card in cards)
		{
			UnityEngine.Object.Destroy(card);
		}
		cards = new List<GameObject>();
		souls = new List<GameObject>();
		soulledObjs = new List<GameObject>();
		playerDead = false;
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
	}

	public void SoulTake(GameObject obj)
	{
		Vector3 velocity = obj.GetComponent<Rigidbody>().velocity;
		obj.SetActive(false);
		GameObject gameObject = UnityEngine.Object.Instantiate(soulPrefab, obj.transform.position, obj.transform.rotation);
		gameObject.GetComponent<LockTransformScript>().lockTransform = obj.transform;
		gameObject.GetComponent<LineRenderer>().enabled = true;
		obj.transform.parent = gameObject.transform;
		gameObject.transform.GetComponent<MeshFilter>().mesh = obj.GetComponent<MeshFilter>().mesh;
		gameObject.GetComponent<Rigidbody>().velocity = velocity;
		SoulData component = obj.GetComponent<SoulData>();
		string text = component.name;
		Color color = Color.red;
		if (component.color == 2)
		{
			color = Color.blue;
		}
		if (component.color == 3)
		{
			color = Color.green;
		}
		Sprite sprite = (Sprite)Resources.Load("icon_" + component.enID + string.Empty + component.color, typeof(Sprite));
		GameObject gameObject2 = UnityEngine.Object.Instantiate(cardPrefab, cardPanel.transform);
		Text component2 = gameObject2.transform.Find("head/num").GetComponent<Text>();
		component2.text = cards.Count + 1 + string.Empty;
		Text component3 = gameObject2.transform.Find("eneName").GetComponent<Text>();
		component3.text = text;
		Image component4 = gameObject2.transform.Find("head/cardCol").GetComponent<Image>();
		component4.color = color;
		Image component5 = gameObject2.transform.Find("eneImage").GetComponent<Image>();
		component5.sprite = sprite;
		cards.Add(gameObject2);
		soulledObjs.Add(obj);
		souls.Add(gameObject);
		int item = int.Parse(obj.GetComponent<SoulData>().enID + string.Empty + obj.GetComponent<SoulData>().color);
		soulDatas.Add(item);
	}

	private void ReleaseSoul(int soulNum, bool doRel)
	{
		int num = soulNum - 1;
		if (doRel)
		{
			relfloats[num] += Time.deltaTime;
			float num2 = Mathf.Abs(Mathf.Clamp(relfloats[num] - relTime, 0f - relTime, 0f)) / relTime;
			if ((bool)cards[num])
			{
				Image component = cards[num].transform.Find("head").GetComponent<Image>();
				Color color = component.color * num2;
				color.a = 1f;
				component.color = color;
				Text component2 = cards[num].transform.Find("head/num").GetComponent<Text>();
				component2.text = num + 1 + string.Empty;
			}
			if (relfloats[num] >= relTime)
			{
				ReturnSoul(num);
			}
		}
		else if ((bool)cards[num])
		{
			relfloats[num] = 0f;
			Image component3 = cards[num].transform.Find("head").GetComponent<Image>();
			Color color2 = new Color(1f, 1f, 1f, 1f);
			component3.color = color2;
			Text component4 = cards[num].transform.Find("head/num").GetComponent<Text>();
			component4.text = num + 1 + string.Empty;
		}
	}

	private void ReturnSoul(int i)
	{
		GameObject gameObject = null;
		GameObject gameObject2 = null;
		relfloats[i] = 0f;
		if ((bool)cards[i])
		{
			GameObject gameObject3 = cards[i];
			cards.Remove(gameObject3);
			UnityEngine.Object.Destroy(gameObject3);
		}
		if ((bool)soulledObjs[i])
		{
			gameObject2 = soulledObjs[i];
			soulledObjs.Remove(gameObject2);
		}
		if ((bool)souls[i])
		{
			SoundManager.instance.PlaySingle("soulReturn", souls[i].transform.position, true);
			gameObject = souls[i];
			souls.Remove(gameObject);
			soulDatas.RemoveAt(i);
			gameObject.GetComponent<LineRenderer>().enabled = false;
		}
		if (soulRet == null)
		{
			soulRet = gameObject;
			soulledRet = gameObject2;
		}
		else
		{
			soulReturnWait.Add(new GameObject[2] { gameObject, gameObject2 });
		}
	}

	public void DropSouls()
	{
		for (int i = 0; i < cards.Count; i++)
		{
			GameObject gameObject = null;
			GameObject gameObject2 = null;
			relfloats[i] = 0f;
			if ((bool)cards[i])
			{
				GameObject gameObject3 = cards[i];
				cards.Remove(gameObject3);
				UnityEngine.Object.Destroy(gameObject3);
			}
			if ((bool)soulledObjs[i])
			{
				gameObject2 = soulledObjs[i];
				soulledObjs.Remove(gameObject2);
			}
			if ((bool)souls[i])
			{
				SoundManager.instance.PlaySingle("soulReturn", souls[i].transform.position, true);
				gameObject = souls[i];
				souls.Remove(gameObject);
				soulDatas.RemoveAt(i);
				gameObject.GetComponent<LineRenderer>().enabled = false;
			}
		}
	}

	private void ActiveReturn()
	{
		soulRet.GetComponent<Collider>().isTrigger = true;
		Vector3 spawnPoint = soulledRet.GetComponent<SoulData>().spawnPoint;
		Vector3 vector = soulRet.transform.InverseTransformPoint(spawnPoint);
		float num = Vector3.Distance(soulRet.transform.position, spawnPoint);
		Vector3 vector2 = spawnPoint - soulRet.transform.position;
		if (num > 1f)
		{
			soulRet.GetComponent<Rigidbody>().AddForce(vector2.normalized * 2f * (num / 3f));
		}
		if (num < 1f)
		{
			RestoreSoul(soulRet, soulledRet);
			soulRet = null;
			soulledRet = null;
		}
	}

	private void RestoreSoul(GameObject a, GameObject obj)
	{
		SoulData component = obj.GetComponent<SoulData>();
		obj.transform.parent = null;
		obj.GetComponent<Rigidbody>().velocity = Vector3.zero;
		obj.transform.position = component.spawnPoint;
		obj.transform.localScale = component.spawnSca;
		obj.transform.rotation = component.spawnRot;
		obj.GetComponent<Health>().ResetHealth(1f);
		obj.SetActive(true);
		UnityEngine.Object.Destroy(a);
	}

	public void SetCheckpoint(GameObject go)
	{
		if ((bool)checkpointObj)
		{
			checkpointObj.GetComponent<ActivateTrigger>().triggered = false;
			CheckpointCol(false, checkpointObj);
		}
		checkpointObj = go;
		checkpntPos = checkpointObj.transform.position;
		cpYrot = checkpointObj.transform.eulerAngles.y;
		lvlCheckpntTime = lvlTimer;
		player.GetComponent<Health>().HealAmt(100);
		SoundManager.instance.PlaySingle("checkpoint1", checkpointObj.transform.position, true);
		CheckpointCol(true, checkpointObj);
	}

	private void CheckpointCol(bool a, GameObject obj)
	{
		Color color = Color.white;
		int num = 0;
		if (a)
		{
			color = Color.red;
			num = 1;
		}
		obj.GetComponentInChildren<MeshRenderer>().sharedMaterial = checkpntMats[num];
		ParticleSystem.MainModule main = obj.GetComponentInChildren<ParticleSystem>().main;
		main.startColor = color;
	}

	private void SetLvlNameText(int index)
	{
		int num = index - 1;
		if (num >= 0)
		{
			lvlColTime = 9f;
			lvlText.text = "LEVEL " + num;
		}
	}

	public void secretGet(int index)
	{
		int num = SceneManager.GetActiveScene().buildIndex - 1;
		if (num >= 0 && secretsData[num, index] != 1)
		{
			secretsData[num, index] = 1;
			SoundManager.instance.PlaySingle("secretGet1");
			secretHide(index);
			int num2 = 0;
			for (int i = 0; i < 3; i++)
			{
				num2 += secretsData[num, i];
			}
			feedMsg("SECRET FOUND (" + num2 + "/3)");
			Analytics.CustomEvent("Secrets", new Dictionary<string, object>
			{
				{ "level", num },
				{ "secretIndex", index }
			});
			saveSecrets();
		}
	}

	private void lvlDone(int index)
	{
		lvlData[index - 1] = 1;
		saveSecrets();
		saveLevels();
	}

	private void secretUpdate()
	{
		int[] array = new int[secretsData.GetUpperBound(0) + 1];
		for (int i = 0; i < array.Length; i++)
		{
			Transform transform = lvlParUI.Find("lvl" + i);
			for (int j = 0; j < 3; j++)
			{
				if ((bool)transform && (bool)transform.Find("secrets/hLayout/sec" + j))
				{
					Transform transform2 = transform.Find("secrets/hLayout/sec" + j);
					if (secretsData[i, j] == 1)
					{
						transform2.GetChild(0).GetComponent<RawImage>().enabled = true;
					}
					else
					{
						transform2.GetChild(0).GetComponent<RawImage>().enabled = false;
					}
				}
			}
		}
	}

	private void lvlUpdate()
	{
		for (int i = 0; i < lvlData.Length; i++)
		{
			Transform transform = lvlParUI.Find("lvl" + (i + 1));
			if ((bool)transform && lvlData[i] == 1)
			{
				transform.GetComponentInChildren<Button>().interactable = true;
			}
		}
	}

	private void secretSetLvl()
	{
		int num = SceneManager.GetActiveScene().buildIndex - 1;
		if (num < 0)
		{
			return;
		}
		SecretItem[] array = UnityEngine.Object.FindObjectsOfType<SecretItem>();
		SecretItem[] array2 = array;
		foreach (SecretItem secretItem in array2)
		{
			secretGOs[secretItem.ID] = secretItem.gameObject;
			if (secretItem.ID >= 0 && secretsData[num, secretItem.ID] == 1)
			{
				secretHide(secretItem.ID);
			}
		}
	}

	private void secretHide(int index)
	{
		int num = SceneManager.GetActiveScene().buildIndex - 1;
		if (num >= 0 && (bool)secretGOs[index] && (bool)secMat)
		{
			secretGOs[index].GetComponent<Renderer>().sharedMaterial = secMat;
			secretGOs[index].GetComponentInChildren<ParticleSystem>().Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
		}
	}

	private void saveSecrets()
	{
		PlayerPrefsX.SetIntArray("Secrets", PlayerPrefsX.Int2Dto1D(secretsData));
	}

	private void saveLevels()
	{
		PlayerPrefsX.SetIntArray("Levels", lvlData);
	}

	public void ClearSave()
	{
		secretsData = new int[3, 3];
		lvlData = new int[3] { 1, 0, 0 };
		secretUpdate();
		lvlUpdate();
		PlayerPrefs.DeleteAll();
		PlayerPrefs.Save();
	}

	private void SetBoss(GameObject obj)
	{
		boss = obj.GetComponent<Health>();
		bossHp.transform.parent.gameObject.SetActive(true);
	}

	private void feedMsg(string msg)
	{
		feedTime = 3f;
		feedText.text = msg;
	}

	public void playerHurt(bool a, bool die)
	{
		if ((bool)hurtOverlay)
		{
			hurtOverlay.SetActive(a);
		}
		if (die)
		{
			dieText.SetActive(a);
			playerDead = true;
		}
	}
}
