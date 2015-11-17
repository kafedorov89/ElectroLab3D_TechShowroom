using UnityEngine;
using System.Collections;

/* This is 3-in-1 script. Using for 
   - automatical circle layout for group of objects (more than 1)
   - automatical objects rotation during the game
   - manual objects rotation by mouse input events 
*/

public enum RotationDir {Right, Left};

public class RotationMaster : MonoBehaviour {


	public GameObject[] targets; //target group of objects
	public Vector3 center;  //rotation center
	public Vector3 axis; //rotation axis
	public float radius;  //layout distance from object to center
	public float startAngle = 0.0f;  //layout start angle 
	public float rotationSpeed = 5.0f; //auto rotation speed
	public float manualRotation = 5.0f;  //power of manual rotation
	public RotationDir rotationDir = RotationDir.Right;
	


	// Use this for initialization
	void Start ()
	{

	}
	
	// Update is called once per frame
	void Update () 
	{
		if (targets == null)
			return;
		int N = targets.Length;
		if (N == 0)
			return;
		//GameObject target;

		ManualRotation ();
		AutoRotation ();
	}

	//Perform manual rotation
	public void ManualRotation()
	{
		int N = targets.Length;
		if(Input.GetMouseButton(0)) //left mouse button click
		{
			float x = Input.GetAxis("Mouse X");
			RotateGroup(-x * manualRotation);
		}
	}

	//Perform auto rotation
	public void AutoRotation()
	{
		int N = targets.Length;
		int dir = (rotationDir == RotationDir.Right ? 1 : -1);
		if (N == 1)
		{
			//targets[0].transform.position = center;
		}
		else
		{
			RotateGroup((float)dir * rotationSpeed * Time.deltaTime);
		}
	}

	//Rotate group of objects
	private void RotateGroup(float angle)
	{
		GameObject target;
		int N = targets.Length;
		for (int i = 0; i < N; ++i) 
		{
			target = targets [i];
			target.transform.RotateAround(center, axis, angle); //rotate in global coords
			target.transform.rotation = Quaternion.identity; //reset local rotation
		}
	}

	//Lay out objects in a circle
	public void DoCircleLayout()
	{
		if (targets == null)
			return;
		int N = targets.Length;
		if (N == 0)
			return;

		float x, y, z, angle;
		float da = 360.0f / (float)N;
		GameObject target;

		if (N == 1)
		{
			targets[0].transform.position = center;
		}
		else
		{
			for (int i = 0; i < N; ++i) 
			{
				target = targets [i];
				angle = startAngle + (float)i * da;
				x = radius * Mathf.Cos (angle/180.0f*Mathf.PI);
				y = target.transform.position.y;
				z = radius * Mathf.Sin (angle/180.0f*Mathf.PI);
				target.transform.position = center + new Vector3 (x, y, z);
			}
		}
	}
}






