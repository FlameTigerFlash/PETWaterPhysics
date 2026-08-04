using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SimpleFacePreprocessor : IPreprocessFaces
{
    private float _deltaTime;
    private TransformData _prevTransform;
    private TransformData _curTransform;

    private Dictionary<Vector3, float> _pointDepth = new();

    public void UpdatePosition(TransformData newPointTransform, float fixedDeltaTime = 1)
    {
        if (_curTransform == null)
        {
            _curTransform = newPointTransform;
            _prevTransform = _curTransform;
        }
        else
        {
            _prevTransform = _curTransform;
            _curTransform = newPointTransform;
        }

        _deltaTime = fixedDeltaTime;

        _pointDepth.Clear();
    }

    public List<Polyhedron> GetPreprocessedFaces(in IReadOnlyList<Polyhedron> faces, WaterData water)
    {
        var plane = water.Plane;

        List<Polyhedron> newFaces = new();
        foreach (var face in faces)
        {
            newFaces.Add(GetPreprocessedFace(face, plane));
        }

        return newFaces;
    }

    private Polyhedron GetPreprocessedFace(in Polyhedron face, Plane plane)
    {
        LinkedList<PointData> verts = new LinkedList<PointData>(face.Vertices);

        TransformLinkedList(verts);
        FillWaterContacts(verts, plane);

        return new Polyhedron(verts);
    }

    private PointType GetPointType(PointData point, Plane plane)
    {
        if (point.Type != PointType.NoneType)
        {
            return point.Type;
        }

        float dist = plane.GetDistanceToPoint(point);
        if (Mathf.Abs(dist) <= float.Epsilon * 100)
        {
            point.Type = PointType.OnWater;
        }
        else if (dist > 0)
        {
            point.Type = PointType.AboveWater;
        }
        else
        {
            point.Type = PointType.BelowWater;
        }
        return point.Type;
    }

    private float GetCachedDistanceToPlane(Vector3 point, Plane plane)
    {
        if (_pointDepth.ContainsKey(point))
        {
            return _pointDepth[point];
        }

        _pointDepth[point] = plane.GetDistanceToPoint(point);
        return _pointDepth[point];
    }

    private void TransformLinkedList(LinkedList<PointData> verts)
    {
        //Debug.Log($"Prev transform: {_prevTransform.Position}, Current transform: {_curTransform.Position}, Delta time: {_deltaTime}");
        for (var current = verts.First; current != null; current = current.Next)
        {
            Vector3 localPos = current.Value.Position;
            Vector3 curPos = _curTransform.Position + (_curTransform.Rotation * localPos);
            Vector3 oldPos = _prevTransform.Position + (_prevTransform.Rotation * localPos);

            current.Value.Position = curPos;
            current.Value.Velocity = (curPos - oldPos) / _deltaTime;
        }
    }

    private void FillWaterContacts(LinkedList<PointData> verts, Plane plane)
    {
        var prev = verts.First;
        var current = prev.Next;
        if (current == null)
        {
            return;
        }

        prev.Value.Depth = -GetCachedDistanceToPlane(prev.Value, plane);
        prev.Value.Type = GetPointType(prev.Value, plane);

        Action<LinkedListNode<PointData>, LinkedListNode<PointData>> iterate = (LinkedListNode<PointData> prev, LinkedListNode<PointData> current) =>
        {
            PointData bTemp = prev.Value, aTemp = current.Value;

            float aDist = GetCachedDistanceToPlane(aTemp, plane);

            aTemp.Depth = -aDist;
            aTemp.Type = GetPointType(aTemp, plane);

            if (aTemp.Type == PointType.AboveWater && bTemp.Type == PointType.BelowWater ||
            aTemp.Type == PointType.BelowWater && bTemp.Type == PointType.AboveWater)
            {
                bool intersects = plane.GetIntersectionPoint(aTemp, bTemp.Position - aTemp.Position, out var waterPoint);
                if (intersects)
                {
                    PointData waterPointData = new PointData(waterPoint);
                    verts.AddAfter(prev, waterPointData);
                    waterPointData.Type = PointType.OnWater;
                    waterPointData.Depth = 0;

                    float a = Vector3.Distance(waterPointData.Position, aTemp.Position);
                    float b = Vector3.Distance(waterPointData.Position, bTemp.Position);

                    waterPointData.Velocity = (a * aTemp.Velocity + b * bTemp.Velocity) / (a + b);
                }
            }
        };

        for (; current != null; current = current.Next)
        {
            iterate(prev, current);
            prev = current;
        }

        iterate(verts.Last, verts.First);
    }
}
