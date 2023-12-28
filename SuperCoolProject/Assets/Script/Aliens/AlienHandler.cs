using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityEngine.VFX;

public class AlienHandler : MonoBehaviour
{
    #region Variables

    public enum AlienState
    {
        looking,
        hunting,
        evading,
        loving,
        roaming
    }

    public enum AlienAge
    {
        resource,
        child,
        sexualActive,
        fullyGrown
    }

    [Header("Tick stats")]
    public float tickTimer;
    public float tickTimerMax = .5f;

    [Header("General AlienStuff")]
    public float alienSpeed = 5;
    public float lookRadius = 10;
    public GameObject[] alienSpecies; // 0:Sphere > 1:Square > 2:Triangle  
    public GameObject resourceSteamGO;
    public ParticleSystem resourceSteam;
    private float delta;
    private float step;
    private int alienLifeResource = 1;
    private int alienLifeChild = 2;
    private int alienLifeSexual = 3;
    private int alienLifeFullGrown = 5;
    public int timeToChild = 5;
    public int timeToSexual = 15;
    public int timeToFullGrown = 25;

    [Header("Dissolve")]
    public Material dissolve;
    public SkinnedMeshRenderer skinRenderer1;
    public SkinnedMeshRenderer skinRenderer2;
    public SkinnedMeshRenderer skinRenderer3;
    public Material[] orignalMaterial;
    public float dissolveRate = 0.0125f;
    public float refreshRate = 0.025f;

    [Header("This Alien")]
    public bool isFemale;
    public int maxAmountOfBabies = 10;
    public int currentSpecies;
    public AlienState currentState;
    public AlienAge currentAge;
    Rigidbody rb;
    Collider coll;
    public RawImage currentStateIcon;
    public Texture[] allStateIcons; // 0: eye, 1: crosshair, 2: wind, 3: heart, 4: shield
    public int alienHealth;
    public float lifeTime = 0;
    public float lustTimer = 0;
    public float hungerTimer = 0;
    public float lustTimerThreshold = 5;
    public float hungerTimerThreshold = 5;
    Vector3 targetPosition = Vector3.one * 1000;
    bool hasInteractedWithPlayer;

    [Header("Target Alien")]
    public GameObject closestAlien = null;
    public GameObject lastClosestAlien = null;
    AlienHandler closestAlienHandler = null;
    int closestAlienIndex;


    public Animation[] anim;

    #endregion

    private void Awake()
    {
        //ResetVariable();
        //DisgardClosestAlien();
        //ActivateCurrentModels(currentSpecies);
        //if (rb == null) { rb = this.GetComponent<Rigidbody>(); }
        //if (coll == null) { coll = this.GetComponent<Collider>(); }
        ////DisableRagdoll();
        //coll.isTrigger = true;

    }

    private void OnEnable()
    {
        ResetVariable();
        DisgardClosestAlien();
        ActivateCurrentModels(currentSpecies);
        if (rb == null) { rb = this.GetComponent<Rigidbody>(); }
        if (coll == null) { coll = this.GetComponent<Collider>(); }
        //DisableRagdoll();
        coll.isTrigger = true;
    }

    // Disable this script when the GameObject moves out of the camera's view
    void OnBecameInvisible()
    {
        alienSpecies[currentSpecies].SetActive(false);
    }

    // Enable this script when the GameObject moves into the camera's view
    void OnBecameVisible()
    {
        alienSpecies[currentSpecies].SetActive(true);
    }

    private void FixedUpdate()
    {
        delta = Time.deltaTime;
        lifeTime += delta;
        lustTimer += delta;
        hungerTimer += delta;
        tickTimer += delta;
        step = (alienSpeed + lifeTime / 25) * delta;

        //Keep alien on the floor (y)
        if (transform.position.y > .2f) { transform.position = new Vector3(transform.position.x, 0.1f, transform.position.z); }
        // Keep Alien on the island
        if (transform.position.x * transform.position.x + transform.position.z * transform.position.z > GameManager.SharedInstance.worldRadius * GameManager.SharedInstance.worldRadius)
        {
            // TODO: better placement than this
            this.gameObject.SetActive(false);
        }


        // Only Render on Tick condition
        while (tickTimer >= tickTimerMax)
        {
            // Handle Aging now with coroutine
            //HandleAging(lifeTime);

            if (closestAlien != null)
            {
                if (currentState == AlienState.hunting)
                {
                    HandleAttacking(closestAlien);
                }
                else if (currentState == AlienState.evading)
                {
                    HandleFleeing(closestAlien);
                }
                else if (currentState == AlienState.loving)
                {
                    HandleLoveApproach(closestAlien);
                }
            }

            if (currentState == AlienState.looking)
            {
                HandleLooking();
            }
            else if (currentState == AlienState.roaming)
            {
                HandleRoaming(delta);
            }

            tickTimer -= tickTimerMax;
        }
        // Finaly process movement
        HandleMovement(step);

    }

