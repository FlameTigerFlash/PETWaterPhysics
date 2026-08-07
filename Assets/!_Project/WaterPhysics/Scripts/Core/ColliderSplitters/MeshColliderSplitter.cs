using System.Collections.Generic;
using UnityEngine;

public class MeshColliderSplitter : BaseColliderSplitter
{

    private MeshCollider _meshCollider;

    public MeshColliderSplitter(MeshCollider meshCollider)
    {
        _meshCollider = meshCollider;
    }

    public override List<Polyhedron> GetFaces()
    {
        List<Polyhedron> faces = new();
        Mesh mesh = _meshCollider.sharedMesh;
        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;

        Vector3 colSize = _meshCollider.transform.lossyScale;

        for (int i = 0; i < triangles.Length; i += 3)
        {
            Vector3 v0 = vertices[triangles[i]];
            Vector3 v1 = vertices[triangles[i + 1]];
            Vector3 v2 = vertices[triangles[i + 2]];

            var list = new LinkedList<PointData>();
            list.AddLast(Vector3.Scale(v2, colSize));
            list.AddLast(Vector3.Scale(v1, colSize));
            list.AddLast(Vector3.Scale(v0, colSize));

            Polyhedron triangle = new Polyhedron(list);
            faces.Add(triangle);
        }

        return faces;
    }
}
