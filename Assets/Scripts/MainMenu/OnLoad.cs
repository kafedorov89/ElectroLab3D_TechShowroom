using UnityEngine;
using System.Collections;

/*
Add this component on object to prepare it
to transfer to other scene (before Application.LoadLevel())

Warning: it does not works on gameobjects which already exist in scene.
Becose its Awake() function already invoked.
Use only with objects which not added in current scene.

Exaple:
GameObject clone = Instantiate(obj);  
clone.AddComponent<OnLoad>(); 
Application.LoadLevel(1); 
*/

public class OnLoad : MonoBehaviour {
	
	//bool isLoadedOnNewScene = false;

	void Awake () 
	{
		DontDestroyOnLoad (gameObject);
	}

	// Use this for initialization
	void Start()
	{
		//SetVisibility (false);
	}

	// Update is called once per frame
	/*void Update()
	{
		if (isLoadedOnNewScene == false)
		{
			if (Application.loadedLevel == 1)
			{
				SetVisibility(true);
				isLoadedOnNewScene = true;
			}
		}
	}*/

	//Set object visible or invisible
	/*void SetVisibility(bool isVisible)
	{
		MeshRenderer[] meshes = GetComponentsInChildren<MeshRenderer>();
		if (meshes == null)
			return;
		foreach (MeshRenderer rend in meshes)
		{
			rend.enabled = isVisible;
		}
	}*/
}
