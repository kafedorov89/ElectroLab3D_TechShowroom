using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

[CustomEditor(typeof(SubsystemList))]
public class SubsystemListEditor : Editor 
{
	string[] vars =  { };
	int index = 0;
	int size = 0;

	GameObject GO;
	SubsystemList t;
	SerializedObject GetTarget;
	SerializedProperty ThisList;

	//int nButtonWidth = 60;

	void OnEnable(){

		t = (SubsystemList)target;
		GetTarget = new SerializedObject(t);
		ThisList = GetTarget.FindProperty("list"); // Find the List in our script and create a refrence of it
		GO = t.gameObject;
	}

	public override void OnInspectorGUI()
	{
		//Update serialized object
		GetTarget.Update ();

		//DrawDefaultInspector ();
		SerializedProperty systemName = GetTarget.FindProperty ("systemName");
		EditorGUILayout.PropertyField (systemName);

		//Get list size and show it
		size = ThisList.arraySize;
		EditorGUILayout.LabelField ("List size: " + size);

		//Show buttons


		//GetTarget.ApplyModifiedProperties();

		/*EditorGUILayout.BeginHorizontal();
		if (GUILayout.Button (new GUIContent ("Add"), GUILayout.Width (nButtonWidth))) 
			Add ();
		if (GUILayout.Button (new GUIContent ("Delete"), GUILayout.Width (nButtonWidth))) 
			Delete ();
		if (GUILayout.Button (new GUIContent ("Del last"), GUILayout.Width (nButtonWidth))) 
			DeleteLast ();
		if (GUILayout.Button (new GUIContent ("Up"), GUILayout.Width (nButtonWidth))) 
			MoveUp ();
		if (GUILayout.Button (new GUIContent ("Down"), GUILayout.Width (nButtonWidth))) 
			MoveDown ();
		EditorGUILayout.EndHorizontal();*/
		//EditorGUILayout.Space();

		EditorGUILayout.BeginHorizontal();
		if (GUILayout.Button (new GUIContent ("Search and build"))) //, GUILayout.Width (200))) 
			SearchAndBuild ();
		EditorGUILayout.EndHorizontal();

		//Create List<string> with names of subsystems,
		//then create popup menu based on it
		List<string> SubsystemNames = new List<string> (0);
		for (int i = 0; i < ThisList.arraySize; ++i)
		{
			SerializedProperty MyListRef = ThisList.GetArrayElementAtIndex(i);
			SerializedProperty MyName = MyListRef.FindPropertyRelative("name");
			SubsystemNames.Add("[" + i + "] " + MyName.stringValue);
		}
		vars = SubsystemNames.ToArray ();

		//Get current selected index in popup menu
		index = EditorGUILayout.Popup("Choose subsystem: ", index, vars);
		if (index >= 0 && index < ThisList.arraySize)
		{
			//Allow user to configure properties of selected item
			SerializedProperty MyListRef2 = ThisList.GetArrayElementAtIndex (index);
			SerializedProperty MyName2 = MyListRef2.FindPropertyRelative ("name");
			SerializedProperty MyGameObj2 = MyListRef2.FindPropertyRelative ("gameObject");
			SerializedProperty MyTextAbout2 = MyListRef2.FindPropertyRelative ("textAbout");
			GUI.enabled = false;
			EditorGUILayout.PropertyField(MyName2);
			EditorGUILayout.PropertyField (MyGameObj2);
			EditorGUILayout.PropertyField (MyTextAbout2);
			GUI.enabled = true;
			//EditorGUILayout.pro
		}

		//push our changes to origin object
		GetTarget.ApplyModifiedProperties();
	}
	//Add new item to the end of list (push back)
	void Add()
	{
		//Debug.Log (size);
		if (ThisList.arraySize > 0)
			ThisList.InsertArrayElementAtIndex(ThisList.arraySize - 1);
		else
			ThisList.InsertArrayElementAtIndex(0);
	}

	//Delete current item
	void Delete()
	{
		if (ThisList.arraySize > 0)
			ThisList.DeleteArrayElementAtIndex(index);
	}

	//Delete item from the end of list (pop back)
	void DeleteLast()
	{
		if (ThisList.arraySize > 0)
			ThisList.DeleteArrayElementAtIndex(ThisList.arraySize - 1);
	}

	//Move current item up in list (<=)
	void MoveUp()
	{
		bool moveResult = ThisList.MoveArrayElement(index, index - 1);
		if (moveResult) index--;
	}

	//Move current item down in list (=>)
	void MoveDown()
	{
		bool moveResult = ThisList.MoveArrayElement(index, index + 1);
		if (moveResult) index++;
	}
	//search subsystems in all children components and build list
	void SearchAndBuild()
	{
		//clear list
		//Debug.Log ();
		ThisList.ClearArray ();
		//Add ();

		//search in childrens
		SubsystemFlag[] flags = GO.transform.GetComponentsInChildren<SubsystemFlag> ();
		Debug.Log("Found " + flags.Length + " subsystems");

		foreach (SubsystemFlag flag in flags)
		{
			Add (); //add new element to the list
			SerializedProperty MyListRef = ThisList.GetArrayElementAtIndex (ThisList.arraySize - 1);
			SerializedProperty MyName = MyListRef.FindPropertyRelative ("name");
			SerializedProperty MyGameObj = MyListRef.FindPropertyRelative ("gameObject");
			SerializedProperty MyTextAbout = MyListRef.FindPropertyRelative ("textAbout");
			MyName.stringValue = flag.subsystemName;
			MyGameObj.objectReferenceValue = flag.gameObject;
			MyTextAbout.stringValue = flag.textAbout;
		}
	}
}

