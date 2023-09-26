using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Pathfinding : MonoBehaviour
{
    Grid grid;

    void Awake()
    {
        grid = GetComponent<Grid>();
        if(grid == null){
            UnityEngine.Debug.LogError("No Grid Component Found!");
        }
    }

    public void FindPath(PathRequest _request, Action<PathResult> _callback)
    {
        Stopwatch sw = new Stopwatch();
        sw.Start();
        //UnityEngine.Debug.Log("Finding Path...");

        Vector3[] waypoints = new Vector3[0];
        bool pathSuccess = false;

        PathfindNode startNode = grid.NodeFromWorldPoint(_request.pathStart);
        PathfindNode targetNode = grid.NodeFromWorldPoint(_request.pathEnd);

        if(startNode.walkable && targetNode.walkable){ // only call the expensive pathfinding method is both nodes are walkable
            Heap<PathfindNode> openSet = new Heap<PathfindNode>(grid.MaxSize);
            HashSet<PathfindNode> closedSet = new HashSet<PathfindNode>();

            openSet.Add(startNode);
            while(openSet.Count > 0)
            {
                PathfindNode currentNode = openSet.RemoveFirst();
                closedSet.Add(currentNode);

                if(currentNode == targetNode)
                {
                    // Small Debug things to show calculation speed
                    sw.Stop();
                    //UnityEngine.Debug.Log("Path Found! It took " + sw.ElapsedMilliseconds + " ms");
                    pathSuccess = true;
                    
                    break;
                }

                foreach(PathfindNode neighbour in grid.GetNeighbours(currentNode))
                {
                    if(!neighbour.walkable || closedSet.Contains(neighbour)){ continue; }

                    int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour) + neighbour.movementPenalty;
                    if(newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                    {
                        neighbour.gCost = newMovementCostToNeighbour;
                        neighbour.hCost = GetDistance(neighbour, targetNode);
                        neighbour.parent = currentNode;
                        if (!openSet.Contains(neighbour))
                        {
                            openSet.Add(neighbour);
                        }
                        else{
                            openSet.UpdateItem(neighbour);
                        }
                    }
                }
            }
        }

        if(pathSuccess){
            waypoints = RetracePath(startNode, targetNode);
            pathSuccess = waypoints.Length > 0;
        }
        _callback(new PathResult(waypoints, pathSuccess, _request.callback));
    }

    // The A* algorithm returns a list of nodes from the target to the start position
    // so this function reverses the order of the nodes to give a list of points in the order
    // the agent should visit them
    Vector3[] RetracePath(PathfindNode _startNode, PathfindNode _endNode)
    {
        List<PathfindNode> path = new List<PathfindNode>();
        PathfindNode currentNode = _endNode;
        while(currentNode != _startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        Vector3[] waypoints = SimplifyPath(path);
        Array.Reverse(waypoints);
        return waypoints;
    }

    // This method cleans up the path by removing any stretches of consecutive nodes
    // that are in a straight line: this way, the path is only turning waypoints and
    // straight lines of motion
    Vector3[] SimplifyPath(List<PathfindNode> _path)
    {
        List<Vector3> waypoints = new List<Vector3>();
        Vector2 dirOld = Vector2.zero;
        
        for(int i = 1; i < _path.Count; i++){
            Vector2 dirNew = new Vector2(_path[i-1].gridX - _path[i].gridX, _path[i-1].gridY - _path[i].gridY);
            if(dirNew != dirOld){
                waypoints.Add(_path[i].worldPosition);

            }
            dirOld = dirNew;
        }
        return waypoints.ToArray();
    }

    int GetDistance(PathfindNode nodeA, PathfindNode nodeB)
    {
        int distX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int distY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        if(distX > distY)
        {
            return 14 * distY + 10 * (distX - distY);
        }
        return 14 * distX + 10 * (distY - distX);
    }
}
