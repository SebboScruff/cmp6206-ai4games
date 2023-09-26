using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetSteeringNode : ActionNode
{
    SteeringAgent steeringAgent;
    [SerializeField] SteeringAgent.SteeringTypes newType;

    public SetSteeringNode()
    {
        this.description = "Sets the Steering type of a steering agent to " + newType;
    }
    
    protected override void OnStart()
    {
        steeringAgent = parentObj.GetComponent<SteeringAgent>();
    }
    protected override void OnStop()
    {
	
    }
    protected override State OnUpdate()
    {
        if(steeringAgent == null){
            Debug.LogError("No Steering behaviours found on BT Agent " + parentObj.name);
            return State.Failure;
        }

        steeringAgent.steeringType = newType;

        return State.Success;
    }
}
