using UnityEngine;
using System;
using System.Collections.Generic;

public enum PointType {AboveWater, OnWater, BelowWater};

public class WaterPressureProcessor : MonoBehaviour
{
    [SerializeField] private Collider _collider;

    [SerializeField, Min(float.Epsilon)] private float _waterDensity = 10f;

    public IReadOnlyList<Force> Forces => _forces;

    private Plane _plane = new Plane();

    private List<Polyhedron> _extendedFaces = new();
    private List<TrapezoidData> _trapezoids = new();
    private List<Force> _forces = new();

    private Dictionary<Vector3, PointType> _pointStatus = new();
    private Dictionary<Vector3, float> _pointDepth = new();

    private BaseColliderSplitter _splitter;

    private Vector3 _transformScale = Vector3.zero;

    private void Awake()
    {
        _transformScale = transform.lossyScale;
        _plane = new Plane(-Physics.gravity.normalized, Vector3.zero);

        if (_collider is BoxCollider)
        {
            _splitter = new BoxColliderSplitter((BoxCollider)_collider, transform);
        }
        else if (_collider is MeshCollider)
        {
            _splitter = new MeshColliderSplitter((MeshCollider)_collider);
        }
        else
        {
            throw new Exception("Collider type not supported.");
        }

        _splitter.Update();
    }

    private void FixedUpdate()
    {
        Vector3 lossyScale = transform.lossyScale;
        if (_transformScale != lossyScale)
        {
            _splitter.Update();
            _transformScale = lossyScale;
        }

        UpdateFaces();
        UpdateTrapezoids();
        UpdateForces();
        //Debug.Log($"Trapezoids count: {_trapezoids.Count}");
        //Debug.Log($"Faces count: {_extendedFaces.Count}");
    }

    private void OnDrawGizmos()
    {
        DrawColoredVerts();
        DrawTrapezoids();
        DrawForces();
    }

    private Force CalculateArchimedesForce(TrapezoidData trapezoidData)
    {
        Trapezoid trapezoid = trapezoidData.Trapezoid;

        Vector3 A = trapezoid.A,
            B = trapezoid.B,
            C = trapezoid.C,
            D = trapezoid.D;

        if (Mathf.Abs(GetSignedDistanceToPlane(A) - GetSignedDistanceToPlane(D)) >= 0.01f || 
            Mathf.Abs(GetSignedDistanceToPlane(C) - GetSignedDistanceToPlane(B)) >= 0.01f)
        {
            Debug.LogError($"Invalid trapezoid: A & D and B & C pairs should have similar depth.\n" +
                $"Currently: \n" +
                $"A: {A}, B: {B}, C: {C}, D: {D}\n" +
                $"BC: {C - B}, AD: {D - A}");
            return Force.Zero;
        }

        if (C - B == Vector3.zero && A - D == Vector3.zero)
        {
            return Force.Zero;
        }

        Vector3 intersectionPoint;
        bool intersection;
        float h;
        if (C - B == Vector3.zero)
        {
            Vector3 baseLine = (D - A);
            Plane aux = new Plane(baseLine, B);
            intersection = aux.GetIntersectionPoint(D, A - D, out intersectionPoint);
            h = Vector3.Distance(B, intersectionPoint);
        }
        else
        {
            Vector3 baseLine = (C - B);
            Plane aux = new Plane(baseLine, A);
            intersection = aux.GetIntersectionPoint(C, B - C, out intersectionPoint);
            h = Vector3.Distance(A, intersectionPoint);
        }
        
        if (!intersection)
        {
            Debug.LogWarning($"Cannot calculate trapezoid height.\n" +
                $"A: {A}, B: {B}, C: {C}, D: {D}\n" +
                $"BC: {C - B}, AD: {D - A}");
            return Force.Zero;
        }

        float a = Vector3.Distance(B, C);
        float b = Vector3.Distance(A, D);
        float g = Vector3.Magnitude(Physics.gravity);
        float h1 = -GetSignedDistanceToPlane(A);

        if (h == 0)
        {
            Debug.LogWarning("Trapezoid should have non-zero height.");
            return Force.Zero;
        }

        float sumAB = a + b;
        float sumA2B = a + 2 * b;
        float sumA3B = a + 3 * b;

        float gamma = Vector3.Angle(_plane.normal, -trapezoidData.Normal) * Mathf.Deg2Rad;

        float cosGamma = Mathf.Cos(gamma);
        float sinGamma = Mathf.Sin(gamma);

        float magnitude = (_waterDensity * g * cosGamma) * ((h1 * h * sumAB / 2) + (sinGamma * h * h * sumA2B / 6));

        float ratio = (2 * h1 * sumA2B + sinGamma * h * sumA3B) /
            (6 * h1 * sumAB + 2 * sinGamma * h * sumA2B);

        Vector3 midBC = (B + C) / 2, midAD = (A + D) / 2;
        Vector3 applicationPoint = (midBC + (midAD - midBC) * ratio);

        Debug.Log($"Angle: {gamma},\n" +
            $"Sin: {sinGamma},\n" +
            $"Cos: {cosGamma},\n" +
            $"H: {h},\n" +
            $"H1: {h1},\n" +
            $"sumAB: {sumAB},\n" +
            $"magnitude: {magnitude}");

        return new Force(_plane.normal * magnitude, applicationPoint);
    }

