using UnityEngine;

[CreateAssetMenu(fileName = "LVAFPConfig", menuName = "Scriptable Objects/WaterForceProcessors/LVAFPConfig")]
public class LVAFPConfig : ForceProcessorConfig
{
    [SerializeField, Min(float.Epsilon)] private float _maxVerticalSpeed = 1f;
    [SerializeField, Min(float.Epsilon)] private float _distanceErrorThreshold = 0.01f;

    public override ICalculateWaterForceEffect CreateProcessor()
    {
        return new LimitedVelocityArchimedesForceProcessor(_maxVerticalSpeed, _distanceErrorThreshold);
    }
}
