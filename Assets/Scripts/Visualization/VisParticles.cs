using UnityEngine;
using System.Collections;
using ParticlePlayground;

public class VisParticles : VisClass {

	//объект, у которого в детях можно сколько угодно систем частиц
	//при активации объекта - мы видим частицы, как они движутся
	//при деактивации объекта - мы перестаем видеть частицы
	public GameObject ParticleGameObject;

	//то есть все управление частицами заключается
	//в активации и деактивации оъекта, на который они навешаны

	//public bool initialState = false; 
	//private bool state = false;
	

	// Update is called once per frame
	void Update () {
	
	}

	public override void StartVis()
	{
		base.StartVis();
		ParticleGameObject.SetActive (true);
	}
	
	public override void StopVis()
	{
		base.StopVis();
		ParticleGameObject.SetActive (false);
	}
	
	public override void Start()
	{
		base.Start();
		//SetAudioSource();
		ParticleGameObject.SetActive (false);
	}
	public void SwitchState()
	{
		//state = !state;
	}
	public void SetState()
	{
		//ParticleGameObject.SetActive (state);
	}
}
