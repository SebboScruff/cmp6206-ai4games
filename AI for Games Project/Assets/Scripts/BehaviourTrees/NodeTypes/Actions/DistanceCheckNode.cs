using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DistanceCheckNode : ActionNode
{
    [SerializeField] float distance = 10f;
    [SerializeField] string targetTag = ""; // this is such an awful way to do this but i cant think of any alternatives
    // Transforms cannot be directly fed into the node from the editor
    // because the trees are cloned at the start of runtime
    [SerializeField] Transform target;
    public DistanceCheckNode()
    {
        this.description = "Checks the distance between the agent and a certain point, returns success if it is closer than the given distance";
    }
    
    protected override void OnStart()
    {
        target = GameObject.FindGameObjectWithTag(targetTag).transform;
    }
    protected override void OnStop()
    {
	
    }
    protected override State OnUpdate()
    {
        if(target == null){
            Debug.LogError("Target not found for BT Agent " + parentObj.name);
        }
        if(Vector3.Distance(parentObj.transform.position, target.position) > distance){
            return State.Failure;
        }
        return State.Success;
    }
}
