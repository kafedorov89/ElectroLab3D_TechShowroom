using UnityEngine;
using System.Collections;


[ExecuteInEditMode]
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
		Stoped = false;
    }

    public DelayTimer()
    {
        Delay = 0.0f;
        DeltaTime = 0.0f;
		Stoped = false;
    }

    public void PrintTime()
    {
		//Debug.Log ("Delay = " + Delay.ToString());
		//Debug.Log ("DeltaTime = " + DeltaTime.ToString());
		//Debug.Log ("StartTime = " + startTime.ToString());
		//Debug.Log ("CurrentTime = " + currentTime.ToString());
    }

    public void TimerStart()
    {
        ResetTimer();
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

		if (DeltaTime >= Delay && !Stoped)
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
        if (Started)
		{
			
            
            if(DrinDrin())
			{
				//Debug.Log("Drin-Drin");
			}
		}


		//DeltaTime = Mathf.Abs(startTime - currentTime);
		//Debug.Log ("DeltaTime = " + DeltaTime);
    }
}
