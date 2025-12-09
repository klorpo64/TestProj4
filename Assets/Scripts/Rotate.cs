using System;
using UnityEngine;

public class Rotate : MonoBehaviour
{
    Transform pivot;
    [SerializeField]
    float speed;
    [SerializeField, Range(0f, 360f)]
    float initialRotation;

    void Start()
    {
        pivot = transform;
        pivot.rotation = Quaternion.Euler(Vector3.up * initialRotation);
    }

    void Update()
    {
        pivot.rotation = Quaternion.Euler(speed * Time.deltaTime * Vector3.up + pivot.rotation.eulerAngles);
    }
}
