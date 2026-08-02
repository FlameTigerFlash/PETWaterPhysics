using System.Collections.Generic;
using UnityEngine;

public interface IPreprocessFaces : IUpdatePosition
{
    public List<Polyhedron> GetPreprocessedFaces(in IReadOnlyList<Polyhedron> faces, Plane plane);
}
