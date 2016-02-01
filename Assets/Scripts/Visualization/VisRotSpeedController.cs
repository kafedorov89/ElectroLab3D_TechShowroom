using UnityEngine;
using System.Collections;

//public enum UnitState {Idle, Starting, Working, Stopping};

public class VisRotSpeedController : VisClass {
	
	public UnitState unitState = UnitState.Idle; //состояние установки 
	
	//public AudioSource soundStart;
	//public AudioSource soundWork;
	//public AudioSource soundStop; //звук должен быть полной копией старта, но только с реверсом
	//public GameObject[] Objs;
	private VisRotation Rot;
	//public VisSound3Step Snd;

	public float L1 =  40.0f; 
	public float L2 = 15.0f;
	public float L3 = 40.0f;

	public float V1 = 0.0f; //стартовая скорость вращения
	public float V2 = 50.0f; //рабочая скорость вращения

	private float t = 0.0f; //момент перехода Idle->Start
	private float t2 = 0.0f; //момент перехода Stop->Idle
	
	private float q = 0.0f; //фиксация времени (для интерполяции) - начало очередного этапа
	private float dt = 0.0f; //сколько нужно для анимации времени

	private float v_from = 0.0f;
	private float v_to = 0.0f;
	//(стартование, остановка)

	public override void StartVis()
	{
		base.StartVis();
		unitState = UnitState.Starting;
		//soundStart.Play ();
		//foreach (VisRotation Rot in Rots)
		Rot.Speed = V1;

		t = Time.time + L1; //когда должен запуститься звук работы
		q = Time.time;
		dt = L1;

		v_from = V1;
		v_to = V2;
	}
	
	public override void StopVis()
	{
		base.StopVis();
		if (unitState == UnitState.Starting) //если мы только стартуем
		{ 
			//останавливаем стартование
			float pos = Time.time - q;
			dt = pos;
			//float p = pos / L1;
			//Debug.Log("Pos: " + pos);
			//soundStart.Stop();
			
			//запускаем остановку
			//soundStop.time = L3 * (1.0f - p);
			//Debug.Log("Stop time: " + soundStop.time);
			
			t2 = Time.time + pos; 
		}
		else if (unitState == UnitState.Working)
		{
			t2 = Time.time + L3;
			dt = L3;
			//soundWork.Stop();
		}
		unitState = UnitState.Stopping;
		q = Time.time;
		v_from = Rot.Speed; 
		v_to = V1;
	}
	
	public override void Start()
	{
		base.Start();
		Rot = VisObject.GetComponent<VisRotation>();
	}

	public override void Update()
	{
		//обновление состояний
		if (unitState == UnitState.Starting && Time.time > t)
		{
			unitState = UnitState.Working;
		}
		if (unitState == UnitState.Stopping && Time.time > t2)
		{
			unitState = UnitState.Idle;
		}

		//обновление скорости
		if (unitState == UnitState.Starting)
		{
			//увеличиваем скорость от 0 до MAX
			//foreach (VisRotation Rot in Rots)
			Rot.Speed = CorrectTimeLerp(q, dt, v_from, v_to);
			//q - момент начала для лерпа
			//dt - сколько нужно времени, чтобы завершить лерп
		}
		else if (unitState == UnitState.Stopping)
		{
			//уменьшаем скорость от ТЕКУЩЕЙ до 0
			//foreach (VisRotation Rot in Rots)
			Rot.Speed = CorrectTimeLerp(q, dt, v_from, v_to);
		}
	}

	public float CorrectTimeLerp(float start_time, float dt, float from, float to)
	{
		float journeyLength = Mathf.Abs (to - from);
		float speed = journeyLength / dt;
		float distCovered = (Time.time - start_time) * speed;
		float fractJourney = distCovered / journeyLength;
		return Mathf.Lerp (from, to, fractJourney);
	}
}
