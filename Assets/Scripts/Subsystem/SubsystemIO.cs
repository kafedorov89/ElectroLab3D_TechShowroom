using UnityEngine;
using System.Collections;

//класс сериализации и десериализации полных данных о подсистеме
//(используется для бэкапирования важных настроек подсистемы,
//которые не хотелось бы случайно потерять)
[System.Serializable]
public class SubsystemIO {

	public string name;  //название подсистемы
	public string UID;   //уникальный идентификатор подистемы
	public string gameObject; 
	public float startCamDistance; //начальная дистанция до камеры при просмотре
	public Vector3 startCamRotation;  //начальный поворот камеры при просмотре
	public string textAbout;  //описание подсистемы

	public SubsystemIO ()
	{
		name = "";
		UID = "";
		gameObject = "";
		startCamDistance = 0.0f;
		startCamRotation = new Vector3();
		textAbout = "";
	}
	public SubsystemIO (Subsystem subsystem)
	{
		name = subsystem.name;
		UID = subsystem.UID;
		gameObject = subsystem.gameObject.ToString ();
		startCamDistance = subsystem.startCamDistance;
		startCamRotation = subsystem.startCamRotation;
		textAbout = subsystem.textAbout;
	}
}
