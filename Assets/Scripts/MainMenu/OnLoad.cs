using UnityEngine;
using System.Collections;


public class OnLoad : MonoBehaviour {

	void Awake () 
	{
		DontDestroyOnLoad (gameObject);
	}

	// Use this for initialization
	void Start()
	{

	}
}
