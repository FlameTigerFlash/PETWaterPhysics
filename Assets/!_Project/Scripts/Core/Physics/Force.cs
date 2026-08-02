using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Force
{
    public Vector3 ForceVector;
    public Vector3 ApplicationPoint;

    public static Force Zero => new Force(Vector3.zero, Vector3.zero);

    public Force(Vector3 force, Vector3 point)
    {
        ForceVector = force;
        ApplicationPoint = point;
    }

    public static Vector3 GetResultantForce(List<Force> forces)
    {
        Vector3 ret = Vector3.zero;
        foreach (Force force in forces)
        {
            ret += force.ForceVector;
        }

        return ret;
    }

    public static Vector3 GetResultantTorque(List<Force> forces, Vector3 center)
    {
        Vector3 ret = Vector3.zero;
        foreach (Force force in forces)
        {
            ret += Vector3.Cross(force.ApplicationPoint - center, force.ForceVector);
        }

        return ret;
    }
}