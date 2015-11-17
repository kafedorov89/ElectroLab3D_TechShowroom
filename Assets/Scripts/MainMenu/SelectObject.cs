using UnityEngine;
using System.Collections;

public class SelectObject : MonoBehaviour {
	
	public float maxDistance = 100; //dont touch too far objects
	public GameObject[] targets; //objects we want to select with mouse
	public float scaleFactor = 1.2f; //how much scale up when object selected

	private Vector3[] scales;   //initial scale of each target
	private Ray ray;   
	private RaycastHit hit; 	

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
		for (int i = 0; i < targets.Length; ++i)
		{
			GameObject obj = targets[i];  //get current object
			//obj.transform.localScale = scales[i];  //set origin scale

			//detect collision between ray and gameobject collider
			ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			if (Physics.Raycast(ray, out hit, maxDistance) && hit.collider.gameObject == obj)
			{
				//set increased scale
				//obj.transform.localScale = scaleFactor * scales[i];

				//mouse click on object => Load Browser level 
				if (Input.GetMouseButtonDown(0))
				{
					GameObject ctrlObject = GameObject.FindWithTag("GameController");
					GameControl ctrlComponent = ctrlObject.GetComponent<GameControl>();
					obj.transform.SetParent(ctrlObject.transform);
					ctrlComponent.SetTarget(obj);
					Application.LoadLevel(1);  
				}
			}
		}
	}
}





