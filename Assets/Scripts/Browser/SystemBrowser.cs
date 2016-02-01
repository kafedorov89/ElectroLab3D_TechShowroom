using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public enum BrowsingMode {System, Subsystem };
public enum ParamInitializationMode {Auto, Manual};

public enum BrowserState {System = 0, ReduceAlpha, ZoomInSubsystem, Subsystem, ZoomOutSubsystem, IncreaseAlpha,
	PlayAnimation};

public enum RenderingMode {Opaque = 0, Cutout = 1, Fade = 2, Transparent = 3};
public enum FadeMode {Continuously, Fast};

public struct MeshPack
{
	public MeshRenderer[] meshes;
}

public class SystemBrowser : MonoBehaviour {

	public BrowsingMode playAnimationMode = BrowsingMode.System;
	public GameObject playAnimationSubsystem;

	private bool taskToPlayAnimationInSystem = false;
	private bool taskToPlayAnimationInSubsystem = false;

	//starting transform of system
	public ParamInitializationMode startCamPositionMode = ParamInitializationMode.Auto;

	public Vector3 startCamPosition = new Vector3(0, 0, 0);
	public Vector3 startCamRotation = new Vector3(0, 0, 0);
	public float startCamDistance = 10.0f;

	GameObject GO; //gameobject of main system
	MeshRenderer[] meshes; //all meshes of 3D model

	MeshPack[] meshesALL; //меши подсистем
	List<MeshRenderer> meshesTrash; //меши мусора (объекты, не входящие ни в одну подсистему)

	private SubsystemList subs; //subsystems
	public SubsystemList Subs
	{
		get { return subs; }
	}
	MouseOrbit orbitNav;  //orbit navigation
	Camera mainCam; //main camera
	GameObject cameraHelper;  //camera looks on it
	BrowserGUI bGUI;  //GUI script

	Vector3 mainPos; //center position of main gameobject

	float alphaMin = 0.0f; //alpha-cannel when subsystem browsing
	float alphaMax = 1.0f; //alpha-cannel when system browsing

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
	private float lastDown0 = 0.0f; //last moment when mouse button down
	private float lastDown1 = 0.0f; //last moment when mouse button down

	private int current_subs_index = -1; //индекс текущей выбранной подсистемы
	public int CurrentSubsystemIndex
	{
		get { return current_subs_index; }
	}

	private Vector3 subFrom, subTo;
	private Vector3 parallelFrom = new Vector3(), parallelTo = new Vector3();
	private float distFrom, distTo;

	//при приближении к подсистеме - вращаем камеру в заданное положение
	//при отдалении - ничего не делаем
	private Quaternion camRotateFrom;
	private Quaternion camRotateTo;

	private float startTime; //start time for some process (alpha changing/camera moving)
	private float subJourneyLength;
	private float sysJourneyLength;
	private int stored_index = -1; 
	public int StoredIndex
	{
		get { return stored_index; }
		set { stored_index = value; }
	}
	private bool isReady = true; //is ready to hadle command from GUI
	public bool IsReady
	{
		get { return isReady; }
	}
	private float m_alpha;
	private BrowserState state = BrowserState.System;
	public BrowserState State
	{
		get { return state; }
	}

	//for raycasting
	private Ray ray;   
	private RaycastHit hit;

	//Dictionary<Material,Material> materialPairs = new Dictionary<Material, Material>();

	// Use this for initialization
	void Start () 
	{
		GameObject canvas = GameObject.FindWithTag ("Player");
		bGUI = canvas.GetComponent<BrowserGUI> (); 
		subs = GetComponent<SubsystemList> (); //list of subsystems
		MarkSubsystemsWithTag();

		GO = gameObject;
		GO.transform.position = new Vector3 (0,0,0);

		CreateCameraHelper(); //create object for camera targeting

		GameObject mainCamObj = GameObject.FindWithTag ("MainCamera"); 
		mainCam = mainCamObj.GetComponent<Camera>();
		orbitNav = mainCam.GetComponent<MouseOrbit>();
		orbitNav.SetTarget (cameraHelper.transform); //напрявляем на помощника

		BuildMeshes (); //search meshes and join it into groups
		//PrepareForAlphaBlending (); //prepare materials
		m_alpha = alphaMax; //initial alpha

		//initial camera rotation and distance
		orbitNav.SetRotation(startCamRotation);
		orbitNav.Distance = startCamDistance;
	}

