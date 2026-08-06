using System;
using System.Collections.Generic;

[Serializable]
public abstract class BaseForceProcessor : ICalculateWaterForceEffect
{
    public abstract ForceEffectData CalculateForceEffect(List<ForceData> forces, WaterData water, RigidBodyData rb, float deltaTime = 0.02f);
}
