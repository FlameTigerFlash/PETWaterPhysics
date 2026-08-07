using System.Collections.Generic;
using UnityEngine;

public abstract class BaseColliderSplitter : IUpdatePosition, IGetFaces
{
    public List<Polyhedron> Faces => _faces;

    protected List<Polyhedron> _faces = new();

    protected TransformData _transform;

    public void Update()
    {
        _faces = GetFaces();
    }

    public void UpdatePosition(TransformData transform, float deltaTime = 1)
    {
        _transform = transform;
    }

    public abstract List<Polyhedron> GetFaces();
}
