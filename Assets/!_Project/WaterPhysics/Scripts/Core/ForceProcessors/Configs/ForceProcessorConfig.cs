using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ForceProcessorConfig", menuName = "Scriptable Objects/WaterForce/ForceProcessorConfig")]
public class ForceProcessorConfig : ScriptableObject
{
    [SerializeReference, SubclassSelector] public List<ITransformForces> ForcePreprocessors = new();
    [SerializeReference, SubclassSelector] public ICalculateWaterForceEffect ForceProcessor = new DummyForceProcessor();
    [SerializeReference, SubclassSelector] public List<ITransformForceEffect> ForcePostprocessors = new();

    private void OnValidate()
    {
        if (ForceProcessor == null)
        {
            ForceProcessor = new DummyForceProcessor();
        }
    }
}
