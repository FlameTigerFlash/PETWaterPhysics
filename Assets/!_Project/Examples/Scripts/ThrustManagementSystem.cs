using UnityEngine;

public class ThrustManagementSystem : MonoBehaviour
{
    [SerializeField] private Thruster _leftThruster;
    [SerializeField] private Thruster _rightThruster;

    [SerializeField, Range(-90, 90)] private float _direction = 0;
    [SerializeField, Range(-100000, 100000)] private float _thrust = 0;

    private void FixedUpdate()
    {
        float cosDirection = Mathf.Cos(Mathf.Deg2Rad * _direction * 2);

        if (_direction < 0)
        {
            _leftThruster.Thrust = _thrust * cosDirection;
            _rightThruster.Thrust = _thrust;
        }
        else
        {
            _rightThruster.Thrust = _thrust * cosDirection;
            _leftThruster.Thrust = _thrust;
        }
    }
}
