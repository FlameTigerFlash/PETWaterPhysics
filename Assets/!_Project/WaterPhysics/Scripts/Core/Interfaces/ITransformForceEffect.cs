using System.Collections.Generic;

public interface ITransformForceEffect
{
    public ForceEffectData TransformForceEffect(ForceEffectData forceEffect, WaterData water, RigidBodyData rb, float deltaTime = 1f);
}
