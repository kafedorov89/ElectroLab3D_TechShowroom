using UnityEngine;
using System.Collections;

public class ButtonReturn : MonoBehaviour {

	// Use this for initialization
	void Start () 
	{
	
	}
	
	// Update is called once per frame
	void Update () 
	{
	
	}

	void OnGUI()
	{
		if (GUI.Button(new Rect(220,20,150,20), "Return"))
		{
			//delete object from previous scene
			//TODO: !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
			//GameObject obj = GameObject.Find("DieselSystem(Clone)");
			//Destroy(obj);
			//obj.
			/*GameObject controlObject = GameObject.Find("GameControl");
			GameControl controlScript = controlObject.GetComponent<GameControl>();
			Destroy(controlScript.t);
			controlScript.t = null;*/

			//load Main Menu
			Application.LoadLevel(0);
		}
	}
}
