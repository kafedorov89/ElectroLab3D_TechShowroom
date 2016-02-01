
// Find Replace Tags Layers v1.41 - a Unity3d Editor Utility.
// Please see the user manual (.pdf) for more information.
// Copyright (c) 2012-2015 PracticePad Inc. All rights reserved.

/*
Change Log

1.41 (1 Jan 2015):
	Updated object reference exception message handling.
	Long file paths are wrapped when displayed in the console.
	Refactoring and comments to improve readability.

1.4 (23 Jan 2014):
	Updated for use with Unity 4.3 undo operations.
	Record undo events in console and text file (if enabled).
	Added refresh to show log file in Project View upon creation.
	Timestamp on output headers for easier review.
	Harmonized console and log views.
	Maintain checkbox states on 0 results. 
	Fixed find criteria text color on Unity Pro.
	Fixed empty log file add.
	
1.34 (15 Nov 2013):
	Added warning for Unity 4.3 (deprecation of Undo.RegisterSceneUndo()).

1.33 (19 Aug 2013):
	Updated .activeInHierarchy conditional compilation for Unity 4.2 and greater.

1.32 (14 Aug 2013):
	Added delete function; check for scene change; UI improvements.

1.21 (26 Mar 2013):
	Updated .activeInHierarchy conditional compilation for Unity 4.0.1 and 4.1.
	
1.2 (10 Feb 2013):
	Fixed null reference exception for Selection.activeGameObject when 0 results.

1.1 (14 Nov 2012): 
	Support Unity 4's .activeInHierarchy, backward-compatible.
	Cosmetic fix for new search on cleared console.
*/

using UnityEditor;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;

public class FindReplaceTagsLayers : EditorWindow
{
	private readonly String logFileName = "TagsLayers.txt";

	// UnityEngine.Application.dataPath has forward slashes
	// as directory separators on both win and *nix systems
	private readonly char unitySeparator = '/';
	private readonly String nl = System.Environment.NewLine;
	private readonly String tab = "\t";

	private readonly String dividerSymbol = "-";
	private readonly int dividerLength = 75;

	// wrapLength < dividerLength in order to keep the native Editor
	// look-and-feel, which uses a proportional (variable-width) font
	private readonly int wrapLength = 52;

	private String logPath;
	private String filePath;
	private bool logFileOn;

	private String currentScene;
	private String lastScene;

	private String consoleString;
	private String logDataString;

	private GameObject[] evalTagsContainer;
	private Dictionary<int,GameObject> resultsContainer;

	private String selectedTag;
	private int selectedLayer;
	
	private bool checkBoxTags;
	private bool checkBoxLayers;
	private bool allGameObjs;
	private int allGobs;
	private String[] gobSet;

	private int resultCount;

	private bool findMode;
	private bool verificationLock;
	private bool showTagWarning;
	private bool showLayerWarning;
	private bool showDeleteWarning;
	private bool performDelete;
	private bool deleted;
	private bool madeChanges;

	private int confirmChange;
	private String changeTag;
	private int changeLayer;
	
	private GUIStyle finderStyle;
	private GUIStyle resultCountStyle;
	private String dividerString;

	private Rect windowSelect;
	private Rect windowOut;

	private Vector2 scrollPos;

	private bool IsActiveGob;
	
	private int lastFrame;
	
	[MenuItem("Window/Find Replace Tags Layers")]
	static void ShowWindow()
	{
		GetWindowWithRect<FindReplaceTagsLayers>(new Rect(100,100,638,269),false,"Tags & Layers");
	}

