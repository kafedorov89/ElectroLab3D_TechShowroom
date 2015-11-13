using UnityEngine;
using System.Collections;

//Simple GUI for browser
//Using old GUI System

public class SimpleGUI : MonoBehaviour {

	SubsystemList Subs;
	SystemBrowser Browser;
	private int index = -1;

	// Use this for initialization
	void Start () 
	{
		Subs = GetComponent<SubsystemList> ();
		Browser = GetComponent<SystemBrowser> ();
	}

	void OnGUI () {

		if (Subs == null) return;
		if (Subs.list == null) return;
		int size = Subs.list.Count;

		int w = Screen.width;
		int h = Screen.height;

		GUIStyle Style1 = new GUIStyle (GUI.skin.label);
		Style1.alignment = TextAnchor.UpperCenter;

		GUIStyle Style2 = new GUIStyle (GUI.skin.label);
		Style2.alignment = TextAnchor.UpperLeft;

		GUI.Label(new Rect(0, 10, Screen.width, 20), Subs.systemName, Style1);
		if (GUI.Button (new Rect(20,20,150,20), "Return"))
		{
			Application.LoadLevel(0);
		}
		if(GUI.Button(new Rect(20,44,150,20), "Main System"))
		{
			index = -1;
			Browser.GoToSystem();
		}
		Subsystem sub;
		for(int i = 0; i < size; ++i)
		{
			sub = Subs.list[i];
			if (sub == null) return;

			if (GUI.Button(new Rect(20, 68 + (i)*24,150,20), "Subsystem " + i))
			{
				index = i;
				Browser.GoToSubsystemWithCheck(index);
			}

		}
		if (index != -1)
		{
			if (index < 0 || index >= Subs.list.Count) return;
			sub = Subs.list[index];
			if (sub == null) return;

			//Show subsystem name
			GUI.Label(new Rect(0, Screen.height - 40, Screen.width, 20), sub.name, Style1);

			//Show text about
			GUI.Box(new Rect(0.7f * w, 40, 0.3f * w, h - 40), sub.textAbout, Style2);
		}
	}
}
