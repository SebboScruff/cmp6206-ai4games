using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class DecoratorNode : BTNode
{
    [HideInInspector]public BTNode child; // decorator nodes always have exactly one child
    public override BTNode Clone(){
        DecoratorNode node = Instantiate(this);
        node.child = child.Clone();
        return node;
    }
}
