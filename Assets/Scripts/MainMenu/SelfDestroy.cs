using UnityEngine;
using System.Collections;

public class SelfDestroy : MonoBehaviour {


	//public int levelWhenDestroy = 1;
	private int previousLevel = 0;

	void Awake()
	{
		DontDestroyOnLoad (gameObject);
	}
		
	void OnLevelWasLoaded(int currentLevel)
	{
		if (currentLevel == 0 && previousLevel == 1) 
		{
			Destroy (gameObject);
			Debug.Log (gameObject.name + " self destroyed");
		}
		previousLevel = currentLevel;
	}
}
