using Unity.VisualScripting;
using UnityEngine;

public struct ForceEffectData
{
    public Vector3 ForceVector;
    public Vector3 TorqueVector;

    public ForceEffectData(Vector3 force, Vector3 torque)
    {
        ForceVector = force;
        TorqueVector = torque;
    }

    public static ForceEffectData operator -(ForceEffectData a)
    {
        return new ForceEffectData(-a.ForceVector, -a.TorqueVector);
    }

    public static ForceEffectData operator +(ForceEffectData a, ForceEffectData b)
    {
        return new ForceEffectData(a.ForceVector + b.ForceVector, a.TorqueVector + b.TorqueVector);
    }
}
