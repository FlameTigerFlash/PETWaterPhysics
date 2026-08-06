using System;
using System.Collections.Generic;

public interface ICalculateWaterForceEffect
{
    public ForceEffectData CalculateForceEffect(List<ForceData> forces, WaterData water, RigidBodyData rb, float deltaTime=1f);
}