    private PointType GetPointType(Vector3 point)
    {
        if (_pointStatus.ContainsKey(point))
        {
            return _pointStatus[point];
        }

        float dist = _plane.GetDistanceToPoint(point);
        if (Mathf.Abs(dist) <= float.Epsilon * 100)
        {
            _pointStatus[point] = PointType.OnWater;
        }
        else if (dist > 0)
        {
            _pointStatus[point] = PointType.AboveWater;
        }
        _pointStatus[point] = PointType.BelowWater;
        return _pointStatus[point];
    }

    private float GetSignedDistanceToPlane(Vector3 point)
    {
        if (_pointDepth.ContainsKey(point))
        {
            return _pointDepth[point];
        }

        _pointDepth[point] = _plane.GetDistanceToPoint(point);
        return _pointDepth[point];
    }

    private void UpdateForces()
    {
        _forces.Clear();
        foreach (var data in _trapezoids)
        {
            Force archimedesForce = CalculateArchimedesForce(data);
            _forces.Add(archimedesForce);
        }
    }

    private void UpdateTrapezoids()
    {
        _trapezoids.Clear();
        foreach (var face in _extendedFaces)
        {
            SplitFace(face);
        }
    }

    private void SplitFace(Polyhedron face)
    {
        if (face.Vertices.Count < 3)
        {
            Debug.LogWarning("Faces must have no less than 3 vertices.");
            return;
        }

        float minHeight = float.NegativeInfinity;
        LinkedListNode<Vector3> upperNode = null;

        bool isFullySubmerged = true;
        int pointsOnWaterCount = 0;

        for (LinkedListNode<Vector3> current = face.Vertices.First; current != null; current = current.Next)
        {
            float curHeight = _plane.GetDistanceToPoint(current.Value);
            if (curHeight > minHeight)
            {
                upperNode = current;
                minHeight = curHeight;
            }

            PointType pointType = GetPointType(current.Value);
            if (pointType == PointType.AboveWater)
            {
                isFullySubmerged = false;
            }
            else if (pointType == PointType.OnWater)
            {
                pointsOnWaterCount++;
            }
        }

        if (upperNode == null)
        {
            Debug.Log("Cannot find upper node.");
            return;
        }

        Vector3 norm = face.GetNormal();

        if (isFullySubmerged)
        {
            SplitSector(upperNode, upperNode, norm);
            return;
        }

        if (pointsOnWaterCount == 0)
        {
            return;
        }

        if (pointsOnWaterCount % 2 != 0)
        {
            Debug.LogWarning("Invalid face splitting");
            return;
        }

        LinkedListNode<Vector3> leftNode = upperNode, rightNode = upperNode;

        while (GetPointType(leftNode.Value) != PointType.OnWater)
        {
            leftNode = leftNode.NextInCircle();
            if (leftNode == rightNode)
            {
                break;
            }
        }
        if (GetPointType(leftNode.Value) != PointType.OnWater)
        {
            Debug.LogWarning("Cannot find point on water.");
            return;
        }
        else
        {
            //Debug.Log("Success.");
        }

        int visitedCount = 0;
        rightNode = leftNode;
        bool isBelowSurface = true;
        while (visitedCount < pointsOnWaterCount && visitedCount <= face.Vertices.Count * 2)
        {
            leftNode = leftNode.NextInCircle();
            PointType pointType = GetPointType(leftNode.Value);

            if (pointType == PointType.AboveWater)
            {
                isBelowSurface = false;
            }
            else if (pointType == PointType.OnWater)
            {
                visitedCount++;
                if (isBelowSurface)
                {
                    SplitSector(leftNode, rightNode, norm);
                }
                isBelowSurface = true;
                rightNode = leftNode;
            }
        }
    }

