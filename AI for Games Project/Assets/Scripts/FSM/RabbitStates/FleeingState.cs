using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FleeingState : State
{
    RabbitFSM owner;
    Transform fleeFrom;
    SteeringAgent steer;
    public FleeingState(RabbitFSM _owner, Transform _fleeFrom)
    {
        this.owner = _owner;
        this.fleeFrom = _fleeFrom;

        steer = owner.GetComponent<SteeringAgent>();
        steer.target = fleeFrom;
    }

    public override void Enter()
    {
        // look away from whatever it's fleeing from, then run in a straight line
        steer.steeringType = SteeringAgent.SteeringTypes.FLEE;
        owner.transform.LookAt(2*owner.transform.position - fleeFrom.position);
        owner.stateText.text = "State: FLEE";
    }

    public override void Execute()
    {
        //owner.transform.Translate(Vector3.forward * owner.moveSpeed * Time.deltaTime);
        owner.transform.LookAt(2*owner.transform.position - fleeFrom.position);
    }

    public override void Exit()
    {
        //Debug.Log("Exiting Flee State");
    }
}
