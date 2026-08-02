using UnityEngine;

public enum PointType { NoneType, AboveWater, OnWater, BelowWater };

public class PointData 
{
    public Vector3 Position = Vector3.zero;
    public Vector3 Velocity = Vector3.zero;
    public PointType Type = PointType.NoneType;
    public float Depth = 0;

    public PointData()
    {
        Position = Vector3.zero;
    }

    public PointData(Vector3 position)
    {
        Position = position;
    }

    public static implicit operator Vector3(PointData data)
    {
        return data?.Position ?? Vector3.zero;
    }

    public static implicit operator PointData(Vector3 vec)
    {
        return new PointData(vec);
    }
}
