using UnityEngine;

public class TransformData
{
    public Vector3 Position = Vector3.zero;
    public Quaternion Rotation = Quaternion.identity;
    public Vector3 LossyScale = Vector3.one;

    public TransformData()
    {
        Position = Vector3.zero;
        Rotation = Quaternion.identity;
    }

    public TransformData(Vector3 pos, Quaternion rot, Vector3 scale)
    {
        Position = pos;
        Rotation = rot;
        LossyScale = scale;
    }

    public TransformData(in Transform transform) : this(transform.position, transform.rotation, transform.lossyScale)
    {

    }

    public Vector3 TransformPoint(Vector3 localPoint)
    {
        return Position + Rotation * localPoint;
    }

    public Vector3 TransformDirection(Vector3 localDirection)
    {
        return Rotation * localDirection;
    }

    public Vector3 InverseTransformPoint(Vector3 worldPoint)
    {
        Vector3 relative = worldPoint - Position;
        return Quaternion.Inverse(Rotation) * relative;
    }

    public Vector3 InverseTransformDirection(Vector3 worldDirection)
    {
        return Quaternion.Inverse(Rotation) * worldDirection;
    }
}
