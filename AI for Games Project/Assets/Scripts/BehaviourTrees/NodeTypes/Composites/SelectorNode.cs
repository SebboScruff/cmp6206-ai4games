using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*  Selectors go through each child, left to right, 
*   until one of them succeeds. When a child return success,
*   the selector also returns success
*/
public class SelectorNode : CompositeNode
{
    int current;

    public SelectorNode()
    {
        this.description = "Executes each child from left to right. Stops and returns success as soon as one child succeeds; returns failure if no children succeed";
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
