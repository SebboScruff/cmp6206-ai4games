using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct PathRequest // a data structure to contain all of the parameters of the Request function
{
    public Vector3 pathStart;
    public Vector3 pathEnd;
    public Action<Vector3[], bool> callback;

    public PathRequest(Vector3 _start, Vector3 _end, Action<Vector3[], bool> _callback){
        pathStart = _start;
        pathEnd = _end;
        callback = _callback;
    }
}

public struct PathResult // a data structure to contain all of the parameters of a Path Result
{
    public Vector3[] path;
    public bool success;
    public Action<Vector3[], bool> callback;

    public PathResult(Vector3[] _path, bool _success, Action<Vector3[], bool> _callback){
        this.path = _path;
        this.success = _success;
        this.callback = _callback;
    }
}

public class PathRequestManager : MonoBehaviour
{
    Queue<PathResult> results = new Queue<PathResult>();
    static PathRequestManager instance;
    Pathfinding pathfinding; // make a reference to the pathfinding class so the manager can access all of the goodies in there

    void Awake()
    {
        instance = this;
        pathfinding = GetComponent<Pathfinding>();
        if(pathfinding == null){
            Debug.LogError("Pathfinding Manager could not find a Pathfinding component!");
        }
    }

    void Update()
    {
        if(results.Count > 0){
            int itemsInQueue = results.Count;
            lock(results){
                for(int i = 0; i < itemsInQueue; i++){
                    PathResult result = results.Dequeue();
                    result.callback(result.path, result.success);
                }
            }
        }
    }

    public static void RequestPath(PathRequest _request)
    {
        ThreadStart threadStart  = delegate{
            instance.pathfinding.FindPath(_request, instance.FinishedProcessingPath);
        };
        threadStart.Invoke();
    }
    public void FinishedProcessingPath(PathResult _result)
    {
        lock(results){ // prevents multiple threads from Enqueuing at the same time
            results.Enqueue(_result);
        }       
    }
}
