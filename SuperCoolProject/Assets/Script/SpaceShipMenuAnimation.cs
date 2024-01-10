using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceShipMenuAnimation : MonoBehaviour
{
    public float bobbleAmplitude = 0.02f;
    public float bobbleSpeed = 0.5f;
    public float bobbleStart = 23.5f;

    private void FixedUpdate()
    {
        float verticleBobMovement = bobbleAmplitude * Mathf.Sin(Time.time / bobbleSpeed) + bobbleStart;
        transform.position = new Vector3(transform.position.x, verticleBobMovement, transform.position.z);
    }
}
