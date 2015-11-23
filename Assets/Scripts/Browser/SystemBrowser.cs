using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum State {System = 0, ReduceAlpha, ZoomInSubsystem, Subsystem, ZoomOutSubsystem, IncreaseAlpha};

public enum RenderingMode {Fade = 2, Transparent = 3};
public enum FadeMode {Continuously, Fast};

public class SystemBrowser : MonoBehaviour {

	//public Material outlineMat; 

	Camera mainCam; //main camera
	GameObject GO; //gameobject of main system
	MeshRenderer[] meshes; //meshes of 3D model
	GameObject clonedSub;  //clone of subsystem
	SubsystemList Subs;
	MouseOrbit orbitNav;  //orbit navigation
	GameObject cameraHelper; //camera looks on it when moving
	BrowserGUI bGUI;

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

	[Tooltip("How should system fade")]
	public FadeMode fadeMode = FadeMode.Continuously;

	[Tooltip("Rendering mode for transparency")]
	public RenderingMode renderingMode = RenderingMode.Fade;

	public float catchTime = 0.25f; //maximum time to catch object
	private float lastDown0 = 0.0f; //when mouse button down
	private float lastDown1 = 0.0f; //when mouse button down

	private int current_subs_index = -1;
	private Vector3 subFrom, subTo;
	private Vector3 parallelFrom = new Vector3(), parallelTo = new Vector3();
	private float distFrom, distTo;
	private float startTime; //start time for some process (alpha changing/camera moving)
	private float subJourneyLength;
	private float sysJourneyLength;
	private int stored_index = -1; 
	private bool isReady = true; //is ready to hadle command from GUI
	private float m_alpha;
	private State state = State.System;
	private List<GameObject> clones = new List<GameObject>();

	private Ray ray;   
	private RaycastHit hit;

	//Is browser ready to handle new command
	public bool IsReady()
	{
		return isReady;
	}

	// Use this for initialization
	void Start () 
	{
		GameObject canvas = GameObject.FindWithTag ("Player");
		bGUI = canvas.GetComponent<BrowserGUI> (); 
		
		GO = gameObject;
		cameraHelper = new GameObject("CameraHelper");

		//+++
		//GameObject ctrlObject = GameObject.FindWithTag("GameController");
		//GameControl ctrlComponent = ctrlObject.GetComponent<GameControl>();
		//cameraHelper.transform.parent = ctrlComponent.target.transform;

		Subs = GetComponent<SubsystemList> ();
		CreateClones ();

		GameObject mainCamObj = GameObject.FindWithTag ("MainCamera"); 
		mainCam = mainCamObj.GetComponent<Camera>();
		orbitNav = mainCam.GetComponent<MouseOrbit>();
		mainPos = GO.transform.position;

		meshes = GetComponentsInChildren<MeshRenderer>();
		PrepareForAlphaBlending ();
		//PrepareForOutline (); //New !!!
		m_alpha = alphaMax;
	}

	// Update is called once per frame
	void Update () 
	{
		//update LMB and RMB down
		if (Input.GetMouseButtonDown (0)) 
			lastDown0 = Time.time;
		if (Input.GetMouseButtonDown (1)) 
			lastDown1 = Time.time;

		//update state
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

		//raycasting
		UpdateRaycasting ();
	}
	void UpdateRaycasting()
	{
		if (state != State.System)
			return;

		ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		bool isHit = Physics.Raycast (ray, out hit, 100.0f);
		if (isHit) 
		{
			//Collider coll = hit.collider;
			GameObject obj = hit.collider.gameObject;
			SubsystemFlag flag = obj.GetComponentInParent<SubsystemFlag>();
			if (flag != null)
			{
				bGUI.textSubsystemName.text = flag.subsystemName;
				//OutlineObject(flag.gameObject);
				if (Input.GetMouseButtonUp(0))
				{
					float deltaTime = Time.time - lastDown0;
					if (deltaTime < catchTime)
					{
						int a = GetSubsystemIndex(flag.gameObject);
						bGUI.ChooseSubsystem(a);
					}
				}
				if (Input.GetMouseButtonUp(1))
				{
					float deltaTime = Time.time - lastDown1;
					if (deltaTime < catchTime)
					{
						int a = GetSubsystemIndex(flag.gameObject);
						bGUI.HideSubsystem(a);
					}
				}
			}
		}
		else
			bGUI.textSubsystemName.text = "";

	}
	/*void OutlineObject(GameObject obj)
	{
		MeshRenderer[] objMeshes = obj.GetComponentsInChildren<MeshRenderer>();
		if (objMeshes == null)
			return;
		foreach (MeshRenderer rend in objMeshes)
		{
			Material m = rend.materials[1];
			m.SetFloat("_outline_enable", 1.0f);
			//Material m = Resources.Load("Outline") as Material;
			//m.SetFloat("_outline_width", 0.3f);
			//rend.materials.SetValue(m, 1);
		}
	}*/
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
			//clone.transform.localScale = GO.transform.localScale;

