using System.Diagnostics.CodeAnalysis;
using System.Collections.Generic;
using System;
using UnityEngine;

[Serializable, RequireComponent(typeof(BoxCollider))]
public class FinGeometry : MonoBehaviour, IGetFaces
{
    [Header("Components")]
    [SerializeField, NotNull] private Transform _parentTransform;

    [SerializeField, NotNull] private BoxCollider _boxCollider;

    [Header("Params")]
    [SerializeField, Min(0)] private float _maxAngularSpeed = 10f;

    [Header("Controls")]
    [SerializeField, Range(-45, 45)] private float _desiredAngle = 0f;

    private BoxColliderSplitter _splitter;

    private void OnValidate()
    {
        _parentTransform ??= transform;
        _boxCollider ??= GetComponent<BoxCollider>();
    }

    private void Awake()
    {
        _splitter = new BoxColliderSplitter(_boxCollider);
    }

    private void Update()
    {
        RotateToDesiredAngle(Time.deltaTime);
    }

    private void RotateToDesiredAngle(float time)
    {
        Vector3 curAngles = transform.localRotation.eulerAngles;
        curAngles.x = 0;
        curAngles.z = 0;
        curAngles.y = Mathf.LerpAngle(curAngles.y, -_desiredAngle, _maxAngularSpeed * time);
        transform.eulerAngles = transform.parent.rotation.eulerAngles + curAngles;
    }

    public List<Polyhedron> GetFaces()
    {
        _splitter.UpdatePosition(new TransformData(_parentTransform), Time.fixedDeltaTime);

        var faces = _splitter.GetFaces();
        foreach (var face in faces)
        {
            face.CalculateArchimedesForce = false;
        }
        return faces;
    }
} 
