using UnityEngine;

[CreateAssetMenu(fileName = "ECRPConfig", menuName = "Scriptable Objects/WaterForceProcessors/ECRPConfig")]
public class ECRPConfig : ForceProcessorConfig
{
    public override ICalculateWaterForceEffect CreateProcessor()
    {
        return new EnergyConservativeResistanceProcessor();
    }
}
