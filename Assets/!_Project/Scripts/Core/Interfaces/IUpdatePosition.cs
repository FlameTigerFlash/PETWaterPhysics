using UnityEngine;

public interface IUpdatePosition
{
    public void UpdatePosition(PointTransform transform, float deltaTime = 1);
}