    private void SplitSector(in LinkedListNode<Vector3> leftNode, in LinkedListNode<Vector3> rightNode, in Vector3 normal)
    {
        if (leftNode.PreviousInCircle() == rightNode)
        {
            return;
        }

        LinkedListNode<Vector3> ln = leftNode, rn = rightNode;

        Vector3 A = ln.Value, B = ln.Value, C = rn.Value, D = rn.Value;
        if (Mathf.Abs(GetSignedDistanceToPlane(B) - GetSignedDistanceToPlane(C)) >= 0.01f)
        {
            Debug.LogError($"Nodes have diffirent depth.\n" +
                $"B: {B};" +
                $"C: {C}");
        }
        do
        {
            B = A;
            C = D;

            Vector3 leftCandidate = ln.PreviousInCircle().Value;
            Vector3 rightCandidate = rn.NextInCircle().Value;

            Vector3 leftPoint, rightPoint;

            float leftDist = GetSignedDistanceToPlane(leftCandidate), rightDist = GetSignedDistanceToPlane(rightCandidate);
            if (leftDist > 0.01f || rightDist > 0.01f)
            {
                Debug.LogError("Splitting must only check submerged points.");
            }
            bool intersects = false;
            if (Mathf.Abs(leftDist - rightDist) <= float.Epsilon * 100)
            {
                ln = ln.PreviousInCircle();
                leftPoint = ln.Value;

                rn = rn.NextInCircle();
                rightPoint = rn.Value;

                if (ln.NextInCircle() == rn)
                {
                    break;
                }
            }
            else if (leftDist > rightDist)
            {
                ln = ln.PreviousInCircle();
                leftPoint = ln.Value;

                Plane parallelPlane = new Plane(_plane.normal, leftPoint);
                intersects = parallelPlane.GetIntersectionPoint(rn.Value, rn.NextInCircle().Value - rn.Value, out rightPoint);
            }
            else
            {
                rn = rn.NextInCircle();
                rightPoint = rn.Value;

                Plane parallelPlane = new Plane(_plane.normal, rightPoint);
                intersects = parallelPlane.GetIntersectionPoint(ln.Value, ln.PreviousInCircle().Value - ln.Value, out leftPoint);
            }

            A = leftPoint;
            D = rightPoint;

            if (GetSignedDistanceToPlane(A) != GetSignedDistanceToPlane(D))
            {
                C = D;
                D = A;
                Debug.LogError($"A and D are not on the same level. A: {A}, B: {B}, C: {C}, D: {D}\n" +
                    $"Intersects: {intersects}");
            }

            if (Vector3.Magnitude(B - A) <= float.Epsilon * 100 || Vector3.Magnitude(B - D) <= float.Epsilon * 100)
            {
                //Debug.Log("Skipping empty trapezoid.");
                continue;
            }

            TrapezoidData trapezoidData = new TrapezoidData();
            trapezoidData.Trapezoid = new Trapezoid(A, B, C, D);
            trapezoidData.Normal = normal;

            _trapezoids.Add(trapezoidData);
        } while (ln != rn && ln.PreviousInCircle() != rn);
    }

    private void UpdateFaces()
    {
        if (_splitter == null)
        {
            return;
        }

        _extendedFaces.Clear();
        _pointStatus.Clear();
        _pointDepth.Clear();

        var faces = _splitter.Faces;
        foreach (var face in faces)
        {
            LinkedList<Vector3> verts = new LinkedList<Vector3>(face.Vertices);

            TransformLinkedList(verts);
            FillWaterContacts(verts);

            _extendedFaces.Add(new Polyhedron(verts));
        }
    }

