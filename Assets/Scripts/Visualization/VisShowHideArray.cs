using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//show or hide array of gameobjects
public class VisShowHideArray : VisClass {
	
	public bool initialState = true; //true = show, false = hide
	private bool state = true;
	public GameObject[] targets;
	//public List<GameObject> targets2;

	// Update is called once per frame
	void Update () {
	
	}

	public override void StartVis()
	{
		base.StartVis();
		SwitchState ();
		SetState ();
	}
	
	public override void StopVis()
	{
		//Debug.Log ("Stop");
		base.StopVis();
		SwitchState ();
		SetState ();
	}
	
	public override void Start()
	{
		//Debug.Log ("Start");
		base.Start();
		state = initialState;
		SetState ();

	}
	public void SwitchState()
	{
		state = !state;
	}
	public void SetState()
	{
		foreach (GameObject t in targets)
		{
			if (t != null)
				t.SetActive (state);
		}

	}

}
