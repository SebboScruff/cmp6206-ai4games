using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomChanceNode : ActionNode
{
    [Range(1,100)] public int chance;
    System.Random rng;
    int val;
    public RandomChanceNode()
    {
        this.description = "Has a parameterised chance to succeed";
    }
    
    protected override void OnStart()
    {
        val = rng.Next(0,101);
    }
    protected override void OnStop()
    {
	
    }
    protected override State OnUpdate()
    {
        if(val < chance){
            return State.Success;
        }
        return State.Failure;
    }
}
