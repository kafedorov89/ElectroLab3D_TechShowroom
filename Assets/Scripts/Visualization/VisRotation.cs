using UnityEngine;
using System.Collections;

public class VisRotation : VisClass {

    private Vector3 RotationVector;
    public float Speed;
    public bool RotationX;
    public bool RotationY;
    public bool RotationZ;
    public bool PositiveRotation;
    //public float AbsDeltaEngle;
    //private float DeltaEngle;

    public float EndAngleValue;
    public bool RotateToAngle;

    public void RotationProcess()
    {
        if (VisStarted)
        {
			VisObject.transform.Rotate(RotationVector, Speed * Time.fixedDeltaTime);

			//Quaternion.Slerp();
			//VisObject.transform.RotateAround(new Vector3(1.0f, 0.0f, 0.0f), );
			//VisObject.transform.localRotation.x += Speed * Time.deltaTime;
			//Debug.Log(VisObject.transform.eulerAngles.x);


        }
    }

    public override void StartVis()
    {
        base.StartVis();
        SetRotation();
        //VisObject.transform.Rotate(RotationVector, Speed * Time.deltaTime);
    }

    public override void StopVis()
    {
        base.StopVis();
        //VisObject.transform.Rotate(RotationVector, 0.0f);
    }

    void SetRotation()
    {
        RotationVector = new Vector3();
        
        if (RotationX)
            RotationVector += new Vector3(1.0f, 0.0f, 0.0f);

        if (RotationY)
        {
            RotationVector += new Vector3(0.0f, 1.0f, 0.0f);
        }

        if (RotationZ)
        {
            RotationVector += new Vector3(0.0f, 0.0f, 1.0f);
        }

        if (!PositiveRotation)
        {
            //Debug.Log("Set negative rotation");
            //RotationVector = (RotationVector) * (-1.0f);
        }
    }
    
    public bool CheckAngle(){
        /*if(RotationVector == new Vector3(1, 0, 0)){
            if ((PositiveRotation && VisObject.transform.localRotation.x > EndAngleValue) || !PositiveRotation && VisObject.transform.localRotation.x < EndAngleValue)
            {
                return false;
            }
        }
        if(RotationVector == new Vector3(0, 1, 0)){
            if ((PositiveRotation && VisObject.transform.localRotation.y > EndAngleValue) || !PositiveRotation && VisObject.transform.localRotation.y < EndAngleValue)
            {
                return false;
            }
        }
        if(RotationVector == new Vector3(0, 0, 1)){
            if ((PositiveRotation && VisObject.transform.localRotation.z > EndAngleValue) || !PositiveRotation && VisObject.transform.localRotation.z < EndAngleValue)
            {
                return false;
            }
        }*/

        Debug.Log("VisObject.transform.localRotation.x = " + VisObject.transform.eulerAngles.x);
        Debug.Log("VisObject.transform.localRotation.y = " + VisObject.transform.eulerAngles.y);
        Debug.Log("VisObject.transform.localRotation.z = " + VisObject.transform.eulerAngles.z);

        //VisObject.transform.localRotation.x
            //VisObject.transform.localRotation.y

        if (RotationX)
        {
            if ((PositiveRotation && VisObject.transform.eulerAngles.x > EndAngleValue) || !PositiveRotation && VisObject.transform.eulerAngles.x < EndAngleValue)
            {
                Debug.Log("RotationX limit");
                return false;
            }
        }
        if (RotationY)
        {
            if ((PositiveRotation && VisObject.transform.eulerAngles.y > EndAngleValue) || !PositiveRotation && VisObject.transform.eulerAngles.y < EndAngleValue)
            {
                Debug.Log("RotationY limit");
                return false;
            }
        }
        if (RotationZ)
        {
            if ((PositiveRotation && VisObject.transform.eulerAngles.z > EndAngleValue) || !PositiveRotation && VisObject.transform.eulerAngles.z < EndAngleValue)
            {
                Debug.Log("RotationZ limit");
                return false;
            }
        }

        return true;
    }
	public override void Update()
	{
		base.Update();

	}
    public void FixedUpdate()
    {
		if(RotateToAngle)
		{
			if (!CheckAngle())
			{
				StopVis();
			}
		}
		
		RotationProcess();   
    }

    public override void Start()
    {
        base.Start();
        SetRotation();
    }
	public static float CalcEulerSafeX(float x)
	{
		if (x >= -90 && x <= 90)
			return x;
		x = x % 180;
		if (x > 0)
			x -= 180;
		else
			x += 180;
		return x;
	}
	public static Vector3 EulerSafeX(ref Vector3 eulerAngles)
	{
		eulerAngles.x = CalcEulerSafeX(eulerAngles.x);
		return eulerAngles;
	}
}
