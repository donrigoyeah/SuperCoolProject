using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour
{
    [Header("Player Variables")]
    public float playerDetectionRadius = 10;
    public float playerResourceScanRadius = 100;
    public Collider[] aliensInRange;
    public Collider[] resourceInRange;
    public bool playerShield;
    public float shieldRechargeTime = 2;
    public bool isCarryingPart;
    public GameObject currentPart;
    public GameObject playerShieldGO;
    public bool isAlive;
    public bool isInteracting;
    public float invincibleFrames = .5f;

    private Material dissolve;
    public float dissolveRate = 0.0125f;
    public float refreshRate = 0.025f;


    [Header("Resource Variables")]
    public float maxSphereResource = 100;
    public float maxSquareResource = 100;
    public float maxTriangleResource = 100;
    public float currentSphereResource;
    public float currentSquareResource;
    public float currentTriangleResource;
    public bool sphereUnfolded = false;
    public bool squareUnfolded = false;
    public bool triangleUnfolded = false;
    AlienHandler[] closestResource = new AlienHandler[] { null, null, null };  // 0:Sphere, 1:Square, 2:Triangle


    public float resourceDrain = .1f;
    public float resourceGain = 5;

    // 0:Sphere, 1:Square, 2:Triangle
    [Header("UI Elements")]
    public Image[] resourcePieCharts;
    public GameObject ResourceUISphere;
    public GameObject ResourceUISquare;
    public GameObject ResourceUITriangle;
    public GameObject[] closestResourceIndicator;  // 0:Sphere, 1:Square, 2:Triangle

    [Header("Audio")]
    [SerializeField] private AudioClip shieldRechargeAudio;
    [SerializeField] private AudioClip shieldBreakAudio;
    [SerializeField] private AudioClip deathAudio;
    private AudioSource audioSource;

    private InputHandler inputHandler;

    private void Awake()
    {
        dissolve = GetComponent<Material>();
        audioSource = GetComponent<AudioSource>();
        inputHandler = GetComponent<InputHandler>();
    }

    private void FixedUpdate()
    {
        HandleResource();
        HandleAlienDetection();
        HandleRespawn();
        HandleGameOver();


        //Debug.Log("HandleHit is here");
    }

    public void HandleHit()
    {
        if (playerShield == false)
        {
            HandleDeath();
        }
        else
        {
            StartCoroutine(ShieldRespawn(shieldRechargeTime));
        }
    }

    private void HandleDeath()
    {
        // TODO: Instanciate GameObject like (deadPlayerBody)
        // Add draggable script to it
        // WHen returned to spaceship, enable upgrades again
        // Make global boolean to handle this

        // Set Variable to disable movement/input
        isAlive = false;
        audioSource.PlayOneShot(deathAudio, 1f);
        // Enable UI Element
        // TODO: Check if all players are dead. otherwise maybe make deathscreen on playerHUD as well

        if (GameManager.SharedInstance.currentCloneJuice < 0)
        {
            GameManager.SharedInstance.hasLost = true;
            return;
        }

        if (GameManager.SharedInstance.players.Count == 1)
        {
            GameManager.SharedInstance.DeathScreen.SetActive(true);
            GameManager.SharedInstance.DeathScreenCloneJuiceUI.fillAmount = GameManager.SharedInstance.currentCloneJuice / GameManager.SharedInstance.maxCloneJuice;
        }

        // Reset all resource variables back to max on new clone
        currentSphereResource = maxSphereResource;
        currentSquareResource = maxSquareResource;
        currentTriangleResource = maxTriangleResource;
        Debug.Log("Press Jump to respawn");
    }

    private void HandleAlienDetection()
    {
        // TODO: This is possible quite cost intense!!!

        int layerMask = 1 << 9; // Lyer 9 is Alien

        aliensInRange = Physics.OverlapSphere(this.transform.position, playerDetectionRadius, layerMask);

        foreach (var item in aliensInRange)
        {
            AlienHandler AH = item.gameObject.GetComponent<AlienHandler>();
            if (AH.currentAge == AlienHandler.AlienAge.resource) { return; }

            AH.closestAlien = this.gameObject;

            if (AH.currentAge == AlienHandler.AlienAge.fullyGrown)
            {
                AH.HandleAttacking(this.gameObject); // this time its not an alienGO but the player
            }
            else
            {
                AH.HandleFleeing(this.gameObject); // this time its not an alienGO but the player
            }
        }
    }

    private void HandleResourceDetection(int neededResource)
    {
        // TODO: This is possible quite cost intense!!!
        int layerMask = 1 << 9; // Lyer 9 is Alien
        float distanceToResource = playerResourceScanRadius;

        if (closestResource[neededResource] != null)
        {
            if (closestResource[neededResource].currentAge != AlienHandler.AlienAge.resource)
            {
                closestResource[neededResource] = null;
                Debug.Log("Resource became unavailable");
                return;
            }
            HandleResourceDetectionIndicator(closestResource[neededResource].transform.position, neededResource);
        }
        else
        {
            Debug.Log("Search for Closest Resource");
            resourceInRange = Physics.OverlapSphere(this.transform.position, playerResourceScanRadius, layerMask);
            foreach (var item in aliensInRange)
            {
                AlienHandler AH = item.gameObject.GetComponent<AlienHandler>();
                if (AH.currentAge != AlienHandler.AlienAge.resource) { continue; }
                if (AH.currentSpecies != neededResource) { continue; }

                float tmpDistance = Vector3.Distance(AH.transform.position, this.transform.position);
                Debug.Log("Distance to Resource: " + tmpDistance);
                if (tmpDistance > distanceToResource) { continue; }

                distanceToResource = tmpDistance;
                closestResource[neededResource] = AH;
                Debug.Log("Found Closest Resource");
            }
        }
    }

    private void HandleResourceDetectionIndicator(Vector3 targetResource, int neededResource)
    {
        Debug.Log("Enable Resource Indicator");

        // 0:Sphere, 1:Square, 2:Triangle
        closestResourceIndicator[neededResource].SetActive(true);
        Vector3 targetRotation = targetResource - this.transform.position;
        Quaternion rotation = Quaternion.LookRotation(targetRotation, Vector3.up);
        closestResourceIndicator[neededResource].transform.rotation = rotation;
    }

    private void DeactivateResourceDetectionIndicator(int neededResource)
    {
        // 0:Sphere, 1:Square, 2:Triangle
        closestResourceIndicator[neededResource].SetActive(false);
    }


    private void HandleResource()
    {

        // 0:Sphere, 1:Square, 2:Triangle
        if (currentSphereResource > 0) { currentSphereResource -= resourceDrain; }
        if (currentSquareResource > 0) { currentSquareResource -= resourceDrain; }
        if (currentTriangleResource > 0) { currentTriangleResource -= resourceDrain; }

        // Only show resource UI if below 75%
        if (currentSphereResource < 3 * maxSphereResource / 4)
        {
            HandleResourceDetection(0);
            if (sphereUnfolded != true)
            {
                StartCoroutine(UnfoldResource(ResourceUISphere, 50));
                sphereUnfolded = true;
                //ResourceUISphere.SetActive(true);
            }
        }
        else
        {
            if (sphereUnfolded != false)
            {
                DeactivateResourceDetectionIndicator(0);
                StartCoroutine(FoldResource(ResourceUISphere));
                sphereUnfolded = false;
                //ResourceUISphere.SetActive(false);
            }
        }

        // Only show resource UI if below 75%
        if (currentSquareResource < 3 * maxSquareResource / 4)
        {
            HandleResourceDetection(1);
            if (squareUnfolded != true)
            {
                StartCoroutine(UnfoldResource(ResourceUISquare, 25));
                squareUnfolded = true;
                //ResourceUISquare.SetActive(true);
            }
        }
        else
        {
            if (squareUnfolded != false)
            {
                DeactivateResourceDetectionIndicator(1);
                StartCoroutine(FoldResource(ResourceUISquare));
                squareUnfolded = false;
                //ResourceUISquare.SetActive(false);
            }
        }

        // Only show resource UI if below 75%
        if (currentTriangleResource < 3 * maxTriangleResource / 4)
        {
            HandleResourceDetection(2);
            if (triangleUnfolded != true)
            {
                StartCoroutine(UnfoldResource(ResourceUITriangle, 0));
                triangleUnfolded = true;
                //ResourceUITriangle.SetActive(true);
            }
        }
        else
        {
            if (triangleUnfolded != false)
            {
                DeactivateResourceDetectionIndicator(2);
                StartCoroutine(FoldResource(ResourceUITriangle));
                triangleUnfolded = false;
                //ResourceUITriangle.SetActive(false);
            }
        }

        // Update UI
        resourcePieCharts[0].fillAmount = currentSphereResource / maxSphereResource;
        resourcePieCharts[1].fillAmount = currentSquareResource / maxSquareResource;
        resourcePieCharts[2].fillAmount = currentTriangleResource / maxTriangleResource;

        // Check if enough resources
        if (currentSphereResource <= 0 ||
            currentSquareResource <= 0 ||
            currentTriangleResource <= 0)
        {
            HandleDeath();
        }
    }

    IEnumerator UnfoldResource(GameObject Resource, float degree)
    {
        Resource.gameObject.SetActive(true);

        RectTransform GORT = Resource.GetComponent<RectTransform>();
        GORT.localScale = Vector3.zero;
        for (int i = 0; i < 10; i++)
        {
            yield return new WaitForSeconds(.5f / 10);
            GORT.localScale = Vector3.one * i / 10;
            GORT.localEulerAngles = new Vector3(0, 0, degree * i / 10);
        }
    }
    IEnumerator FoldResource(GameObject Resource)
    {
        RectTransform GORT = Resource.GetComponent<RectTransform>();
        GORT.localScale = Vector3.one;
        for (int i = 0; i < 10; i++)
        {
            yield return new WaitForSeconds(.5f / 10);
            GORT.localScale = Vector3.one - Vector3.one * i / 10;
        }
        GORT.localEulerAngles = Vector3.zero;
        Resource.gameObject.SetActive(false);
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

    private void HandleRespawn()
    {
        if (!isAlive && inputHandler.inputJumping)
        {
            if (GameManager.SharedInstance.DeathScreen.activeInHierarchy)
            {
                GameManager.SharedInstance.DeathScreen.SetActive(false);
            }


            GameManager.SharedInstance.HandleCloneJuiceDrain();
            // TODO: Add Transition/ Fade to black/ camera shutter effect?!
            this.gameObject.transform.position = Vector3.zero;
            isAlive = true;
        }
    }

    private void HandleGameOver()
    {
        if (GameManager.SharedInstance.hasLost && inputHandler.inputJumping)
        {
            SceneManager.LoadScene("MenuScene");
        }
    }

    IEnumerator ShieldRespawn(float timeToRecharge)
    {
        float counter = 0;
        while (dissolve.GetFloat("_DissolveAmount") < 1)
        {
            counter += dissolveRate;
            for (int i = 0; i <= 10; i++)
            {
                dissolve.SetFloat("_DissolveAmount", counter);
                yield return new WaitForSeconds(refreshRate);
            }
        }

        playerShield = false;
        audioSource.PlayOneShot(shieldBreakAudio, 1f);
        playerShieldGO.SetActive(false);
        yield return new WaitForSeconds(timeToRecharge);
        dissolve.SetFloat("_DissolveAmount", 0);
        playerShield = true;
        audioSource.PlayOneShot(shieldRechargeAudio, 1f);
        playerShieldGO.SetActive(true);
    }
}
