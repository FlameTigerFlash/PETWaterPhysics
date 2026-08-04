using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework.Constraints;

public class Polyhedron
{
    public LinkedList<PointData> Vertices => new LinkedList<PointData>(_vertices);

    public int NormalMultiplier => _normalMultiplier;

    private LinkedList<PointData> _vertices;

    private int _normalMultiplier = -1;

    public Polyhedron(LinkedList<PointData> vertices)
    {
        _vertices = new LinkedList<PointData>(vertices);
    }

    public Polyhedron(Vector3[] vertices)
    {
        _vertices = new LinkedList<PointData>();
        foreach (var vec in vertices)
        {
            _vertices.AddLast(vec);
        }
    }

    public Polyhedron(PointData[] pointDataArr)
    {
        _vertices = new LinkedList<PointData>(pointDataArr);
    }

    public Vector3 GetNormal()
    {
        if (_vertices.Count < 3)
        {
            Debug.LogWarning("Not enough vertices to calculate normal.");
            return Vector3.zero;
        }

        var firstEl = _vertices.First;
        var secondEl = firstEl.Next;
        var thirdEl = secondEl.Next;

        Vector3 a = secondEl.Value.Position - firstEl.Value.Position;
        while (Vector3.Cross(a, (thirdEl.Value.Position - firstEl.Value.Position)) == Vector3.zero)
        {
            if (thirdEl == null)
            {
                Debug.LogError("Polyhedron must have at list 3 non-collinear vertices in order to calculate normal.");
                return Vector3.zero;
            }
            thirdEl = thirdEl.Next;
        }

        Vector3 normal = _normalMultiplier * Vector3.Cross(a, (thirdEl.Value.Position - firstEl.Value.Position)).normalized;

        return normal;
    }

    public void FlipNormal()
    {
        _normalMultiplier *= -1;
    }
}