	void OnEnable()
	{
		logPath = Application.dataPath.ToString();
		filePath = logPath + Path.DirectorySeparatorChar + logFileName;
		logFileOn = false;

		currentScene = EditorApplication.currentScene;
		lastScene = EditorApplication.currentScene;

		evalTagsContainer = null;
		resultsContainer = new Dictionary<int,GameObject>();

		resultCount = resultsContainer.Count();
		selectedTag = "Untagged";
		selectedLayer = 0;

		checkBoxTags = true;
		checkBoxLayers = true;
		allGobs = 0;
		gobSet = new String[]{"All Active","Parent/Child"};

		findMode = true;
		verificationLock = false;
		showTagWarning = false;
		showLayerWarning = false;
		showDeleteWarning = false;
		performDelete = false;
		deleted = false;
		madeChanges = false;

		confirmChange = 0;
		changeTag = "";
		changeLayer = -1;

		consoleString = "";
		logDataString = "";
		dividerString = SetDivider();

		finderStyle = new GUIStyle();
		finderStyle.alignment = TextAnchor.MiddleCenter;
		if (EditorGUIUtility.isProSkin)
		{
			finderStyle.normal.textColor = Color.gray;
		}
		
		resultCountStyle = new GUIStyle();
		resultCountStyle.alignment = TextAnchor.MiddleCenter;
		if (EditorGUIUtility.isProSkin)
		{
			resultCountStyle.normal.textColor = Color.gray;
		}

		windowSelect = new Rect(4,10,201,250);
		windowOut = new Rect(208,10,425,250);
		
		lastFrame = 0;
	}
	
	void OnGUI()
	{
		BeginWindows();
		
		if (findMode == true) 
		{
			windowSelect = GUI.Window(0, windowSelect, SelectionWindow, "Find Criteria");
		}
		else if (findMode == false)
		{
			windowSelect = GUI.Window(0, windowSelect, SelectionWindow, "Replace or Delete Criteria");
		}
		
		windowOut = GUI.Window(1, windowOut, OutputWindow, "Console");

		if (Event.current.type == EventType.ValidateCommand && Event.current.commandName == "UndoRedoPerformed")
		{
			int thisFrame = Time.frameCount;
			recordUndoRedo(thisFrame);
		}

		EndWindows();
	}
	
