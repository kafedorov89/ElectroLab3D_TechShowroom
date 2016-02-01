using UnityEngine;
using System.Collections;

public class VisAngular : VisClass {
	
	//AudioSource source;
	Rigidbody body;
	// Use this for initialization
	//void Start () {
	
	//}
	
	
	
	public override void StartVis()
	{
		base.StartVis();
		//SetRotation();
		//source.Play ();
		
	}
	
	public override void StopVis()
	{
		base.StopVis();
		//source.Pause ();
		//source.Stop ();
		
	}
	
	public override void Start()
	{
		base.Start();
		//SetAudioSource();
		body = VisObject.GetComponent<Rigidbody> ();
		body.angularVelocity = new Vector3 (50.0f, 0.0f, 0.0f);
	}
	public void SetAudioSource()
	{
		//source = GetComponent<AudioSource>();
	}
}

