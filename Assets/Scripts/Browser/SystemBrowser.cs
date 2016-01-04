using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//using ParticlePlayground;

public enum ParamInitializationMode {Auto, Manual};

public enum State {System = 0, ReduceAlpha, ZoomInSubsystem, Subsystem, ZoomOutSubsystem, IncreaseAlpha,
	PlayAnimation};

public enum RenderingMode {Opaque = 0, Cutout = 1, Fade = 2, Transparent = 3};
public enum FadeMode {Continuously, Fast};

public struct MeshPack
{
	public MeshRenderer[] meshes;
}

public class SystemBrowser : MonoBehaviour {

	//public Material outlineMat; 
	//public PlaygroundParticlesC[] Particles;
	//public GameObject ParticleController;

	//starting transform of system
	public ParamInitializationMode startPositionMode = ParamInitializationMode.Auto;

	//public bool setStartCameraPosition = false;
	//public bool setStartCameraRotation = false;
	//public bool setStartCameraDistance = false;
	public Vector3 startPosition = new Vector3(0, 0, 0);
	public Vector3 startRotation = new Vector3(0, 0, 0);
	public float startDistance = 10.0f;

	Camera mainCam; //main camera

	//GameObject CameraRotation

	GameObject GO; //gameobject of main system
	MeshRenderer[] meshes; //all meshes of 3D model

	MeshPack[] meshesALL; //меши подсистем
	List<MeshRenderer> meshesTrash; //меши мусора (объекты, не входящие ни в одну подсистему)

	GameObject clonedSub;  //clone of subsystem
	SubsystemList Subs;
	MouseOrbit orbitNav;  //orbit navigation
	GameObject cameraHelper; //camera looks on it when moving
	BrowserGUI bGUI;

	Vector3 mainPos;
	Vector3 subPos;

	//[Tooltip("Value of system alpha-cannel when subsystem is browsing. Set 0 for system invisible")]
	//[Range(0.0f,1.0f)]
	private float alphaMin = 0.0f; //alpha-cannel when subsystem browsing
	private float alphaMax = 1.0f; //alpha-cannel when system browsing

	//[Range(0.0f,1.0f)]
	//public float alphaWhenPlay = 0.1f; //alpha-channel when playing animation

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
		GO.transform.position = new Vector3 (0,0,0);
		cameraHelper = new GameObject("CameraHelper");

		//камера направляется на специально созданного помощника
		//начальное положение помощника может быть рассчитано 
		//автоматически - как центр общий мешей, либо задано пользователем
		if (startPositionMode == ParamInitializationMode.Auto)
			cameraHelper.transform.position = CalcCenterOfGameObject2 (GO);
		else
			cameraHelper.transform.position = startPosition;
		//+++
		//GameObject ctrlObject = GameObject.FindWithTag("GameController");
		//GameControl ctrlComponent = ctrlObject.GetComponent<GameControl>();
		//cameraHelper.transform.parent = ctrlComponent.target.transform;

		Subs = GetComponent<SubsystemList> ();

		GameObject mainCamObj = GameObject.FindWithTag ("MainCamera"); 
		mainCam = mainCamObj.GetComponent<Camera>();
		orbitNav = mainCam.GetComponent<MouseOrbit>();
		orbitNav.SetTarget (cameraHelper.transform); //напрявляем на помощника

		mainPos = startPosition; //GO.transform.position;

		BuildMeshes ();

		PrepareForAlphaBlending ();

		m_alpha = alphaMax;

