using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class WaterForceAggregator : MonoBehaviour
{
    #region fields
    [SerializeField] private WaterForceCalculationSystem _pressureProcessor;

    [Header("Ballast")]
    [SerializeField] private List<Ballast> _ballasts = new();

    [Header("Water settings")]
    [SerializeField] private Vector3 _standardCurrent = Vector3.zero;
    [SerializeField, Min(1f)] private float _waterDensity = 1000f;

    [Header("Archimedes ForceData")]
    [SerializeField] private bool _applyArchimedesForce = true;

    [Header("Water Resistance ForceData")]
    [SerializeField] private bool _applyResistanceForce = true;

    [Header("Water force processors")]
    [SerializeField] private ForceProcessorConfig _resistanceProcessorConfig;
    [SerializeField] private ForceProcessorConfig _archimedesProcessorConfig;

    private ICalculateWaterForceEffect _resistanceProcessor;
    private ICalculateWaterForceEffect _archimedesProcessor;

    private Rigidbody _rb;

    private WaterData _water;
    #endregion

    private void Awake()
    {
        var plane = new Plane(-Physics.gravity.normalized, Vector3.zero);
        SetupWater(plane, _standardCurrent, _waterDensity);

        _rb = GetComponent<Rigidbody>();
        SetupBallast();

        _resistanceProcessor = (_resistanceProcessorConfig == null) ? new DummyForceProcessor() : _resistanceProcessorConfig.CreateProcessor();
        _archimedesProcessor = (_archimedesProcessorConfig == null) ? new DummyForceProcessor() : _archimedesProcessorConfig.CreateProcessor();
    }

    private void FixedUpdate()
    {
        if (_pressureProcessor == null)
        {
            return;
        }

        SetupWater(new Plane(-Physics.gravity.normalized, Vector3.zero), _standardCurrent, _waterDensity);
        _pressureProcessor.UpdatePosition(new TransformData(transform), Time.fixedDeltaTime);
        _pressureProcessor.SetWaterData(_water);
        _pressureProcessor.FullUpdate();

        RigidBodyData rbData = new RigidBodyData(_rb);

        ForceEffectData resistanceEffect = new(Vector3.zero, Vector3.zero), archimedesEffect = new(Vector3.zero, Vector3.zero);

        if (_applyResistanceForce)
        {
            var resistanceForces = _pressureProcessor.ResistanceForces;
            resistanceEffect = _resistanceProcessor.CalculateForceEffect(resistanceForces, _water, rbData, Time.fixedDeltaTime);
            rbData = rbData.ApplyForceEffect(resistanceEffect, Time.fixedDeltaTime);
        }

        if (_applyArchimedesForce)
        {
            var archimedesForces = _pressureProcessor.ArchimedesForces;
            archimedesEffect = _archimedesProcessor.CalculateForceEffect(archimedesForces, _water, rbData, Time.fixedDeltaTime);
            rbData = rbData.ApplyForceEffect(archimedesEffect, Time.fixedDeltaTime);
        }

        ForceEffectData totalEffect = resistanceEffect + archimedesEffect;
        _rb.AddForce(totalEffect.ForceVector);
        _rb.AddTorque(totalEffect.TorqueVector);
    }

    private void OnDrawGizmos()
    {
        if (_rb == null)
        {
            return;
        }
        Vector3 centerOfMassPos = _rb.worldCenterOfMass;

        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(centerOfMassPos, 0.5f);
    }

    public void SetupWater(Plane plane, Vector3 current, float density = 1000f)
    {
        _water = new WaterData(plane, current, density);
    }

    private void SetupBallast()
    {
        bool wasKinematic = false;
        if (_rb.isKinematic)
        {
            wasKinematic = true;
            _rb.isKinematic = false;
            _rb.ResetCenterOfMass();
            _rb.ResetInertiaTensor();
        }
        foreach (var ballast in _ballasts)
        {
            ballast.Collider.enabled = false;
        }
        Vector3 oldInertiaTensor = _rb.inertiaTensor;
        _rb.ResetCenterOfMass();

        //Debug.Log($"Resetting center of mass to {_rb.centerOfMass}");

        float oldMass = _rb.mass;
        Vector3 oldWorldCenterOfMass = _rb.worldCenterOfMass;

        float totalBallastMass = 0f;
        Vector3 totalBallastMoment = Vector3.zero;

        foreach (var ballast in _ballasts)
        {
            float mass = ballast.GetMass();
            totalBallastMass += mass;
            totalBallastMoment += mass * ballast.CenterOfMass;
        }

        float totalMass = oldMass + totalBallastMass;
        _rb.mass = totalMass;

        Vector3 newWorldCenterOfMass = (oldMass * oldWorldCenterOfMass + totalBallastMoment) / totalMass;
        _rb.centerOfMass = transform.InverseTransformPoint(newWorldCenterOfMass);

        _rb.ResetInertiaTensor();
        Vector3 hullInertia = _rb.inertiaTensor;
        Quaternion hullInertiaRot = _rb.inertiaTensorRotation;

        Vector3 ballastInertia = Vector3.zero;
        Quaternion selfRotation = transform.rotation;
        foreach (var ballast in _ballasts)
        {
            Vector3 connector = transform.InverseTransformPoint(ballast.CenterOfMass) - _rb.centerOfMass;
            float mass = ballast.GetMass();
            Vector3 localInertia = ballast.GetInertiaTensor();

            ballastInertia.x += localInertia.x + mass * (connector.y * connector.y + connector.z * connector.z);
            ballastInertia.y += localInertia.y + mass * (connector.x * connector.x + connector.z * connector.z);
            ballastInertia.z += localInertia.z + mass * (connector.x * connector.x + connector.y * connector.y);
        }

        _rb.inertiaTensor = hullInertia + ballastInertia;

        if (wasKinematic)
        {
            _rb.isKinematic = true;
        }
        Debug.Log($"Local center of mass: {_rb.centerOfMass}");
        Debug.Log($"Old mass: {oldMass}, new mass: {totalMass}.");
        Debug.Log($"Old center of mass: {oldWorldCenterOfMass}, new center of mass: {newWorldCenterOfMass} = {_rb.worldCenterOfMass}.");
        Debug.Log($"Old inertia tensor: {oldInertiaTensor}, new inertia tensor: {_rb.inertiaTensor}.");
        Debug.Log($"Old inertia tensor rotation: {hullInertiaRot}, new inertia tensor rotation: {_rb.inertiaTensorRotation}.");
    }
}