    private void TransformLinkedList(LinkedList<Vector3> verts)
    {
        for (var current = verts.First; current != null; current = current.Next)
        {
            Vector3 pos = transform.position + (transform.rotation * current.Value);
            current.Value = pos;
        }
    }

    private void FillWaterContacts(LinkedList<Vector3> verts)
    {
        var prev = verts.First;
        var current = prev.Next;
        if (current == null)
        {
            return;
        }

        Action<LinkedListNode<Vector3>, LinkedListNode<Vector3>> iterate = (LinkedListNode<Vector3> prev, LinkedListNode<Vector3> current) =>
        {
            Vector3 aTemp = prev.Value, bTemp = current.Value;
            float aDist = GetSignedDistanceToPlane(aTemp);

            if (!_plane.SameSide(aTemp, bTemp))
            {
                if (aDist > 0)
                {
                    _pointStatus[aTemp] = PointType.AboveWater;
                    _pointStatus[bTemp] = PointType.BelowWater;
                }
                else
                {
                    _pointStatus[bTemp] = PointType.AboveWater;
                    _pointStatus[aTemp] = PointType.BelowWater;
                }

                bool intersects = _plane.GetIntersectionPoint(aTemp, bTemp - aTemp, out var waterPoint);
                if (intersects)
                {
                    verts.AddAfter(prev, waterPoint);
                    _pointStatus[waterPoint] = PointType.OnWater;
                }
            }

            else
            {
                if (aDist > 0)
                {
                    _pointStatus[aTemp] = PointType.AboveWater;
                }
                else
                {
                    _pointStatus[aTemp] = PointType.BelowWater;
                }
                _pointStatus[bTemp] = _pointStatus[aTemp];
            }
        };

        for (; current != null; current = current.Next)
        {
            iterate(prev, current);
            prev = current;
        }

        iterate(verts.Last, verts.First);
    }

    private void DrawColoredVerts()
    {
        foreach (var face in _extendedFaces)
        {
            foreach (var vert in face.Vertices)
            {
                PointType pointType = GetPointType(vert);
                if (pointType == PointType.OnWater)
                {
                    Gizmos.color = Color.blue;
                }
                else if (pointType == PointType.AboveWater)
                {
                    Gizmos.color = Color.green;
                }
                else
                {
                    Gizmos.color = Color.red;
                }

                Gizmos.DrawSphere(vert, 0.3f);
            }
        }
    }

    private void DrawTrapezoids()
    {
        //Debug.Log($"{_trapezoids.Count} trapezoids.");
        Gizmos.color = Color.yellow;
        foreach (var trapezoidData in _trapezoids)
        {
            Trapezoid trapezoid = trapezoidData.Trapezoid;

            Gizmos.DrawWireSphere(trapezoid.A, 0.2f);
            Gizmos.DrawWireSphere(trapezoid.B, 0.2f);
            Gizmos.DrawWireSphere(trapezoid.C, 0.2f);
            Gizmos.DrawWireSphere(trapezoid.D, 0.2f);

            Gizmos.DrawLine(trapezoid.A, trapezoid.B);
            Gizmos.DrawLine(trapezoid.B, trapezoid.C);
            Gizmos.DrawLine(trapezoid.C, trapezoid.D);
            Gizmos.DrawLine(trapezoid.D, trapezoid.A);

            Vector3 midPoint = (trapezoid.A + trapezoid.B + trapezoid.C + trapezoid.D) / 4;
            Vector3 norm = trapezoidData.Normal;

            if (norm == Vector3.zero)
            {
                Debug.LogWarning("Zero normal");
            }

            Gizmos.DrawLine(midPoint, midPoint + norm);
        }
    }

    private void DrawForces()
    {
        Gizmos.color = Color.purple;
        foreach (var force in _forces)
        {
            Gizmos.DrawLine(force.ApplicationPoint, force.ApplicationPoint - force.ForceVector / 10);
        }
    }
}

public class TrapezoidData
{
    public Trapezoid Trapezoid;

    public Vector3 Normal;

    public float Depth;
}