using UnityEngine;
using System.Collections;
using UnityEditor;

namespace ElectroLab3D{
    [CustomEditor(typeof(AddLOD))]
    public class AddLODEditor : Editor {
        AddLOD addLOD;
        
        public void OnEnable()
        {
            addLOD = (AddLOD)target;
        }
        
        public override void OnInspectorGUI()
        {
            // Draw the default inspector
            DrawDefaultInspector ();
            
            if (GUILayout.Button ("Add LOD Recursively")) {
                Debug.Log ("Add LOD Recursively");
                addLOD.Add(addLOD.transform);
            }
        }

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
}
