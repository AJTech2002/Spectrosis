using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RayMath
{
    public static Vector2 ClosestPointOnLineSegment(Vector2 A, Vector2 B, Vector2 P)
    {
        Vector2 AP = P - A;
        Vector2 AB = B - A;
        float abSquare = AB.sqrMagnitude; // dot product of itself
        float apDotAb = Vector2.Dot(AP, AB);
        float t = Mathf.Clamp(apDotAb / abSquare, 0, 1);
        return A + t * AB;
    }
}
