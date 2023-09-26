using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ActionNode : BTNode
{
    // Action Nodes are leaf nodes in this tree - no behaviours added at this level
    // Each individual action node will inherit from here and have its behaviours declared on a node-by-node basis
}
