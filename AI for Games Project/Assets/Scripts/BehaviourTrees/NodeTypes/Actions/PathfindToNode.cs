using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathfindToNode : ActionNode
{
    [SerializeField] Transform target;
    [SerializeField] GameObject aStarManager;
    Pathfinding pathfinding;
    PathRequestManager pathRequestManager;

    PathFindUnit pathFindUnit;

    public PathfindToNode()
    {
        this.description = "Utilise the A* Pathfinding Implementation to navigate to a point";
    }
    
    protected override void OnStart()
    {
        pathfinding = aStarManager.GetComponent<Pathfinding>();
        pathRequestManager = aStarManager.GetComponent<PathRequestManager>();
        pathFindUnit = parentObj.GetComponent<PathFindUnit>();

        pathFindUnit.target = target;
    }
    protected override void OnStop()
    {
	
    }
    protected override State OnUpdate()
    {
        if(pathfinding == null){
            Debug.LogError("Could not find PathFinding component!");
            return  State.Failure;
        }
        if(pathRequestManager == null){
            Debug.LogError("Could not find PathRequestManager component!");
            return  State.Failure;
        }
        if(pathFindUnit == null){
            Debug.LogError("Could not find PathFindUnit component!");
            return  State.Failure;
        }
        // TODO make this functional lmao
        pathFindUnit.StartCoroutine(pathFindUnit.UpdatePath());
        return State.Success;
    }
}
