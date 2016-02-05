using UnityEngine;
using System.Collections;
using HutongGames.PlayMaker;

public class VisFSM : VisClass {

	//public GameObject gameObjWithFSM;
	private PlayMakerFSM unitFSM; //конечный автомат, который управляет установкой

	private GameObject cameraRotation;
	//private GameObject cameraDistance;
	private GameObject cameraParallel;
	private MouseOrbit orbit;

	private float distance = 0.0f;
	public float Distance
	{
		get 
		{
			if (orbit != null)
				return orbit.Distance;
			else
				return 0.0f;
		}
		set 
		{
			distance = value;
			if (orbit != null)
				orbit.Distance = distance;
		}
	}

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

		unitFSM = VisObject.GetComponent<PlayMakerFSM> ();
		if (unitFSM == null)
			Debug.LogError ("Missing FSM");
	}

	public override void Update ()
	{
		base.Update ();
	}

	void OnLevelWasLoaded(int currentLevel)
	{
		if (currentLevel == 1) 
		{
			cameraParallel = GameObject.FindGameObjectWithTag ("MainCamera");
			if (cameraParallel != null) 
			{
				unitFSM.FsmVariables.GetFsmGameObject ("CameraParallel").Value = cameraParallel;
			}
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

	public float ClampAngle(float angle)
	{
		float arcsin = Mathf.Asin (Mathf.Sin (angle / 180.0f * Mathf.PI));
		angle = arcsin * 180.0f / Mathf.PI;
		return angle;
	}

	public void ClampCameraRotationFrom()
	{
		Vector3 cameraRotationFrom = unitFSM.FsmVariables.GetFsmVector3 ("CameraRotationFrom").Value;
		float x = cameraRotationFrom.x;
		float y = cameraRotationFrom.y;
		float z = cameraRotationFrom.z;
		x = ClampAngle (x);
		unitFSM.FsmVariables.GetFsmVector3 ("CameraRotationFrom").Value = new Vector3 (x, y, z);
	}

	/*public void SetDistanceFrom()
	{
		if (orbit != null)
			unitFSM.FsmVariables.GetFsmFloat ("DistanceFrom").Value = - orbit.cameraDistance.transform.localPosition.z;
	}

	public void SetDistance()
	{
		if (orbit != null)
			orbit.Distance = unitFSM.FsmVariables.GetFsmFloat ("Distance").Value;
	}

	public void ResetCameraParallel()
	{
		if (orbit != null)
			orbit.transform.localPosition = new Vector3 (0,0,0);
	}*/
}

