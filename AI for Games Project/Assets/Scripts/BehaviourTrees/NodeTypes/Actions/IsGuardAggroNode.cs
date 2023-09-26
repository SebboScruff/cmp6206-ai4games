using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsGuardAggroNode : ActionNode
{
    Guard guard;
    public IsGuardAggroNode()
    {
        this.description = "Checks the guard's aggro bool to return Success if still aggro, or failure if not";
    }
    
    protected override void OnStart()
    {
        guard = parentObj.GetComponent<Guard>();
    }
    protected override void OnStop()
    {
        
    }
    protected override State OnUpdate()
    {
        if(guard.isAggro){
            return State.Success;
        }
        return State.Failure;
    }
}
