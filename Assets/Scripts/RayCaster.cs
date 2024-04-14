using System;
using System.Collections;
using System.Collections.Generic;
using Shapes;
using UnityEngine;

[Serializable]
public struct Hit
{
    public bool cameOutOfMouth;
    public float totalDistanceTravelled;
    public int hits;
    public float cutOff;
    public float attack;
    public float sustain;
    public float release;
    public float decay;
    public Vector3 hitPoint;
    public float intensity; // IGNORE
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
    
    public Hit CastRay (Vector3 origin, Vector3 directionNotNormalized, Drawing drawing, MaterialRegistrySO.ObjectMaterial initialMaterial, float initialPower, Polyline rayLine = null)
    {
        float attackLength = directionNotNormalized.magnitude;
        Vector3 direction = directionNotNormalized.normalized;
        
        // List<Hit> hitPoints = new List<Hit>();

        List<Vector3> points = new List<Vector3>();
        List<Color> colors = new List<Color>();
        
        points.Add(origin);
        colors.Add(Color.green);

        Hit hit = new Hit()
        {
            intensity = initialPower,
            cutOff = initialMaterial.cutoff,
            attack = initialMaterial.attack,
            decay = initialMaterial.decay,
            sustain = initialMaterial.sustain,
            release =  initialMaterial.release,
            totalDistanceTravelled = 0,
            hits = 1,
            hitPoint = origin,
        };
        
        List<Line> lines = drawing.lines;

        Vector3 currentPoint = origin;
        Vector3 lastPoint = origin;
        Vector3 normal = Vector3.zero;
        Vector3 lastDirection = direction;

        int iterations = 100;
        
        // 50 Iterations
        for (int i = 0; i < iterations; i++)
        {
            bool foundAnIntersection = false;

            Vector3 closestHitPoint = Vector3.positiveInfinity;
            Line closestLine = null;
            
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
                        if (Vector3.Distance(lastPoint, intersection) < 0.1f || Vector3.Distance(lastPoint, intersection) > Vector3.Distance(lastPoint, closestHitPoint))
                        {
                            continue;
                        }
                        
                        lastDirection = (intersection- lastPoint).normalized;
                        normal = normalLine;
                        
                        closestHitPoint = intersection;
                        closestLine = line;
                       
                        foundAnIntersection = true;
                        
                    }
                    

                }

            }
        
            direction = Vector3.Reflect(lastDirection, normal);
            
            if (foundAnIntersection)
            {
                if (hit.intensity > 0.05f && closestLine.mat.type != MaterialRegistrySO.ObjectMaterialType.Mouth)
                {
                    hit.intensity *= 1f - closestLine.mat.damping; // Dampen
                    hit.attack = (hit.attack + (closestLine.mat.attack))/2f;
                    hit.decay = (hit.decay + (closestLine.mat.decay )) / 2f;
                    hit.sustain = (hit.sustain + (closestLine.mat.sustain )) / 2f;
                    hit.release = (hit.release + (closestLine.mat.release )) / 2f;

                    hit.hits += 1;
                    hit.totalDistanceTravelled += Vector3.Distance(lastPoint, closestHitPoint);
                    hit.cutOff = (hit.cutOff + (closestLine.mat.cutoff )) / 2f;
                    
                    // Add more property propogation here
                    
                    // Debug.DrawLine(new Vector3(lastPoint.x, lastPoint.y, lastPoint.z), closestHitPoint,
                    //     Color.Lerp(Color.magenta, Color.green, hit.intensity));

                    points.Add(closestHitPoint);
                    colors.Add(Color.Lerp(new Color(1f, 0.0f, 1f, hit.intensity), Color.green, hit.intensity));
                    
                    
                    lastPoint = closestHitPoint;

                   

                    continue;
                }
                else
                {
                    if (closestLine.mat.type == MaterialRegistrySO.ObjectMaterialType.Mouth)
                    {

                        points.Add(closestHitPoint + lastDirection * 5f);
                        colors.Add(Color.yellow);
                        hit.cameOutOfMouth = true;
                    }

                    if (rayLine) rayLine.SetPoints(points, colors);
                    if (rayLine)

                    for (int c = 0; c < rayLine.points.Count; c++)
                    {
                        rayLine.SetPointThickness(c, 0.4f);
                    }

                    if (closestLine.mat.type == MaterialRegistrySO.ObjectMaterialType.Mouth)
                    {
                        rayLine.SetPointThickness(points.Count-1, 3f);
                    }
                    
                    break;
                    
                }
            }
            else
            {
                if (i == 0)
                {
                    direction = -direction;
                }
                
                // Debug.DrawRay(lastPoint, direction*100f, Color.yellow);

                points.Add(lastPoint);
                colors.Add(Color.clear);
                    
                if (rayLine) rayLine.SetPoints(points, colors);
                
                if (rayLine)
                for (int c = 0; c < rayLine.points.Count; c++)
                {
                    rayLine.SetPointThickness(c, 0.4f);
                }
                
                if (i > 0)
                {
                    break;
                }
            }
            
        }

        return hit;

    }
    
}
