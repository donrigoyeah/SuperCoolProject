using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandleRotationOfPart : MonoBehaviour
{
    public Transform rotatingObject;
    public bool roundabout;
    public int minDegree;
    public int maxDegree;
    public float timeMultiplier;
    private bool hasFlipped = false;

    float currentRotation;

    private void FixedUpdate()
    {
        if (roundabout)
        {
            currentRotation += Mathf.Sin(Time.time * timeMultiplier);
        }
        else
        {
            if(currentRotation < maxDegree && hasFlipped == false)
            {
                currentRotation += Mathf.Sin(Time.time * timeMultiplier);
            }
            else if(currentRotation > maxDegree)
            {
                hasFlipped = true;
                currentRotation -= Mathf.Sin(Time.time * timeMultiplier);
            }
            else if(currentRotation < minDegree)
            {
                hasFlipped = false;
            }
        }

        rotatingObject.rotation = Quaternion.Euler(0, currentRotation, 0);
    }
}
