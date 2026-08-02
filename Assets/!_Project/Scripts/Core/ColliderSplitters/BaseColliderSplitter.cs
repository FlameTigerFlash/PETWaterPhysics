using System.Collections.Generic;
using UnityEngine;

public abstract class BaseColliderSplitter : IUpdatePosition
{
    public IReadOnlyList<Polyhedron> Faces => _faces;

    protected List<Polyhedron> _faces = new();

    protected PointTransform _transform;

    public void Update()
    {
        _faces = SplitCollider();
    }

    public void UpdatePosition(PointTransform transform, float deltaTime = 1)
    {
        _transform = transform;
    }

    protected abstract List<Polyhedron> SplitCollider();
}
