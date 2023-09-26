using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuardAttackNode : ActionNode
{
    Guard guard;
    public GuardAttackNode()
    {
        this.description = "Issues an attack command and then returns Success";
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
        guard.Attack();
        return State.Success;
    }
}
