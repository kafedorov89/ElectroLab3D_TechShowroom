using UnityEngine;
using System.Collections;
using UnityEditor;


	[CustomEditor(typeof(DelayTimer))]
	public class DelayTimerEditor : Editor {

		DelayTimer delayTimer;
		
		public void OnEnable()
		{
			delayTimer = (DelayTimer)target;
		}
		
		public override void OnInspectorGUI()
		{
			// Draw the default inspector
			DrawDefaultInspector ();
			
			if (GUILayout.Button ("Timer Start")) {
				Debug.Log ("Timer Start");
				delayTimer.TimerStart();
			}
			
			if (GUILayout.Button ("Timer Stop")) {
				Debug.Log ("Timer Stop");
				delayTimer.TimerStop();
			}

			if (GUILayout.Button ("Timer Reset")) {
				Debug.Log ("Timer Reset");
				delayTimer.ResetTimer();
			}
		}

		// Use this for initialization
		void Start () {
		
		}
		
		// Update is called once per frame
		void Update () {
		
		}
	}
