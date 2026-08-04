using System.Collections.Generic;
using UnityEngine;

public class DummyForceProcessor : ICalculateWaterForceEffect
{
    public ForceEffectData CalculateForceEffect(List<ForceData> forces, WaterData water, RigidBodyData rb, float deltaTime=0.02f)
    {
        Vector3 forceVector = ForceData.GetResultantForce(forces);
        Vector3 torque = ForceData.GetResultantTorque(forces, rb.WorldCenterOfMass);

        return new ForceEffectData(forceVector, torque);
    }
}
