using UnityEngine;
using System.Collections;

public enum UnitState {Idle, Starting, Working, Stopping};

public class VisSound3Step : VisClass {
	
	public UnitState unitState = UnitState.Idle; //состояние установки 

	public AudioSource soundStart;
	public AudioSource soundWork;
	public AudioSource soundStop; //звук должен быть полной копией старта, но только с реверсом

	private float L1, L2, L3;

	private float t = 0.0f; //момент перехода Start->Work
	private float t2 = 0.0f; //момент перехода Stop->Idle

	
	public override void StartVis()
	{
		if (unitState == UnitState.Idle) //Idle => Starting
		{
			base.StartVis();
			unitState = UnitState.Starting;
			soundStart.Play ();
			t = Time.time + L1; //когда должен запуститься звук работы
		}
		else if (unitState == UnitState.Stopping)
		{
			base.StartVis();
			unitState = UnitState.Starting;

			//останавливаем стартование
			float pos = soundStop.time;
			float p = pos / L3;
			soundStop.Stop();

			//запускаем остановку
			soundStart.time = L1 * (1.0f - p);
			soundStart.Play ();
			//Debug.Log("Stop time: " + soundStop.time);

			t = Time.time + pos;

		}
	}
	
	public override void StopVis()
	{
		if (unitState == UnitState.Idle || unitState == UnitState.Stopping)
		{
			return;
		}
		else
		{
			if (unitState == UnitState.Starting) //если мы только стартуем
			{ 
				//останавливаем стартование
				float pos = soundStart.time;
				float p = pos / L1;
				soundStart.Stop();

				//запускаем остановку
				soundStop.time = L3 * (1.0f - p);
				//Debug.Log("Stop time: " + soundStop.time);

				t2 = Time.time + pos; 
			}
			if (unitState == UnitState.Working) //если работаем
			{
				t2 = Time.time + L3;
				soundWork.Stop();
			}
			soundStop.Play ();
			unitState = UnitState.Stopping;
		}
	}

	public override void StopImmidiately()
	{
		unitState = UnitState.Idle;
		soundStart.Stop ();
		soundWork.Stop ();
		soundStop.Stop ();
		t = t2 = 0.0f;
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
		if (unitState == UnitState.Idle)
		{
			return;
		}
		if (unitState == UnitState.Starting && Time.time > t)
		{
			unitState = UnitState.Working;
			soundStart.Stop();
			soundWork.time = 5.0f;
			soundWork.Play();
		}
		if (unitState == UnitState.Stopping && Time.time > t2)
		{
			unitState = UnitState.Idle;
			base.StopVis();
		}
		if (unitState == UnitState.Working)
		{
			if (soundWork.time > 10.0f)
				soundWork.time = 5.0f;
		}
	}
}
