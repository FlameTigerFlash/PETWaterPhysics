using System.Diagnostics.CodeAnalysis;
using UnityEngine;

public class Thruster : MonoBehaviour
{
    [SerializeField, NotNull] private Rigidbody _rb;
    [SerializeField] private Transform _point;

    [Range(-100000, 100000)] public float Thrust = 0f;

    private void OnValidate()
    {
        _point ??= transform;
    }

    private void FixedUpdate()
    {
        _rb.AddForceAtPosition(_point.forward * Thrust, _point.position, ForceMode.Force);
    }
}
