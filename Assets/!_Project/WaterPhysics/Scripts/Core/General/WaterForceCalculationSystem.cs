using UnityEngine;
using System;
using System.Collections.Generic;

public class WaterForceCalculationSystem : MonoBehaviour, IUpdatePosition
{
    #region fields
    [Header("General")]
    [SerializeField] private Collider _collider;

    [Header("Additional faces")]
    [SerializeField] private List<MonoBehaviour> _mbFaceProviders = new();

    [Header("Debug")]
    [SerializeField] private bool _drawColoredVerts = false;
    [SerializeField] private bool _drawTriangles = false;
    [SerializeField] private bool _drawVelocity = false;
    [SerializeField] private bool _drawFaceNormals = false;

    public List<ForceData> ArchimedesForces => _archForces;
    public List<ForceData> ResistanceForces => _resistanceForces;

    private List<Polyhedron> _mainFaces = new();
    private List<Polyhedron> _allFaces = new();
    private List<Polyhedron> _extendedFaces = new();
    private List<TriangleData> _triangles = new();

    private List<IGetFaces> _faceProviders = new();

    private List<ForceData> _archForces = new();
    private List<ForceData> _resistanceForces = new();

    private BaseColliderSplitter _splitter;
    private WaterForceCalculator _waterForceCalculator;
    private IPreprocessFaces _facePreprocessor;

    private TransformData _prevTransform;
    private TransformData _curTransform;

    private WaterData _water;

    private float _deltaTime = 1f;
    #endregion

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

        foreach (var provider in _mbFaceProviders)
        {
            if (!(provider is IGetFaces))
            {
                Debug.LogError($"Face providers must extend the {nameof(IGetFaces)} interface. {nameof(provider)} does not.");
                continue;
            }
            _faceProviders.Add(provider as IGetFaces);
        }

        //_collider.enabled = false;

        _facePreprocessor = new SimpleFacePreprocessor();
        _waterForceCalculator = new WaterForceCalculator();
    }

    private void OnDrawGizmosSelected()
    {
        if (_drawColoredVerts)
        {
            DrawColoredVerts();
        }
        if (_drawTriangles)
        {
            DrawTriangles();
        }
        if (_drawVelocity)
        {
            DrawVelocity();
        }
        if (_drawFaceNormals)
        {
            DrawFaceNormals();
        }
    }

    public void SetWaterData(WaterData water)
    {
        _water = water;
    }

    public void UpdatePosition(TransformData newPointTransform, float fixedDeltaTime = 0)
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
        _archForces = _waterForceCalculator.GetArchimedesForces(_triangles, _water);
        _resistanceForces = _waterForceCalculator.GetWaterResistanceForces(_triangles, _water);
    }

    public void UpdateColliderGeometry()
    {
        _allFaces.Clear();
        if (_splitter != null)
        {
            _splitter.Update();
            _mainFaces = _splitter.Faces;
        }
        else
        {
            _mainFaces.Clear();
        }

        foreach (var faceGetter in _faceProviders)
        {
            if (faceGetter == null)
            {
                continue;
            }
            _allFaces.AddRange(faceGetter.GetFaces());
        }
        _allFaces.AddRange(_mainFaces);
    }

    private void UpdateFaces()
    {
        if (_allFaces.Count == 0)
        {
            return;
        }
        _extendedFaces = _facePreprocessor.GetPreprocessedFaces(_allFaces, _water);
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

    private void DrawFaceNormals()
    {
        Gizmos.color = Color.white;
        foreach (var face in _extendedFaces)
        {
            Vector3 center = Vector3.zero;
            foreach (var vert in face.Vertices)
            {
                center += vert.Position;
            }
            center /= face.Vertices.Count;

            Vector3 norm = face.GetNormal();

            Gizmos.DrawLine(center, center + norm * 3);
        }
    }

    private void DrawTriangles()
    {
        Gizmos.color = Color.yellow;
        foreach (var triangleData in _triangles)
        {
            Vector3 norm = triangleData.GetNormal();

            if (norm == Vector3.zero)
            {
                continue;
            }

            Gizmos.DrawWireSphere(triangleData.A, 0.2f);
            Gizmos.DrawWireSphere(triangleData.B, 0.2f);
            Gizmos.DrawWireSphere(triangleData.C, 0.2f);

            Gizmos.DrawLine(triangleData.A, triangleData.B);
            Gizmos.DrawLine(triangleData.B, triangleData.C);
            Gizmos.DrawLine(triangleData.C, triangleData.A);

            Vector3 midPoint = triangleData.Centroid;

            Gizmos.DrawLine(midPoint, midPoint + norm);
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