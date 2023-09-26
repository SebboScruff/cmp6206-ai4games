using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitNode : ActionNode
{   
    public float duration = 1.0f;
    float startTime;

    public WaitNode()
    {
        description = "waits a certain amount of time before returning success";
    }
    protected override void OnStart()
    {
        startTime = Time.time;
    }
    protected override void OnStop()
    {
        
    }
    protected override State OnUpdate()
    {
        if(Time.time - startTime > duration){
            return  State.Success;
        }
        return State.Running;
    }
}
