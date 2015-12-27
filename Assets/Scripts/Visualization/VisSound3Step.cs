using UnityEngine;
using System.Collections;

public enum UnitState {Idle, Starting, Working, Stopping};

public class VisSound3Step : VisClass {
	
	public UnitState unitState = UnitState.Idle; //состояние установки 

	public AudioSource soundStart;
	public AudioSource soundWork;
	public AudioSource soundStop;

	private float L1, L2, L3;

	private float t, t2;

	
	public override void StartVis()
	{
		base.StartVis();
		unitState = UnitState.Starting;
		soundStart.Play ();
		t = Time.time + L1; //когда должен запуститься звук работы
	}
	
	public override void StopVis()
	{
		base.StopVis();
		if (unitState == UnitState.Starting) //если мы только стартуем
		{ 
			//останавливаем стартование
			float pos = soundStart.time;
			float p = pos / L1;
			//Debug.Log("Pos: " + pos);
			soundStart.Stop();

			//запускаем остановку
			soundStop.time = L3 * (1.0f - p);
			//Debug.Log("Stop time: " + soundStop.time);

			t2 = Time.time + pos; 
		}
		else if (unitState == UnitState.Working)
		{
			t2 = Time.time + L3;
			soundWork.Stop();
		}
		soundStop.Play ();

		unitState = UnitState.Stopping;
	}
	
	public override void Start()
	{
		base.Start();
		L1 = soundStart.clip.length;
		L2 = soundWork.clip.length; 
		L3 = soundStop.clip.length; 
	}

	public override void Update()
	{
		if (unitState == UnitState.Starting &&
			Time.time > t)
		{
			unitState = UnitState.Working;
			soundStart.Stop();
			soundWork.Play();
		}
		if (unitState == UnitState.Stopping &&
		    Time.time > t2)
		{
			unitState = UnitState.Idle;
		}
	}
	//public void SetAudioSource()
	//{
	//	source = GetComponent<AudioSource>();
	//}
}
