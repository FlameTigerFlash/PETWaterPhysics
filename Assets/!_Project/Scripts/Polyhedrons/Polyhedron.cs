using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework.Constraints;

public class Polyhedron
{
    public LinkedList<Vector3> Vertices => new LinkedList<Vector3>(_vertices);

    public int NormalMultiplier => _normalMultiplier;

    private LinkedList<Vector3> _vertices;

    private int _normalMultiplier = -1;

    public Polyhedron(LinkedList<Vector3> vertices)
    {
        _vertices = new LinkedList<Vector3>(vertices);
    }

    public Polyhedron(Vector3[] vertices)
    {
        _vertices = new LinkedList<Vector3>(vertices);
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

        Vector3 a = secondEl.Value - firstEl.Value;
        while (Vector3.Cross(a, (thirdEl.Value - firstEl.Value)) == Vector3.zero)
        {
            if (thirdEl == null)
            {
                Debug.LogError("Polyhedron must have at list 3 non-collinear vertices in order to calculate normal.");
                return Vector3.zero;
            }
            thirdEl = thirdEl.Next;
        }

        Vector3 normal = _normalMultiplier * Vector3.Cross(a, (thirdEl.Value - firstEl.Value)).normalized;

        return normal;
    }

    public void FlipNormal()
    {
        _normalMultiplier *= -1;
    }
}
