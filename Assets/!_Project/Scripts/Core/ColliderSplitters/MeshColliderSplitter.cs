using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class MeshColliderSplitter : BaseColliderSplitter
{

    private MeshCollider _meshCollider;

    public MeshColliderSplitter(MeshCollider meshCollider)
    {
        _meshCollider = meshCollider;
    }

    protected override List<Polyhedron> SplitCollider()
    {
        List<Polyhedron> faces = new();
        Mesh mesh = _meshCollider.sharedMesh;
        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;

        for (int i = 0; i < triangles.Length; i += 3)
        {
            Vector3 v0 = vertices[triangles[i]];
            Vector3 v1 = vertices[triangles[i + 1]];
            Vector3 v2 = vertices[triangles[i + 2]];

            var list = new LinkedList<PointData>();
            list.AddLast(v2);
            list.AddLast(v1);
            list.AddLast(v0);

            Polyhedron triangle = new Polyhedron(list);
            faces.Add(triangle);
        }

        return faces;
    }
}
