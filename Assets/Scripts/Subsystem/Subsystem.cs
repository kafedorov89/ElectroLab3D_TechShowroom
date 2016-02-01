using UnityEngine;
using System.Collections;

[System.Serializable]
public class Subsystem /*: MonoBehaviour*/ {

	public string name;  //название подсистемы
	public string UID;  //уникальный идентификатор подсистемы
	public GameObject gameObject;  
	public float startCamDistance; //начальная дистанция до камеры при просмотре
	public Vector3 startCamRotation;  //начальный поворот камеры при просмотре

	[TextArea(3,10)]
	public string textAbout;  //описание подсистемы

	//TODO: сделать свойства, как полагается
	/*public string Name
	{
		get { return name; }
		set { name = value;}
	}*/

	public Subsystem()
	{
		name = "some subsystem";
		gameObject = null; //transform.gameObject;
		textAbout = "some description";
		startCamDistance = 5.0f;
		startCamRotation = new Vector3 (20,300,0); 
	}
}
