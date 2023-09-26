using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFindUnit : MonoBehaviour
{
    const float minPathUpdateTime = 0.2f;
    const float pathUpdateMoveThreshold = 0.5f;

    public bool showDebugThings = true;
    [SerializeField] Color debugPathColor;

    public Transform target;
    [SerializeField]public float speed = 5f; // how fast does the agent move
    [SerializeField]float turnSpeed = 5f; // how fast does the agent turn
    [SerializeField]float turnDistance = 5f; // how far away from a waypoint does the agent start turning toward the next waypoint?
    [SerializeField]float stoppingDistance = 10; // how far from the end does the agent slow down?

    Path path;

    void Awake()
    {
        //Debug.Log("Distance to target: " + Vector3.Distance(transform.position, target.position));
    }
    void Start()
    {
        // TODO Move this out of start at some point and give it some actual conditions
        StartCoroutine(UpdatePath());
    }
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.F4)){
            showDebugThings = !showDebugThings;
        }
    }

    public void OnPathFound(Vector3[] _waypoints, bool _pathSuccessful)
    {
        if(_pathSuccessful){
            path = new Path(_waypoints, transform.position, turnDistance, stoppingDistance);
            StopCoroutine("FollowPath");
            StartCoroutine("FollowPath");
        }
    }

    public IEnumerator UpdatePath()
    {
        if(Time.timeSinceLevelLoad < 0.3f){         // this helps to dodge the first few frames of runtime, where Time.deltaTime
            yield return new WaitForSeconds(0.3f);  // values are abnormally large
        }
        // request a path immediately
        PathRequestManager.RequestPath(new PathRequest(transform.position, target.position, OnPathFound));

        float sqrMoveThreshold = pathUpdateMoveThreshold * pathUpdateMoveThreshold;
        Vector3 targetPosOld = target.position;
        Transform targetOld = target;

        while(true){ // continually request new paths throughout runtime provided that the target is moving
            yield return new WaitForSeconds(minPathUpdateTime);
            // update the path if the target has moved
            if((target.position - targetPosOld).sqrMagnitude > sqrMoveThreshold){
                PathRequestManager.RequestPath(new PathRequest(transform.position, target.position, OnPathFound));
                targetPosOld = target.position;
            }
            // update the path if the actual target transform itself has changed
            else if(target != targetOld){
                PathRequestManager.RequestPath(new PathRequest(transform.position, target.position, OnPathFound));
                targetOld = target;
            }            
        }
    }

    IEnumerator FollowPath()
    {
        bool followingPath = true;
        int pathIndex = 0;
        transform.LookAt(path.lookPoints[0]);

        float speedPercent = 1;

        while(followingPath){
            Vector2 pos2D = new Vector2(transform.position.x, transform.position.z);
            // this is a While rather than an If for cases where the agent moves past multiple turn boundaries in a single frame
            while(path.turnBoundaries[pathIndex].HasCrossedLine(pos2D)){
                if(pathIndex == path.finishLineIndex){
                    followingPath = false; // agent has made it to the destination
                    break;
                }
                else{
                    pathIndex++;
                }
            }

            if(followingPath){
                // Slow the agent down as it approaches the end of its path, similar to the 'Arrive' steering behaviour
                if(pathIndex >= path.slowDownIndex && stoppingDistance > 0){
                    speedPercent = Mathf.Clamp01(path.turnBoundaries[path.finishLineIndex].DistanceFromPoint(pos2D) / stoppingDistance);
                    if(speedPercent < 0.01f){   // stop the path when the agent is really close 
                        followingPath = false;  // to prevent snailpacing right at the end
                    }
                }            
                Quaternion targetRotation = Quaternion.LookRotation(path.lookPoints[pathIndex] - transform.position);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * turnSpeed); 
                transform.Translate(Vector3.forward * Time.deltaTime * speed * speedPercent, Space.Self);
            }

            yield return null;
        }
    }

    // Visualise the unit's path
    public void OnDrawGizmos()
    {
        if(path != null && showDebugThings){
            path.DrawWithGizmos(debugPathColor);
        }
    }
}
