using UnityEngine;
using System.Collections;

public class VisPositionLerp : VisClass {

    public Transform StartPosition;
    public Transform EndPosition;

    public bool Reverse;
    public float StartTime;
    private bool StartTimeInited;

    public float Speed;

    public void MovingProcess()
    {
        //Debug.Log("DeltaTime = " + ((Time.time - StartTime) * Speed).ToString());

        if (!Reverse)
        {
            VisObject.transform.localPosition = Vector3.Lerp(StartPosition.localPosition, EndPosition.localPosition, (Time.time - StartTime) * Speed);
            VisObject.transform.localRotation =  Quaternion.Lerp(StartPosition.localRotation, EndPosition.localRotation, (Time.time - StartTime) * Speed);
        }
        else
        {
            VisObject.transform.localPosition = Vector3.Lerp(EndPosition.localPosition, StartPosition.localPosition, (Time.time - StartTime) * Speed);
            VisObject.transform.localRotation = Quaternion.Lerp(EndPosition.localRotation, StartPosition.localRotation, (Time.time - StartTime) * Speed);
        }
    }

    public override void StartVis()
    {
        base.StartVis(); //Doesn't use directly
        //VisObject.transform.Rotate(RotationVector, Speed * Time.deltaTime);
    }

    public override void StopVis()
    {
        base.StopVis(); //Doesn't use directly
        //VisObject.transform.Rotate(RotationVector, 0.0f);
    }
    
    // Use this for initialization
	public override void Start () {
        base.Start();

        /*if (UseThisObject)
        {
            StartPosition = this.gameObject;
            EndPosition = this.gameObject;
        }*/
    }
	
	// Update is called once per frame
	public override void Update () {
	    base.Update();

        if (VisStarted)
        {
            if (!StartTimeInited)
            {
                StartTimeInited = true;
                StartTime = Time.time;
            }

            MovingProcess();

            //if (VisObject.transform.localPosition == EndPosition.transform.localPosition)
            //!Reverse
            if (!Reverse && ((VisObject.transform.position == EndPosition.transform.position) && (VisObject.transform.localEulerAngles == EndPosition.transform.localEulerAngles)))// && VisObject.transform.localRotation == EndPosition.transform.localRotation)
            {
                //Debug.Log("Position was reached");
                StopVis();
                StartTimeInited = false;
            }

            else if(Reverse && ((VisObject.transform.position == StartPosition.transform.position) && (VisObject.transform.localEulerAngles == StartPosition.transform.localEulerAngles)))
            {
                //Debug.Log("2");
                Debug.Log("Position was reached");
                StopVis();
                StartTimeInited = false;
            }

            /*Debug.Log("VisObject.transform.position = " + VisObject.transform.position);
            Debug.Log("VisObject.transform.localEulerAngles = " + VisObject.transform.localEulerAngles);

            Debug.Log("EndPosition.transform.position = " + EndPosition.transform.position);
            Debug.Log("EndPosition.transform.localEulerAngles = " + EndPosition.transform.localEulerAngles);
            
            Debug.Log("StartPosition.transform.position = " + StartPosition.transform.position);
            Debug.Log("StartPosition.transform.localEulerAngles = " + StartPosition.transform.localEulerAngles);*/

            
            
           
            
            //Debug.Log("localEulerAngles = " + VisObject.transform.localEulerAngles);
        }
	}
}
