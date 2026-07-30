using System.Collections.Generic;
using UnityEngine;

public class BoxColliderSplitter : BaseColliderSplitter
{
    private BoxCollider _boxCollider;

    private Transform _transform;

    public BoxColliderSplitter(BoxCollider boxCollider, Transform transform)
    {
        _boxCollider = boxCollider;
        _transform = transform;
    }

    protected override List<Polyhedron> SplitCollider()
    {
        List<Polyhedron> faces = new();

        Bounds bounds = _boxCollider.bounds;
        Vector3 relCenter = bounds.center - _transform.position;
        Vector3 halfSize = Vector3.Scale(_boxCollider.size, _transform.lossyScale) / 2;

        Vector3 ufr = relCenter + new Vector3(halfSize.x, halfSize.y, halfSize.z),
            ufl = relCenter + new Vector3(-halfSize.x, halfSize.y, halfSize.z),
            ubr = relCenter + new Vector3(halfSize.x, halfSize.y, -halfSize.z),
            ubl = relCenter + new Vector3(-halfSize.x, halfSize.y, -halfSize.z),
            lfr = relCenter + new Vector3(halfSize.x, -halfSize.y, halfSize.z),
            lfl = relCenter + new Vector3(-halfSize.x, -halfSize.y, halfSize.z),
            lbr = relCenter + new Vector3(halfSize.x, -halfSize.y, -halfSize.z),
            lbl = relCenter + new Vector3(-halfSize.x, -halfSize.y, -halfSize.z);

        Vector3[] frontVertices = { ufl, ufr, lfr, lfl },
            rightVertices = { ufr, ubr, lbr, lfr },
            rearVertices = { ubr, ubl, lbl, lbr },
            leftVertices = { ubl, ufl, lfl, lbl },
            bottomVertices = { lfl, lfr, lbr, lbl },
            topVertices = { ufr, ufl, ubl, ubr };

        //Debug.Log($"UFR: {ufr}\n" +
        //    $"UFL: {ufl}\n" +
        //    $"UBR: {ubr}\n" +
        //    $"UBL: {ubl}\n" +
        //    $"LFR: {lfr}\n" +
        //    $"LFL: {lfl}\n" +
        //    $"LBR: {lbr}\n" +
        //    $"LBL: {lbl}\n");

        faces.Add(new Polyhedron(frontVertices));
        faces.Add(new Polyhedron(rightVertices));
        faces.Add(new Polyhedron(rearVertices));
        faces.Add(new Polyhedron(leftVertices));
        faces.Add(new Polyhedron(bottomVertices));
        faces.Add(new Polyhedron(topVertices));

        return faces;
    }
}
