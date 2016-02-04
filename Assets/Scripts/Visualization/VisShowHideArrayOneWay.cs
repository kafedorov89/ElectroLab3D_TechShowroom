using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//show or hide array of gameobjects
public class VisShowHideArrayOneWay : VisClass
{

    public bool initialState;// = true; //true = show, false = hide
    //private bool state = true;
    public GameObject[] targets;
    //public List<GameObject> targets2;



    public override void StartVis()
    {
        base.StartVis();
        SetState();
    }

    public override void StopVis()
    {
        //Debug.Log ("Stop");
        base.StopVis();
    }

    public void SetState()
    {
        foreach (GameObject t in targets)
        {
            if (t != null)
                t.SetActive(initialState);
        }

    }

}
