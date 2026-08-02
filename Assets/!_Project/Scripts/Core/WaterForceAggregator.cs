using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(Rigidbody))]
public class WaterForceAggregator : MonoBehaviour
{
    [SerializeField] private WaterPressureProcessor _pressureProcessor;

    [SerializeField] private Vector3 _standardCurrent = Vector3.zero;

    [SerializeField] private float _maxRotationGainPerSecond = 1000f;

    [SerializeField] private float _maxArchimedesAccelerationGrowthPerSecond = 3f;

    [SerializeField] private bool _applyArchimedesForce = true;
    [SerializeField] private bool _applyResistanceForce = true;

    private Rigidbody _rb;
    private Plane _plane;

    private float _prevArchimedesForceMagnitude = 0;

    private void Awake()
    {
        _plane = new Plane(-Physics.gravity, Vector3.zero);
        _rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (_pressureProcessor == null)
        {
            return;
        }

        _pressureProcessor.UpdatePosition(new PointTransform(transform), Time.fixedDeltaTime);
        _pressureProcessor.Current = _standardCurrent;
        _pressureProcessor.FullUpdate();

        var archimedesForces = _pressureProcessor.ArchimedesForces;
        var resistanceForces = _pressureProcessor.ResistanceForces;

        if (_applyArchimedesForce)
        {
            ProcessArchimedesForces(archimedesForces);
        }
        if (_applyResistanceForce)
        {
            ProcessResistanceForces(resistanceForces);
        }
    }

    private void ProcessResistanceForces(List<Force> resistanceForces)
    {
        Vector3 resForce = Force.GetResultantForce(resistanceForces);
        Vector3 resTorque = Force.GetResultantTorque(resistanceForces, _rb.worldCenterOfMass);

        float expectedVelocityChange = (resForce / _rb.mass).magnitude;
        if (expectedVelocityChange > _rb.linearVelocity.magnitude)
        {
            resForce /= (expectedVelocityChange / _rb.linearVelocity.magnitude);
        }

        float resistanceAcceration = Vector3.Dot(_rb.linearVelocity.normalized, resForce);
        if (resistanceAcceration > 0)
        {
            resForce -= _rb.linearVelocity.normalized * resistanceAcceration;
        }

        float rotationGain = new Vector3(resTorque.x / _rb.inertiaTensor.x, resTorque.y / _rb.inertiaTensor.y, resTorque.z / _rb.inertiaTensor.z).magnitude;
        if (rotationGain >= _maxRotationGainPerSecond)
        {
            resTorque /= (rotationGain / _maxRotationGainPerSecond);
        }

        _rb.AddForce(resForce, ForceMode.Force);
        _rb.AddTorque(resTorque, ForceMode.Force);
    }

    private void ProcessArchimedesForces(List<Force> archimedesForces)
    {
        Vector3 resForce = Force.GetResultantForce(archimedesForces);
        resForce = (_plane.normal * Vector3.Dot(resForce, _plane.normal));

        float resForceMagnitude = resForce.magnitude;
        float maxMagnitudeGrowth = _rb.mass * _maxArchimedesAccelerationGrowthPerSecond * Time.fixedDeltaTime;

        float totalMagnitude = Mathf.Clamp(resForceMagnitude, _prevArchimedesForceMagnitude - maxMagnitudeGrowth, _prevArchimedesForceMagnitude + maxMagnitudeGrowth);
        resForce = resForce.normalized * totalMagnitude;

        Vector3 resTorque = Force.GetResultantTorque(archimedesForces, _rb.worldCenterOfMass);
        float rotationGain = new Vector3(resTorque.x / _rb.inertiaTensor.x, resTorque.y / _rb.inertiaTensor.y, resTorque.z / _rb.inertiaTensor.z).magnitude;
        if (rotationGain >= _maxRotationGainPerSecond)
        {
            resTorque /= (rotationGain / _maxRotationGainPerSecond);
        }

        _prevArchimedesForceMagnitude = resForce.magnitude;

        _rb.AddForce(resForce, ForceMode.Force);
        _rb.AddTorque(resTorque, ForceMode.Force);
    }
}
