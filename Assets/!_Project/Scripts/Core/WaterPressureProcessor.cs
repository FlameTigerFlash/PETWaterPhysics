using UnityEngine;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;

public class WaterPressureProcessor : MonoBehaviour, IUpdatePosition
{
    [SerializeField] private Collider _collider;

    [SerializeField, Min(float.Epsilon)] private float _waterDensity = 10f;

    public Vector3 Current { get; set; } = Vector3.zero;

    public List<Force> ArchimedesForces => _archForces;
    public List<Force> ResistanceForces => _resistanceForces;

    private Plane _plane = new Plane(-Physics.gravity.normalized, Vector3.zero);

    private List<Polyhedron> _extendedFaces = new();
    private List<TriangleData> _triangles = new();

    private List<Force> _archForces = new();
    private List<Force> _resistanceForces = new();

    private Dictionary<Vector3, float> _pointDepth = new();

    private BaseColliderSplitter _splitter;
    private WaterForceCalculator _waterForceCalculator;
    private IPreprocessFaces _facePreprocessor;

    private PointTransform _prevTransform;
    private PointTransform _curTransform;

    private IReadOnlyList<Polyhedron> _colliderFaces;

    private float _deltaTime = 1f;

    private void Awake()
    {
        if (_collider is BoxCollider)
        {
            _splitter = new BoxColliderSplitter((BoxCollider)_collider);
        }
        else if (_collider is MeshCollider)
        {
            _splitter = new MeshColliderSplitter((MeshCollider)_collider);
        }
        else
        {
            throw new Exception("Collider type not supported.");
        }

        _facePreprocessor = new SimpleFacePreprocessor();
        _waterForceCalculator = new WaterForceCalculator();
    }

    private void OnDrawGizmos()
    {
        DrawColoredVerts();
        DrawTriangles();
        DrawForces();
        DrawVelocity();
    }

    public void SetPlane(Plane plane)
    {
        _plane = plane;
    }

    public void UpdatePosition(PointTransform newPointTransform, float fixedDeltaTime = 0)
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

        _splitter.UpdatePosition(_curTransform, _deltaTime);
        _facePreprocessor.UpdatePosition(_curTransform, _deltaTime);
    }

    public List<TriangleData> GetTrianglesFromFaces(in List<Polyhedron> faces)
    {
        return _waterForceCalculator.GetTrianglesFromFaces(faces);
    }

    public void FullUpdate()
    {
        UpdateColliderGeometry();
        UpdateFaces();
        _triangles = GetTrianglesFromFaces(_extendedFaces);
        _archForces = _waterForceCalculator.GetArchimedesForces(_triangles, _plane);
        _resistanceForces = _waterForceCalculator.GetWaterResistanceForces(_triangles, Current);
    }

    public void UpdateColliderGeometry()
    {
        if (_splitter == null)
        {
            return;
        }

        _splitter.Update();
        _colliderFaces = _splitter.Faces;
    }

    private void UpdateFaces()
    {
        if (_colliderFaces == null)
        {
            return;
        }
        _extendedFaces = _facePreprocessor.GetPreprocessedFaces(_colliderFaces, _plane);
    }

    private void DrawColoredVerts()
    {
        foreach (var face in _extendedFaces)
        {
            foreach (var vert in face.Vertices)
            {
                PointType pointType = vert.Type;
                if (pointType == PointType.OnWater)
                {
                    Gizmos.color = Color.blue;
                }
                else if (pointType == PointType.AboveWater)
                {
                    Gizmos.color = Color.green;
                }
                else if (pointType == PointType.BelowWater)
                {
                    Gizmos.color = Color.red;
                }
                else
                {
                    Gizmos.color = Color.black;
                }

                Gizmos.DrawSphere(vert, 0.3f);
            }
        }
    }

    private void DrawTriangles()
    {
        Gizmos.color = Color.yellow;
        foreach (var triangleData in _triangles)
        {
            Gizmos.DrawWireSphere(triangleData.A, 0.2f);
            Gizmos.DrawWireSphere(triangleData.B, 0.2f);
            Gizmos.DrawWireSphere(triangleData.C, 0.2f);

            Gizmos.DrawLine(triangleData.A, triangleData.B);
            Gizmos.DrawLine(triangleData.B, triangleData.C);
            Gizmos.DrawLine(triangleData.C, triangleData.A);

            Vector3 midPoint = triangleData.Centroid;
            Vector3 norm = triangleData.GetNormal();

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
        foreach (var force in _archForces)
        {
            Gizmos.DrawLine(force.ApplicationPoint, force.ApplicationPoint - force.ForceVector / 1000);
        }

        Gizmos.color = Color.green;
        foreach (var force in _resistanceForces)
        {
            Gizmos.DrawLine(force.ApplicationPoint, force.ApplicationPoint - force.ForceVector);
        }
    }

    private void DrawVelocity()
    {
        Gizmos.color = Color.red;
        foreach (var fig in _extendedFaces)
        {
            foreach (var vert in fig.Vertices)
            {
                Vector3 vel = vert.Velocity;
                Gizmos.DrawLine(vert.Position, vert.Position + vert.Velocity);
            }
        }
    }
}