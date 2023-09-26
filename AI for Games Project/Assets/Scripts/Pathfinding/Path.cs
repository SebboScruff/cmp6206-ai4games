using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Path
{
    public readonly Vector3[] lookPoints;
    public readonly Line[] turnBoundaries;
    public readonly int finishLineIndex;
    public readonly int slowDownIndex;
    
    public Path(Vector3[] _waypoints, Vector3 _startPos, float _turnDist, float _stoppingDistance){
        lookPoints = _waypoints;
        turnBoundaries = new Line[lookPoints.Length];
        finishLineIndex = turnBoundaries.Length - 1;

        Vector2 previousPoint = Vec3ToVec2(_startPos);
        for(int i = 0; i < lookPoints.Length; i++){
            Vector2 currentPoint = Vec3ToVec2(lookPoints[i]);
            Vector2 dirToCurrent = (currentPoint - previousPoint).normalized;
            // if this point is the final point, do not subtract the turn distance; otherwise, do subtract the turn distance
            Vector2 turnBoundaryPoint = (i==finishLineIndex) ? currentPoint : currentPoint - dirToCurrent * _turnDist;

            turnBoundaries[i] = new Line(turnBoundaryPoint, previousPoint - dirToCurrent * _turnDist);
            previousPoint = turnBoundaryPoint;
        }

        float distFromEndPoint = 0;
        for(int i = lookPoints.Length-1; i > 0; i--){
            distFromEndPoint += Vector3.Distance(lookPoints[i], lookPoints[i-1]);
            if(distFromEndPoint > _stoppingDistance){
                slowDownIndex = i;
                break;
            }
        }
    }

    Vector2 Vec3ToVec2(Vector3 _v) // standard v3 to v2 casting turns (x, y, z) into (x, y)
    {
        return new Vector2(_v.x, _v.z); // convert an (x, y, z) vector3 into an (x, z) vector2
    }

    public void DrawWithGizmos(Color _c)
    {
        Gizmos.color = _c;
        foreach(Vector3 p in lookPoints){
            Gizmos.DrawCube(p + Vector3.up, Vector3.one);
        }
        Gizmos.color = Color.white;
        foreach(Line l in turnBoundaries){
            l.DrawWithGizmos(10f);
        }
    }
}
