using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Converters;


public class GameControl : MonoBehaviour {

	public static GameControl control;

	public GameObject target; //object transfered to next scene
	private int previousLevel = 0;
	private bool first = true;

	//bool IsFirstApplicationLoad = true;

	void Awake()
	{
		//Singleton desing pattern
		if (control == null)
		{
			DontDestroyOnLoad (gameObject);
			control = this;
		}
		else 
		{
			Destroy(gameObject);
		}
	}

	// Use this for initialization
	void Start () 
	{
		/*if (first)
		{
			first = false;
			LoadSubsystemsDataFromFile ();
		}*/
	}

	void LoadSubsystemsDataFromFile ()
	{
		//int N = 0, M = 0;
		//List<string> errors = new List<string>();
		GameObject[] units = GameObject.FindGameObjectsWithTag ("Browser");
		SubsystemList subs;
		foreach (GameObject unit in units)
		{
			subs = unit.GetComponent<SubsystemList> ();
			subs.LoadFromFile ("Data");
		}
	}

	// Update is called once per frame
	void Update () {


	}

	//Set target object
	public void SetTarget(GameObject t)
	{
		target = t;
	}

	void OnLevelWasLoaded(int currentLevel)
	{
		LoadSubsystemsDataFromFile ();

		if (currentLevel == 0 && previousLevel == 1) 
		{
			Destroy (target);
			target = null;
		} 
		else if (currentLevel == 1 && previousLevel == 0)
		{
			//Switch light
			LightSwitcher switcher = target.GetComponent<LightSwitcher> ();
			GameObject mainMenuLight = switcher.mainMenuLight;
			GameObject browserLight = switcher.browserLight;
			if (mainMenuLight != null)
				mainMenuLight.SetActive (false);
			if (browserLight != null)
				browserLight.SetActive (true);

			//BrowserGUI gui = t.GetComponent<BrowserGUI>();
			SystemBrowser browser = target.GetComponent<SystemBrowser>();
			//if (gui != null) gui.enabled = true;
			if (browser != null) browser.enabled = true;

			BoxCollider box = target.GetComponent<BoxCollider>();
			//if (gui != null) gui.enabled = true;
			if (box != null) box.enabled = false;

			//target.transform.position = browser.startPosition;
			
			Camera mainCam = Camera.main;
			MouseOrbit orbitNav = mainCam.GetComponent<MouseOrbit>();
			 
			if (orbitNav != null){ 
				orbitNav.SetTarget(target.transform);
				//orbitNav.target = target.transform;
			}
		}
		previousLevel = currentLevel;
	}
}
