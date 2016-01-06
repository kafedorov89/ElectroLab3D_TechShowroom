using UnityEngine;
using System.Collections;
using HutongGames.PlayMaker;

public class VisPM : VisClass {

	public GameObject gameObjWithFSM;
	private PlayMakerFSM unitFSM; //конечный автомат, который управляет установкой

	//public GameObject cameraRotation; 

	//public AudioSource soundStart;
	//public AudioSource soundWork;
	//public AudioSource soundStop;

	private GameObject cameraRotation;
	private MouseOrbit orbit;

	//private float L1, L2, L3;

	// Use this for initialization
	//void Start () {

	//}

	public override void StartVis()
	{
		base.StartVis();
		unitFSM.SendEvent ("Start");

	}

	public override void StopVis()
	{
		base.StopVis();
		unitFSM.SendEvent ("Stop");
	}

	public override void StopImmidiately()
	{
		unitFSM.SendEvent ("StopNow");
	}

	public override void Start()
	{
		base.Start();

		//L1 = soundStart.clip.length;
		//L2 = soundWork.clip.length; 
		//L3 = soundStop.clip.length; 

		unitFSM = gameObjWithFSM.GetComponent<PlayMakerFSM> ();
		if (unitFSM == null)
			Debug.LogError ("Missing FSM");


	}

	public override void Update ()
	{
		base.Update ();
		/*if (unitFSM.ActiveStateName.CompareTo ("Starting")==0)
			unitFSM.FsmVariables.GetFsmFloat ("pos").Value = soundStart.time;
		if (unitFSM.ActiveStateName.CompareTo ("Stopping")==0)
			unitFSM.FsmVariables.GetFsmFloat ("pos").Value = soundStop.time;*/
	}

	void OnLevelWasLoaded(int currentLevel)
	{
		if (currentLevel == 1) 
		{
			cameraRotation = GameObject.Find ("CameraRotation");
			if (cameraRotation != null) 
			{
				unitFSM.FsmVariables.GetFsmGameObject ("CameraRotation").Value = cameraRotation;
				orbit = cameraRotation.GetComponentInChildren<MouseOrbit> ();
			}
			else
				Debug.LogError ("Missing CameraRotation");
		}
	}
}

