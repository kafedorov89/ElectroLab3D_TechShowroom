using UnityEngine;
using System.Collections;

public class GameControl : MonoBehaviour {

	public static GameControl control;

	private GameObject target; //object transfered to next scene
	private int previousLevel = 0;

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
	void Start () {
	


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
		if (currentLevel == 0 && previousLevel == 1) 
		{
			Destroy (target);
			target = null;
		} 
		else if (currentLevel == 1 && previousLevel == 0)
		{
			target.transform.localPosition = new Vector3();

			//BrowserGUI gui = t.GetComponent<BrowserGUI>();
			SystemBrowser browser = target.GetComponent<SystemBrowser>();
			//if (gui != null) gui.enabled = true;
			if (browser != null) browser.enabled = true;

			BoxCollider box = target.GetComponent<BoxCollider>();
			//if (gui != null) gui.enabled = true;
			if (box != null) box.enabled = false;
			
			Camera mainCam = Camera.main;
			MouseOrbit orbitNav = mainCam.GetComponent<MouseOrbit>();
			if (orbitNav != null) orbitNav.target = target.transform;
		}
		previousLevel = currentLevel;
	}
}