	void SelectionWindow(int windowNumber)
	{
		// Find Mode: Select a tag and/or layer.
		if (findMode == true && checkBoxTags == false && verificationLock == false)
		{
			selectedTag = EditorGUI.TagField(new Rect(54,128,120,20),selectedTag);
		}
		if (findMode == true && checkBoxLayers == false && verificationLock == false)
		{
			selectedLayer = EditorGUI.LayerField(new Rect(54,170,120,20),selectedLayer);
		}
		// Replace/Delete Mode: Select a tag and/or layer.
		if (findMode == false && checkBoxTags == true && verificationLock == false)
		{
			selectedTag = EditorGUI.TagField(new Rect(45,102,120,20),selectedTag);
		}
		if (findMode == false && checkBoxLayers == true && verificationLock == false)
		{
			selectedLayer = EditorGUI.LayerField(new Rect(45,140,120,20),selectedLayer);
		}

		if (verificationLock == false)
		{
			if (findMode == true)
			{
				GUI.Label(new Rect(0,12,windowSelect.width,40),"Find GameObjects in Scene:",finderStyle);
				GUI.Label(new Rect(0,32,windowSelect.width,40),GetShortSceneName(),finderStyle);
				allGobs = GUI.Toolbar(new Rect(3,68,195,20),allGobs,gobSet);

				// Note: allGobs 0 and 1 are array indices - not true/false.
				if (allGobs == 0)
				{
					allGameObjs = true;
				}
				else if(allGobs == 1) 
				{
					allGameObjs = false;
				}
			}
			
			if(findMode == true)
			{
				checkBoxTags = GUI.Toggle(new Rect(54,106,10,20),checkBoxTags,"");
				checkBoxLayers = GUI.Toggle(new Rect(54,151,10,20),checkBoxLayers,"");
				
				GUI.Label(new Rect(80,107,100,20),"All Tags");
				GUI.Label(new Rect(80,152,100,20),"All Layers");
			}
			else if(findMode == false)
			{
				checkBoxTags = GUI.Toggle(new Rect(44,82,10,20),checkBoxTags,"");
				checkBoxLayers = GUI.Toggle(new Rect(44,120,10,20),checkBoxLayers,"");
				
				GUI.Label(new Rect(60,84,100,20),"Replace Tag");
				GUI.Label(new Rect(60,122,100,20),"Replace Layer");
			}
		}

		resultCount = resultsContainer.Count();

		if (resultCount > 0)
		{
			resultCountStyle.fontStyle = FontStyle.Bold;
		}
		else
		{
			resultCountStyle.fontStyle = FontStyle.Normal;
		}

		if (findMode == false)
		{
			GUI.Label(new Rect(0,28,windowSelect.width,20),"Matched GameObjects: " + resultCount.ToString(),resultCountStyle);
			GUI.Label(new Rect(0,38,windowSelect.width,40),"Scene: " + GetShortSceneName(),resultCountStyle);
		}
		
		if (findMode == true && verificationLock == false)
		{
			#region Find Mode
			if(GUI.Button(new Rect(3,200,195,19),"Find"))
			{
				evalTagsContainer = null;
				resultsContainer.Clear();

				#region Step 1, Option a): Evaluate tags on selected gob and its children.
				if (allGameObjs == false)
				{
					try
					{
						Transform[] parentChildTransforms = Selection.activeGameObject.GetComponentsInChildren<Transform>();
						GameObject[] parentChildren = new GameObject[parentChildTransforms.Count()];
						for (int i = 0; i < parentChildTransforms.Count(); i += 1)
						{
							parentChildren[i] = parentChildTransforms[i].gameObject;
						}

						if (checkBoxTags == true)
						{
							evalTagsContainer = parentChildren;
						}
						else if (selectedTag == "Untagged")
						{
							evalTagsContainer = parentChildren;
							Dictionary<int,GameObject> noTagContainer = new Dictionary<int,GameObject>();
	
							foreach (GameObject gob in evalTagsContainer)
							{
								if (gob.tag == "Untagged" || gob.tag == "")
								{
									noTagContainer.Add(gob.GetInstanceID(),gob);
								}
							}
							evalTagsContainer = new GameObject[noTagContainer.Count];
							noTagContainer.Values.CopyTo(evalTagsContainer,0);
						}
						else
						{
							evalTagsContainer = parentChildren;
							Dictionary<int,GameObject> selectedTagContainer = new Dictionary<int,GameObject>();
							foreach (GameObject gob in evalTagsContainer)
							{
								if (gob.tag == selectedTag)
								{
									selectedTagContainer.Add(gob.GetInstanceID(),gob);
								}
							}
							evalTagsContainer = new GameObject[selectedTagContainer.Count];
							selectedTagContainer.Values.CopyTo(evalTagsContainer,0);
						}
					}
					catch (Exception ex)
					{
						// Handle non-GameObject types (e.g. scripts). Prefabs are of type GameObject.
						if (ex.Message == "" || ex.Message.Contains("Object reference not set to an instance of an object"))
						{
							CheckConsoleOverflow();
							consoleString += dividerString + nl;
							consoleString += "Selection is not a GameObject." + nl;
							consoleString += "Please select an active GameObject in the Scene/Hierarchy," + nl;
							consoleString += "or select 'All Active' GameObjects to search the entire scene." + nl;
							consoleString += dividerString + nl;
							scrollPos = new Vector2(scrollPos.x, float.MaxValue);
							resultsContainer.Clear();
						}
						else
						{
							CheckConsoleOverflow();
							consoleString += dividerString + nl;
							consoleString += "Unhandled Exception: " + ex.Message.ToString() + nl;
							consoleString += "Please contact: support@practicepadinc.com";
							consoleString += nl;
							consoleString += nl;
							consoleString += dividerString + nl;
							scrollPos = new Vector2(scrollPos.x, float.MaxValue);
						}
						return;
					}
				}
				#endregion

				#region Step 1, Option b): Evaluate tags on all gobs.
				else if (allGameObjs == true)
				{
					if (checkBoxTags == true)
					{
						evalTagsContainer = GameObject.FindObjectsOfType(typeof(GameObject)) as GameObject[];
					}
					else if (selectedTag == "Untagged")
					{
						evalTagsContainer = GameObject.FindObjectsOfType(typeof(GameObject)) as GameObject[];
						Dictionary<int,GameObject> noTagContainer = new Dictionary<int,GameObject>();

						foreach (GameObject gob in evalTagsContainer)
						{
							if (gob.tag == "Untagged" || gob.tag == "")
							{
								noTagContainer.Add(gob.GetInstanceID(),gob);
							}
						}
						evalTagsContainer = new GameObject[noTagContainer.Count];
						noTagContainer.Values.CopyTo(evalTagsContainer,0);
					}
					else
					{
						evalTagsContainer = GameObject.FindGameObjectsWithTag(selectedTag);
					}
				}
				#endregion

				#region Step 2: Evaluate layers from either Step 1 a) or 1 b) above.
				Dictionary<int,GameObject> inLayerContainer = new Dictionary<int,GameObject>();
	
				foreach (GameObject gob in evalTagsContainer)
				{
					if (checkBoxLayers == true)
					{
						inLayerContainer.Add(gob.GetInstanceID(),gob);
					}
					else if (checkBoxLayers == false)
					{
						if (gob.layer == selectedLayer)
						{
							inLayerContainer.Add(gob.GetInstanceID(),gob);
						}
					}
				}
				#endregion
	
				#region Step 3: Display +/- Write Results of Find.
				resultsContainer = inLayerContainer;

				consoleString += dividerString + nl;
				consoleString += "Scene Name: " + EditorApplication.currentScene + nl + nl;
				
				if(allGameObjs == true)
				{
					consoleString += "All GameObjects in scene: " + allGameObjs + nl;
				}
				else
				{
					consoleString += "Selected GameObject (and children): " + Selection.activeGameObject.name + nl;
				}
				
				if(checkBoxTags == true)
				{
					consoleString += "All tags: " + checkBoxTags + nl;
				}
				else
				{
					consoleString += "Selected tag: " + selectedTag + nl;
				}
				
				if(checkBoxLayers == true)
				{
					consoleString += "All layers: " + checkBoxLayers + nl;
				}
				else
				{
					consoleString += "Selected layer: #" + selectedLayer +
						", " + LayerMask.LayerToName(selectedLayer) + nl;
				}
				if (resultsContainer.Count != 0)
				{
					consoleString += "GameObject Count: " + resultsContainer.Count + nl + nl;
				}
				else
				{
					consoleString += nl;
				}
				consoleString += DateTimeOffset.Now + nl;
				consoleString += dividerString + nl;

				if(logFileOn)
				{
					logDataString += dividerString + nl;
					logDataString += "Scene Name: " + EditorApplication.currentScene + nl + nl;
					
					if(allGameObjs == true)
					{
						logDataString += "All GameObjects in scene: " + allGameObjs + nl;
					}
					else
					{
						logDataString += "Selected GameObject (and children): " + Selection.activeGameObject.name + nl;
					}

					if(checkBoxTags == true)
					{
						logDataString += "All tags: " + checkBoxTags + nl;
					}
					else
					{
						logDataString += "Selected tag: " + selectedTag + nl;
					}
					
					if(checkBoxLayers == true)
					{
						logDataString += "All layers: " + checkBoxLayers + nl;
					}
					else
					{
						logDataString += "Selected layer: #" + selectedLayer +
							", " + LayerMask.LayerToName(selectedLayer) + nl;
					}
					if (resultsContainer.Count != 0)
					{
						logDataString += "GameObject Count: " + resultsContainer.Count + nl + nl;
					} else
					{
						logDataString += nl;
					}
					logDataString += DateTimeOffset.Now + nl;
					logDataString += dividerString + nl;
				}

				foreach (KeyValuePair<int,GameObject> kvp in resultsContainer)
				{
					// Display results in Console view.
					CheckConsoleOverflow();
					consoleString += (kvp.Value.GetInstanceID() + " " + kvp.Value.name.ToString() + nl + tab +
						"Tag:" + kvp.Value.tag + tab +
						"Layer #"	+ kvp.Value.layer + ", " + LayerMask.LayerToName(kvp.Value.layer)) + nl;
					consoleString += nl;
					
					// Record results in log file if selected.
					if(logFileOn)
					{
						logDataString += kvp.Value.GetInstanceID() + "," + kvp.Value.name.ToString() + "," +
						"Tag:" + kvp.Value.tag + "," +
						"Layer #" + kvp.Value.layer + "," + LayerMask.LayerToName(kvp.Value.layer) + nl;
					}

					scrollPos = new Vector2(scrollPos.x, float.MaxValue);
				}

				if(logFileOn)
				{
					WriteTextToFile(logDataString);
				}
				
				if (resultsContainer.Count == 0)
				{
					if (Selection.activeGameObject != null)
					{
						#if (UNITY_3_5)
							// Versions 3.5x.
							IsActiveGob = Selection.activeGameObject.active;
						#else
							// Versions 4 and later.
							IsActiveGob = Selection.activeGameObject.activeInHierarchy;
						#endif
					}
					else
					{
						IsActiveGob = false;
					}
					
					if (allGameObjs == true || IsActiveGob == true)
					{
						CheckConsoleOverflow();
						consoleString += nl;
						consoleString += "No active GameObjects found based on selection criteria:"+ nl + nl;
						if(checkBoxTags == false)
						{
							consoleString += tab + "Tag: " + selectedTag + nl + nl;
						}
						if(checkBoxLayers == false)
						{
							consoleString += tab + "Layer: #" + selectedLayer + ", " +
												 LayerMask.LayerToName(selectedLayer) + nl + nl;
						}
						consoleString += nl;
						scrollPos = new Vector2(scrollPos.x, float.MaxValue);
						
						if(logFileOn)
						{
							logDataString += nl;
							logDataString += "No active GameObjects found based on selection criteria:"+ nl + nl;
							if(checkBoxTags == false)
							{
								logDataString += tab + "Tag: " + selectedTag + nl + nl;
							}
							if(checkBoxLayers == false)
							{
								logDataString += tab + "Layer: #" + selectedLayer + ", " +
									LayerMask.LayerToName(selectedLayer) + nl + nl;
							}
							logDataString += nl;
							WriteTextToFile(logDataString);
						}
					}
					else 
					{
						// Handle selected inactive GameObjects, prefabs, or null (other types handled in catch clause).
						CheckConsoleOverflow();
						consoleString += dividerString + nl;
						consoleString += "Please select an active GameObject in the Scene/Hierarchy," + nl;
						consoleString += "or select 'All Active' GameObjects to search the entire scene." + nl;
						consoleString += dividerString + nl;
						scrollPos = new Vector2(scrollPos.x, float.MaxValue);
					}
					resultsContainer.Clear();
				}
				else
				{
					// Reset check boxes for replace mode.
					checkBoxTags = false;
					checkBoxLayers = false;
				}
	
				resultCount = resultsContainer.Count;
				#endregion
			}
			#endregion
		}

		if (resultCount > 0)
		{
			findMode = false;

			if (verificationLock == false)
			{
				if (GUI.Button(new Rect(3,220,195,19),"Back to Find")) 
				{
					findMode = true;
					checkBoxTags = true;
					checkBoxLayers = true;
					allGobs = 0;
					resultsContainer.Clear();
					
					if(consoleString != "")
					{
						consoleString += nl + nl;
					}
					
					scrollPos = new Vector2(scrollPos.x, float.MaxValue);

					if(logFileOn)
					{
						logDataString += nl + nl;
						WriteTextToFile(logDataString);
					}
				}
			}

			#region Replace or Delete Mode
			if (findMode == false)
			{
				switch (confirmChange)
				{
					case 0 :
						if (GUI.Button(new Rect(3,174,195,19),"Replace Tag and/or Layer"))
						{
							#region Pre-Verification State (Replace)
							verificationLock = true;
	
							if (checkBoxTags == true && checkBoxLayers == false)
							{
								changeTag = selectedTag;
							}
							else if (checkBoxTags == false && checkBoxLayers == true)
							{
								changeLayer = selectedLayer;
							}
							else if (checkBoxTags == true && checkBoxLayers == true)
							{
								changeTag = selectedTag;
								changeLayer = selectedLayer;
							}
							else if (checkBoxTags == false && checkBoxLayers == false)
							{
								CheckConsoleOverflow();
								consoleString += dividerString + nl;
								consoleString += "Please select a specific Tag and/or Layer to change to." + nl;
								consoleString += dividerString + nl;
								scrollPos = new Vector2(scrollPos.x, float.MaxValue);
								verificationLock = false;
								break;
							}
	
							if (checkBoxTags == true)
							{
								showTagWarning = true;
							}
							if (checkBoxLayers == true)
							{
								showLayerWarning = true;
							}
							confirmChange = 1;
							#endregion
						}
						
						if (GUI.Button(new Rect(3,197,195,19),"Delete GameObjects"))
						{
							#region Pre-Verification State (Delete)
							verificationLock = true;
							showDeleteWarning = true;
							performDelete = true;
							confirmChange = 1;
							#endregion
						}
						break;
	
					case 1:
						madeChanges = false;
						if (showDeleteWarning == true)
						{
							GUI.color = Color.red;
						}

						if (GUI.Button(new Rect(3,180,195,19),"Verify"))
						{
							#region Perform Replace or Delete Operations
							showTagWarning = false;
							showLayerWarning = false;
							showDeleteWarning = false;

							#if (UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2)
							Undo.RegisterSceneUndo("Tags Layers");
							#else
							// Register GameObject states iteratively (below).
							#endif
							
							#region Replace tags loop
							if (changeTag != "")
							{
								foreach (KeyValuePair<int,GameObject> kvp in resultsContainer)
								{
									if(!kvp.Value.tag.Equals(changeTag))
									{
										#if (!(UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2))
										Undo.RecordObject(kvp.Value,kvp.Key.ToString());
										#endif
										kvp.Value.tag = changeTag;
										madeChanges = true;
									}
								}
								if(logFileOn && madeChanges)
								{
									logDataString += nl;
									logDataString += "Tags of the above GameObjects have been changed to: " + changeTag + nl + nl;
									WriteTextToFile(logDataString);
								}
							}
							#endregion

							#region Replace layers loop
							if (changeLayer != -1)
							{
								foreach (KeyValuePair<int,GameObject> kvp in resultsContainer)
								{
									if(!kvp.Value.layer.Equals(changeLayer))
									{
										#if (!(UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2))
										Undo.RecordObject(kvp.Value,kvp.Key.ToString());
										#endif
										kvp.Value.layer = changeLayer;
										madeChanges = true;
									}
								}
								if(logFileOn && madeChanges)
								{
									logDataString += nl;
									logDataString += "Layers of the above GameObjects have been changed to: #" + changeLayer +
														 ", " + LayerMask.LayerToName(changeLayer) + nl + nl;
									WriteTextToFile(logDataString);
								}
							}
							#endregion
							
							#region Delete loop
							if (performDelete == true)
							{
								#if (UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2)
								foreach (KeyValuePair<int,GameObject> kvp in resultsContainer)
								{
									GameObject.DestroyImmediate(kvp.Value);
								}
								#else
								foreach (KeyValuePair<int,GameObject> kvp in resultsContainer)
								{
									if (kvp.Value != null)
									{
										Undo.DestroyObjectImmediate(kvp.Value);
									}
								}
								#endif

								performDelete = false;
								deleted = true;
								if(logFileOn)
								{
									logDataString += nl;
									logDataString += "The selected GameObjects were deleted from the scene.";
									logDataString += nl;
									WriteTextToFile(logDataString);
								}
							}
							#endregion
							
							CheckConsoleOverflow();

							if (madeChanges == true)
							{
								if(!changeTag.Equals(""))
								{
									consoleString += "Tags replaced with: " + changeTag + nl;
								}
								if(changeLayer != -1)
								{
									consoleString += "Layers replaced with: #" + changeLayer + ", " +
														 LayerMask.LayerToName(changeLayer) + nl;
								}

								consoleString += nl + "Changes complete." + nl + tab + "Use the Unity 'Edit->Undo' menu to set" +
									nl + tab + "GameObjects back to their previous state." + nl;
							}
							else if (deleted == true)
							{
								consoleString += "The selected GameObjects have been deleted from the scene." + nl;
								consoleString += tab + "Use the Unity 'Edit->Undo' menu to attempt" +
									nl + tab + "recovery of the GameObjects." + nl;
							}
							else
							{
								consoleString += "No changes were indicated by:" + nl;
								if(!changeTag.Equals(""))
								{
									consoleString += tab + "Replace Tags with: " + changeTag + nl;
								}
								if(changeLayer != -1)
								{
									consoleString += tab + "Replace Layers with: #" + changeLayer + ", " +
														 LayerMask.LayerToName(changeLayer) + nl;
								}
								consoleString += nl;
							}

							scrollPos = new Vector2(scrollPos.x, float.MaxValue);

							confirmChange = 0;
							changeTag = "";
							changeLayer = -1;
							verificationLock = false;
							if (madeChanges == true)
							{
								checkBoxTags = false;
								checkBoxLayers = false;
							}
							else if (deleted == true)
							{
								findMode = true;
								checkBoxTags = true;
								checkBoxLayers = true;
								allGobs = 0;
								resultsContainer.Clear();
								
								if(consoleString != "")
								{
									consoleString += dividerString + nl + nl;
								}
								
								scrollPos = new Vector2(scrollPos.x, float.MaxValue);
							}
							#endregion
						}
						
						GUI.color = Color.white;
						if (GUI.Button(new Rect(3,203,195,19),"Cancel"))
						{
							#region Cancel
							showTagWarning = false;
							showLayerWarning = false;
							showDeleteWarning = false;

							confirmChange = 0;
							changeTag = "";
							changeLayer = -1;
							verificationLock = false;
							checkBoxTags = false;
							checkBoxLayers = false;							

							CheckConsoleOverflow();
							consoleString += nl + "Last Operation Canceled. No changes were made." + nl + nl;
							scrollPos = new Vector2(scrollPos.x, float.MaxValue);
							#endregion
						}
						break;
				}
		
				if(showTagWarning == true)
				{
					GUI.Label(new Rect(0,73,windowSelect.width,50),"Tag for " + resultCount + " matched" +
						nl + "item(s) will be: " + nl + changeTag, resultCountStyle);
				}

				if(showLayerWarning == true)
				{
					GUI.Label(new Rect(0,117,windowSelect.width,50),"Layer for " + resultCount + " matched" + 
						nl + "items(s) will be #" + changeLayer + ", " + nl + LayerMask.LayerToName(changeLayer),
						resultCountStyle);
				}
				
				if(showDeleteWarning == true)
				{
					GUI.Label(new Rect(0,82,windowSelect.width,100),"All found GameObjects" + nl + 
						"will be deleted. This" + nl + "operation may or may" + nl + "not be recoverable;" + nl + 
						"see Unity's" + nl + "Undo documentation.", resultCountStyle);
				}
			}
			#endregion
		}
	}
	
