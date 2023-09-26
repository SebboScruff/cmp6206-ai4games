using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanGuardAttackNode : ActionNode
{
    Guard guard;
    public CanGuardAttackNode()
    {
        this.description = "Checks both Range and Cooldown to determine whether or not this guard can attack";
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
        if(guard.atkCooldownCurrent <= 0){ // cooldown check first because its less expensive
            if(Vector3.Distance(parentObj.transform.position, guard.player.transform.position) <= guard.atkRange){ // range check
                return State.Success;
            }
        }
        return State.Failure;     
    }
}
