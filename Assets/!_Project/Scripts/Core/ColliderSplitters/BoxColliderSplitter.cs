using System.Collections.Generic;
using UnityEngine;

public class BoxColliderSplitter : BaseColliderSplitter
{
    private BoxCollider _boxCollider;

    public BoxColliderSplitter(BoxCollider boxCollider)
    {
        _boxCollider = boxCollider;
    }

    protected override List<Polyhedron> SplitCollider()
    {
        List<Polyhedron> faces = new();

        Bounds bounds = _boxCollider.bounds;
        Vector3 relCenter = bounds.center - _transform.Position;
        Vector3 halfSize = Vector3.Scale(_boxCollider.size, _boxCollider.transform.lossyScale) / 2;

        Vector3 ufr = new PointData(relCenter + new Vector3(halfSize.x, halfSize.y, halfSize.z)),
            ufl = new PointData(relCenter + new Vector3(-halfSize.x, halfSize.y, halfSize.z)),
            ubr = new PointData(relCenter + new Vector3(halfSize.x, halfSize.y, -halfSize.z)),
            ubl = new PointData(relCenter + new Vector3(-halfSize.x, halfSize.y, -halfSize.z)),
            lfr = new PointData(relCenter + new Vector3(halfSize.x, -halfSize.y, halfSize.z)),
            lfl = new PointData(relCenter + new Vector3(-halfSize.x, -halfSize.y, halfSize.z)),
            lbr = new PointData(relCenter + new Vector3(halfSize.x, -halfSize.y, -halfSize.z)),
            lbl = new PointData(relCenter + new Vector3(-halfSize.x, -halfSize.y, -halfSize.z));

        PointData[] frontVertices = { ufl, ufr, lfr, lfl },
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
