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

	SubsystemList t;
	SerializedObject GetTarget;
	SerializedProperty ThisList;

	int nButtonWidth = 60;

	void OnEnable(){

		t = (SubsystemList)target;
		GetTarget = new SerializedObject(t);
		ThisList = GetTarget.FindProperty("list"); // Find the List in our script and create a refrence of it
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
		EditorGUILayout.BeginHorizontal();
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
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.Space();

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
		if (index >= 0 && index < size)
		{
			//Allow user to configure properties of selected item
			SerializedProperty MyListRef2 = ThisList.GetArrayElementAtIndex (index);
			SerializedProperty MyName2 = MyListRef2.FindPropertyRelative ("name");
			SerializedProperty MyGameObj2 = MyListRef2.FindPropertyRelative ("gameObject");
			SerializedProperty MyTextAbout2 = MyListRef2.FindPropertyRelative ("textAbout");
			EditorGUILayout.PropertyField (MyName2);
			EditorGUILayout.PropertyField (MyGameObj2);
			EditorGUILayout.PropertyField (MyTextAbout2);
		}

		//push our changes to origin object
		GetTarget.ApplyModifiedProperties();
	}
	//Add new item to the end of list (push back)
	void Add()
	{
		if (size > 0)
			ThisList.InsertArrayElementAtIndex(size - 1);
		else
			ThisList.InsertArrayElementAtIndex(0);
	}

	//Delete current item
	void Delete()
	{
		if (size > 0)
			ThisList.DeleteArrayElementAtIndex(index);
	}

	//Delete item from the end of list (pop back)
	void DeleteLast()
	{
		if (size > 0)
			ThisList.DeleteArrayElementAtIndex(size - 1);
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
}

