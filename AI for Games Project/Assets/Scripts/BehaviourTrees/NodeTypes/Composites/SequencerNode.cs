using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*  Sequences go through each child, left to right, 
*   until one of them fails. When a child return failure,
*   the sequence immediately stops and also return failure
*/
public class SequencerNode : CompositeNode
{
    int current;

    public SequencerNode()
    {
        this.description = ("Executes each child node from left to right. Returns up the tree with Success if all children succeed; immediately fails if any child fails");
    }
    protected override void OnStart()
    {
        current = 0;
    }
    protected override void OnStop()
    {
        
    }
    protected override State OnUpdate()
    {
        BTNode child = children[current];
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
