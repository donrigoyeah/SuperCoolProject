using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour
{
    [Header("Player Variables")]
    public float playerDetectionRadius = 10;
    public Collider[] aliensInRange;


    [Header("Resource Variables")]
    float maxSphereResource = 100;
    float maxSquareResource = 100;
    float maxTriangleResource = 100;

    public float currentSphereResource;
    public float currentSquareResource;
    public float currentTriangleResource;

    public float resourceDrain = .1f;
    public float resourceGain = 5;

    public Image[] resourcePieCharts;

    [Header("Player Variables")]
    public bool hasShield;
    //public GameObject shield;

    private void FixedUpdate()
    {
        HandleResourceUI();
        HandleDeath();
        HandleAlienDetection();
    }


    private void HandleDeath()
    {
        if (currentSphereResource <= 0 ||
            currentSquareResource <= 0 ||
            currentTriangleResource <= 0)
        {
            Debug.Log("Player died");
        }
    }


    private void HandleAlienDetection()
    {
        int layerMask = 1 << 9; // Lyer 9 is Alien

        aliensInRange = Physics.OverlapSphere(this.transform.position, playerDetectionRadius, layerMask);

        foreach (var item in aliensInRange)
        {
            AlienHandler AH = item.gameObject.GetComponent<AlienHandler>();
            if (AH != null)
            {
                if (AH.lifeTime > AH.timeToChild)
                {
                    AH.closestAlien = this.gameObject;
                    AH.HandleFleeing(this.gameObject); // this time its not an alienGO but the player
                }
            }
        }
    }

    private void HandleResourceUI()
    {
        // 0:Sphere, 1:Square, 2:Triangle
        if (currentSphereResource > 0) { currentSphereResource -= resourceDrain; }
        if (currentSquareResource > 0) { currentSquareResource -= resourceDrain; }
        if (currentTriangleResource > 0) { currentTriangleResource -= resourceDrain; }

        // Update UI
        resourcePieCharts[0].fillAmount = currentSphereResource / maxSphereResource;
        resourcePieCharts[1].fillAmount = currentSphereResource / maxSphereResource;
        resourcePieCharts[2].fillAmount = currentSphereResource / maxSphereResource;
    }

    public void HandleGainResource(int rescourseIndex)
    {
        if (rescourseIndex == 0)
        {
            currentSphereResource += resourceGain;
            if (currentSphereResource > maxSphereResource)
            {
                currentSphereResource = maxSphereResource;
            }
        }
        if (rescourseIndex == 1)
        {
            currentSquareResource += resourceGain;
            if (currentSquareResource > maxSquareResource)
            {
                currentSquareResource = maxSquareResource;
            }
        }
        if (rescourseIndex == 2)
        {
            currentTriangleResource += resourceGain;
            if (currentTriangleResource > maxTriangleResource)
            {
                currentTriangleResource = maxTriangleResource;
            }
        }
    }
}
