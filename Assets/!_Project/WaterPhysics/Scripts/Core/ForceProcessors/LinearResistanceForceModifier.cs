using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LinearResistanceForceModifier : ITransformForces
{
    [SerializeField] protected Vector3 _frontalResistanceCoef;
    [SerializeField] protected Vector3 _backwardResistanceCoef;

    public List<ForceData> TransformForces(List<ForceData> forces, WaterData water, RigidBodyData rb, float deltaTime = 1f)
    {
        _frontalResistanceCoef = new Vector3(Mathf.Abs(_frontalResistanceCoef.x), Mathf.Abs(_frontalResistanceCoef.y), Mathf.Abs(_frontalResistanceCoef.z));
        _backwardResistanceCoef = new Vector3(Mathf.Abs(_backwardResistanceCoef.x), Mathf.Abs(_backwardResistanceCoef.y), Mathf.Abs(_backwardResistanceCoef.z));
        
        Vector3 forwardDirection = rb.TrData.Rotation * Vector3.forward;
        Vector3 rightDirection = rb.TrData.Rotation * Vector3.right;
        Vector3 upDirection = rb.TrData.Rotation * Vector3.up;

        for (int i = 0; i < forces.Count; i++)
        {
            ForceData force = forces[i];
            Vector3 forceVector = forces[i].ForceVector;

            ApplyResistanceModifier(ref forceVector, forwardDirection, _frontalResistanceCoef.z, _backwardResistanceCoef.z);
            ApplyResistanceModifier(ref forceVector, rightDirection, _frontalResistanceCoef.x, _backwardResistanceCoef.x);
            ApplyResistanceModifier(ref forceVector, upDirection, _frontalResistanceCoef.y, _backwardResistanceCoef.y);

            force.ForceVector = forceVector;
            forces[i] = force;
        }

        return forces;
    }

    private void ApplyResistanceModifier(ref Vector3 forceVector, Vector3 dir, float forwardModifier, float backwardModifier)
    {
        dir = dir.normalized;

        Vector3 tangentPart = Vector3.Project(forceVector, dir), normalPart = forceVector - tangentPart;
        if (Vector3.Dot(tangentPart, dir) >= 0)
        {
            tangentPart *= backwardModifier;
        }
        else
        {
            tangentPart *= forwardModifier;
        }

        forceVector = tangentPart + normalPart;
    }
}