    public void HandleLooking()
    {
        HandleStateIcon(0); // 0: eye, 1: crosshair, 2: wind, 3: heart, 4: shield, 5: clock, 6: loader
        StartCoroutine(DoNothingForRandomTime()); // SHort time where alien just stands and looks

        if (anim[currentSpecies] != null)
        {
            if (currentSpecies != 0)
            {
                anim[currentSpecies].Play("Armature|IDLE");
            }
        }

        #region Find closest Alien
        int layerMask = 1 << 9; // Lyer 9 is Alien
        Collider[] aliensInRange;
        aliensInRange = Physics.OverlapSphere(this.transform.position, lookRadius, layerMask);

        float closestDistance = lookRadius; ;

        for (int i = 0; i < aliensInRange.Length; i++)
        {
            // Prevent checking on the last alien
            if (aliensInRange[i].gameObject == lastClosestAlien ||
                aliensInRange[i].gameObject == this.gameObject)
            {
                continue;
            }

            closestAlienHandler = aliensInRange[i].gameObject.GetComponent<AlienHandler>();

            if (currentSpecies == closestAlienHandler.currentSpecies) // Same species
            {
                if (closestAlienHandler.currentAge == AlienAge.resource)
                {
                    DisgardClosestAlien();
                    continue;
                }
                else if (isFemale != closestAlienHandler.isFemale) // Opposite Sex
                {
                    if (currentAge == AlienAge.sexualActive)
                    {
                        if (lustTimer < lustTimerThreshold)
                        {
                            DisgardClosestAlien();
                            continue;
                        }
                    }
                }
                else // Same Sex (cannot reproduce...)
                {
                    DisgardClosestAlien();
                    continue;
                }
            }
            else // other Species
            {
                closestAlienIndex = closestAlienHandler.currentSpecies;

                if (currentSpecies == closestAlienIndex + 1 || (currentSpecies == 0 && closestAlienIndex == 2)) // potential food
                {
                    if (hungerTimer < hungerTimerThreshold)
                    {
                        DisgardClosestAlien();
                        continue;
                    }
                }
            }

            // Ensures to act upon the closest alien
            float dist = Vector3.Distance(aliensInRange[i].transform.position, transform.position);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                // Set the variables
                closestAlien = aliensInRange[i].gameObject;
                targetPosition = closestAlien.transform.position;
            }

            // Stop looking if alien is really close
            //if (dist < alertDistanceThreshold) { break; }
        }

        #endregion


        #region Find fitting interaction with closest alien
        if (closestAlien == null)
        {
            currentState = AlienState.roaming;
        }
        // Have Alien look around for some time before following action
        else if (closestAlien != null || closestAlienHandler != null)
        {
            #region Who eats who
            // Check to which state the alien switches
            // 0:Sphere > 1:Square > 2:Triangle 
            // Triangle eats Square / 2 eats 1
            // Square eats Sphere / 1 eats 0
            // Sphere eats Triangle / 0 eats 2
            #endregion

            // Dublicated checks, but just in case
            if (closestAlienIndex == currentSpecies)
            {
                currentState = AlienState.loving;
            }
            else if ((currentSpecies == closestAlienIndex - 1 || (currentSpecies == 2 && closestAlienIndex == 0))) // 0:Sphere > 1:Square > 2:Triangle 
            {
                currentState = AlienState.evading;
            }
            else if ((currentSpecies == closestAlienIndex + 1 || (currentSpecies == 0 && closestAlienIndex == 2)) && hungerTimer > hungerTimerThreshold) // 0:Sphere > 1:Square > 2:Triangle 
            {
                currentState = AlienState.hunting;
            }
        }

