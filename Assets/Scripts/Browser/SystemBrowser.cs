using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum State {System = 0, ReduceAlpha, ZoomInSubsystem, Subsystem, ZoomOutSubsystem, IncreaseAlpha};

public enum RenderingMode {Fade = 2, Transparent = 3};

public class SystemBrowser : MonoBehaviour {

	Camera mainCam; //main camera
	GameObject GO; //gameobject of main system
	MeshRenderer[] meshes; //meshes of 3D model
	GameObject clonedSub;  //clone of subsystem
	SubsystemList Subs;
	MouseOrbit orbitNav;  //orbit navigation
	GameObject cameraHelper; //camera looks on it when moving

	Vector3 mainPos;
	Vector3 subPos;

	[Tooltip("Value of system alpha-cannel when subsystem is browsing. Set 0 for system invisible")]
	public float alphaMin = 0.0f; //alpha-cannel when subsystem browsing
	private float alphaMax = 1.0f; //alpha-cannel when system browsing

	[Tooltip("Time in seconds for alpha-cannel reducing and increasing process")]
	public float shiftAlphaTime = 1.0f; //time for change system alpha-channel

	[Tooltip("Time in seconds for zoom in and zoom out process")]
	public float shiftZoomTime = 1.0f; //time for zoom subsystem in or out

	[Tooltip("How mush camera distance must be reduced when zoom in")]
	public float zoomIncrement = 2.0f; //насколько приближать подсистему к камере

	[Tooltip("Rendering mode for transparency")]
	public RenderingMode renderingMode = RenderingMode.Fade;

	private int current_subs_index = -1;
	private Vector3 subFrom, subTo;
	private float distFrom, distTo;
	private float startTime; //start time for some process (alpha changing/camera moving)
	private float subJourneyLength;
	private float sysJourneyLength;
	private int stored_index = -1; 
	private bool isReady = true; //is ready to hadle command from GUI
	private float m_alpha;
	private State state = State.System;
	private List<GameObject> clones = new List<GameObject>();

	//Is browser ready to handle new command
	public bool IsReady()
	{
		return isReady;
	}

	// Use this for initialization
	void Start () 
	{
		GO = gameObject;
		cameraHelper = new GameObject("CameraHelper");

		Subs = GetComponent<SubsystemList> ();
		CreateClones ();

		GameObject mainCamObj = GameObject.FindWithTag ("MainCamera"); 
		mainCam = mainCamObj.GetComponent<Camera>();
		orbitNav = mainCam.GetComponent<MouseOrbit>();
		mainPos = GO.transform.position;

		meshes = GetComponentsInChildren<MeshRenderer>();
		PrepareForAlphaBlending ();
		m_alpha = alphaMax;
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (state == State.System) {
			if (stored_index != -1)
				GoToSubsystem (stored_index);
			else
				isReady = true;
		}
		if (state == State.ReduceAlpha) 
			ReduceAlpha (); 
		if (state == State.ZoomInSubsystem) 
			ZoomInSub();
		if (state == State.ZoomOutSubsystem) 
			ZoomOutSub();
		if (state == State.IncreaseAlpha) 
			IncreaseAlpha();
	}

	//Create clone for each subsystem gameobject
	void CreateClones()
	{
		GameObject original, clone;
		for (int i = 0; i < Subs.list.Count; ++i) 
		{
			original = Subs.list[i].gameObject;
			if (original == null) continue;

			clone = Instantiate(original);
			clones.Add(clone);
			clone.SetActive(false);
			clone.transform.rotation = original.gameObject.transform.rotation;
			clone.transform.position = original.gameObject.transform.position;
			clone.transform.localScale = GO.transform.localScale;
		}
	}

