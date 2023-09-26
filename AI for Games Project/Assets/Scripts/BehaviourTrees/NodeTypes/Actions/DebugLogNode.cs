using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugLogNode : ActionNode
{
    public string message;

    public DebugLogNode()
    {
        this.description = "Outputs a message on Start, Update, and Stop, then returns Success";
    }
    protected override void OnStart()
    {
        Debug.Log($"OnStart{message}");
    }
    protected override void OnStop()
    {
        Debug.Log($"OnStop{message}");
    }
    protected override State OnUpdate()
    {
        Debug.Log($"OnUpdate{message}");
        return State.Success;
    }
}
