using UnityEngine;

public interface IUpdatePosition
{
    public void UpdatePosition(TransformData transform, float deltaTime = 1);
}
