using System.Collections.Generic;
using UnityEngine;

public class LimitedVelocityArchimedesForceProcessor : ICalculateWaterForceEffect
{
    [Min(float.Epsilon)] private float _maxSupportedVerticalSpeed = 1f;

    [Min(float.Epsilon)] private float _distanceErrorThreshold = 0.01f;

    public LimitedVelocityArchimedesForceProcessor()
    {

    }

    public LimitedVelocityArchimedesForceProcessor(float maxSupportedSpeed = 1f, float distanceErrorThreshold = 0.01f)
    {
        _maxSupportedVerticalSpeed = Mathf.Abs(maxSupportedSpeed);
        _distanceErrorThreshold = Mathf.Abs(distanceErrorThreshold);
    }

    public ForceEffectData CalculateForceEffect(List<ForceData> forces, WaterData water, RigidBodyData rb, float deltaTime = 0.02f)
    {
        Vector3 forceVector = ForceData.GetResultantForce(forces);
        Vector3 torque = ForceData.GetResultantTorque(forces, rb.WorldCenterOfMass);

        forceVector = Vector3.Project(forceVector, water.Plane.normal.normalized);

        float dist = torque.magnitude / forceVector.magnitude;
        if (dist < _distanceErrorThreshold)
        {
            torque *= ((_distanceErrorThreshold - dist) / _distanceErrorThreshold);
        }

        float curVerticalSpeed = Vector3.Dot(rb.LinearVelocity, water.Plane.normal.normalized);
        float maxAcceleration = Mathf.Max(_maxSupportedVerticalSpeed - curVerticalSpeed, 0);

        float curAcceleration = forceVector.magnitude / rb.Mass * deltaTime;

        if (curAcceleration > maxAcceleration)
        {
            forceVector *= (maxAcceleration / curAcceleration);
        }
        return new ForceEffectData(forceVector, torque);
    }
}
