using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public struct ForceData
{
    public Vector3 ForceVector;
    public Vector3 ApplicationPoint;

    public static ForceData Zero => new ForceData(Vector3.zero, Vector3.zero);

    public ForceData(Vector3 force, Vector3 point)
    {
        ForceVector = force;
        ApplicationPoint = point;
    }

    public static Vector3 GetResultantForce(List<ForceData> forces)
    {
        Vector3 ret = Vector3.zero;
        foreach (ForceData force in forces)
        {
            ret += force.ForceVector;
        }

        return ret;
    }

    public static Vector3 GetResultantTorque(List<ForceData> forces, Vector3 center)
    {
        Vector3 ret = Vector3.zero;
        foreach (ForceData force in forces)
        {
            ret += Vector3.Cross(force.ApplicationPoint - center, force.ForceVector);
        }

        return ret;
    }
}