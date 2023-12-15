using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour
{
    [Header("Resource Variables")]
    public float maxSphereResource = 10;
    public float maxSquareResource = 10;
    public float maxTriangleResource = 10;

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
        HandleResources();
    }


    private void HandleResources()
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
}
