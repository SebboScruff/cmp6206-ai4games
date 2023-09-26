using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathfindNode : IHeapItem<PathfindNode>
{
    public bool walkable;
    public Vector3 worldPosition;

    public int gridX, gridY;

    public int movementPenalty;

    public int gCost;
    public int hCost;
    public PathfindNode parent;
    int heapIndex;

    public PathfindNode(bool _walkable, Vector3 _worldPos, int _gridX, int _gridY, int _penalty) {
        walkable = _walkable;
        worldPosition = _worldPos;
        gridX = _gridX;
        gridY = _gridY;
        movementPenalty = _penalty;
    }

    public int fCost
    {
        get { return gCost + hCost; }
    }

    public int HeapIndex
    {
        get { return heapIndex; }
        set { heapIndex = value; }
    }

    public int CompareTo(PathfindNode nodeToCompare)
    {
        int compare = fCost.CompareTo(nodeToCompare.fCost);
        if(compare == 0){
            compare = hCost.CompareTo(nodeToCompare.hCost);
        }
        return -compare;
    }
}
