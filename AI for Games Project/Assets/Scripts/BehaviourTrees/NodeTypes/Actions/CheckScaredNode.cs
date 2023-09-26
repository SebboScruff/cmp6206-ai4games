using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckScaredNode : ActionNode
{
    public ResourceGatherer resourceGatherer;
    public CheckScaredNode()
    {
        this.description = "Checks if the agent is currently frightened. Returns success if the agent is; failure if they aren't";
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
        if(resourceGatherer.isScared){
            return State.Success;
        }
        return State.Failure;
    }
}
