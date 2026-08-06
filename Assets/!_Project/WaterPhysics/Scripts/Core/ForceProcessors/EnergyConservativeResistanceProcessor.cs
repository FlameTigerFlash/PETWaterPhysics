using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public class EnergyConservativeResistanceProcessor : BaseForceProcessor
{
    public override ForceEffectData CalculateForceEffect(List<ForceData> forces, WaterData water, RigidBodyData rb, float deltaTime = 0.02f)
    {
        Vector3 forceVector = ForceData.GetResultantForce(forces);
        Vector3 torque = ForceData.GetResultantTorque(forces, rb.WorldCenterOfMass);

        float maxMagnitude = forceVector.magnitude;
        if (maxMagnitude == 0)
        {
            if (torque.magnitude == 0)
            {
                return new ForceEffectData(Vector3.zero, Vector3.zero);
            }
            maxMagnitude = torque.magnitude;
        }
        float optimalMagnitude = CalculateOptimalMagnitude(forceVector, torque, water, rb, deltaTime);

        forceVector *= (optimalMagnitude / maxMagnitude);
        torque *= (optimalMagnitude / maxMagnitude);

        return new ForceEffectData(forceVector, torque);
    }

    private float CalculateOptimalMagnitude(Vector3 forceVector, Vector3 torque, WaterData water, RigidBodyData rb, float deltaTime = 0.02f)
    {
        Vector3 normalizedAcceleration = forceVector.normalized;
        Vector3 normalizedTorque = torque.normalized;
        Vector3 relativeVelocity = rb.LinearVelocity - water.Current;

        float maxMagnitude = forceVector.magnitude;

        if (maxMagnitude == 0)
        {
            if (normalizedTorque.magnitude == 0)
            {
                return 0;
            }
            maxMagnitude = normalizedTorque.magnitude;
        }

        float deltaVelX = normalizedAcceleration.x / rb.Mass * deltaTime,
            deltaVelY = normalizedAcceleration.y / rb.Mass * deltaTime,
            deltaVelZ = normalizedAcceleration.z / rb.Mass * deltaTime;

        float massMult = (rb.Mass / 2);

        float xSpeedA = massMult * Mathf.Pow(deltaVelX, 2),
            ySpeedA = massMult * Mathf.Pow(deltaVelY, 2),
            zSpeedA = massMult * Mathf.Pow(deltaVelZ, 2);

        float xSpeedB = massMult * 2 * deltaVelX * relativeVelocity.x,
            ySpeedB = massMult * 2 * deltaVelY * relativeVelocity.y,
            zSpeedB = massMult * 2 * deltaVelZ * relativeVelocity.z;

        float deltaRotX = normalizedTorque.x / rb.InertiaTensor.x * deltaTime,
            deltaRotY = normalizedTorque.y / rb.InertiaTensor.y * deltaTime,
            deltaRotZ = normalizedTorque.z / rb.InertiaTensor.z * deltaTime;

        Vector3 inertiaMult = rb.InertiaTensor / 2;

        float xRotA = inertiaMult.x * Mathf.Pow(deltaRotX, 2),
            yRotA = inertiaMult.y * Mathf.Pow(deltaRotY, 2),
            zRotA = inertiaMult.z * Mathf.Pow(deltaRotZ, 2);

        float xRotB = inertiaMult.x * 2 * deltaRotX * rb.AngularVelocity.x,
            yRotB = inertiaMult.y * 2 * deltaRotY * rb.AngularVelocity.y,
            zRotB = inertiaMult.z * 2 * deltaRotZ * rb.AngularVelocity.z;

        float totalA = xSpeedA + ySpeedA + zSpeedA + xRotA + yRotA + zRotA;
        float totalB = xSpeedB + ySpeedB + zSpeedB + xRotB + yRotB + zRotB;

        if (totalA == 0)
        {
            return 0;
        }

        float optimal = (-totalB / (totalA * 2));
        return Mathf.Clamp(optimal, 0, maxMagnitude);
    }
}
