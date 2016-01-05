using UnityEngine;
using System.Collections;

public class VisGTU : MonoBehaviour {

	//Список эффектов
	//1. поток воздуха - анимированная текстура
	public GameObject VisStreamObject;

	//2. вращение лопаток
	public GameObject VisRotationObject1;
	public GameObject VisRotationObject2;

	//3. звук турбины
	public AudioSource soundStart;
	public AudioSource soundWork;
	public AudioSource soundStop; //звук должен быть полной копией старта, но только с реверсом

	private float L1, L2, L3; //длина звуков

	private float t = 0.0f; //момент перехода Start->Work
	private float t2 = 0.0f; //момент перехода Stop->Idle

	//4. подмена закрытой установки на открытую
	public GameObject VisShowObject;
	public GameObject VisHideObject;

	//5. огонь в форсунках
	//public GameObject 

	public UnitState unitState = UnitState.Idle; 

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
