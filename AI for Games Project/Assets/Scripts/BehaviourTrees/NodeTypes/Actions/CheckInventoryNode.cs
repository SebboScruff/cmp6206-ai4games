using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckInventoryNode : ActionNode
{
    public ResourceGatherer resourceGatherer;
    public CheckInventoryNode()
    {
        this.description = "Does this agent have available inventory space? Returns false if inventory space remaining <= that agent's work cost";
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
        if(resourceGatherer.inventoryRemaining <= resourceGatherer.workInventoryCost){
            return State.Failure;
        }
        else{
            return State.Success;  
        }       
    }
}
