using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveForwardNode : ActionNode
{
    public float distanceToMove = 1f;

    public MoveForwardNode()
    {
        this.description = "Moves the agent forwards a set distance";
    }
    
    protected override void OnStart()
    {
	
    }
    protected override void OnStop()
    {
	
    }
    protected override State OnUpdate()
    {
        parentObj.transform.Translate(Vector3.forward * distanceToMove);
        return State.Success;
    }
}
