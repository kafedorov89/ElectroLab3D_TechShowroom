using UnityEngine;
using System.Collections;
using UnityEditor;

namespace ElectroLab3D
{
    [CustomEditor(typeof(AddLOD))]
    public class AddLODEditor : Editor 
	{
        AddLOD addLOD;
        
        public void OnEnable()
        {
            addLOD = (AddLOD)target;
        }
        
        public override void OnInspectorGUI()
        {
            // Draw the default inspector
            DrawDefaultInspector ();
            
			//Button "Add"
            if (GUILayout.Button ("Add LODs Recursively")) 
			{
                Debug.Log ("Add LOD Recursively");
                addLOD.Add(addLOD.transform);
				Debug.Log ("Finished");
            }

			//Button "Delete"
			if (GUILayout.Button ("Delete LODs")) 
			{
				Debug.Log ("Deleting LODs");
				addLOD.Delete(addLOD.transform);
				Debug.Log ("Finished");
			}
        }

		// Use this for initialization
		void Start ()
		{
		
		}
		
		// Update is called once per frame
		void Update () 
		{
		
		}
	}
}
