using UnityEngine;
using System.Collections;

public class DelayTimer : MonoBehaviour{

    private float startTime;
    private float currentTime;
    public bool Started;
    public float Delay; //Delay in seconds
    public float DeltaTime;
    public bool Stoped;

    public DelayTimer(float delay)
    {
        Delay = delay;
        DeltaTime = 0.0f;
    }

    public DelayTimer()
    {
        Delay = 0.0f;
        DeltaTime = 0.0f;
    }

    public void PrintTime()
    {
        //Debug.Log("DeltaTime = " + DeltaTime.ToString());
        //Debug.Log("StartTime = " + startTime);
        //Debug.Log("CurrentTime = " + currentTime);
    }

    public void TimerStart()
    {
        Started = true;
        Stoped = false;
        startTime = Time.fixedTime;
        currentTime = Time.fixedTime;
    }

    public void ResetTimer()
    {
        TimerStop();
        Stoped = false;
    }

    public void TimerStop()
    {
        Started = false;
        Stoped = true;
        startTime = 0.0f;
        currentTime = 0.0f;
    }

    public bool DrinDrin()
    {
        currentTime = Time.fixedTime;
        DeltaTime = Mathf.Abs(startTime - currentTime);

        if (DeltaTime >= Delay)
        {
            TimerStop();
            return true;
        }
        else
        {
            return false;
        }
    }

    void Update()
    {
        //DeltaTime = Mathf.Abs(startTime - currentTime);
    }
}
