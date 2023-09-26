using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetStateNode : ActionNode
{
    public ResourceGatherer.States newState;
    public ResourceGatherer resourceGatherer;

    public SetStateNode()
    {
        this.description = "Changes the agent's current active state to " + newState;
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
        if(resourceGatherer == null){
            return State.Failure;
        }
        if(resourceGatherer.currentState != newState){
            resourceGatherer.currentState = newState;
        }
        return State.Success;
    }
}
