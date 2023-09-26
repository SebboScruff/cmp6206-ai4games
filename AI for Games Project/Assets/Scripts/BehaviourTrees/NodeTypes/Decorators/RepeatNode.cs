using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A Decorator node to infinitely continue its child node's behaviours
// TODO Toggle between infinite and a set number of repeats 
public class RepeatNode : DecoratorNode
{
    public bool infinite = true;
    public int numOfRepeats = 1;
    private int runsRemaining;

    public RepeatNode()
    {
        this.description = "Repeats its subtree, either a certain number of times or indefinitely";
    }
    
    protected override void OnStart()
    {
        runsRemaining = numOfRepeats;
    }
    protected override void OnStop()
    {
        
    }
    protected override State OnUpdate()
    {
        if(infinite){
            child.Update();
            return State.Running;
        }
        // NOTE: This is kinda weird - if a non-infinite repeater node leads directly to an action, it works fine.
        // If it leads into a composite, it runs exactly once then stops
        do{
            child.Update();
            runsRemaining--;
        }while(runsRemaining > 0);
        return State.Success;
    }
}
