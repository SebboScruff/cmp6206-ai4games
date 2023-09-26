using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestAction : ActionNode
{
    public TestAction()
    {
        this.description = "[Node Description Here]";
    }
    
    protected override void OnStart()
    {
	
    }
    protected override void OnStop()
    {
	
    }
    protected override State OnUpdate()
    {
        return State.Success;
    }
}
