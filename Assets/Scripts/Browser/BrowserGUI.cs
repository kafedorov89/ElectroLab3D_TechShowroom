using UnityEngine;
using System.Collections;
using UnityEngine.UI;

//Advanced GUI for browser
//Using new UI System

public class BrowserGUI : MonoBehaviour {

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

	private GameObject target;
	private SubsystemList list;
	private SystemBrowser browser;

	// Use this for initialization
	void Start () 
	{
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

	//Go to selected subsystem
	public void ChooseSubsystem(int index)
	{
		//ToogleDropDown ();
		dropDownMenu.SetActive (false);
		if (browser.IsReady () == false)
			return;

		compile.interactable = true;
		help.interactable = true;
		textSubsystemName.text = list.list [index].name;
		textAbout.text = list.list [index].textAbout;
		browser.GoToSubsystemWithCheck(index);
	}

	//Go to whole system browsing
	public void Compile()
	{
		if (browser.IsReady () == false)
			return;

		//new fuctional +++
		foreach (Subsystem sub in list.list)
			sub.gameObject.SetActive (true);
		//new fuctional ---

		textSubsystemName.text = "";
		HideTextAbout ();
		help.interactable = false;
		compile.interactable = false;
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
		browser.Play ();
	}
}