	//пометить gameobject-ы тегами, чтобы их было лечге искать
	void MarkSubsystemsWithTag()
	{
		foreach (Subsystem subsystem in subs.list)
		{
			subsystem.gameObject.tag = "Subsystem";
		}
	}

	private GameObject FindGameObjectWithTagInParents(GameObject start, string tag)
	{
		Transform[] parents = start.GetComponentsInParent<Transform>();
		foreach (Transform parent in parents) 
		{
			if (parent.gameObject.CompareTag (tag))
				return parent.gameObject;
		}
		return null;
	}

	void CreateCameraHelper()
	{
		//создаем помощника - именно на него будет направлена камера
		//в течение всего времени просмотра установки;
		//куда будет двигаться просмотрщик - туда и камера;
		cameraHelper = new GameObject("CameraHelper");

		//начальное положение помощника может быть рассчитано 
		//автоматически - как центр общий мешей, либо задано пользователем
		if (startCamPositionMode == ParamInitializationMode.Auto)
			mainPos = CalcCenterOfGameObject2 (GO);
		else
			mainPos = startCamPosition;

		//initial camera position = position of target object
		cameraHelper.transform.position = mainPos;
	}

	void BuildMeshes()
	{
		//суть в том, что надо собрать группы мешей, которые относятся к каждой подсистеме;
		//все остальные меши надо объеденить в отдельную группу
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
			//SubsystemFlag flag = m.gameObject.GetComponentInParent<SubsystemFlag>();
			GameObject flag = FindGameObjectWithTagInParents(m.gameObject, "Subsystem");
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
		if (state == BrowserState.System) 
		{
			if (stored_index != -1)
				GoToSubsystem (stored_index);
			else 
			{
				if (taskToPlayAnimationInSystem)
				{
					taskToPlayAnimationInSystem = false;
					bGUI.PlayAnimation ();
				}
				isReady = true;
			}
		}
		if (state == BrowserState.ReduceAlpha) 
			ReduceAlpha (); 
		if (state == BrowserState.ZoomInSubsystem) 
			ZoomInSub();
		if (state == BrowserState.ZoomOutSubsystem) 
			ZoomOutSub();
		if (state == BrowserState.IncreaseAlpha) 
			IncreaseAlpha();

		//raycasting - left or right click on subsystem
		UpdateRaycasting ();
	}

	void UpdateRaycasting()
	{
		if (state != BrowserState.System)
			return;
		if (EventSystem.current.IsPointerOverGameObject ()) //защита от клика сквозь интерфейс
			return;

		ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		bool isHit = Physics.Raycast (ray, out hit, 100.0f);
		if (isHit) 
		{

			GameObject obj = hit.collider.gameObject;
			//SubsystemFlag flag = obj.GetComponentInParent<SubsystemFlag>();
			GameObject flag = FindGameObjectWithTagInParents(obj, "Subsystem");
			if (flag != null)
			{
				Subsystem subsystem = GetSubsystemByGameObject (flag);
				bGUI.textSubsystemName.text = subsystem.name;
				if (Input.GetMouseButtonUp(0)) //left mouse click
				{
					float deltaTime = Time.time - lastDown0;
					if (deltaTime < catchTime)
					{
						int a = GetSubsystemIndex(subsystem.gameObject);
						bGUI.ChooseSubsystem(a); //go to selected subsystem
					}
				}
				if (Input.GetMouseButtonUp(1)) //right mouse click
				{
					float deltaTime = Time.time - lastDown1;
					if (deltaTime < catchTime)
					{
						int a = GetSubsystemIndex(subsystem.gameObject);
						bGUI.HideSubsystem(a); //hide selected subsystem
					}
				}
			}
		}
		else
			bGUI.textSubsystemName.text = "";

	}

	//===============================================================================
	// Prepare standart shader for transparency
	//===============================================================================
	/*void PrepareForAlphaBlending()
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
	}*/

