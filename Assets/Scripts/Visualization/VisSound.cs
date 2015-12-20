using UnityEngine;
using System.Collections;

public class VisSound : VisClass {

	AudioSource source;
    
    // Use this for initialization
	//void Start () {
	
	//}
	


	public override void StartVis()
	{
		base.StartVis();
		//SetRotation();
		source.Play ();

	}
	
	public override void StopVis()
	{
		base.StopVis();
		//source.Pause ();
		source.Stop ();

	}

	public override void Start()
	{
		base.Start();
		SetAudioSource();
	}
	public void SetAudioSource()
	{
		source = GetComponent<AudioSource>();
	}
}
