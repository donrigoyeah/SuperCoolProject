using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class AlienHandler : MonoBehaviour
{
    #region Variables

    enum AlienState
    {
        roaming,
        hunting,
        evading,
        loving
    }

    enum AlienAge
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
    private float delta;
    private float step;
    private int alienLifeResource = 1;
    private int alienLifeChild = 2;
    private int alienLifeSexual = 3;
    private int alienLifeFullGrown = 5;
    private float alertDistanceThreshold = 2;
    public int timeToChild = 5;
    public int timeToSexual = 15;
    public int timeToFullGrown = 25;


    [Header("This Alien")]
    AlienState currentState;
    AlienAge currentAge;
    Rigidbody rb;
    Collider coll;
    public RawImage currentStateIcon;
    public Texture[] allStateIcons; // 0: eye, 1: crosshair, 2: wind, 3: heart, 4: shield
    public int alienHealth;
    public float lifeTime = 0;
    public float lustTimer = 0;
    public float hungerTimer = 0;
    public bool isFemale;
    public int currentSpecies;
    Vector3 targetPosition = Vector3.one * 1000;

    [Header("Target Alien")]
    public GameObject closestAlien = null;
    public GameObject lastClosestAlien = null;
    AlienHandler closestAlienHandler = null;
    int closestAlienIndex;

    #endregion

    private void Awake()
    {
        ResetVariable();
        DisgardClosestAlien();
        ActivateCurrentModels(currentSpecies);
        if (rb == null) { rb = this.GetComponent<Rigidbody>(); }
        if (coll == null) { coll = this.GetComponent<Collider>(); }
        //DisableRagdoll();
        coll.isTrigger = true;
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
        // Keep player on the island
        if (transform.position.x * transform.position.x + transform.position.z * transform.position.z > GameManager.SharedInstance.worldRadius * GameManager.SharedInstance.worldRadius)
        {
            // TODO: better placement than this
            this.gameObject.SetActive(false);
        }

        // Only Render on Tick condition
        while (tickTimer >= tickTimerMax)
        {
            HandleAging(lifeTime);

            if (currentAge == AlienAge.resource)
            {
                break;
            }
            else if (currentState == AlienState.roaming)
            {
                HandleRoaming();
            }
            else if (currentState == AlienState.hunting)
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

            tickTimer -= tickTimerMax;
        }

        // Finaly execute movement
        HandleMovement(step);
    }

    public void HandleRoaming()
    {
        HandleStateIcon(0); // 0: eye, 1: crosshair, 2: wind, 3: heart, 4: shield, 5: clock, 6: loader
        HandleFindRandomSpot();

        int layerMask = 1 << 9; // Lyer 9 is Alien
        Collider[] aliensInRange;
        aliensInRange = Physics.OverlapSphere(this.transform.position, lookRadius, layerMask);

        float closestDistance = lookRadius; ;
        // TODO: Make a better closest alien selection
        // Maybe if aggressor is near evade rather then love making (?!)
        // JUst to find closest alien
        for (int i = 0; i < aliensInRange.Length; i++)
        {
            if (aliensInRange[i].gameObject == lastClosestAlien || aliensInRange[i].gameObject == this.gameObject) continue;

            closestAlien = aliensInRange[i].gameObject;
            closestAlienHandler = closestAlien.GetComponent<AlienHandler>();

            if (closestAlienHandler.currentAge == AlienAge.resource) { continue; } // just resource/eggs
            if (currentSpecies == closestAlienHandler.currentSpecies && isFemale != closestAlienHandler.isFemale) continue; // can not mate
            if (hungerTimer < 10 && (currentSpecies == closestAlienIndex + 1 || (currentSpecies == 0 && closestAlienIndex == 2))) { continue; } // not hungry

            // Ensures to act upon the closest alien
            float dist = Vector3.Distance(aliensInRange[i].transform.position, transform.position);
            if (dist < closestDistance) { closestDistance = dist; }

            targetPosition = closestAlien.transform.position;
            closestAlienIndex = closestAlienHandler.currentSpecies;

            if (dist < alertDistanceThreshold) { break; }
        }

        if (closestAlien != null && closestAlienHandler != null)
        {
            // Check to which state the alien switches
            // 0:Sphere > 1:Square > 2:Triangle 
            // Triangle eats Square / 2 eats 1
            // Square eats Sphere / 1 eats 0
            // Sphere eats Triangle / 0 eats 2
            if (closestAlienIndex == currentSpecies &&
                lifeTime > 20 &&
                lustTimer > 10 &&
                isFemale != closestAlienHandler.isFemale &&
                closestAlienHandler.lustTimer > 10)
            {
                currentState = AlienState.loving;
            }
            else if ((currentSpecies == closestAlienIndex - 1 || (currentSpecies == 2 && closestAlienIndex == 0))) // 0:Sphere > 1:Square > 2:Triangle 
            {
                currentState = AlienState.evading;
            }
            else if ((currentSpecies == closestAlienIndex + 1 || (currentSpecies == 0 && closestAlienIndex == 2)) && hungerTimer > 10) // 0:Sphere > 1:Square > 2:Triangle 
            {
                currentState = AlienState.hunting;
            }
            else
            {
                currentState = AlienState.roaming;
            }
        }
        else
        {
            HandleFindRandomSpot();
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
    }

    private void HandleFindRandomSpot()
    {
        HandleStateIcon(0); // 0: eye, 1: crosshair, 2: wind, 3: heart, 4: shield, 5: clock, 6: loader
        if (targetPosition == Vector3.one * 1000 || Vector3.Distance(transform.position, targetPosition) < 1)
        {
            float randDirX = UnityEngine.Random.Range(0, 2) - .5f;
            float randDirY = UnityEngine.Random.Range(0, 2) - .5f;
            targetPosition = transform.position + new Vector3(randDirX, 0, randDirY) * 10;

            // Check if new coordinate is within circle
            if (targetPosition.x * targetPosition.x + targetPosition.z * targetPosition.z > GameManager.SharedInstance.worldRadius * GameManager.SharedInstance.worldRadius)
            {
                targetPosition = Vector3.one * 1000;
            }
        }
    }

    public void HandleFleeing(GameObject targetAlien)
    {
        if (!targetAlien.activeInHierarchy || Vector3.Distance(targetAlien.transform.position, transform.position) > lookRadius)
        {
            closestAlien = null;
            currentState = AlienState.roaming;
        }
        HandleStateIcon(2); // 0: eye, 1: crosshair, 2: wind, 3: heart, 4: shield, 5: clock, 6: loader
        targetPosition = this.transform.position + (this.transform.position - targetAlien.transform.position);
    } // Use this here on the player as well to scare the aliens away

    public void HandleAttacking(GameObject targetAlien) // Player makes them flee as well and by acting als targetAlien in PlayerManager
    {
        if (!targetAlien.activeInHierarchy)
        {
            closestAlien = null;
            currentState = AlienState.roaming;
        }
        HandleStateIcon(1); // 0: eye, 1: crosshair, 2: wind, 3: heart, 4: shield, 5: clock, 6: loader
        targetPosition = targetAlien.transform.position; // Update targetPosition only every tick update
    }

    private void HandleLoveApproach(GameObject targetAlien)
    {
        if (!targetAlien.activeInHierarchy)
        {
            closestAlien = null;
            currentState = AlienState.roaming;
        }
        HandleStateIcon(3); // 0: eye, 1: crosshair, 2: wind, 3: heart, 4: shield, 5: clock, 6: loader
        targetPosition = targetAlien.transform.position; // Update targetPosition only every tick update
    }

    private void HandleMating()
    {
        if (isFemale)
        {
            GameObject alienPoolGo = PoolManager.SharedInstance.GetPooledAliens();
            if (alienPoolGo != null)
            {
                AlienHandler newBornAlien;
                newBornAlien = alienPoolGo.GetComponent<AlienHandler>();
                newBornAlien.currentSpecies = currentSpecies;
                newBornAlien.isFemale = UnityEngine.Random.Range(0, 2) == 1;
                newBornAlien.HandleAging(0);
                alienPoolGo.SetActive(true);

                // TODO: Spawn them somewhere near, in the middle (?!)
                alienPoolGo.transform.position = new Vector3(transform.position.x, 0.5f, transform.position.z) + Vector3.forward;
            }
        }
        lustTimer = 0;
        DisgardClosestAlien();
    }

    public void HandleAging(float lifeTime)
    {
        if (lifeTime < timeToChild)
        {
            currentAge = AlienAge.resource;
            alienHealth = alienLifeResource;
            transform.localScale = Vector3.one * 0.2f;
        }
        else if (lifeTime > timeToChild)
        {
            currentAge = AlienAge.child;
            alienHealth = alienLifeChild;
            transform.localScale = Vector3.one * .5f;
        }
        else if (lifeTime > timeToSexual)
        {
            currentAge = AlienAge.sexualActive;
            alienHealth = alienLifeSexual;
            transform.localScale = Vector3.one;
        }
        else if (lifeTime > timeToFullGrown)
        {
            currentAge = AlienAge.fullyGrown;
            alienHealth = alienLifeFullGrown;
            transform.localScale = Vector3.one * 1.2f;
        }
    }

    private void HandleMovement(float step)
    {
        if (currentAge != AlienAge.resource)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, step);
            if (Vector3.Distance(transform.position, targetPosition) < .5f)
            {
                DisgardClosestAlien();
                HandleFindRandomSpot();
            }
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
                if (currentSpecies != otherAlien.currentSpecies) { this.gameObject.SetActive(false); } // You the resource gets trampled

            }
            else
            {
                if (currentSpecies == otherAlien.currentSpecies)
                {
                    if (currentAge == AlienAge.sexualActive &&
                       otherAlien.currentAge == AlienAge.sexualActive &&
                       lustTimer > 10 &&
                       otherAlien.lustTimer > 10 &&
                       otherAlien.currentAge != AlienAge.resource)
                    {
                        // Spawn new Species
                        HandleMating();
                    }
                }
                else if (
                    hungerTimer > 10 &&
                    (currentSpecies == otherAlien.currentSpecies + 1 ||
                    (currentSpecies == 0 && otherAlien.currentSpecies == 2)))
                {
                    // This aliens eats the other
                    // 0:Sphere > 1:Square > 2:Triangle 
                    // Triangle eats Square / 2 eats 1
                    // Square eats Sphere / 1 eats 0
                    // Sphere eats Triangle / 0 eats 2

                    otherAlien.gameObject.SetActive(false);
                    currentState = AlienState.roaming;
                    hungerTimer = 0;
                    DisgardClosestAlien();
                }

            }
        }
        // Handle Bullet interaction
        else if (other.gameObject.CompareTag("Bullet"))
        {
            other.gameObject.SetActive(false);
            alienHealth--;
            // Handle Alien Death
            if (alienHealth == 0)
            {
                // TODO: Add Coroutine & Ragdoll to show impact/force of bullets
                //EnableRagdoll();
                this.gameObject.SetActive(false);
            };
        }
        // Handle Player interaction && is also put in trigger / trigger state changes in HandleAging()
        else if (other.gameObject.CompareTag("Player"))
        {
            PlayerManager PM = other.gameObject.GetComponent<PlayerManager>();

            // Handle Gathering resource
            if (currentAge == AlienAge.resource)
            {
                PM.HandleGainResource(currentSpecies);
                this.gameObject.SetActive(false);
            }
            else
            {
                PM.HandleHit();
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
        lastClosestAlien = closestAlien;
        closestAlien = null;
        closestAlienHandler = null;
        currentState = AlienState.roaming;
        targetPosition = Vector3.one * 1000;
    }

    void ResetVariable()
    {
        lifeTime = UnityEngine.Random.Range(0, 10) * -1;
        lustTimer = 0;
        hungerTimer = 0;
        alienHealth = alienLifeResource;
        isFemale = UnityEngine.Random.Range(0, 2) == 1;
        currentAge = AlienAge.resource;
        HandleStateIcon(6); // 0: eye, 1: crosshair, 2: wind, 3: heart, 4: shield, 5: clock, 6: loader
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
}
