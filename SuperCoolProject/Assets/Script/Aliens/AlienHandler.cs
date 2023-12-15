using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AlienHandler : MonoBehaviour
{
    // Use these as states instead of boolean
    enum AlienState
    {
        roaming,
        hunting,
        evading,
        loving
    }

    // Use this for interaction
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
    public GameObject[] alienSpecies; // 0:Sphere, 1:Square, 2:Triangle
    private float delta;
    private float step;
    private int alienLifeResource = 1;
    private int alienLifeChild = 1;
    private int alienLifeSexual = 3;
    private int alienLifeFullGrown = 5;
    private float alertDistanceThreshold = 2;


    [Header("This Alien")]
    AlienState currentState;
    AlienAge currentAge;
    Rigidbody rb;
    public RawImage currentStateIcon;
    public Texture[] allStateIcons; // 0: eye, 1: crosshair, 2: wind, 3: heart, 4: shield
    public int alienHealth;
    public float lifeTime = 0;
    public float mateTimer = 0;
    public bool isFemale;
    public int currentSpecies;
    Vector3 targetPosition = Vector3.one * 1000;

    [Header("Target Alien")]
    public GameObject closestAlien = null;
    public GameObject lastClosestAlien = null;
    AlienHandler closestAlienHandler = null;
    int closestAlienIndex;

    private void Awake()
    {
        lifeTime = UnityEngine.Random.Range(0, 10);
        mateTimer = 0;
        alienHealth = alienLifeResource;
        closestAlien = null;
        closestAlienHandler = null;
        isFemale = UnityEngine.Random.Range(0, 2) == 1;
        currentState = AlienState.roaming;
        currentAge = AlienAge.resource;
        currentStateIcon.texture = allStateIcons[5]; // 0: eye, 1: crosshair, 2: wind, 3: heart, 4: shield, 5: clock, 6: loader
        rb = this.GetComponent<Rigidbody>();
        ActivateCurrentModels(currentSpecies);
    }

    private void OnEnable()
    {
        lifeTime = 0;
        mateTimer = 0;
        alienHealth = alienLifeResource;
        closestAlien = null;
        closestAlienHandler = null;
        isFemale = UnityEngine.Random.Range(0, 2) == 1;
        currentState = AlienState.roaming;
        currentAge = AlienAge.resource;

        HandleStateIcon(5); // 0: eye, 1: crosshair, 2: wind, 3: heart, 4: shield, 5: clock, 6: loader
        ActivateCurrentModels(currentSpecies);
        rb = this.GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        delta = Time.deltaTime;
        lifeTime += delta;
        mateTimer += delta;
        tickTimer += delta; // Used for update not every fixedUpdate but lesser
        step = alienSpeed * delta;

        KeepInBoundaries();
        HandleMovement(step);

        // Only Render on Tick condition
        while (tickTimer >= tickTimerMax)
        {
            HandleAging(lifeTime);

            if (currentState == AlienState.roaming && currentAge != AlienAge.resource)
            {
                HandleLooking();
                if (closestAlien != null)
                {
                    HandleRoaming();
                }
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
    }

    private void HandleStateIcon(int index)
    {
        currentStateIcon.texture = allStateIcons[index]; // 0: eye, 1: crosshair, 2: wind, 3: heart, 4: shield, 5: clock, 6: loader
    }

    private void HandleMovement(float step)
    {
        if (currentAge != AlienAge.resource)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, step);
        }
    }


    private void HandleAttacking(GameObject targetAlien)
    {
        HandleStateIcon(1); // 0: eye, 1: crosshair, 2: wind, 3: heart, 4: shield, 5: clock, 6: loader
        targetPosition = targetAlien.transform.position; // Update targetPosition only every tick update
        Debug.DrawLine(transform.position, targetPosition);
    }

    private void HandleFleeing(GameObject targetAlien)
    {
        HandleStateIcon(2); // 0: eye, 1: crosshair, 2: wind, 3: heart, 4: shield, 5: clock, 6: loader
        targetPosition = targetAlien.transform.position - transform.position;
        Debug.DrawLine(transform.position, targetPosition, Color.blue);
    }

    private void HandleLoveApproach(GameObject targetAlien)
    {
        HandleStateIcon(3); // 0: eye, 1: crosshair, 2: wind, 3: heart, 4: shield, 5: clock, 6: loader
        targetPosition = targetAlien.transform.position; // Update targetPosition only every tick update
        Debug.DrawLine(transform.position, targetPosition, Color.green);
    }

    private void HandleMating()
    {
        if (isFemale)
        {
            GameObject alienPoolGo = PoolManager.SharedInstance.GetPooledAliens();
            if (alienPoolGo != null)
            {
                Debug.Log("Baby spawned at: " + this.transform.position);
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
        lastClosestAlien = closestAlien;
        closestAlien = null;
        closestAlienHandler = null;
        targetPosition = Vector3.one * 1000;
        currentState = AlienState.roaming;
        mateTimer = 0;
    }

    public void HandleLooking()
    {
        HandleStateIcon(0); // 0: eye, 1: crosshair, 2: wind, 3: heart, 4: shield, 5: clock, 6: loader

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
            float dist = Vector3.Distance(aliensInRange[i].transform.position, transform.position);
            if (dist < closestDistance) { closestDistance = dist; }

            closestAlien = aliensInRange[i].gameObject;
            targetPosition = closestAlien.transform.position;
            closestAlienHandler = closestAlien.GetComponent<AlienHandler>();
            closestAlienIndex = closestAlienHandler.currentSpecies;

            if (dist < alertDistanceThreshold) { break; }
        }

        if (closestAlien != null && closestAlienHandler != null)
        {
            // Check to which state the alien switches
            if (closestAlienIndex == currentSpecies && lifeTime > 20 && mateTimer > 10 && isFemale != closestAlienHandler.isFemale)
            {
                currentState = AlienState.loving;
            }
            else if (currentSpecies == closestAlienIndex - 1 || (currentSpecies == 2 && closestAlienIndex == 0)) // 0:Sphere, 1:Square, 2:Triangle
            {
                currentState = AlienState.evading;
            }
            else if (currentSpecies == closestAlienIndex + 1 || (currentSpecies == 0 && closestAlienIndex == 2)) // 0:Sphere, 1:Square, 2:Triangle
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
            currentState = AlienState.roaming;
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

    private void HandleRoaming()
    {
        HandleStateIcon(0); // 0: eye, 1: crosshair, 2: wind, 3: heart, 4: shield, 5: clock, 6: loader
        if (targetPosition == Vector3.one * 1000 || transform.position == targetPosition)
        {
            float randDirX = UnityEngine.Random.Range(0, 2) - .5f;
            float randDirY = UnityEngine.Random.Range(0, 2) - .5f;
            targetPosition = transform.position + new Vector3(randDirX, 0, randDirY) * 10;

            if (targetPosition.x > GameManager.SharedInstance.worldBoundaryX ||
                targetPosition.x < GameManager.SharedInstance.worldBoundaryMinusX ||
                targetPosition.z < GameManager.SharedInstance.worldBoundaryMinusZ ||
                targetPosition.z > GameManager.SharedInstance.worldBoundaryZ)
            {
                targetPosition = Vector3.one * 1000;
            }
        }
    }

    public void HandleAging(float lifeTime)
    {
        if (lifeTime < 10)
        {
            DisableRagdoll();
            currentAge = AlienAge.resource;
            alienHealth = alienLifeResource;
            transform.localScale = Vector3.one * 0.2f;
        }
        else if (lifeTime > 10)
        {
            EnableRagdoll();
            currentAge = AlienAge.child;
            alienHealth = alienLifeChild;
            transform.localScale = Vector3.one * .5f;
        }
        else if (lifeTime > 15)
        {
            currentAge = AlienAge.sexualActive;
            alienHealth = alienLifeSexual;
            transform.localScale = Vector3.one;
        }
        else if (lifeTime > 25)
        {
            currentAge = AlienAge.fullyGrown;
            alienHealth = alienLifeFullGrown;
            transform.localScale = Vector3.one * 1.2f;
        }
    }

    private void KeepInBoundaries()
    {
        //Keep alien on the floor
        if (transform.position.y > .2f) { transform.position = new Vector3(transform.position.x, 0.1f, transform.position.z); }

        //Keep alien within the game board
        if (transform.position.x > GameManager.SharedInstance.worldBoundaryX) { transform.position = new Vector3(GameManager.SharedInstance.worldBoundaryX, transform.position.y, transform.position.z); }
        if (transform.position.x < GameManager.SharedInstance.worldBoundaryMinusX) { transform.position = new Vector3(GameManager.SharedInstance.worldBoundaryMinusX, transform.position.y, transform.position.z); }
        if (transform.position.z > GameManager.SharedInstance.worldBoundaryZ) { transform.position = new Vector3(transform.position.x, transform.position.y, GameManager.SharedInstance.worldBoundaryZ); }
        if (transform.position.z < GameManager.SharedInstance.worldBoundaryMinusZ) { transform.position = new Vector3(transform.position.x, transform.position.y, GameManager.SharedInstance.worldBoundaryMinusZ); }
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Alien"))
        {
            AlienHandler otherAlien = collision.gameObject.GetComponent<AlienHandler>();
            if (otherAlien.currentSpecies == currentSpecies && lifeTime > 20 && mateTimer > 10)
            {
                // Spawn new Species
                HandleMating();
            }
            else if (otherAlien.currentSpecies > currentSpecies || (otherAlien.currentSpecies == 3 && currentSpecies == 0)) // 0:Sphere, 1:Square, 2:Triangle
            {
                // Got eaten
                this.gameObject.SetActive(false);
            }
            else if (otherAlien.currentSpecies < currentSpecies || (otherAlien.currentSpecies == 0 && currentSpecies == 3)) // 0:Sphere, 1:Square, 2:Triangle
            {
                // You eat
                otherAlien.gameObject.SetActive(false);
                currentStateIcon.texture = allStateIcons[0]; // 0: eye, 1: crosshair, 2: wind, 3: heart, 4: shield, 5: clock, 6: loader
                currentState = AlienState.roaming;
                closestAlien = null;
            }
        }
        else if (collision.gameObject.CompareTag("Bullet"))
        {
            collision.gameObject.SetActive(false);
            alienHealth--;
            // Handle Alien Death
            if (alienHealth == 0)
            {
                this.gameObject.SetActive(false);
            };
        }
        else if (collision.gameObject.CompareTag("Player"))
        {
            // Handle Gathering resource
            if (currentAge == AlienAge.resource)
            {
                PlayerManager PM = collision.gameObject.GetComponent<PlayerManager>();
                PM.HandleGainResource(currentSpecies);
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

    void DisableRagdoll()
    {
        rb.isKinematic = true;
        rb.detectCollisions = false;
    }
    void EnableRagdoll()
    {
        rb.isKinematic = false;
        rb.detectCollisions = true;
    }

}
