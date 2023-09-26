using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// For Debugging Purposes, this is a node that just automatically returns failure
public class AutoFailNode : ActionNode
{
    public AutoFailNode()
    {
        this.description = "Automatically fails. For debugging & testing purposes";
    }
    
    protected override void OnStart()
    {

    }
    protected override void OnStop()
    {

    }
    protected override State OnUpdate()
    {
        return State.Failure;
    }
}