	void OutputWindow(int windowNumber)
	{
		scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(400), GUILayout.Height(200));		

		GUILayout.Label(consoleString);

		EditorGUILayout.EndScrollView();

		logFileOn = (GUI.Toggle(new Rect(5,223,66,20),logFileOn,"Text Log"));

		if(logFileOn)
		{
			if(GUI.Button(new Rect(75,223,170,20),"Show Text File Path"))
			{
				CheckConsoleOverflow();
				consoleString += dividerString + nl;
				if (logPath.Length > wrapLength)
				{
					consoleString += wrapText(logPath, unitySeparator);
					consoleString += nl;
				}
				else 
				{
					consoleString += logPath + nl;
				}
				consoleString += nl;
				consoleString += "File name: " + logFileName + nl;
				consoleString += dividerString + nl;
				scrollPos = new Vector2(scrollPos.x, float.MaxValue);
			}
		}

		if(GUI.Button(new Rect(248,223,170,20),"Clear Console"))
		{
			consoleString = "";
			scrollPos = new Vector2(scrollPos.x, float.MaxValue);
		}
	}

	void CheckConsoleOverflow()
	{
		if(consoleString.Length >= 16000)
		{
			consoleString = "";
			consoleString += dividerString + nl;
			consoleString += "Console text display capacity was exceeded here." + nl;
			consoleString += "Find/Replace data and the text log file (if on) are not affected." + nl;
			consoleString += dividerString + nl;
		}
	}

	void WriteTextToFile(String StringOutput)
	{
		#if UNITY_EDITOR_WIN
			String outputPath = filePath.Replace(unitySeparator, Path.DirectorySeparatorChar);
		#else
			String outputPath = filePath;
		#endif

		if (!File.Exists(outputPath))
		{
			using (StreamWriter textFile = File.CreateText(outputPath))
			{
				textFile.Write(StringOutput);
			}
		}
		else
		{
			using (StreamWriter textFile = File.AppendText(outputPath))
			{
				textFile.Write(StringOutput);
			}
		}
		AssetDatabase.Refresh();
		logDataString = "";
	}

	String SetDivider()
	{
		String div = "";
		for(int i=1; i<dividerLength; i+=1)
		{
			div += dividerSymbol;
		}
		return div;
	}
	
	String GetShortSceneName() 
	{
		String shortSceneName = EditorApplication.currentScene.Replace("Assets/","");
		shortSceneName = shortSceneName.Replace(".unity","");
		return shortSceneName;
	}
	
	void OnHierarchyChange()
	{	
		resultsContainer.Clear();
		resultCount = resultsContainer.Count();

		performDelete = false;
		deleted = false;
		madeChanges = false;
		
		showTagWarning = false;
		showLayerWarning = false;
		showDeleteWarning = false;

		confirmChange = 0;
		changeTag = "";
		changeLayer = -1;
		verificationLock = false;
		
		findMode = true;
		checkBoxTags = true;
		checkBoxLayers = true;

		currentScene = EditorApplication.currentScene;
			
		if (!currentScene.Equals(lastScene))
		{
			CheckConsoleOverflow();
			consoleString += dividerString + nl;
			consoleString += "Scene has changed, results cleared." + nl;
			consoleString += dividerString + nl + nl;
			scrollPos = new Vector2(scrollPos.x, float.MaxValue);
			
			lastScene = EditorApplication.currentScene;
		}
	}
	
	void recordUndoRedo(int thisFrame)
	{
		if (thisFrame != lastFrame)
		{
			CheckConsoleOverflow();
			consoleString += nl + dividerString + nl;
			consoleString += "UnityEditor undo/redo performed, check assets." + nl + dividerString + nl;
			scrollPos = new Vector2(scrollPos.x, float.MaxValue);
			
			if(logFileOn)
			{
				logDataString += nl + dividerString + nl;
				logDataString += "UnityEditor undo/redo performed, check assets." + nl + dividerString + nl;
				WriteTextToFile(logDataString);
			}
			
			lastFrame = thisFrame;
		}
	}

	String wrapText(String input, char separator)
	{
		String output = "";

		while (input.Length > wrapLength)
		{
			String left = input.Substring(0, wrapLength);
			String right = input.Substring(wrapLength);
			int leftLength = wrapLength;

			if (left.Contains(separator))
			{
				leftLength = left.LastIndexOf(separator);
				String tempRight = left.Substring(leftLength) + right;
				right = tempRight;
			}
			output += left.Substring(0, leftLength);
			output += nl;
			input = right;
		}
		output += input;
		return output;
	}

	void OnInspectorUpdate()
	{
		Repaint();
	}
}
