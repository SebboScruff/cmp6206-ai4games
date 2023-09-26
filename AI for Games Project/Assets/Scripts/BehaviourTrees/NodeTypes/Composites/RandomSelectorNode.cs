using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomSelectorNode : CompositeNode
{
    int current;
    private static System.Random rng = new System.Random();

    public RandomSelectorNode()
    {
        this.description = "Executes each child randomly. Stops and returns success as soon as one child succeeds; returns failure if no children succeed";
    }
    
    protected override void OnStart()
    {
        for(int i = children.Count - 1; i > 0; i--){
            int k = rng.Next(i + 1);
            BTNode child = children[k];
            children[k] = children[i];
            children[i] = child;
        }

        current = 0;
    }
    protected override void OnStop()
    {
	
    }
    protected override State OnUpdate()
    {
        var child = children[current];
        switch(child.Update()){
            case State.Running:
                return State.Running;
            case State.Success:
                return State.Success;
            case State.Failure:
                current++;
                break;
        }
        // if this node has executed every child node, it will return success, otherwise it will continue running
        return current == children.Count ? State.Failure : State.Running;
    }
}