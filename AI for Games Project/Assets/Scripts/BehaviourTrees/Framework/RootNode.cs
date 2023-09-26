using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RootNode : BTNode
{
    [HideInInspector]public BTNode child;

    public RootNode()
    {
        this.description = "The start point of this behaviour tree";
    }

    protected override void OnStart()
    {
        
    }
    protected override void OnStop()
    {
        
    }
    protected override State OnUpdate()
    {
        return child.Update();
    }
    public override BTNode Clone(){
        RootNode node = Instantiate(this);
        node.child = child.Clone();
        return node;
    }
}