	public void EnableTransparency()
	{
		foreach (MeshRenderer rend in meshes)
		{
			foreach (Material material in rend.materials)
			{
				material.SetFloat ("_Mode", (float)RenderingMode.Transparent); //fade or transparent
				material.SetInt ("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
				material.SetInt ("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
				//material.SetInt ("_ZWrite", 0);
				material.DisableKeyword ("_ALPHATEST_ON");
				material.EnableKeyword ("_ALPHABLEND_ON");
				material.DisableKeyword ("_ALPHAPREMULTIPLY_ON");
				material.renderQueue = 3000;

				//AlphaBlend.SetupMaterialWithBlendMode (material, RenderingMode.Fade);
			}
		}
	}

	public Subsystem GetSubsystemByGameObject(GameObject obj)
	{
		foreach (Subsystem subsytem in subs.list)
		{
			if (subsytem.gameObject == obj)
				return subsytem;
		}

		return null;
	}

	public void DisableTransparency()
	{
		if (meshes == null)
			return;
		foreach (MeshRenderer rend in meshes)
		{
			foreach (Material material in rend.materials)
			{
				/*
				//material.SetFloat ("_Mode", 1.0f);
				material.SetFloat ("_Mode", (float)RenderingMode.Opaque); //fade or transparent
				//material.SetInt ("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
				//material.SetInt ("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
				//material.SetInt ("_ZWrite", 1);
				material.EnableKeyword ("_ALPHATEST_ON");
				material.DisableKeyword ("_ALPHABLEND_ON");
				material.EnableKeyword ("_ALPHAPREMULTIPLY_ON");
				material.renderQueue = 2000;
				*/

				/*
				material.SetInt ("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
				material.SetInt ("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
				material.SetInt ("_ZWrite", 1);
				material.DisableKeyword ("_ALPHATEST_ON");
				material.DisableKeyword ("_ALPHABLEND_ON");
				material.DisableKeyword ("_ALPHAPREMULTIPLY_ON");
				material.renderQueue = -1;
				*/

				AlphaBlend.SetupMaterialWithBlendMode (material, RenderingMode.Opaque);
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
						SetAlphaForMaterial (material, a);
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
				SetAlphaForMaterial (material, a);
			}
		}
	}

	public void SetAlphaForMaterial (Material material, float a)
	{
		//для нестандартных шейдеров
		//(ShaderForge, специальный узел - node_op - контролирует параметр opacity)
		if (material.HasProperty("_node_op"))
			material.SetFloat("_node_op", a);

		//для стандартного шейдера
		SetColorMaterialAlpha(material, "_Color", a);
	}

	//TODO: fix increadible code duplicate !!!
	void Move(GameObject obj, float start_time, float shift_time, Vector3 from, Vector3 to)
	{
		if (from == to)
			return;
		
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
		if (from == to)
			return;

		float journeyLength = Mathf.Abs (to - from);
		float speed = journeyLength / shift_time;
		float distCovered = (Time.time - start_time) * speed;
		float fractJourney = distCovered / journeyLength;
		float dist = Mathf.Lerp (from, to, fractJourney);
		
		orb.Distance = dist;
		//orb.cameraDistance.transform.localPosition = new Vector3(orb.cameraDistance.transform.localPosition.x, 
		 //                                                   orb.cameraDistance.transform.localPosition.y, 
		 //                                                   -dist);
	}
		

	void ChangeAlpha(GameObject obj, float start_time, float shift_time, float from, float to)
	{
		if (from == to)
			return;
		
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

	// Go to subsystem browsing with check current situation ====================================
	public void GoToSubsystemWithCheck(int subs_index)
	{
		if (isReady == false) return;
		//if index not valid OR this is current zoomed subsystem
		if (subs_index == -1 || subs_index == current_subs_index) return;
		if (Subs.list[subs_index].gameObject == null) return;

		if (current_subs_index == -1)
		{
			GoToSubsystem (subs_index);
		}
		else
		{
			stored_index = subs_index;
			GoToSystem();
		}
	}

	//===============================================================================
	//Is inactive subsystems 
	//===============================================================================
	public bool IsHiddenSubsystems()
	{
		foreach (Subsystem sub in Subs.list)
		{
			if (sub.gameObject.activeInHierarchy == false)
				return true;
		}
		return false;
	}

	//===============================================================================
	//
	//===============================================================================
	public void HideSubsystem(int index)
	{
		Subs.list [index].gameObject.SetActive (false);
	}

	//===============================================================================
	// Go to system browsing
	//===============================================================================
	public void GoToSystem()
	{
		if (isReady == false) //browser is busy
			return;
		if (current_subs_index == -1) //no zoomed subs
			return;

		isReady = false;
		FixTime ();

		//switch point from and point to, start distance and finish distance
		SwithVector3Value (ref subTo, ref subFrom);
		//SwithVector3Value (ref parallelTo, ref parallelFrom);

		parallelFrom = orbitNav.gameObject.transform.localPosition;
		parallelTo = new Vector3 (0, 0, 0);

		distFrom = orbitNav.distance;
		distTo = startCamDistance; //Mathf.Min(orbitNav.distanceMax, orbitNav.distance + zoomIncrement);

		state = BrowserState.ZoomOutSubsystem; //next state
	}

	//===============================================================================
	// Go to subsystem browsing; NOT FOR EXTERNAL USING!
	//===============================================================================
	public void GoToSubsystem(int subs_index)
	{
		if (IsHiddenSubsystems ())
			Subs.list [subs_index].gameObject.SetActive (true);

		isReady = false;
		state = BrowserState.ReduceAlpha;
		EnableTransparency ();
		FixTime ();
		current_subs_index = subs_index;
		if (stored_index != -1) //clear memory about task
			stored_index = -1;
	}

	//===============================================================================
	//Reduce alpha-channel of whole system
	//===============================================================================
	void ReduceAlpha()
	{
		if (m_alpha != alphaMin)
		{
			if (fadeMode == FadeMode.Continuously)
				ChangeAlpha (GO, startTime, shiftAlphaTime, alphaMax, alphaMin);
			else
			{
				m_alpha = alphaMin;
				//SetAllMeshesVisibility(false); //кроме выбранной
			}
		}
		else
		{
			PrepareForZoomIn ();
		}
	}

	//===============================================================================
	//
	//===============================================================================
	void PrepareForZoomIn()
	{
		state = BrowserState.ZoomInSubsystem; //next state
		SetAllMeshesVisibility(false); //кроме выбранной //!!!
		DisableTransparency();
		FixTime();

		subFrom = mainPos;
		subTo = CalcCenterOfGameObject2(Subs.list[current_subs_index].gameObject); 

		//панорамирование нужно будет сбросить
		parallelFrom = orbitNav.transform.localPosition;
		parallelTo = new Vector3(0,0,0);

		distFrom = orbitNav.distance;
		//distTo = Mathf.Max(orbitNav.distanceMin, orbitNav.distance - zoomIncrement);
		distTo = Mathf.Max(orbitNav.distanceMin, Subs.list[current_subs_index].startCamDistance);

		camRotateFrom = orbitNav.transform.rotation;
		camRotateTo = Quaternion.Euler (Subs.list [current_subs_index].startCamRotation);
	}

	//===============================================================================
	//Increase alpha-channel of whole system 
	//===============================================================================
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
			state = BrowserState.System; //next state
			current_subs_index = -1;
			DisableTransparency ();
		}
	}

	//===============================================================================
	//Move camera closer to selected subsystem
	//===============================================================================
	void ZoomInSub()
	{
		bool IsEnd = false;

		if (Time.time > startTime + shiftZoomTime) //если время кончилось
		{
			IsEnd = true;
			cameraHelper.transform.position = subTo;
			orbitNav.gameObject.transform.localPosition = parallelTo;
			orbitNav.cameraRotation.transform.localRotation = camRotateTo;
			orbitNav.Distance = distTo;
		}
		else
		{
			if ((cameraHelper.transform.position != subTo) ||
			    (orbitNav.gameObject.transform.localPosition != parallelTo) ||
			    (orbitNav.cameraRotation.transform.localRotation != camRotateTo) ||
			    (orbitNav.Distance != distTo)) {
				//Move(cameraHelper, startTime, shiftZoomTime, subFrom, subTo);
				//MoveLocal(orbitNav.gameObject, startTime, shiftZoomTime, parallelFrom, parallelTo);
				//ChangeDist(orbitNav, startTime, shiftZoomTime, distFrom, distTo);

				float t = (Time.time - startTime) / shiftZoomTime;
				cameraHelper.transform.position = Vector3.Lerp (subFrom, subTo, t);
				orbitNav.gameObject.transform.localPosition = Vector3.Lerp (parallelFrom, parallelTo, t);
				orbitNav.cameraRotation.transform.localRotation = Quaternion.Lerp (camRotateFrom, camRotateTo, t);
				orbitNav.Distance = Mathf.Lerp (distFrom, distTo, t);
			}
			else
				IsEnd = true;
		}
			
		if (IsEnd)
		{
			state = BrowserState.Subsystem; //next state
			isReady = true;
			bGUI.ReceiveEventSubsystem ();
			if (taskToPlayAnimationInSubsystem)
			{
				taskToPlayAnimationInSubsystem = false;
				bGUI.PlayAnimation ();
			}
		}
	}

	//===============================================================================
	//Move camera further from selected subsystem
	//===============================================================================
	void ZoomOutSub()
	{
		bool IsEnd = false;

		if (Time.time > startTime + shiftZoomTime) //если время кончилось
		{
			IsEnd = true;

			//ставим все на конечные позиции
			cameraHelper.transform.position = subTo;
			orbitNav.gameObject.transform.localPosition = parallelTo;
			orbitNav.Distance = distTo;
		}
		else
		{
			if ((cameraHelper.transform.position != subTo) ||
				(orbitNav.gameObject.transform.localPosition != parallelTo) ||
				(orbitNav.Distance != distTo))
			{
				//Move(cameraHelper, startTime, shiftZoomTime, subFrom, subTo);
				//MoveLocal(orbitNav.gameObject, startTime, shiftZoomTime, parallelFrom, parallelTo);
				//ChangeDist(orbitNav, startTime, shiftZoomTime, distFrom, distTo);

				float t = (Time.time - startTime) / shiftZoomTime;
				cameraHelper.transform.position = Vector3.Lerp(subFrom, subTo, t);
				orbitNav.gameObject.transform.localPosition = Vector3.Lerp (parallelFrom, parallelTo, t);
				orbitNav.Distance = Mathf.Lerp (distFrom, distTo, t);
			}
			else 
			{
				IsEnd = true;
			}
		}

		if (IsEnd)
			PrepareForIncreaseAlpha ();
	}
	//===============================================================================
	//
	//===============================================================================
	void PrepareForIncreaseAlpha()
	{
		FixTime();
		EnableTransparency ();
		state = BrowserState.IncreaseAlpha; //next state
		SetAllMeshesVisibility(true); //кроме выбранной //!!!
	}

	//===============================================================================
	//Save current time as start time of next process (animation)
	//===============================================================================
	void FixTime()
	{
		startTime = Time.time;
	}

	//===============================================================================
	//float a <=> b
	//===============================================================================
	void SwithFloatValue(ref float a, ref float b)
	{
		float temp;
		temp = a;
		a = b;
		b = temp;
	}

	//===============================================================================
	//Vector3 a <=> b
	//===============================================================================
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

	//===============================================================================
	//
	//===============================================================================
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

	//===============================================================================
	// Set object visible or invisible
	//===============================================================================
	void SetVisibility(GameObject obj, bool isVisible)
	{
		MeshRenderer[] meshes = obj.GetComponentsInChildren<MeshRenderer>();
		foreach (MeshRenderer rend in meshes)
		{
			rend.enabled = isVisible;
		}
	}

	//===============================================================================
	// Mass center for all meshes
	//===============================================================================
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

	//===============================================================================
	// Total bounding box for all meshes
	//===============================================================================
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
	public void GiveTaskToPlayAnimation(BrowsingMode mode)
	{
		if (mode == BrowsingMode.System)
			taskToPlayAnimationInSystem = true;
		else
			taskToPlayAnimationInSubsystem = true;
	}

	public int GetIndexOfSubsystem(GameObject subsystemGameObject)
	{
		for (int i = 0; i < Subs.list.Count; ++i)
		{
			if (Subs.list [i].gameObject == subsystemGameObject)
				return i;
		}
		return -1;
	}
	public Subsystem GetSelectedSubsystem()
	{
		if (current_subs_index == -1)
			return null;
		else
			return Subs.list [current_subs_index];
	}
}
