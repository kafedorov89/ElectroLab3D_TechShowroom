using UnityEngine;
using System.Collections;

public class SubsystemFlag : MonoBehaviour {

	public string subsystemName = "Subsystem";

	public float startCamDistance = 5.0f;

	public Vector3 startCamRotation = new Vector3(20,300,0);

	[TextArea(3,10)]
	public string textAbout = "Text about";

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
