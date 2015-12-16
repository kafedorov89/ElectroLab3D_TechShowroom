using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VisAnimation : VisClass {

    public bool Loop;
    public int loops;
    public int LoopCount;
    public string waitStateName;
    public string workStateName;

    public string[] AnimatorStateNames;
    private Animator animator { get; set; }
    private Dictionary<int, string> NameTable { get; set; }

    int waitState;
    int workState;

    private void BuildNameTable()
    {
        NameTable = new Dictionary<int, string>();

        foreach (string stateName in AnimatorStateNames)
        {
            NameTable[Animator.StringToHash(stateName)] = stateName;
        }
    }

    public int GetLoopCount()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        int loops = (int)Mathf.Floor(stateInfo.normalizedTime / stateInfo.length);

        Debug.Log("Loops = " + loops);

        return loops;
    }

    public string GetCurrentAnimatorStateName()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        string stateName;
        if (NameTable.TryGetValue(stateInfo.shortNameHash, out stateName))
        {
            return stateName;
        }
        else
        {
            Debug.LogWarning("Unknown animator state name.");
            return string.Empty;
        }
    }

    void Start()
    {
        waitState = Animator.StringToHash("Base." + waitStateName);
        workState = Animator.StringToHash("Base." + workStateName);
    }
    
    public override void StartVis()
    {
        animator.SetTrigger("Start");
    }

    public override void StopVis()
    {
        animator.SetTrigger("Stop");
    }

    void Update()
    {
        if (GetLoopCount() >= LoopCount)
        {
            StopVis();
        }
    }
}
