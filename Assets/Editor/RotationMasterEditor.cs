using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

[CustomEditor(typeof(RotationMaster))]
public class RotationMasterEditor : Editor {
	
	// Use this for initialization
	void Start () 
	{

	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public override void OnInspectorGUI()
	{	
		DrawDefaultInspector ();

		RotationMaster m = (RotationMaster)target; 
		if (GUILayout.Button (new GUIContent ("Do circle layout"), GUILayout.Width (120)))
			m.DoCircleLayout ();
	}

}
