using UnityEngine;
using System.Collections;

public class ShowHideOnLoad : MonoBehaviour {

	public GameObject[] targets;

	// Use this for initialization
	void Start () 
	{
		Hide ();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void Show ()
	{
		Debug.Log ("Show");
		foreach (GameObject obj in targets)
			obj.SetActive (true);
	}

	void Hide ()
	{
		Debug.Log ("Hide");
		foreach (GameObject obj in targets)
			obj.SetActive (false);
	}

	void OnLevelWasLoaded (int currentLevel)
	{
		//if (currentLevel == 0)
		//{
		//	
		//	Hide ();
		//}
		if (currentLevel == 1)
		{
			
			Show ();
		}
	}
}