	void PrepareForAlphaBlending()
	{
		if (meshes == null)
			return;
		foreach (MeshRenderer rend in meshes)
		{
			foreach (Material material in rend.materials)
			{
				material.SetFloat ("_Mode", (float)renderingMode); //fade or transparent
				material.SetInt ("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
				material.SetInt ("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
				//material.SetInt ("_ZWrite", 0);
				material.DisableKeyword ("_ALPHATEST_ON");
				material.EnableKeyword ("_ALPHABLEND_ON");
				material.DisableKeyword ("_ALPHAPREMULTIPLY_ON");
				material.renderQueue = 3000;
			}
		}
	}

	public void SetAlpha(float a)
	{
		float r, g, b;
		foreach (MeshRenderer rend in meshes)
		{
			foreach (Material material in rend.materials)
			{
				if (material.HasProperty("_Color"))
				{
					Color clr = material.GetColor("_Color");
					r = clr.r;
					g = clr.g;
					b = clr.b;
					material.SetColor("_Color", new Color(r, g, b, a));
				}
			}
		}
	}

	//TODO: fix increadible code duplicate !!!
	void Move(GameObject obj, float start_time, float shift_time, Vector3 from, Vector3 to)
	{
		float journeyLength = Vector3.Distance(from, to);
		float speed = journeyLength / shift_time;
		float distCovered = (Time.time - start_time) * speed;
		float fractJourney = distCovered / journeyLength;

		obj.transform.position = Vector3.Lerp(from, to, fractJourney);
	}
	void ChangeDist(MouseOrbit orb, float start_time, float shift_time, float from, float to)
	{
		float journeyLength = Mathf.Abs (to - from);
		float speed = journeyLength / shift_time;
		float distCovered = (Time.time - start_time) * speed;
		float fractJourney = distCovered / journeyLength;
		float dist = Mathf.Lerp (from, to, fractJourney);
		
		orb.distance = dist;
		orb.cameraDistance.transform.localPosition = new Vector3(orb.cameraDistance.transform.localPosition.x, 
		                                                    orb.cameraDistance.transform.localPosition.y, 
		                                                    -dist);
	}
	void ChangeAlpha(GameObject obj, float start_time, float shift_time, float from, float to)
	{
		float journeyLength = Mathf.Abs (to - from);
		float speed = journeyLength / shift_time;
		float distCovered = (Time.time - start_time) * speed;
		float fractJourney = distCovered / journeyLength;
		float alpha = Mathf.Lerp (from, to, fractJourney);
		m_alpha = alpha;
		SetAlpha(alpha);
	}
	//--- increadible code duplicate --- !!!

	// Go to subsystem browsing with check current situation
	public void GoToSubsystemWithCheck(int subs_index)
	{
		if (isReady == false) return;
		//if index not valid OR this is current zoomed subsystem
		if (subs_index == -1 || subs_index == current_subs_index) return;
		if (Subs.list[subs_index].gameObject == null) return;

		if (current_subs_index == -1)
			GoToSubsystem(subs_index);
		else
		{
			stored_index = subs_index;
			GoToSystem();
		}
	}
	//Go to system browsing
	public void GoToSystem()
	{
		if (isReady == false) //browser is busy
			return;
		if (current_subs_index == -1) //no zoomed subs
			return;

		isReady = false;
		FixTime ();
		orbitNav.target = GO.transform;

		//switch point from and point to, start distance
		//and finish distance
		SwithVector3Value (ref subTo, ref subFrom);
		SwithFloatValue (ref distFrom, ref distTo);

		orbitNav.target = cameraHelper.transform; //look at camera helper
		state = State.ZoomOutSubsystem; //next state

		current_subs_index = -1;
	}

	// Go to subsystem browsing; NOT FOR EXTERNAL USING!
	void GoToSubsystem(int subs_index)
	{
		isReady = false;
		state = State.ReduceAlpha;
		FixTime ();
		current_subs_index = subs_index;
		if (stored_index != -1) //clear memory about task
			stored_index = -1;

		clonedSub = clones[subs_index];
		clonedSub.SetActive (true);
	}

	//Reduce alpha-channel of whole system
	void ReduceAlpha()
	{
		if (m_alpha != alphaMin) {
			ChangeAlpha (GO, startTime, shiftAlphaTime, alphaMax, alphaMin);
		}
		else
		{
			FixTime();
			state = State.ZoomInSubsystem; //next state

			cameraHelper.transform.position = GO.transform.position;
			orbitNav.target = cameraHelper.transform;

			subTo = clonedSub.transform.position;
			subFrom = GO.transform.position; 

			distFrom = orbitNav.distance;
			distTo = Mathf.Max(orbitNav.distanceMin, orbitNav.distance - zoomIncrement);
		}
	}

	//Increase alpha-channel of whole system 
	void IncreaseAlpha()
	{
		if (m_alpha != alphaMax)
			ChangeAlpha (GO, startTime, shiftAlphaTime, alphaMin, alphaMax);
		else
		{
			state = State.System; //next state
			clonedSub.SetActive(false);
			orbitNav.target = GO.transform;
		}
	}

	//Move camera closer to selected subsystem
	void ZoomInSub()
	{
		if (cameraHelper.transform.position != subTo)
		{
			Move(cameraHelper, startTime, shiftZoomTime, subFrom, subTo);
			ChangeDist(orbitNav, startTime, shiftZoomTime, distFrom, distTo);
		}
		else 
		{
			state = State.Subsystem; //next state
			isReady = true;
			orbitNav.target = clonedSub.transform;
		}
	}

	//Move camera further from selected subsystem
	void ZoomOutSub()
	{
		if (cameraHelper.transform.position != subTo)
		{
			Move(cameraHelper, startTime, shiftZoomTime, subFrom, subTo);
			ChangeDist(orbitNav, startTime, shiftZoomTime, distFrom, distTo);
		}
		else 
		{
			FixTime();
			state = State.IncreaseAlpha; //next state
		}
	}

	//Save current time as start time of next process (animation)
	void FixTime()
	{
		startTime = Time.time;
	}

	//float a <=> b
	void SwithFloatValue(ref float a, ref float b)
	{
		float temp;
		temp = a;
		a = b;
		b = temp;
	}

	//Vector3 a <=> b
	void SwithVector3Value(ref Vector3 a, ref Vector3 b)
	{
		Vector3 temp;
		temp = a;
		a = b;
		b = temp;
	}
}
