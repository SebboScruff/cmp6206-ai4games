using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WanderState : State
{
    RabbitFSM owner;
    SteeringAgent steer;
    public WanderState(RabbitFSM _owner)
    {
        this.owner = _owner;

        steer = owner.GetComponent<SteeringAgent>();
        steer.target = null;
    }
    
    public override void Enter()
    {
        //Debug.Log("Entering Wander State");
        steer.steeringType = SteeringAgent.SteeringTypes.WANDER;
        owner.stateText.text = "State: WANDER";
    }

    public override void Execute()
    {       
       //Debug.Log("Currently Wandering");
    }

    public override void Exit()
    {
        //Debug.Log("Exiting State");
    }
}
