using UnityEngine;
using System.Collections;

public class VisClass : MonoBehaviour {

    public bool UseThisObject;
    
    public float DelayBefore; //Delay befor visualization in second
    //public float DelayAfter;

    //public bool Enabled;
    public bool Activated;
    public bool VisStarted;

    public bool ManualStart;
    public bool ManualStop;

    public string VisID;
    //public bool isVis;
    public GameObject VisObject;

    DelayTimer delayBeforTimer;

    //DelayTimer delayAfterTimer;


    public virtual void StartVis()
    {
        Debug.Log("StartVis");
        VisStarted = true;
    }

    public virtual void StopVis()
    {
        Debug.Log("StopVis");
        VisStarted = false;
        Activated = false;
    }

    // Use this for initialization
    public virtual void Start () {
        delayBeforTimer = new DelayTimer();

        if (UseThisObject)
        {
            VisObject = this.gameObject;
        }
        //delayAfterTimer = new DelayTimer(DelayAfter);
    }

    public void Activate()
    {
        Debug.Log("Activate");
        Activated = true;
        delayBeforTimer.ResetTimer();
        delayBeforTimer.Delay = DelayBefore;
        delayBeforTimer.TimerStart();
    }

    /*public void Enable()
    {
        Enabled = true;
    }*/

    /*public void Disable()
    {
        Enabled = false;
        Deactivate();
    }*/

    // Update is called once per frame
    public virtual void Update () {
        if (ManualStart)
        {
            ManualStart = false;
            Activate();
        }

        if (ManualStop)
        {
            ManualStop = false;
            //Deactivate();
        }

        if (Activated && !delayBeforTimer.Stoped)
        {
            Debug.Log("Activated");

            if (delayBeforTimer.DrinDrin() && !VisStarted)
            {
                //Enabled = false;
                StartVis();
                delayBeforTimer.TimerStop();
            }
            else
            {
                delayBeforTimer.PrintTime();
            }
        }
    }
}
