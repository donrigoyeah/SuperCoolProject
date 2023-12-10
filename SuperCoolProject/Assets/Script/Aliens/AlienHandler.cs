using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

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

    [Header("General")]
    public float alienSpeed = 5;
    public float lookRadius = 10;
    public GameObject[] alienSpecies; // 0:Sphere, 1:Square, 2:Triangle
    private float delta;
    private float step;

    [Header("This Alien")]
    AlienState currentState;
    AlienAge currentAge;
    public float lifeTime = 0;
    public float mateTimer = 0;
    public bool isFemale;
    public int currentSpecies;
    Vector3 targetSpot = Vector3.one * 1000;

    [Header("Target Alien")]
    public GameObject closestAlien = null;
    public GameObject lastClosestAlien = null;
    AlienHandler closestAlienHandler = null;
    int closestAlienIndex;
    public Vector3 targetPosition;

    private void Awake()
    {
        lifeTime = 0;
        mateTimer = 0;
        closestAlien = null;
        closestAlienHandler = null;
        isFemale = UnityEngine.Random.Range(0, 2) == 1;
        currentState = AlienState.roaming;
        currentAge = AlienAge.sexualActive;
    }

    private void OnEnable()
    {
        lifeTime = 0;
        mateTimer = 0;
        closestAlien = null;
        closestAlienHandler = null;
        isFemale = UnityEngine.Random.Range(0, 2) == 1;
        currentState = AlienState.roaming;
        currentAge = AlienAge.resource;
    }

    private void FixedUpdate()
    {
        delta = Time.deltaTime;
        lifeTime += delta;
        mateTimer += delta;
        step = alienSpeed * delta;

        HandleAging(lifeTime);
        KeepInBoundaries();

        if (currentState == AlienState.roaming)
        {
            Idle(step);
            FindClosestAlien();
        }
        else if (closestAlien != null)
        {
            if (currentState == AlienState.hunting)
            {
                HandleAttacking(step);
            }
            else if (currentState == AlienState.evading)
            {
                HandleFleeing(step);
            }
            else if (currentState == AlienState.loving)
            {
                HandleLoveApproach(step);
            }
        }
    }

    void Idle(float step)
    {
        if (targetSpot == Vector3.one * 1000 || transform.position == targetSpot)
        {
            float randDirX = UnityEngine.Random.Range(0, 2) - .5f;
            float randDirY = UnityEngine.Random.Range(0, 2) - .5f;
            targetSpot = transform.position + new Vector3(randDirX, 0, randDirY) * 5;

            if (targetSpot.x > GameManager.SharedInstance.worldBoundaryX ||
                targetSpot.x < GameManager.SharedInstance.worldBoundaryMinusX ||
                targetSpot.z < GameManager.SharedInstance.worldBoundaryMinusZ ||
                targetSpot.z > GameManager.SharedInstance.worldBoundaryZ)
            {
                targetSpot = Vector3.one * 1000;
            }
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, targetSpot, step);
        }
    }

    private void HandleAging(float lifeTime)
    {
        if (lifeTime > 5)
        {
            currentAge = AlienAge.child;
        }
        else if (lifeTime > 10)
        {
            currentAge = AlienAge.sexualActive;

        }
        else if (lifeTime > 40)
        {
            currentAge = AlienAge.fullyGrown;

        }
    }

    private void KeepInBoundaries()
    {
        if (transform.position.x > GameManager.SharedInstance.worldBoundaryX) { transform.position = new Vector3(GameManager.SharedInstance.worldBoundaryX, transform.position.y, transform.position.z); }
        if (transform.position.x < GameManager.SharedInstance.worldBoundaryMinusX) { transform.position = new Vector3(GameManager.SharedInstance.worldBoundaryMinusX, transform.position.y, transform.position.z); }
        if (transform.position.z > GameManager.SharedInstance.worldBoundaryZ) { transform.position = new Vector3(transform.position.x, transform.position.y, GameManager.SharedInstance.worldBoundaryZ); }
        if (transform.position.z < GameManager.SharedInstance.worldBoundaryMinusZ) { transform.position = new Vector3(transform.position.x, transform.position.y, GameManager.SharedInstance.worldBoundaryMinusZ); }
    }

    private void HandleAttacking(float step)
    {
        transform.position = Vector3.MoveTowards(transform.position, closestAlien.transform.position, step);
        Debug.DrawLine(transform.position, closestAlien.transform.position, Color.red);
    }

    private void HandleFleeing(float step)
    {
        Vector3 fleeDir = closestAlien.transform.position - transform.position;
        transform.position = Vector3.MoveTowards(transform.position, fleeDir, step);
        Debug.DrawLine(transform.position, fleeDir, Color.blue);
    }

    private void HandleLoveApproach(float step)
    {
        transform.position = Vector3.MoveTowards(transform.position, closestAlien.transform.position, step);
        Debug.DrawLine(transform.position, closestAlien.transform.position, Color.green);

    }

    private void HandleMating()
    {
        if (isFemale)
        {
            GameObject alienPoolGo = PoolManager.SharedInstance.GetPooledAliens();
            if (alienPoolGo != null)
            {
                AlienHandler newBornAlien;
                alienPoolGo.SetActive(true);
                newBornAlien = alienPoolGo.GetComponent<AlienHandler>();
                newBornAlien.alienSpecies[currentSpecies].SetActive(true);
                newBornAlien.currentSpecies = currentSpecies;
                isFemale = UnityEngine.Random.Range(0, 2) == 1;

                // TODO: Spawn them somewhere near, in the middle (?!)
                alienPoolGo.transform.position = new Vector3(transform.position.x, 0.5f, transform.position.z) + Vector3.forward;
            }
        }
        lastClosestAlien = closestAlien;
        closestAlien = null;
        closestAlienHandler = null;
        mateTimer = 0;
    }

    public GameObject FindClosestAlien()
    {
        int layerMask = 1 << 9; // Lyer 9 is Enemy
        Collider[] aliensInRange;
        aliensInRange = Physics.OverlapSphere(this.transform.position, lookRadius, layerMask);

        // TODO: Make a better closest alien selection
        // Maybe if aggressor is near evade rather then love making (?!)
        for (int i = 0; i < aliensInRange.Length; i++)
        {
            if (aliensInRange[i] == lastClosestAlien) continue;

            float dist = Vector3.Distance(aliensInRange[i].transform.position, transform.position);
            if (dist > .1f && dist < lookRadius)
            {
                closestAlien = aliensInRange[i].gameObject;
                closestAlienIndex = closestAlien.GetComponent<AlienHandler>().currentSpecies;
                break;
            }
        }

        if (closestAlien != null)
        {

            // Check to which state the alien switches
            if (closestAlienIndex == currentSpecies && lifeTime > 20 && mateTimer > 10 && isFemale != closestAlienHandler.isFemale)
            {
                // Handle loving
                currentState = AlienState.loving;
            }
            else if (closestAlienIndex > currentSpecies || (currentSpecies == 3 && closestAlienIndex == 0)) // 0:Sphere, 1:Square, 2:Triangle
            {
                // Handle evading
                currentState = AlienState.evading;

            }
            else if (closestAlienIndex < currentSpecies || (currentSpecies == 0 && closestAlienIndex == 3)) // 0:Sphere, 1:Square, 2:Triangle
            {
                // Handle attacking
                currentState = AlienState.hunting;
            }

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
                closestAlien = null;
            }
            else if (otherAlien.currentSpecies < currentSpecies || (otherAlien.currentSpecies == 0 && currentSpecies == 3)) // 0:Sphere, 1:Square, 2:Triangle
            {
                // You eat
                otherAlien.gameObject.SetActive(false);
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

    public void ActivateCurrentModels(int currentSpeyiesIndex)
    {
        foreach (var item in alienSpecies)
        {
            item.SetActive(false);
        }
        alienSpecies[currentSpeyiesIndex].SetActive(true);
    }



}
