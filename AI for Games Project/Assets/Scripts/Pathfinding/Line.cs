using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This line struct is used in path smoothing
public struct Line
{
    const float verticalGradient = 1e5f; // shorthand for 100,000 - a very large gradient

    float gradient;
    float yIntercept;
    Vector2 pointA, pointB; // two fairly arbitrary points along the line
    float perpendicularGradient;
    bool approachSide;

    public Line(Vector2 _pointOnLine, Vector2 _perpendicularPoint){
        // Constructor that takes in 2 world points then mathematically solved for various properties
        float dx = _pointOnLine.x - _perpendicularPoint.x;
        float dy = _pointOnLine.y - _perpendicularPoint.y;

        if(dx == 0){
            perpendicularGradient = verticalGradient;
        }
        else{
            perpendicularGradient = dy/dx;
        }

        if(perpendicularGradient == 0){
            gradient = verticalGradient;
        }
        else{
            gradient = -1 / perpendicularGradient;
        }

        yIntercept = _pointOnLine.y - gradient * _pointOnLine.x; // rearrange y=mx+c i.e. c = y-mx

        pointA = _pointOnLine;
        pointB = _pointOnLine + new Vector2(1, gradient);

        approachSide = false; // set it temporarily as false so that the GetSide method can be declared
        approachSide = GetSide(_perpendicularPoint);
    }

    bool GetSide(Vector2 _p) // checks if a point is on one side of the line or another
    {
        return (_p.x - pointA.x) * (pointB.y - pointA.y) > (_p.y - pointA.y) * (pointB.x - pointA.x);
    }

    public bool HasCrossedLine(Vector2 _p)
    {
        return GetSide(_p) != approachSide;
    }

    public void DrawWithGizmos(float _length)
    {
        Vector3 lineDir = new Vector3(1, 0, gradient).normalized;
        Vector3 lineCentre = new Vector3(pointA.x, 0, pointA.y) + Vector3.up;

        Gizmos.DrawLine(lineCentre - lineDir * _length/2f, lineCentre + lineDir * _length/2f);
    }

    public float DistanceFromPoint(Vector2 _p)
    {
        float yInterceptPerp = _p.y - perpendicularGradient * _p.x;
        float intersectX = (yInterceptPerp - yIntercept) / (gradient - perpendicularGradient);
        float intersectY = gradient * intersectX + yIntercept;
        
        return Vector2.Distance(_p, new Vector2(intersectX, intersectY));
    }
}
