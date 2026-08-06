using System;
using UnityEngine;
using System.Collections.Generic;

public static class PlaneExtensions
{
    public static bool GetIntersectionPoint(this Plane plane, Vector3 origin, Vector3 direction, out Vector3 intersectionPoint)
    {
        intersectionPoint = origin;

        Ray ray = new Ray(origin, direction);
        bool intersects = plane.Raycast(ray, out var enter);
        if (!intersects)
        {
            return false;
        }

        intersectionPoint = origin + direction.normalized * enter;
        return true;
    }
}
