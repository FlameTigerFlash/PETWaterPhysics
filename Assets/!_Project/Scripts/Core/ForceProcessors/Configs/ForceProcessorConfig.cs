using UnityEngine;


public abstract class ForceProcessorConfig : ScriptableObject
{
    public abstract ICalculateWaterForceEffect CreateProcessor();
}
