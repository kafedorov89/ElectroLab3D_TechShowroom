using UnityEngine;
using System.Collections;

public class SelectObject : MonoBehaviour {
	
	public float maxDistance = 100; //dont touch too far objects
	public GameObject[] targets; //objects we want to select with mouse
	public float scaleFactor = 1.2f; //how much scale up when object selected

	private Vector3[] scales;   //initial scale of each target
	private Ray ray;   
	private RaycastHit hit; 	
	
	public float catchTime = 0.25f; //maximum time to catch object
	private float lastDown = 0.0f; //when mouse button down

	// Use this for initialization
	void Start () 
	{
		//SaveInitialScale();
	}
	
	//Save initial scale of all target objects
	void SaveInitialScale()
	{
		scales = new Vector3[targets.Length];
		for(int i = 0; i < targets.Length; ++i)
		{
			scales[i] = targets[i].transform.localScale;
		}
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (Input.GetMouseButtonDown (0)) 
		{
			lastDown = Time.time;
		}

		ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		if (Physics.Raycast(ray, out hit, maxDistance))
		{
			for (int i = 0; i < targets.Length; ++i)
			{
				GameObject obj = targets[i];  //get current object
				if (hit.collider.gameObject == obj)
				{
					//mouse click on object => Load Browser level 
					if (Input.GetMouseButtonUp(0))
					{
						float deltaTime = Time.time - lastDown;
						//print(deltaTime);
						if (deltaTime < catchTime)
						{
							GameObject ctrlObject = GameObject.FindWithTag("GameController");
							GameControl ctrlComponent = ctrlObject.GetComponent<GameControl>();
							obj.transform.SetParent(ctrlObject.transform);
							ctrlComponent.SetTarget(obj);
							Application.LoadLevel(1);  
						}
					}
					break;
				}
			}	
		}
	}
}