        #region Loop over List approach
        //for (int i = 0; i < PoolManager.SharedInstance.AlienPool.Count; i++)  //list of gameObjects to search through
        //{
        //    if (PoolManager.SharedInstance.AlienPool[i] == this.gameObject || PoolManager.SharedInstance.AlienPool[i] == lastClosestAlien) continue;

        //    float dist = Vector3.Distance(PoolManager.SharedInstance.AlienPool[i].transform.position, transform.position);
        //    if (dist < lookRadius)
        //    {
        //        closestAlien = PoolManager.SharedInstance.AlienPool[i];
        //        closestAlienIndex = closestAlien.GetComponent<AlienHandler>().currentSpecies;
        //        break;
        //    }
        //}
        //return closestAlien;
        #endregion

        #endregion
    }

    private void HandleRoaming(float delta)
    {
        HandleStateIcon(5); // 0: eye, 1: crosshair, 2: wind, 3: heart, 4: shield, 5: clock, 6: loader
        HandleFindRandomSpot();
    }

    private void HandleFindRandomSpot()
    {
        if (targetPosition == Vector3.one * 1000)
        {
            float randDirX = UnityEngine.Random.Range(0, 2) - .5f;
            float randDirZ = UnityEngine.Random.Range(0, 2) - .5f;
            targetPosition = transform.position + new Vector3(randDirX, 0, randDirZ) * 10;

        }

        // When arrived at position, chill for a bit then look again
        if (Vector3.Distance(transform.position, targetPosition) < .1f)
        {
            StartCoroutine(DoNothingForRandomTime());
            currentState = AlienState.looking;
            targetPosition = Vector3.one * 1000;
        }

        // Check if new coordinate is within circle
        if (targetPosition.x * targetPosition.x + targetPosition.z * targetPosition.z > GameManager.SharedInstance.worldRadius * GameManager.SharedInstance.worldRadius)
        {
            targetPosition = Vector3.one * 1000;
        }
    }

    IEnumerator DoNothingForRandomTime()
    {
        float lookTime = UnityEngine.Random.Range(10, 30) / 10;
        yield return new WaitForSeconds(lookTime);
        currentState = AlienState.looking;
    }


    public void HandleFleeing(GameObject targetAlien)
    {
        if (!targetAlien.activeInHierarchy || Vector3.Distance(targetAlien.transform.position, transform.position) > lookRadius + 1)
        {
            closestAlien = null;
            currentState = AlienState.looking;
        }

        HandleStateIcon(2); // 0: eye, 1: crosshair, 2: wind, 3: heart, 4: shield, 5: clock, 6: loader
        targetPosition = this.transform.position + (this.transform.position - targetAlien.transform.position);
        //Debug.Log("Escaping Vecotr: " + targetPosition);
        //Debug.DrawLine(this.transform.position, this.transform.position + (this.transform.position - targetAlien.transform.position), Color.green);
    } // Use this here on the player as well to scare the aliens away

    public void HandleAttacking(GameObject targetAlien) // Player makes them flee as well and by acting als targetAlien in PlayerManager
    {
        if (!targetAlien.activeInHierarchy)
        {
            closestAlien = null;
            currentState = AlienState.looking;
        }
        if (targetAlien.CompareTag("Player"))
        {
            HandleStateIcon(4); // 0: eye, 1: crosshair, 2: wind, 3: heart, 4: shield, 5: clock, 6: loader
        }
        else
        {
            HandleStateIcon(1); // 0: eye, 1: crosshair, 2: wind, 3: heart, 4: shield, 5: clock, 6: loader
        }
        targetPosition = targetAlien.transform.position; // Update targetPosition only every tick update
    }

    private void HandleLoveApproach(GameObject targetAlien)
    {
        if (!targetAlien.activeInHierarchy)
        {
            closestAlien = null;
            currentState = AlienState.looking;
        }
        HandleStateIcon(3); // 0: eye, 1: crosshair, 2: wind, 3: heart, 4: shield, 5: clock, 6: loader
        targetPosition = targetAlien.transform.position; // Update targetPosition only every tick update
    }

    private void HandleMating()
    {
        if (isFemale)
        {
            int amountOfBabies = UnityEngine.Random.Range(1, maxAmountOfBabies);

            for (var i = 0; i < amountOfBabies; i++)
            {
                GameObject alienPoolGo = PoolManager.SharedInstance.GetPooledAliens();
                if (alienPoolGo != null)
                {
                    AlienHandler newBornAlien;
                    newBornAlien = alienPoolGo.GetComponent<AlienHandler>();
                    newBornAlien.currentSpecies = currentSpecies;
                    alienPoolGo.SetActive(true);

                    // TODO: Spawn them somewhere near, in the middle (?!)
                    alienPoolGo.transform.position = new Vector3(transform.position.x, 0.5f, transform.position.z) + Vector3.forward;
                }
            }
        }
        lustTimer = 0;
        DisgardClosestAlien();
    }

    public IEnumerator HandleAge()
    {
        // Resource Life
        resourceSteamGO.SetActive(true);
        currentAge = AlienAge.resource;
        alienHealth = alienLifeResource;
        transform.localScale = Vector3.one * 0.2f;
        yield return new WaitForSeconds(timeToChild);
        // Child Life
        resourceSteamGO.SetActive(false);
        currentAge = AlienAge.child;
        alienHealth = alienLifeChild;
        transform.localScale = Vector3.one * .5f;
        yield return new WaitForSeconds(timeToSexual);
        // Sexual active Life
        currentAge = AlienAge.sexualActive;
        alienHealth = alienLifeSexual;
        transform.localScale = Vector3.one;
        yield return new WaitForSeconds(timeToFullGrown);
        // Full Grown Life
        currentAge = AlienAge.fullyGrown;
        alienHealth = alienLifeFullGrown;
        transform.localScale = Vector3.one * 1.2f;

    }

    //public void HandleAging(float lifeTime)
    //{
    //    if (lifeTime < timeToChild)
    //    {
    //        currentAge = AlienAge.resource;
    //        alienHealth = alienLifeResource;
    //        transform.localScale = Vector3.one * 0.2f;
    //    }
    //    else if (lifeTime > timeToChild && lifeTime < timeToSexual && lifeTime < timeToFullGrown)
    //    {
    //        resourceSteamGO.SetActive(false);
    //        currentAge = AlienAge.child;
    //        alienHealth = alienLifeChild;
    //        transform.localScale = Vector3.one * .5f;
    //    }
    //    else if (lifeTime > timeToSexual && lifeTime < timeToFullGrown)
    //    {
    //        currentAge = AlienAge.sexualActive;
    //        alienHealth = alienLifeSexual;
    //        transform.localScale = Vector3.one;
    //    }
    //    else if (lifeTime > timeToFullGrown)
    //    {
    //        currentAge = AlienAge.fullyGrown;
    //        alienHealth = alienLifeFullGrown;
    //        transform.localScale = Vector3.one * 1.2f;
    //    }
    //}

    private void HandleMovement(float step)
    {
        if (currentAge != AlienAge.resource && currentState != AlienState.looking)
        {
            if (anim[currentSpecies] != null)
            {
                anim[currentSpecies].Play("Armature|WALK");
            }

            transform.position = Vector3.MoveTowards(transform.position, targetPosition, step);

            if (Vector3.Distance(transform.position, targetPosition) < .1f)
            {
                currentState = AlienState.looking;
                DisgardClosestAlien();
            }

            transform.LookAt(targetPosition, Vector3.up);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Handle Alien interaction
        if (other.gameObject.CompareTag("Alien"))
        {
            AlienHandler otherAlien = other.gameObject.GetComponent<AlienHandler>();
            if (currentAge == AlienAge.resource)
            {
                if (currentSpecies != otherAlien.currentSpecies) // You the resource gets trampled
                {
                    this.gameObject.SetActive(false);
                }
            }
            else
            {
                if (currentAge == AlienAge.sexualActive && currentSpecies == otherAlien.currentSpecies)
                {
                    if (lustTimer > lustTimerThreshold)
                    {
                        // Spawn new Species
                        HandleMating();
                    }

                }
                else if (
                    hungerTimer > hungerTimerThreshold &&
                    (currentSpecies == otherAlien.currentSpecies + 1 ||
                    (currentSpecies == 0 && otherAlien.currentSpecies == 2)))
                {
                    #region Who Eats Who
                    // This aliens eats the other
                    // 0:Sphere > 1:Square > 2:Triangle 
                    // Triangle eats Square / 2 eats 1
                    // Square eats Sphere / 1 eats 0
                    // Sphere eats Triangle / 0 eats 2
                    #endregion

                    // Handles eat other alien
                    hungerTimer = 0;
                    otherAlien.gameObject.SetActive(false);
                    DisgardClosestAlien();
                }

            }
            return;
        }
        // Handle Bullet interaction
        else if (other.gameObject.CompareTag("Bullet"))
        {
            // Cannot shoot resource
            if (currentAge == AlienAge.resource) { return; }

            // Needs to deactivate this here so it does not trigger multiple times
            // Maybe deactive the Collider on the Bullet and then make sure to enable it again if new spawned
            other.gameObject.SetActive(false);
            alienHealth--;

            // Handle Alien Death
            if (alienHealth <= 0)
            {
                
                // TODO: Add Coroutine & Ragdoll to show impact/force of bullets
                //EnableRagdoll();
                if (currentSpecies == 0) { GameManager.SharedInstance.sphereKilled++; }
                if (currentSpecies == 1) { GameManager.SharedInstance.squareKilled++; }
                if (currentSpecies == 2) { GameManager.SharedInstance.triangleKilled++; }

                StartCoroutine(Dissolve());
                return;
                // this.gameObject.SetActive(false);
            };
        }
        // Handle Player interaction && is also put in trigger / trigger state changes in HandleAging()
        else if (other.gameObject.CompareTag("Player"))
        {
            if (hasInteractedWithPlayer == false)
            {
                PlayerManager PM = other.gameObject.GetComponent<PlayerManager>();
                // Handle Gathering resource
                if (currentAge == AlienAge.resource)
                {
                    hasInteractedWithPlayer = true;
                    PM.HandleGainResource(currentSpecies);
                    this.gameObject.SetActive(false);
                }
                else
                {
                    hasInteractedWithPlayer = true;
                    PM.HandleHit();
                    this.gameObject.SetActive(false);
                }
            }
        }
    }

    public void ActivateCurrentModels(int currentSpeziesIndex)
    {
        foreach (var item in alienSpecies)
        {
            item.SetActive(false);
        }

        alienSpecies[currentSpeziesIndex].SetActive(true);
    }

    private void HandleStateIcon(int index)
    {
        currentStateIcon.texture = allStateIcons[index]; // 0: eye, 1: crosshair, 2: wind, 3: heart, 4: shield, 5: clock, 6: loader
    }

    void DisgardClosestAlien()
    {
        if (closestAlien != null)
        {
            lastClosestAlien = closestAlien;
        }
        closestAlien = null;
        closestAlienHandler = null;
        currentState = AlienState.looking;
        targetPosition = Vector3.one * 1000;
    }

    void ResetVariable()
    {
        timeToChild += UnityEngine.Random.Range(0, 10);
        alienHealth = alienLifeResource;
        currentAge = AlienAge.resource;
        lustTimer = 0;
        hungerTimer = 0;
        isFemale = UnityEngine.Random.Range(0, 2) == 1;
        HandleStateIcon(6); // 0: eye, 1: crosshair, 2: wind, 3: heart, 4: shield, 5: clock, 6: loader

        // TODO: Place this at better location
        ParticleSystem.MainModule ma = resourceSteam.main;
        if (currentSpecies == 0)
        {
            ma.startColor = Color.blue;
        }
        else if (currentSpecies == 1)
        {
            ma.startColor = Color.green;
        }
        else if (currentSpecies == 2)
        {
            ma.startColor = Color.red;
        }
        StartCoroutine(HandleAge());
    }

    void DisableRagdoll()
    {
        rb.isKinematic = true;
        rb.detectCollisions = false;
        //coll.isTrigger = true;
    }

    void EnableRagdoll()
    {
        rb.isKinematic = false;
        rb.detectCollisions = true;
        coll.isTrigger = false;
    }

    IEnumerator Dissolve()
    {
        switch (currentSpecies)
        {
            case 0:
                skinRenderer1.material = dissolve;
                break;
            case 1:
                skinRenderer2.material = dissolve;
                break;
            default:
                skinRenderer3.material = dissolve;
                break;
        }

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
        dissolve.SetFloat("_DissolveAmount", 0);
        this.gameObject.SetActive(false);

        switch (currentSpecies)
        {
            case 0:
                skinRenderer1.material = orignalMaterial[0];
                break;
            case 1:
                skinRenderer2.material = orignalMaterial[1];
                break;
            default:
                skinRenderer3.material = orignalMaterial[2];
                break;
        }

    }
}
