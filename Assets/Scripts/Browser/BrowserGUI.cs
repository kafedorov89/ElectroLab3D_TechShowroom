using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

//Advanced GUI for browser
//Using new UI System

public class BrowserGUI : MonoBehaviour {


	//Vizualizations
	VisClass[] Visualizations;
	bool VisState = false; //false = stopped, true = playing

	public Text textSystemName;
	public Text textSubsystemName;
	public Text textAbout;
	public GameObject textAboutPanel;
	public GameObject dropDownMenu;
	public GameObject dropDownButton;
	public Button menu;
	public Button compile;
	public Button home;
	public Button help;
	public Button play;
	public Button stop;

	private GameObject target;
	private SubsystemList list;
	private SystemBrowser browser;

	// Use this for initialization
	void Start () 
	{
		stop.transform.gameObject.SetActive (false);

		SetInitialGUIState ();

		//get target browsing object
		target = GameObject.FindGameObjectWithTag ("Browser");
		if (target == null)
			return;

		//get components
		list = target.GetComponent<SubsystemList> ();
		browser = target.GetComponent<SystemBrowser> ();
		if (list == null || browser == null)
			return;

		//get visualizations
		Visualizations = target.GetComponentsInChildren<VisClass>();

		textSystemName.text = list.systemName;
		if (list.list.Count > 0)
			menu.interactable = true;

		AddDropDownButtonsAssociatedWithSubsystems ();
	}

	//Instantiate all buttons associated with subsystems
	void AddDropDownButtonsAssociatedWithSubsystems()
	{
		for (int i = 0; i < list.list.Count; ++i)
		{
			GameObject copy = Instantiate(dropDownButton);
			Text t = copy.GetComponentInChildren<Text>();
			if (t != null)
				t.text = list.list[i].name;
			
			copy.transform.SetParent(dropDownMenu.transform);
			
			RectTransform rect = copy.GetComponent<RectTransform>();
			float w = rect.sizeDelta.x;
			float h = rect.sizeDelta.y;
			rect.anchoredPosition = new Vector2(0 + w/2, 0 - h/2 - i * h);
			
			int index = i; //save index only this way; DONT MOVE FROM CYCLE! 
			
			Button b = copy.GetComponent<Button>();
			b.onClick.AddListener(() => ChooseSubsystem(index));
		}
	}

	void SetInitialGUIState()
	{
		SetInitialButtonStates ();
		SetInitialTextContent ();
		dropDownMenu.SetActive (false);
	}
	void SetInitialButtonStates()
	{
		menu.interactable = false;
		compile.interactable = false;
		home.interactable = true;
		help.interactable = false;
	}
	void SetInitialTextContent()
	{
		textSystemName.text = "";
		textSubsystemName.text = "";
		textAbout.text = "";
		textAboutPanel.SetActive (false);
	}

	public void LoadLevel(int level)
	{
		Application.LoadLevel (level);
	}

	public void HomeButtonClicked()
	{
		Stop (true);
		Application.LoadLevel (0);
	}

	//Go to selected subsystem
	public void ChooseSubsystem(int index)
	{
		//ToogleDropDown ();
		dropDownMenu.SetActive (false);
		if (browser.IsReady == false)
			return;

		//browser.GoToSubsystemWithCheck(index);

		if (index == -1 || index == browser.CurrentSubsystemIndex) return;
		if (browser.Subs.list[index].gameObject == null) return;
		//если сейчас проигрывается анимация, её надо остановить
		if (VisState) 
		{
			Stop (true);
		}

		//переходим к выбранной, или сначала собираемся, а потом переходим
		if (browser.CurrentSubsystemIndex == -1)
			browser.GoToSubsystem(index);
		else
		{
			browser.StoredIndex = index;
			Compile ();
		}
	}

	public void ReceiveEventSubsystem()
	{
		int index = browser.CurrentSubsystemIndex;
		textSubsystemName.text = list.list [index].name;
		textAbout.text = list.list [index].textAbout;

		compile.interactable = true;
		help.interactable = true;
	}

	//Go to whole system browsing
	public void Compile()
	{
		if (browser.IsReady == false)
			return;

		//new fuctional +++
		foreach (Subsystem sub in list.list)
			sub.gameObject.SetActive (true);
		//new fuctional ---

		textSubsystemName.text = "";
		HideTextAbout ();
		help.interactable = false;
		compile.interactable = false;
		Stop (true); //
		browser.GoToSystem();
	}
	public void HideSubsystem(int index)
	{
		compile.interactable = true;
		browser.HideSubsystem (index);
	}

	//Text About on
	void ShowTextAbout()
	{
		textAboutPanel.SetActive (true);
	}

	//Text About off
	void HideTextAbout()
	{
		textAboutPanel.SetActive(false);
	}

	//Text About on-off
	public void ToogleTextAbout()
	{
		ToogleGameObjectActivity (textAboutPanel);
	}

	//Drop Down menu on-off
	public void ToogleDropDown()
	{
		ToogleGameObjectActivity (dropDownMenu);
	}

	public void ToogleGameObjectActivity(GameObject obj)
	{
		obj.SetActive (!obj.activeInHierarchy);
	}
	public void Play()
	{
		if (browser.IsReady == false)
			return;
		if (browser.playAnimationMode == BrowsingMode.System) //если анимация проигрывается в системе
		{ 
			if (browser.State == BrowserState.System) //если мы и так в системе
			{ 
				PlayAnimation();
			} 
			else
			{
				browser.GiveTaskToPlayAnimation (BrowsingMode.System);
				Compile ();
			}
		} 
		else   //если анимация проигрывается в подсистеме
		{
			int indexOfSub = browser.GetIndexOfSubsystem (browser.playAnimationSubsystem);
			if (indexOfSub == -1)
				return;

			if (browser.State == BrowserState.Subsystem) //если мы в подсистеме
			{
				if (indexOfSub == browser.CurrentSubsystemIndex) //если мы в нужной подсистеме
				{ 
					PlayAnimation ();
				} 
				else  //мы в какой-то другой подсистеме
				{
					browser.GiveTaskToPlayAnimation (BrowsingMode.Subsystem);
					ChooseSubsystem (indexOfSub);
				}
			}
			else
			{
				
				if (indexOfSub != -1)
				{
					browser.GiveTaskToPlayAnimation (BrowsingMode.Subsystem);
					ChooseSubsystem (indexOfSub);
				}
			}

		}
	}
	public void PlayAnimation()
	{
		//set current state
		VisState = true;

		//switch buttons
		play.transform.gameObject.SetActive (false);
		stop.transform.gameObject.SetActive (true);

		//start all visualizations
		foreach (VisClass vis in Visualizations)
			vis.StartVis ();
	}
	public void Stop(bool now)
	{
		//if (VisState == true)
		//{
			VisState = false;

			//switch buttons
			play.transform.gameObject.SetActive (true);
			stop.transform.gameObject.SetActive (false);

			//stop all visualizations
			foreach (VisClass vis in Visualizations)
			{
				if (!now)
					vis.StopVis ();
				else
					vis.StopImmidiately ();
			}
		//}
	}

	public void Update()
	{
		if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
		{
			if (EventSystem.current.IsPointerOverGameObject () == false)
			{
				dropDownMenu.SetActive (false);
			}
		}
	}
}
