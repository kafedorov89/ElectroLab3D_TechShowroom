using UnityEngine;
using System.Collections;

public class VisShowHide : VisClass {

    public bool hideFlag;

    public override void StartVis()
    {
        //VisObject. = true;
        VisObject.SetActive(!hideFlag);
        //VisFSM.FsmVariables.GetFsmBool("Enable").Value = true;
    }
}
