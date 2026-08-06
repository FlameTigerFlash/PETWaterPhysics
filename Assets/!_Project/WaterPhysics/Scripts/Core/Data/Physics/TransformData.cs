using UnityEngine;

public class TransformData
{
    public Vector3 Position = Vector3.zero;
    public Quaternion Rotation = Quaternion.identity;
    public float Scale = 1.0f;

    public TransformData()
    {
        Position = Vector3.zero;
        Rotation = Quaternion.identity;
    }

    public TransformData(Vector3 pos, Quaternion rot, float scale = 1.0f)
    {
        Position = pos;
        Rotation = rot;
        Scale = scale;
    }

    public TransformData(in Transform transform) : this(transform.position, transform.rotation)
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