		ResetStartTransform ();
	}

	//set initial camera position
	void ResetStartTransform()
	{
		orbitNav.SetRotation(startRotation);
		orbitNav.Distance = startDistance;
	}
	void BuildMeshes()
	{
		//суть в том, что надо собрать группы мешей, которые относятся к каждой подсистеме
		//а также все остальные меши надо объеденить в отдельную группу
		int N = Subs.list.Count;
		meshesALL = new MeshPack[N];

		for (int i = 0; i < N; ++i)
		{
			//получаем все меши данной подсистемы
			MeshRenderer[] subsystemMeshes = Subs.list[i].gameObject.GetComponentsInChildren<MeshRenderer>();
			meshesALL[i].meshes = subsystemMeshes;

		}

		//получаем вообще все меши
		meshes = gameObject.GetComponentsInChildren<MeshRenderer>();

		meshesTrash = new List<MeshRenderer> (0); //заводим новый массив
		//теперь смотрим, какие меши нигде не присутствуют
		foreach (MeshRenderer m in meshes)
		{
			//если среди родителя не найден флаг подсистемы
			SubsystemFlag flag = m.gameObject.GetComponentInParent<SubsystemFlag>();
			if (flag == null)
				meshesTrash.Add(m); //добавляем мусорный меш
		}

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

			GameObject obj = hit.collider.gameObject;
			SubsystemFlag flag = obj.GetComponentInParent<SubsystemFlag>();
			if (flag != null)
			{
				bGUI.textSubsystemName.text = flag.subsystemName;
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

	//Create clone for each subsystem gameobject //DEPRECATED
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

			clone.transform.parent = GO.transform; //<=
			clone.transform.localScale = original.gameObject.transform.localScale;
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
				//material.SetFloat ("_Mode", 1.0f);
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
		//float r, g, b;
		/*foreach (MeshRenderer rend in meshes)
		{
			foreach (Material material in rend.materials)
			{
				if (material.HasProperty("_node_op"))
					material.SetFloat("_node_op", a);
				SetColorMaterialAlpha(material, "_Color", a);
			}
		}*/
		SetAlphaForSubsystems (a);
		SetAlphaForTrash (a);
	}
	public void SetAlphaForSubsystems(float a)
	{
		for (int i = 0; i < meshesALL.Length; ++i)
		{
			if (i != current_subs_index) //для выбранной подсистемы ничего не делаем!
			{
				foreach (MeshRenderer rend in meshesALL[i].meshes)
				{
					foreach (Material material in rend.materials)
					{
						if (material.HasProperty("_node_op"))
							material.SetFloat("_node_op", a);
						SetColorMaterialAlpha(material, "_Color", a);
					}
				}
			}
		}
	}
	public void SetAlphaForTrash(float a)
	{
		foreach (MeshRenderer rend in meshesTrash)
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

		//current_subs_index = -1;
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

		//clonedSub = clones[subs_index];
		//clonedSub.SetActive (true);
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
				SetAllMeshesVisibility(false); //кроме выбранной
			}
		}
		else
		{
			SetAllMeshesVisibility(false); //кроме выбранной //!!!
			FixTime();
			state = State.ZoomInSubsystem; //next state

			//cameraHelper.transform.position = GO.transform.position;
			//orbitNav.target = cameraHelper.transform;
			orbitNav.SetTarget(cameraHelper.transform);

			subFrom = GO.transform.position;
			//subTo = clonedSub.transform.position;
			subTo = CalcCenterOfGameObject(Subs.list[current_subs_index].gameObject); //clonedSub

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
			//clonedSub.SetActive(false);
			//orbitNav.target = GO.transform;
			orbitNav.SetTarget(GO.transform);
			current_subs_index = -1;
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
			SetAllMeshesVisibility(true); //кроме выбранной //!!!
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
		//foreach (MeshRenderer mesh in meshes)
		//	mesh.enabled = isVisible;
		for (int i = 0; i < meshesALL.Length; ++i) 
		{
			if (i != current_subs_index)
			{
				foreach (MeshRenderer m in meshesALL[i].meshes)
				{
					m.enabled = isVisible;
				}
			}
		}
		foreach (MeshRenderer m in meshesTrash)
		{
			m.enabled = isVisible;
		}
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
	//mass center for all meshes
	Vector3 CalcCenterOfGameObject(GameObject obj)
	{
		int N = 0;
		Vector3 center = new Vector3(0,0,0);
		MeshRenderer[] meshes = obj.GetComponentsInChildren<MeshRenderer>();
		foreach (MeshRenderer rend in meshes)
		{
			center += rend.bounds.center;
			N++;
		}
		center /= (float)N;
		return center;
	}
	//total bounding box for all meshes
	Vector3 CalcCenterOfGameObject2(GameObject obj)
	{
		Vector3 center = new Vector3(0,0,0);
		Vector3 minPoint = new Vector3(0,0,0);
		Vector3 maxPoint = new Vector3(0,0,0);
		MeshRenderer[] meshes = obj.GetComponentsInChildren<MeshRenderer>();
		minPoint = meshes [0].bounds.min;
		maxPoint = meshes [0].bounds.max;
		foreach (MeshRenderer rend in meshes)
		{
			minPoint = Vector3.Min (minPoint, rend.bounds.min);
			maxPoint = Vector3.Max (maxPoint, rend.bounds.max);
		}

		center = (minPoint + maxPoint)/2;
		return center;
	}
}
