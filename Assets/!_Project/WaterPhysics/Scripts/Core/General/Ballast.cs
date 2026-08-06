using System.Numerics;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class Ballast : MonoBehaviour
{
    [SerializeField] private BoxCollider _boxCollider;

    [SerializeField, Range(500f, 30000f)] float _density = 2000f;

    public UnityEngine.Vector3 TrueColliderSize => UnityEngine.Vector3.Scale(_boxCollider.size, transform.localScale);

    public Collider Collider => _boxCollider;

    public UnityEngine.Vector3 CenterOfMass => transform.rotation * _boxCollider.center + transform.position;

    private void OnValidate()
    {
        _boxCollider ??= GetComponent<BoxCollider>();
    }

    private void Awake()
    {
        _boxCollider.enabled = false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.pink;
        Gizmos.DrawSphere(CenterOfMass, 0.4f);
    }

    public float GetMass()
    {
        float volume = TrueColliderSize.x * TrueColliderSize.y * TrueColliderSize.z;
        return volume * _density;
    }

    public UnityEngine.Vector3 GetInertiaTensor()
    {
        float sqrHx = Mathf.Pow(TrueColliderSize.x / 2, 2), sqrHy = Mathf.Pow(TrueColliderSize.y / 2, 2), sqrHz = Mathf.Pow(TrueColliderSize.z / 2, 2);
        float thirdOfMass = GetMass() / 3;

        return new UnityEngine.Vector3(sqrHy + sqrHz, sqrHx + sqrHz, sqrHx + sqrHy) * thirdOfMass;
    }
}
