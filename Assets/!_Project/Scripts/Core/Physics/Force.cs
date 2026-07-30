using System;
using UnityEngine;

public struct Force
{
    public Vector3 ForceVector;
    public Vector3 ApplicationPoint;

    public static Force Zero => new Force(Vector3.zero, Vector3.zero);

    public Force(Vector3 force, Vector3 point)
    {
        ForceVector = force;
        ApplicationPoint = point;
    }
}