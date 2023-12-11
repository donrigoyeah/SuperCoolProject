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
    private float alienLifeResource = 1;
    private float alienLifeChild = 10;
    private float alienLifeSexual = 30;
    private float alienLifeFullGrown = 50;


    [Header("This Alien")]
    AlienState currentState;
    AlienAge currentAge;
    Rigidbody rb;
    public RawImage currentStateIcon;
    public Texture[] allStateIcons; // 0: eye, 1: crosshair, 2: wind, 3: heart, 4: shield
    public float alienHealth;
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
        lifeTime = 0;
        mateTimer = 0;
        alienHealth = alienLifeResource;
        closestAlien = null;
        closestAlienHandler = null;
        isFemale = UnityEngine.Random.Range(0, 2) == 1;
        currentState = AlienState.roaming;
        currentAge = AlienAge.resource;
        HandleAging(0);
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
        HandleAging(0);
        rb = this.GetComponent<Rigidbody>();
        ActivateCurrentModels(currentSpecies);
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

            if (currentState == AlienState.roaming)
            {
                FindClosestAlien();
                HandleLookForPlaceToIdle(step);
            }
            else if (closestAlien != null)
            {
                if (currentState == AlienState.hunting)
                {
                    HandleAttacking(closestAlien, step);
                }
                else if (currentState == AlienState.evading)
                {
                    HandleFleeing(closestAlien, step);
                }
                else if (currentState == AlienState.loving)
                {
                    HandleLoveApproach(closestAlien, step);
                }
            }
            tickTimer -= tickTimerMax;
        }
    }

    private void HandleMovement(float step)
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, step);
    }

    private void HandleLookForPlaceToIdle(float step)
    {
        if (targetPosition == Vector3.one * 1000 || transform.position == targetPosition)
        {
            float randDirX = UnityEngine.Random.Range(0, 2) - .5f;
            float randDirY = UnityEngine.Random.Range(0, 2) - .5f;
            targetPosition = transform.position + new Vector3(randDirX, 0, randDirY) * 5;

            if (targetPosition.x > GameManager.SharedInstance.worldBoundaryX ||
                targetPosition.x < GameManager.SharedInstance.worldBoundaryMinusX ||
                targetPosition.z < GameManager.SharedInstance.worldBoundaryMinusZ ||
                targetPosition.z > GameManager.SharedInstance.worldBoundaryZ)
            {
                targetPosition = Vector3.one * 1000;
            }
        }
    }

    private void HandleAttacking(GameObject targetAlien, float step)
    {
        targetPosition = targetAlien.transform.position; // Update targetPosition only every tick update
        Debug.DrawLine(transform.position, targetPosition, Color.red);
    }

    private void HandleFleeing(GameObject targetAlien, float step)
    {
        targetPosition = targetAlien.transform.position - transform.position;
        Debug.DrawLine(transform.position, targetPosition, Color.blue);
    }

    private void HandleLoveApproach(GameObject targetAlien, float step)
    {
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
                alienPoolGo.SetActive(true);

                // TODO: Spawn them somewhere near, in the middle (?!)
                alienPoolGo.transform.position = new Vector3(transform.position.x, 0.5f, transform.position.z) + Vector3.forward;
            }
        }
        lastClosestAlien = closestAlien;
        closestAlien = null;
        closestAlienHandler = null;
        mateTimer = 0;
        currentStateIcon.texture = allStateIcons[0]; // 0: eye, 1: crosshair, 2: wind, 3: heart, 4: shield
        currentState = AlienState.roaming;
    }

    public GameObject FindClosestAlien()
    {
        int layerMask = 1 << 9; // Lyer 9 is Alien
        Collider[] aliensInRange;
        aliensInRange = Physics.OverlapSphere(this.transform.position, lookRadius, layerMask);

        // TODO: Make a better closest alien selection
        // Maybe if aggressor is near evade rather then love making (?!)
        // JUst to find closest alien
        for (int i = 0; i < aliensInRange.Length; i++)
        {
            if (aliensInRange[i] == lastClosestAlien) continue;

            float dist = Vector3.Distance(aliensInRange[i].transform.position, transform.position);
            if (dist < lookRadius)
            {
                closestAlien = aliensInRange[i].gameObject;
                targetPosition = closestAlien.transform.position;
                closestAlienHandler = closestAlien.GetComponent<AlienHandler>();
                closestAlienIndex = closestAlienHandler.currentSpecies;
                break;
            }
        }

        if (closestAlien != null && closestAlienHandler != null)
        {
            // Check to which state the alien switches
            if (closestAlienIndex == currentSpecies && lifeTime > 20 && mateTimer > 10 && isFemale != closestAlienHandler.isFemale)
            {
                // Handle loving
                currentStateIcon.texture = allStateIcons[3]; // 0: eye, 1: crosshair, 2: wind, 3: heart, 4: shield
                currentState = AlienState.loving;
            }
            else if (closestAlienIndex > currentSpecies || (currentSpecies == 2 && closestAlienIndex == 0)) // 0:Sphere, 1:Square, 2:Triangle
            {
                // Handle evading
                currentStateIcon.texture = allStateIcons[2]; // 0: eye, 1: crosshair, 2: wind, 3: heart, 4: shield
                currentState = AlienState.evading;

            }
            else if (closestAlienIndex < currentSpecies || (currentSpecies == 0 && closestAlienIndex == 2)) // 0:Sphere, 1:Square, 2:Triangle
            {
                // Handle attacking
                currentStateIcon.texture = allStateIcons[1]; // 0: eye, 1: crosshair, 2: wind, 3: heart, 4: shield
                currentState = AlienState.hunting;
            }
        }
        else
        {
            currentStateIcon.texture = allStateIcons[0]; // 0: eye, 1: crosshair, 2: wind, 3: heart, 4: shield
            currentState = AlienState.roaming;
        }
        return closestAlien;

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

    private void HandleAging(float lifeTime)
    {
        if (lifeTime < 5)
        {
            currentAge = AlienAge.resource;
            alienHealth = alienLifeResource;
            transform.localScale = Vector3.one * 0.2f;
        }
        else if (lifeTime > 5)
        {
            currentAge = AlienAge.child;
            alienHealth = alienLifeChild;
            transform.localScale = Vector3.one * .4f;
        }
        else if (lifeTime > 10)
        {
            currentAge = AlienAge.sexualActive;
            alienHealth = alienLifeSexual;
            transform.localScale = Vector3.one * .7f;
        }
        else if (lifeTime > 40)
        {
            currentAge = AlienAge.fullyGrown;
            alienHealth = alienLifeFullGrown;
            transform.localScale = Vector3.one;
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
                currentStateIcon.texture = allStateIcons[0]; // 0: eye, 1: crosshair, 2: wind, 3: heart, 4: shield
                currentState = AlienState.roaming;
                closestAlien = null;
            }
        }
        else if (collision.gameObject.CompareTag("Bullet"))
        {
            // TODO: handle hit by bullet
        }
        else if (collision.gameObject.CompareTag("Player"))
        {
            // TODO: Handle player collision
            // here also farming resource if state is child
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
