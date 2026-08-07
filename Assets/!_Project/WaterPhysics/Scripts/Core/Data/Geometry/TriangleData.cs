using UnityEngine;

public class TriangleData
{
    public PointData A, B, C;

    public bool CalculateArchimedesForce = true;
    public bool CalculateResistanceForce = true;

    public Vector3 MidBC => (B.Position + C.Position) / 2;
    public Vector3 MidAC => (A.Position + C.Position) / 2;
    public Vector3 MidAB => (B.Position + A.Position) / 2;

    public Vector3 Centroid => (A.Position + B.Position + C.Position) / 3;

    public Vector3 MCenterA => (A.Position * 3 + MidBC * 2) / 5;
    public Vector3 MCenterB => (B.Position * 3 + MidAC * 2) / 5;
    public Vector3 MCenterC => (C.Position * 3 + MidAB * 2) / 5;

    public bool FacesOutwards = false;

    public TriangleData(PointData a, PointData b, PointData c)
    {
        A = a;
        B = b;
        C = c;
    }

    public float GetArea()
    {
        return Mathf.Abs(Vector3.Cross(B.Position - A.Position, C.Position - A.Position).magnitude) / 2f;
    }

    public Vector3 GetNormal()
    {
        int multiplier = FacesOutwards ? 1 : -1;

        Vector3 normal = (Vector3.Cross(B.Position - A.Position, C.Position - A.Position) * multiplier).normalized;
        return normal;
    }

    public Vector3 GetArchimedesForceCenter()
    {
        float depthSum = A.Depth + B.Depth + C.Depth;
        if (depthSum == 0)
        {
            return Centroid;
        }

        return (A.Position * (depthSum + A.Depth) + 
            B.Position * (depthSum + B.Depth) + 
            C.Position * (depthSum + C.Depth)) / 
            (depthSum * 4);
    }
}
