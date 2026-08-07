using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class BoxColliderSplitter : BaseColliderSplitter
{
    private BoxCollider _boxCollider;

    public BoxColliderSplitter(BoxCollider boxCollider)
    {
        _boxCollider = boxCollider;
    }

    public override List<Polyhedron> GetFaces()
    {
        List<Polyhedron> faces = new();

        Bounds bounds = _boxCollider.bounds;
        Vector3 relCenter = _transform.InverseTransformPoint(bounds.center);
        Vector3 halfSize = Vector3.Scale(_boxCollider.size, _boxCollider.transform.localScale) / 2;

        Vector3 trueHalfSizeX = ToRemoteRotation(halfSize.x * Vector3.right);
        Vector3 trueHalfSizeY = ToRemoteRotation(halfSize.y * Vector3.up);
        Vector3 trueHalfSizeZ = ToRemoteRotation(halfSize.z * Vector3.forward);

        Vector3 ufr = new PointData(relCenter + trueHalfSizeX + trueHalfSizeY + trueHalfSizeZ),
            ufl = new PointData(relCenter - trueHalfSizeX + trueHalfSizeY + trueHalfSizeZ),
            ubr = new PointData(relCenter + trueHalfSizeX + trueHalfSizeY - trueHalfSizeZ),
            ubl = new PointData(relCenter - trueHalfSizeX + trueHalfSizeY - trueHalfSizeZ),
            lfr = new PointData(relCenter + trueHalfSizeX - trueHalfSizeY + trueHalfSizeZ),
            lfl = new PointData(relCenter - trueHalfSizeX - trueHalfSizeY + trueHalfSizeZ),
            lbr = new PointData(relCenter + trueHalfSizeX - trueHalfSizeY - trueHalfSizeZ),
            lbl = new PointData(relCenter - trueHalfSizeX - trueHalfSizeY - trueHalfSizeZ);

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

    private Vector3 ToRemoteRotation(Vector3 vec)
    {
        return Quaternion.Inverse(_transform.Rotation) * (_boxCollider.transform.rotation * vec);
    }
}
