using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public enum Material
{
    Wood,
    Metal,
    Plastic,
    Glass,
    String,
    Rubber,
    Sponge
}

[Serializable]
public struct Hit
{
    public float totalDistanceTravelled;
    public int hits;
    public float cutOff;
    public float attack;
    public float decay;
    public float tremoloFrequency;
    public float tremoloAmplitude;
    public Vector3 hitPoint;
    public float intensity;
    public Material material;
}

[Serializable]
public class RayCaster
{

     
   

    
    public static bool RayIntersectsLine(Vector3 rayOrigin, Vector3 rayDirection, Vector3 lineStart, Vector3 lineEnd, Vector3 normal, out Vector3 intersection)
    {
        intersection = Vector3.zero;
        Vector3 lineDir = lineEnd - lineStart;
        Vector3 lineNormal = normal;

        // Check if the ray and line are parallel
        if (lineNormal == Vector3.zero)
            return false;

        float denom = Vector3.Dot(lineNormal, rayDirection);
        
        
        
        if (Mathf.Approximately(denom, 0))
            return false; // No intersection, the ray is parallel to the plane of the line

        // Compute the intersection point on the plane
        float t = Vector3.Dot(lineNormal, lineStart - rayOrigin) / denom;
        if (t < 0)
            return false; // The intersection is behind the ray's origin

        intersection = rayOrigin + t * rayDirection;

        // Check if the intersection point is within the line segment
        float lineSegmentLength = lineDir.magnitude;
        Vector3 closestPointOnLine = lineStart + Vector3.Project(intersection - lineStart, lineDir);
        float distanceFromStart = (closestPointOnLine - lineStart).magnitude;
        float distanceFromEnd = (closestPointOnLine - lineEnd).magnitude;

        // Check if the closest point on the line segment is close enough to the intersection point
        if (distanceFromStart + distanceFromEnd - lineSegmentLength > 1e-5)
            return false;

        return true;
    }
    
    public static Vector3 CalculateNormalFromLine(Vector3 pointA, Vector3 pointB)
    {
        Vector3 lineDirection = pointB - pointA;
        // Assume a normal in the plane (like z = 0 in 2D for a 3D vector)
        Vector3 normal = new Vector3(-lineDirection.y, lineDirection.x, 0).normalized;
        return normal;
    }
    
    public List<Hit> CastRay (Vector3 origin, Vector3 directionNotNormalized, Drawing drawing)
    {
        float attackLength = directionNotNormalized.magnitude;
        Vector3 direction = directionNotNormalized.normalized;
        
        List<Hit> hitPoints = new List<Hit>();
        
        List<Line> lines = drawing.lines;

        Vector3 currentPoint = origin;
        Vector3 lastPoint = origin;
        Vector3 normal = Vector3.zero;
        Vector3 lastDirection = direction;

        int iterations = 10;
        
        // 50 Iterations
        for (int i = 0; i < iterations; i++)
        {
            bool foundAnIntersection = false;

            Hit closestHitPoint = new Hit()
            {
                hitPoint =  Vector3.positiveInfinity,
            };
            
            foreach (var line in lines)
            {
                for (int j = 0; j < line.points.Count - 1; j++)
                {
                    Vector3 lineStart = line.points[j];
                    Vector3 lineEnd = line.points[j + 1];
                    Vector3 intersection;
                    Vector3 normalLine = CalculateNormalFromLine(lineStart, lineEnd);
                    if (RayIntersectsLine(lastPoint, direction, lineStart, lineEnd, normalLine, out intersection))
                    {
                        if (Vector3.Distance(lastPoint, intersection) < 0.1f || Vector3.Distance(lastPoint, intersection) > Vector3.Distance(lastPoint, closestHitPoint.hitPoint))
                        {
                            continue;
                        }


                        
                        Hit hit = new Hit();
                        hit.hitPoint = intersection;
                        hit.intensity = 1;
                        hit.material = Material.Wood;
                        
                        lastDirection = (intersection- lastPoint).normalized;
                        normal = normalLine;
                        
                        closestHitPoint = hit;
                       
                        foundAnIntersection = true;
                        
                    }
                    

                }

            }
        
            direction = Vector3.Reflect(lastDirection, normal);
            
            if (foundAnIntersection)
            {
                hitPoints.Add(closestHitPoint);
                Debug.DrawLine(new Vector3(lastPoint.x, lastPoint.y, lastPoint.z), closestHitPoint.hitPoint, Color.Lerp( Color.magenta, Color.green, ((float)i/(float)iterations)));
                lastPoint = closestHitPoint.hitPoint;
                continue;
            }
            else
            {
                if (i == 0)
                {
                    direction = -direction;
                }
                
                Debug.DrawRay(lastPoint, direction*100f, Color.yellow);

                if (i > 0)
                {
                    break;
                }
            }
            
        }

        return hitPoints;

    }
    
}
