using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomSequenceNode : CompositeNode
{
    int current;
    private static System.Random rng = new System.Random();

    public RandomSequenceNode()
    {
        this.description = "Executes each child node randomly. Returns up the tree with Success if all children succeed; immediately fails if any child fails";
    }
    protected override void OnStart()
    {
        // randomise the order of the children using a Fisher-Yates shuffle
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
                current++;
                break;
            case State.Failure:
                return State.Failure; // immediately fail this node if any of the children have failed
        }
        // if this node has executed every child node, it will return success, otherwise it will continue running
        return current == children.Count ? State.Success : State.Running;
    }
}
