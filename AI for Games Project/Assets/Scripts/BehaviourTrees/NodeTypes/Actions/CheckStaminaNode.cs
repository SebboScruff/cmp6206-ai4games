using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckStaminaNode : ActionNode
{
    public ResourceGatherer resourceGatherer;
    public CheckStaminaNode()
    {
        this.description = "Does this agent have enough stamina to work? Returns false if agent's stamina is below their tiredness threshold";
    }
    
    protected override void OnStart()
    {
        resourceGatherer = parentObj.GetComponent<ResourceGatherer>();
    }
    protected override void OnStop()
    {
	
    }
    protected override State OnUpdate()
    {
        if(resourceGatherer.stamina > resourceGatherer.tirednessThreshold){
            return State.Success;
        }
        else{
            return State.Failure;
        }
    }
}
