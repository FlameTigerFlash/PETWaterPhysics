using UnityEngine;


public struct RigidBodyData
{
    public TransformData TrData;

    public float Mass;

    public Vector3 WorldCenterOfMass;

    public Vector3 InertiaTensor;
    public Vector3 LinearVelocity;
    public Vector3 AngularVelocity;

    public RigidBodyData(TransformData tr, float mass, Vector3 center, Vector3 linearVelocity, Vector3 inertiaTensor, Vector3 angularVelocity)
    {
        TrData = tr;
        Mass = mass;
        WorldCenterOfMass = center;
        InertiaTensor = inertiaTensor;
        LinearVelocity = linearVelocity;
        AngularVelocity = angularVelocity;
    }

    public RigidBodyData(in Rigidbody rb)
    {
        TrData = new TransformData(rb.transform);
        Mass = rb.mass;
        WorldCenterOfMass = rb.worldCenterOfMass;
        InertiaTensor = rb.inertiaTensor;
        LinearVelocity = rb.linearVelocity;
        AngularVelocity = rb.angularVelocity;
    }

    public RigidBodyData ApplyForceEffect(ForceEffectData forceEffect, float deltaTime)
    {
        Vector3 newLinearVelocity = LinearVelocity + forceEffect.ForceVector * deltaTime / Mass;

        Vector3 localTorque = Quaternion.Inverse(TrData.Rotation) * forceEffect.TorqueVector;
        Vector3 localAngularAccel = new Vector3(
            localTorque.x / InertiaTensor.x,
            localTorque.y / InertiaTensor.y,
            localTorque.z / InertiaTensor.z
        );
        Vector3 worldAngularAccel = TrData.Rotation * localAngularAccel;
        Vector3 newAngularVelocity = AngularVelocity + worldAngularAccel * deltaTime;

        return new RigidBodyData(TrData, Mass, WorldCenterOfMass, newLinearVelocity, InertiaTensor, newAngularVelocity);
    }
}
