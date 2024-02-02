using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetRotationHandler : MonoBehaviour
{
    Transform MyTransform;

    public float rotationSpeed = 10;

    private void Awake()
    {
        MyTransform = this.transform;
    }

    private void FixedUpdate()
    {
        MyTransform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
    }
}
