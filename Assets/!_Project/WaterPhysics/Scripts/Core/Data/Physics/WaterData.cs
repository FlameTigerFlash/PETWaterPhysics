using UnityEngine;

public struct WaterData
{
    public Plane Plane;

    public float Density;

    public Vector3 Current;

    public WaterData(Plane plane)
    {
        Plane = plane;
        Density = 1000f;
        Current = Vector3.zero;
    }

    public WaterData(Plane plane, Vector3 current, float density = 1000f)
    {
        Plane = plane;
        Density = density;
        Current = current;
    }
}
