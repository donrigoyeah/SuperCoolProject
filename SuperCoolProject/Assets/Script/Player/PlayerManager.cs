using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
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
    public bool isAlive;
    public bool isInteracting;
    public float invincibleFrames = .5f;
    public float timeSinceLastHit;
    public GameObject LightBeam;
    public GameObject deadPlayer;
    public GameObject playerShieldGO;
    public GameObject player;

    [Header("Dissolve")]
    public Material dissolve;
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
    Transform MyTransform;

    public float resourceDrain = .1f;
    public float resourceGain = 5;

    // 0:Sphere, 1:Square, 2:Triangle
    [Header("UI Elements")]
    public Image[] resourcePieCharts;
    public GameObject ResourceUISphere;
    public GameObject ResourceUISquare;
    public GameObject ResourceUITriangle;
    public GameObject[] closestResourceIndicator;  // 0:Sphere, 1:Square, 2:Triangle
    public Light resourceIndicatorLight;
    public Material[] resourceMaterial; // 0:Sphere, 1:Square, 2:Triangle
    public float currentResourceSphere;
    public float currentResourceSquare;
    public float currentResourceTriangle;
    
    
    [Header("Audio")]
    [SerializeField] private AudioClip shieldRechargeAudio;
    [SerializeField] private AudioClip shieldBreakAudio;
    [SerializeField] private AudioClip deathAudio;
    private AudioSource audioSource;

    private InputHandler inputHandler;
    private Animator playerAnim;
    private AlienHandler CurrentSurroundingAH;

    private int alienLayerMask = 1 << 9; // Lyer 9 is Alien, so only use this layer

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        inputHandler = GetComponent<InputHandler>();
        MyTransform = GetComponent<Transform>();
        playerAnim = GetComponentInChildren<Animator>();

        dissolve = playerShieldGO.gameObject.GetComponent<Renderer>().material;
        // dissolve.SetFloat("_DissolveAmount", 0);
    }

    private void FixedUpdate()
    {
        timeSinceLastHit += Time.deltaTime;
        HandleSurroundingAliens();
        HandleResource();
        HandleRespawn();
        HandleGameOver();


    }

    public void HandleHit()
    {
        if (timeSinceLastHit < invincibleFrames)
        {
            return;
        }
        else
        {
            if (playerShield == false)
            {
                HandleDeath();
            }
            else
            {
                StartCoroutine(ShieldRespawn(shieldRechargeTime));
            }
            timeSinceLastHit = 0;
            return;
        }
    }

    private void HandleDeath()
    {
        Debug.Log("One TODO left here, remove already done TODO once you verify that they are alright");
        // TODO: Instanciate GameObject like (deadPlayerBody)
        // Add draggable script to it
        // WHen returned to spaceship, enable upgrades again
        // Make global boolean to handle this

        // Set Variable to disable movement/input
        isAlive = false;
        StopAllCoroutines();
        audioSource.PlayOneShot(deathAudio, 1f);

        playerAnim.SetBool("IsDead", true);

        GameObject deadPlayerInstance = Instantiate(deadPlayer, MyTransform.position, Quaternion.identity);

        Rigidbody deadPlayerRigidbody = deadPlayerInstance.GetComponent<Rigidbody>();
        if (deadPlayerRigidbody != null)
        {
            Vector3 forceDirection = Vector3.up;
            float forceMagnitude = 1f;
            deadPlayerRigidbody.AddForce(forceDirection * forceMagnitude, ForceMode.Impulse);
        }

        // Enable UI Element
        // TODO: Check if all players are dead. otherwise maybe make deathscreen on playerHUD as well

        if (GameManager.Instance.currentCloneJuice < 0)
        {
            GameManager.Instance.hasLost = true;
            return;
        }

        if (GameManager.Instance.players.Count == 1)
        {
            GameManager.Instance.DeathScreen.SetActive(true);
            GameManager.Instance.DeathScreenCloneJuiceUI.fillAmount = GameManager.Instance.currentCloneJuice / GameManager.Instance.maxCloneJuice;
        }

        // Reset all resource variables back to max on new clone
        currentSphereResource = maxSphereResource;
        currentSquareResource = maxSquareResource;
        currentTriangleResource = maxTriangleResource;
    }

    private void HandleSurroundingAliens()
    {
        aliensInRange = Physics.OverlapSphere(MyTransform.position, playerDetectionRadius, alienLayerMask);

        for (int i = 0; i < aliensInRange.Length; i++)
        {
            CurrentSurroundingAH = aliensInRange[i].gameObject.GetComponent<AlienHandler>();
            if (CurrentSurroundingAH.brainWashed == false)
            {
                if (CurrentSurroundingAH.currentAge == AlienHandler.AlienAge.resource) // If sorrounding Alien is resource, put into resource Array
                {
                    closestResource[CurrentSurroundingAH.currentSpecies] = CurrentSurroundingAH;
                    continue;
                }

                if (CurrentSurroundingAH.brainWashed) // Interaction with player in TutorialScene, prevents HandleUpdateTarget error
                {
                    return;
                }

                CurrentSurroundingAH.targetAlien = this.gameObject;

                if (CurrentSurroundingAH.currentAge == AlienHandler.AlienAge.fullyGrown)
                {
                    CurrentSurroundingAH.currentState = AlienHandler.AlienState.hunting;
                    CurrentSurroundingAH.HandleAttacking(this.gameObject, true); // this time its not an alienGO but the player, isAttackingPlayer
                    continue;
                }
                else
                {
                    CurrentSurroundingAH.currentState = AlienHandler.AlienState.evading;
                    CurrentSurroundingAH.HandleFleeing(this.gameObject, true); // this time its not an alienGO but the player, isEvadingPlayer
                    continue;
                }
            }
        }
    }

    private void HandleResourceDetection(int neededResource)
    {
        // TODO: This is possible quite cost intense!!!
        // TODO: Make an Array of resources, add aliens to it after spawning, remove when eaten or evolved

        #region Find via OverlappingSphere

        //int layerMask = 1 << 9; // Lyer 9 is Alien
        //float distanceToResource = playerResourceScanRadius;

        //if (closestResource[neededResource] != null)
        //{
        //    closestResourceIndicator[neededResource].SetActive(true);
        //    HandleResourceDetectionIndicator(closestResource[neededResource].transform.position, neededResource);
        //    if (closestResource[neededResource].currentAge != AlienHandler.AlienAge.resource)
        //    {
        //        closestResource[neededResource] = null;
        //        //Debug.Log("Resource became unavailable");
        //        return;
        //    }

        //    if (closestResource[neededResource].gameObject.activeInHierarchy == false)
        //    {
        //        closestResource[neededResource] = null;
        //    }
        //    return;
        //}
        //closestResourceIndicator[neededResource].SetActive(false);

        ////Debug.Log("Search for Closest Resource");
        //resourceInRange = Physics.OverlapSphere(this.transform.position, playerResourceScanRadius, layerMask);
        //foreach (var item in aliensInRange)
        //{
        //    AlienHandler AH = item.gameObject.GetComponent<AlienHandler>();
        //    if (AH.currentAge != AlienHandler.AlienAge.resource) { continue; }
        //    if (AH.currentSpecies != neededResource) { continue; }

        //    float tmpDistance = Vector3.Distance(AH.transform.position, this.transform.position);
        //    //Debug.Log("Distance to Resource: " + tmpDistance);
        //    if (tmpDistance > distanceToResource) { continue; }

        //    distanceToResource = tmpDistance;
        //    closestResource[neededResource] = AH;
        //    //Debug.Log("Found Closest Resource");
        //}

        #endregion

        #region Find via ListScan

        if (closestResource[neededResource] != null)
        {
            if (closestResource[neededResource].currentAge != AlienHandler.AlienAge.resource ||
                closestResource[neededResource].gameObject.activeInHierarchy == false)
            {
                closestResource[neededResource] = null;
                return;
            }

            closestResourceIndicator[neededResource].SetActive(true);
            HandleResourceDetectionIndicator(closestResource[neededResource].transform.position, neededResource);
            return;
        }

        if (closestResource[neededResource] == null)
        {
            closestResourceIndicator[neededResource].SetActive(false);

            float dist = 1000;
            float currentDist;

            int loopAmount =
                neededResource == 0 ? AlienManager.Instance.resourceSphere.Count :
                neededResource == 1 ? AlienManager.Instance.resourceSquare.Count :
                neededResource == 2 ? AlienManager.Instance.resourceTriangle.Count : 0;

            List<AlienHandler> ResourceList =
               neededResource == 0 ? AlienManager.Instance.resourceSphere :
               neededResource == 1 ? AlienManager.Instance.resourceSquare :
               neededResource == 2 ? AlienManager.Instance.resourceTriangle : null;

            for (int i = 0; i < loopAmount; i++)
            {
                currentDist = Vector3.Distance(ResourceList[i].transform.position, MyTransform.position);
                if (currentDist < dist)
                {
                    dist = currentDist;
                    closestResource[neededResource] = ResourceList[i];

                    if (dist < 10)
                    {
                        break;
                    }
                }
            }
        }

        #endregion

    }

    private void HandleResourceDetectionIndicator(Vector3 targetResource, int neededResource)
    {
        if (targetResource == null)
        {
            closestResourceIndicator[neededResource].SetActive(false);
            return;
        }

        // 0:Sphere, 1:Square, 2:Triangle
        closestResourceIndicator[neededResource].SetActive(true);
        Vector3 targetRotation = targetResource - MyTransform.position;
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

        currentResourceSphere = maxSphereResource - currentSphereResource;
        currentResourceSquare = maxSquareResource - currentSquareResource;
        currentResourceTriangle = maxTriangleResource - currentTriangleResource;
        
        MaterialEmmissionControler(1);

        if (currentResourceSphere >= 50)
        {
            MaterialEmmissionControler(10);

            if (currentResourceSphere >= 75)
            {
                MaterialEmmissionControler(20);
            }
        }
        
        /*// Only show resource UI if below 75%
        if (currentSphereResource < 3 * maxSphereResource / 4)
        {
            
            if (bulbFlashing)
            {
                // StartCoroutine(BulbFlashing());
                bulbFlashing = false;
            }

            //StartCoroutine(HandleResourceLightIndicator(0));
            HandleResourceDetection(0);
            if (sphereUnfolded != true)
            {
                // StartCoroutine(UnfoldResource(ResourceUISphere, 50));
                sphereUnfolded = true;
                //ResourceUISphere.SetActive(true);
            }
        }
        else
        {
            if (sphereUnfolded != false)
            {
                DeactivateResourceDetectionIndicator(0);
                // StartCoroutine(FoldResource(ResourceUISphere));
                sphereUnfolded = false;
                //ResourceUISphere.SetActive(false);
            }
        }*/

        /*// Only show resource UI if below 75%
        if (currentSquareResource < 3 * maxSquareResource / 4)
        {
            //StartCoroutine(HandleResourceLightIndicator(1));
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
            //StartCoroutine(HandleResourceLightIndicator(2));
            HandleResourceDetection(2);
            if (triangleUnfolded != true)
            {
                // StartCoroutine(UnfoldResource(ResourceUITriangle, 0));
                triangleUnfolded = true;
                //ResourceUITriangle.SetActive(true);
            }
        }
        else
        {
            if (triangleUnfolded != false)
            {
                DeactivateResourceDetectionIndicator(2);
                // StartCoroutine(FoldResource(ResourceUITriangle));
                triangleUnfolded = false;
                //ResourceUITriangle.SetActive(false);
            }
        }*/

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

    private void MaterialEmmissionControler(float multiplyer)
    {
        resourceMaterial[0].SetColor("_EmissionColor", Color.blue * Mathf.PingPong(currentResourceSphere * multiplyer, 2f));
        resourceMaterial[1].SetColor("_EmissionColor", Color.yellow * Mathf.PingPong(currentResourceSquare * multiplyer, 4f));
        resourceMaterial[2].SetColor("_EmissionColor", Color.red * Mathf.PingPong(currentResourceTriangle * multiplyer, 2f));
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
            if (GameManager.Instance.DeathScreen.activeInHierarchy)
            {
                GameManager.Instance.DeathScreen.SetActive(false);
            }

            GameManager.Instance.HandleCloneJuiceDrain();
            // TODO: Add Transition/ Fade to black/ camera shutter effect?!

            isAlive = true;

            MyTransform.position = Vector3.zero;
        }
    }

    private void HandleGameOver()
    {
        if (GameManager.Instance.hasLost && inputHandler.inputJumping)
        {
            SceneManager.LoadScene("MenuScene");
        }
    }

    public IEnumerator UnfoldResource(GameObject Resource, float degree)
    {
        int steps = 50;
        float animationDuration = .5f;
        Resource.gameObject.SetActive(true);
        RectTransform GORT = Resource.GetComponent<RectTransform>();
        GORT.localScale = Vector3.zero;
        for (int i = 0; i < steps; i++)
        {
            yield return new WaitForSeconds(animationDuration / steps);
            GORT.localScale = Vector3.one * 3 * i / steps;
            GORT.localEulerAngles = new Vector3(0, 0, degree * i / steps);
        }
    }

    public IEnumerator FoldResource(GameObject Resource)
    {
        int steps = 50;
        float animationDuration = .5f;

        RectTransform GORT = Resource.GetComponent<RectTransform>();
        GORT.localScale = Vector3.one * 2;
        for (int i = 0; i < steps; i++)
        {
            yield return new WaitForSeconds(animationDuration / steps);
            GORT.localScale = Vector3.one * 2 - Vector3.one * 2 * i / steps;
        }
        GORT.localEulerAngles = Vector3.zero;
        Resource.gameObject.SetActive(false);
    }

    IEnumerator ShieldRespawn(float timeToRecharge)
    {
        float counter = 0;
        int steps = 30;
        while (dissolve.GetFloat("_DissolveAmount") < 1)
        {
            counter += dissolveRate;
            for (int i = 0; i <= steps; i++)
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

    IEnumerator HandleResourceLightIndicator(int resource)
    {
        switch (resource)
        {
            case 0:
                resourceIndicatorLight.color = Color.blue;
                Debug.Log("blue");
                break;
            case 1:
                resourceIndicatorLight.color = Color.green;
                Debug.Log("green");
                break;
            case 2:
                resourceIndicatorLight.color = Color.red;
                Debug.Log("red");
                break;
        }

        resourceIndicatorLight.enabled = true;

        yield return new WaitForSeconds(3f); // Light on
        resourceIndicatorLight.enabled = false;

        yield return new WaitForSeconds(3); // Light off
        resourceIndicatorLight.enabled = true;
        resourceIndicatorLight.enabled = false;
    }

}
