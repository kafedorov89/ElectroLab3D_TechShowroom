using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class VisPositionLerp : VisClass {

    public Transform StartPosition;
    public Transform EndPosition;

    public bool Reverse;
    public float StartTime;
    private bool StartTimeInited;

    public float Speed;

    public bool ReverseLoop;

    public void MovingProcess()
    {
        //Debug.Log("DeltaTime = " + ((Time.time - StartTime) * Speed).ToString());
        if (!Reverse)
        {
            VisObject.transform.position = Vector3.Lerp(StartPosition.position, EndPosition.position, (Time.time - StartTime) * Speed);
            VisObject.transform.rotation = Quaternion.Lerp(StartPosition.rotation, EndPosition.rotation, (Time.time - StartTime) * Speed);
        }
        else
        {
            VisObject.transform.position = Vector3.Lerp(EndPosition.position, StartPosition.position, (Time.time - StartTime) * Speed);
            VisObject.transform.rotation = Quaternion.Lerp(EndPosition.rotation, StartPosition.rotation, (Time.time - StartTime) * Speed);
        }
    }

    public override void StartVis()
    {
        base.StartVis(); //Doesn't use directly
        StartTimeInited = false;
        //VisObject.transform.Rotate(RotationVector, Speed * Time.deltaTime);
    }

    public override void StopVis()
    {
        if (ReverseLoop)
        {
            NextVis = this;
            Reverse = !Reverse;
        }
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

        if (VisStarted && StartPosition != null && EndPosition != null)
        {
            if (!StartTimeInited)
            {
                StartTimeInited = true;
                StartTime = Time.time;
            }

            MovingProcess();

            //if (VisObject.transform.localPosition == EndPosition.transform.localPosition)
            //!Reverse
            if (!Reverse && ((VisObject.transform.position == EndPosition.transform.position) && (VisObject.transform.rotation == EndPosition.transform.rotation)))// && VisObject.transform.localRotation == EndPosition.transform.localRotation)
            {
                //Debug.Log("Position was reached");
                StopVis();
                StartTimeInited = false;
            }

            else if (Reverse && ((VisObject.transform.position == StartPosition.transform.position) && (VisObject.transform.rotation == StartPosition.transform.rotation)))
            {
                //Debug.Log("2");
                Debug.Log("Position was reached");
                StopVis();
                StartTimeInited = false;
            }
        }
	}
}