			clone.transform.parent = GO.transform; //<=
			clone.transform.localScale = original.gameObject.transform.localScale;
			//clone.transform.sca = original.gameObject.transform.lossyScale;
		}
	}

	//prepare standart shader for transparency
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
	/*void PrepareForOutline()
	{
		if (meshes == null)
			return;
		foreach (MeshRenderer rend in meshes)
		{

			Material m = outlineMat;

			m.SetFloat("_outline_enable", 0.0f);
			//m.SetFloat("_outline_width", 0.0f);
			//m.SetColor("_outline_color", new Color(0,1,0,0));

			//m.shader = Shader.Find("Outline");
			Material[] oldMats = rend.materials;
			Material[] newMats = new Material[oldMats.Length+1];
			for (int i = 0; i < oldMats.Length; ++i)
				newMats[i] = oldMats[i];
			newMats[oldMats.Length] = m;
			rend.materials = newMats;
		}
	}*/

	public void SetAlpha(float a)
	{
		//float r, g, b;
		foreach (MeshRenderer rend in meshes)
		{
			foreach (Material material in rend.materials)
			{
				if (material.HasProperty("_node_op"))
					material.SetFloat("_node_op", a);
				SetColorMaterialAlpha(material, "_Color", a);
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
	void MoveLocal(GameObject obj, float start_time, float shift_time, Vector3 from, Vector3 to)
	{
		if (from == to)
			return;

		float journeyLength = Vector3.Distance(from, to);
		float speed = journeyLength / shift_time;
		float distCovered = (Time.time - start_time) * speed;
		float fractJourney = distCovered / journeyLength;

		//Debug.Log (from + "/" + to);
		obj.transform.localPosition = Vector3.Lerp(from, to, fractJourney);
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
	public int GetSubsystemIndex(GameObject obj)
	{
		List<Subsystem> list = Subs.list;
		for (int i = 0; i < list.Count; ++i)
		{
			if (list[i].gameObject == obj)
				return i;
		}
		return -1;
	}

	// Go to subsystem browsing with check current situation
	public void GoToSubsystemWithCheck(int subs_index)
	{
		if (isReady == false) return;
		//if index not valid OR this is current zoomed subsystem
		if (subs_index == -1 || subs_index == current_subs_index) return;
		if (Subs.list[subs_index].gameObject == null) return;

		if (current_subs_index == -1 && IsHiddenSubsystems()==false)
			GoToSubsystem(subs_index);
		else
		{
			stored_index = subs_index;
			GoToSystem();
		}
	}
	public bool IsHiddenSubsystems()
	{
		foreach (Subsystem sub in Subs.list)
		{
			if (sub.gameObject.activeInHierarchy == false)
				return true;
		}
		return false;
	}
	public void HideSubsystem(int index)
	{
		Subs.list [index].gameObject.SetActive (false);
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
		//orbitNav.target = GO.transform;
		//orbitNav.SetTarget (GO.transform;);

		//new fuctional +++
		//foreach (Subsystem sub in Subs.list)
		//	sub.gameObject.SetActive (true);
		//new fuctional ---

		//switch point from and point to, start distance
		//and finish distance
		SwithVector3Value (ref subTo, ref subFrom);

		SwithVector3Value (ref parallelTo, ref parallelFrom);
		//parallelFrom = orbitNav.gameObject.transform.localPosition;
		//parallelTo = new Vector3(0,0,0);

		//SwithFloatValue (ref distFrom, ref distTo);
		distFrom = orbitNav.distance;
		distTo = Mathf.Min(orbitNav.distanceMax, orbitNav.distance + zoomIncrement);

		//orbitNav.target = cameraHelper.transform; //look at camera helper
		orbitNav.SetTarget (cameraHelper.transform);
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
		if (m_alpha != alphaMin)
		{
			if (fadeMode == FadeMode.Continuously)
				ChangeAlpha (GO, startTime, shiftAlphaTime, alphaMax, alphaMin);
			else
			{
				m_alpha = alphaMin;
				SetAllMeshesVisibility(false);
			}
		}
		else
		{
			FixTime();
			state = State.ZoomInSubsystem; //next state

			//cameraHelper.transform.position = GO.transform.position;
			//orbitNav.target = cameraHelper.transform;
			orbitNav.SetTarget(cameraHelper.transform);

			subFrom = GO.transform.position;
			subTo = clonedSub.transform.position;
			//Debug.Log(subTo);
			//subTo = clones[current_subs_index].transform.position;
			//subFrom = orbitNav.cameraRotation.transform.position;

			parallelFrom = orbitNav.transform.localPosition;
			parallelTo = new Vector3(0,0,0);

			distFrom = orbitNav.distance;
			distTo = Mathf.Max(orbitNav.distanceMin, orbitNav.distance - zoomIncrement);
		}
	}

	//Increase alpha-channel of whole system 
	void IncreaseAlpha()
	{
		if (m_alpha != alphaMax)
		{
			if (fadeMode == FadeMode.Continuously)
				ChangeAlpha (GO, startTime, shiftAlphaTime, alphaMin, alphaMax);
			else
			{
				m_alpha = alphaMax;
				SetAllMeshesVisibility(true);
			}
		}
		else
		{
			state = State.System; //next state
			clonedSub.SetActive(false);
			//orbitNav.target = GO.transform;
			orbitNav.SetTarget(GO.transform);
		}
	}

	//Move camera closer to selected subsystem
	void ZoomInSub()
	{
		if (cameraHelper.transform.position != subTo)
		{
			Move(cameraHelper, startTime, shiftZoomTime, subFrom, subTo);
			MoveLocal(orbitNav.gameObject, startTime, shiftZoomTime, parallelFrom, parallelTo);
			//Move(orbitNav.cameraRotation, startTime, shiftZoomTime, subFrom, subTo);
			ChangeDist(orbitNav, startTime, shiftZoomTime, distFrom, distTo);
		}
		else 
		{
			state = State.Subsystem; //next state
			isReady = true;
			//orbitNav.target = clonedSub.transform;
			orbitNav.SetTarget(cameraHelper.transform);

			//mainCam.transform.LookAt(GO.transform);
		}
	}

	//Move camera further from selected subsystem
	void ZoomOutSub()
	{
		if (cameraHelper.transform.position != subTo)
		{
			Move(cameraHelper, startTime, shiftZoomTime, subFrom, subTo);
			MoveLocal(orbitNav.gameObject, startTime, shiftZoomTime, parallelFrom, parallelTo);
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
	void SetColorMaterialAlpha(Material material, string colorName, float a)
	{
		if (material.HasProperty (colorName))
		{
			Color clr = material.GetColor(colorName);
			material.SetColor (colorName, new Color (clr.r, clr.g, clr.b, a));
		}
	}
	void SetAllSubsystemsActivity(bool isActive)
	{
		foreach (Subsystem sub in Subs.list)
			sub.gameObject.SetActive (isActive);
	}
	void SetAllSubsystemsVisibility(bool isVisible)
	{
		foreach (Subsystem sub in Subs.list)
			SetVisibility (sub.gameObject, isVisible);
	}
	void SetAllMeshesVisibility(bool isVisible)
	{
		foreach (MeshRenderer mesh in meshes)
			mesh.enabled = isVisible;
	}
	//Set object visible or invisible
	void SetVisibility(GameObject obj, bool isVisible)
	{
		MeshRenderer[] meshes = obj.GetComponentsInChildren<MeshRenderer>();
		foreach (MeshRenderer rend in meshes)
		{
			rend.enabled = isVisible;
		}
	}
}
