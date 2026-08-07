using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class WaterForceCalculator
{
    public List<ForceData> GetArchimedesForces(in List<TriangleData> underwaterTriangles, WaterData water)
    {
        var plane = water.Plane;
        var density = water.Density;

        List<ForceData> ret = new();
        Vector3 normal = plane.normal.normalized;

        foreach (TriangleData triangle in underwaterTriangles)
        {
            if (!triangle.CalculateArchimedesForce)
            {
                continue;
            }
            float centroidDepth = (triangle.A.Depth + triangle.B.Depth + triangle.C.Depth) / 3;

            float magnitude = density * Physics.gravity.magnitude * triangle.GetArea() * centroidDepth;

            Vector3 applicationPoint = triangle.GetArchimedesForceCenter();

            ret.Add(new ForceData(-triangle.GetNormal() * magnitude, applicationPoint));
        }

        return ret;
    }

    public List<ForceData> GetWaterResistanceForces(in List<TriangleData> underwaterTriangles, WaterData water, float sd = 1)
    {
        var current = water.Current;
        var density = water.Density;

        List<ForceData> ret = new();
        float waterConstant = sd * density;

        foreach (TriangleData triangle in underwaterTriangles)
        {
            if (!triangle.CalculateResistanceForce)
            {
                continue;
            }
            float triangleArea = triangle.GetArea();

            Vector3 triangleNormal = triangle.GetNormal().normalized;
            Vector3 AFullVelocity = (triangle.A.Velocity - current),
                BFullVelocity = (triangle.B.Velocity - current),
                CFullVelocity = (triangle.C.Velocity - current);

            //Debug.Log($"Velocity: {AFullVelocity}, {BFullVelocity}, {CFullVelocity}.");

            float ADot = Vector3.Dot(AFullVelocity, triangleNormal),
                BDot = Vector3.Dot(BFullVelocity, triangleNormal),
                CDot = Vector3.Dot(CFullVelocity, triangleNormal);

            float magnitudeA = waterConstant * triangleArea * ADot * Mathf.Abs(ADot) / 6,
                magnitudeB = waterConstant * triangleArea * BDot * Mathf.Abs(BDot) / 6,
                magnitudeC = waterConstant * triangleArea * CDot * Mathf.Abs(CDot) / 6;

            ret.Add(new ForceData(-triangleNormal * magnitudeA, triangle.MCenterA));
            ret.Add(new ForceData(-triangleNormal * magnitudeB, triangle.MCenterB));
            ret.Add(new ForceData(-triangleNormal * magnitudeC, triangle.MCenterC));
        }

        return ret;
    }

    public List<TriangleData> GetTrianglesFromFaces(in List<Polyhedron> faces)
    {
        List<TriangleData> ret = new();
        foreach (var face in faces)
        {
            if (face == null)
            {
                continue;
            }

            Vector3 faceNormal = face.GetNormal();
            if (faceNormal.magnitude == 0)
            {
                continue;
            }

            var newTriangles = SplitFace(face);
            foreach (var triangle in newTriangles)
            {
                Vector3 triangleNormal = triangle.GetNormal();
                if (triangleNormal.magnitude == 0)
                {
                    continue;
                }
                if (Vector3.Dot(triangleNormal, faceNormal) == 0)
                {
                    continue;
                }
                if (Vector3.Dot(triangleNormal, faceNormal) <= 0)
                {
                    triangle.FacesOutwards = (!triangle.FacesOutwards);
                }
                triangle.CalculateArchimedesForce = face.CalculateArchimedesForce;
                triangle.CalculateResistanceForce = face.CalculateResistanceForce;
            }
            ret.AddRange(newTriangles);
        }
        return ret;
    }

    protected List<TriangleData> SplitFace(in Polyhedron face)
    {
        List<TriangleData> ret = new();
        if (face.Vertices.Count < 3)
        {
            Debug.LogWarning("Faces must have no less than 3 vertices.");
            return ret;
        }

        float minDepth = float.PositiveInfinity;
        LinkedListNode<PointData> upperNode = null;
        int cnt = 0;
        for (LinkedListNode<PointData> current = face.Vertices.First; current != null; current = current.Next)
        {
            var data = current.Value;
            if (data.Depth < minDepth)
            {
                upperNode = current;
                minDepth = data.Depth;
            }
            cnt++;
        }

        LinkedListNode<PointData> startingNode = face.Vertices.First;
        if (minDepth <= 0 && upperNode.Value.Type != PointType.BelowWater)
        {
            startingNode = upperNode;
            //Debug.Log($"Starting with upper: {upperNode.Value.Position}");
        }

        LinkedListNode<PointData> forwardNode = startingNode;
        List<PointData> points = new();
        do
        {
            if (forwardNode.Value.Type == PointType.AboveWater)
            {
                if (points.Count >= 3)
                {
                    ret.AddRange(Triangulate(points));
                }
                points.Clear();
            }
            else
            {
                points.Add(forwardNode.Value);
            }
            forwardNode = forwardNode.NextInCircle();
        } while (forwardNode != startingNode);

        if (points.Count >= 3)
        {
            ret.AddRange(Triangulate(points));
        }

        return ret;
    }

    protected List<TriangleData> Triangulate(List<PointData> points)
    {
        List<TriangleData> ret = new();
        if (points.Count < 3)
        {
            return ret;
        }

        for (int i = 2; i < points.Count; i++)
        {
            var triangle = new TriangleData(points[0], points[i-1], points[i]);
            if (triangle.GetArea() < float.Epsilon * 100)
            {
                continue;
            }
            ret.Add(triangle);
        }

        return ret;
    }
}
