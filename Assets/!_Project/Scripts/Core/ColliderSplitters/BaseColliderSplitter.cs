using System.Collections.Generic;
using UnityEngine;

public abstract class BaseColliderSplitter
{
    public IReadOnlyList<Polyhedron> Faces => _faces;

    protected List<Polyhedron> _faces = new();

    public void Update()
    {
        _faces = SplitCollider();
    }

    protected abstract List<Polyhedron> SplitCollider();
}
