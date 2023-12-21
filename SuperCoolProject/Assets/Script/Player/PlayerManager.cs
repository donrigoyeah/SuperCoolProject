using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour
{
    [Header("Player Variables")]
    public float playerDetectionRadius = 10;
    public Collider[] aliensInRange;
    public bool playerShield;
    public float shieldRechargeTime = 2;
    public bool isCarryingPart;
    public GameObject currentPart;
    public float timeSinceLastHit;
    public GameObject playerShieldGO;

    [Header("Resource Variables")]
    float maxSphereResource = 100;
    float maxSquareResource = 100;
    float maxTriangleResource = 100;
    public float currentSphereResource;
    public float currentSquareResource;
    public float currentTriangleResource;

    public float resourceDrain = .1f;
    public float resourceGain = 5;

    // 0:Sphere, 1:Square, 2:Triangle
    [Header("UI Elements")]
    public Image[] resourcePieCharts;
    public GameObject ResourceUISphere;
    public GameObject ResourceUISquare;
    public GameObject ResourceUITriangle;

    private void FixedUpdate()
    {
        float delta = Time.deltaTime;
        timeSinceLastHit += delta;

        HandleResource();
        HandleAlienDetection();

        // TODO: Make this maybe coroutine ?!
        if (playerShield == false && timeSinceLastHit > shieldRechargeTime && GameManager.SharedInstance.hasShieldGenerator)
        {
            playerShieldGO.SetActive(true);
            playerShield = true;
        }
    }

    public void HandleHit()
    {
        if (playerShield == false)
        {
            HandleDeath();
        }
        else
        {
            timeSinceLastHit = 0;
            playerShield = false;
            playerShieldGO.SetActive(false);
        }
    }


    private void HandleDeath()
    {
        // TODO: Instanciate GameObject like (deadPlayerBody)
        // Add draggable script to it
        // WHen returned to spaceship, enable upgrades again
        // Make global boolean to handle this

        currentSphereResource = maxSphereResource;
        currentSquareResource = maxSquareResource;
        currentTriangleResource = maxTriangleResource;

        GameManager.SharedInstance.HandleCloneJuiceDrain();

        // TODO: Better respawn thingi
        this.gameObject.transform.position = Vector3.zero;
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
                AH.closestAlien = this.gameObject;

                if (AH.currentAge != AlienHandler.AlienAge.fullyGrown)
                {
                    AH.HandleFleeing(this.gameObject); // this time its not an alienGO but the player
                }
                else if (AH.currentAge == AlienHandler.AlienAge.fullyGrown)
                {
                    AH.HandleAttacking(this.gameObject); // this time its not an alienGO but the player
                }
            }
        }
    }

    private void HandleResource()
    {
        // 0:Sphere, 1:Square, 2:Triangle
        if (currentSphereResource > 0) { currentSphereResource -= resourceDrain; }
        if (currentSquareResource > 0) { currentSquareResource -= resourceDrain; }
        if (currentTriangleResource > 0) { currentTriangleResource -= resourceDrain; }

        // Only show resource UI if below 75%
        if (currentSphereResource < 3 * maxSphereResource / 4) { ResourceUISphere.SetActive(true); } else { ResourceUISphere.SetActive(false); }
        if (currentSquareResource < 3 * maxSquareResource / 4) { ResourceUISquare.SetActive(true); } else { ResourceUISquare.SetActive(false); }
        if (currentTriangleResource < 3 * maxTriangleResource / 4) { ResourceUITriangle.SetActive(true); } else { ResourceUITriangle.SetActive(false); }

        // Update UI
        resourcePieCharts[0].fillAmount = currentSphereResource / maxSphereResource;
        resourcePieCharts[1].fillAmount = currentSphereResource / maxSphereResource;
        resourcePieCharts[2].fillAmount = currentSphereResource / maxSphereResource;

        // Check if enough resources
        if (currentSphereResource <= 0 ||
            currentSquareResource <= 0 ||
            currentTriangleResource <= 0)
        {
            HandleDeath();
        }
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
