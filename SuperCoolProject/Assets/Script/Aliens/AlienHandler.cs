using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class AlienHandler : MonoBehaviour
{
    [Header("General")]
    public float alienSpeed = 5;
    public float lookRadius = 10;
    public GameObject[] alienSpecies; // 0:Sphere, 1:Square, 2:Triangle
    public bool isLooking;
    public bool wantsToMate;
    public bool chasingPrey;
    public bool evadingPredetor;

    [Header("This Alien")]
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
        DeactivateAllModels();
    }


    private void OnEnable()
    {
        DeactivateAllModels();
        lifeTime = 0;
        mateTimer = 0;
        isFemale = UnityEngine.Random.Range(0, 2) == 1;
        closestAlien = null;
        closestAlienHandler = null;
    }

    private void FixedUpdate()
    {
        lifeTime += Time.deltaTime;
        mateTimer += Time.deltaTime;
        float step = alienSpeed * Time.deltaTime; // calculate distance to move

        if (closestAlien != null || closestAlienHandler != null)
        {
            isLooking = false;
            targetPosition = closestAlien.transform.position;
            if (closestAlienIndex == currentSpecies && lifeTime > 20 && mateTimer > 10 && isFemale != closestAlienHandler.isFemale)
            {
                // Handle balz
                HandleLoveApproach(step);
            }
            else if (closestAlienIndex > currentSpecies || (currentSpecies == 3 && closestAlienIndex == 0)) // 0:Sphere, 1:Square, 2:Triangle
            {
                // Handle running away
                HandleFleeing(step);
            }
            else if (closestAlienIndex < currentSpecies || (currentSpecies == 0 && closestAlienIndex == 3)) // 0:Sphere, 1:Square, 2:Triangle
            {
                // Handle attacking
                HandleAttacking(step);
            }
        }
        else
        {
            isLooking = true;

            Idle(step);
            FindClosestAlien();
        }
        KeepInBoundaries();
    }
    private void KeepInBoundaries()
    {
        if (transform.position.x > GameManager.SharedInstance.worldBoundaryX) { transform.position = new Vector3(GameManager.SharedInstance.worldBoundaryX, transform.position.y, transform.position.z); }
        if (transform.position.x < GameManager.SharedInstance.worldBoundaryMinusX) { transform.position = new Vector3(GameManager.SharedInstance.worldBoundaryMinusX, transform.position.y, transform.position.z); }
        if (transform.position.z > GameManager.SharedInstance.worldBoundaryZ) { transform.position = new Vector3(transform.position.x, transform.position.y, GameManager.SharedInstance.worldBoundaryZ); }
        if (transform.position.z < GameManager.SharedInstance.worldBoundaryMinusZ) { transform.position = new Vector3(transform.position.x, transform.position.y, GameManager.SharedInstance.worldBoundaryMinusZ); }
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

    private void HandleAttacking(float step)
    {
        chasingPrey = true;
        transform.position = Vector3.MoveTowards(transform.position, closestAlien.transform.position, step);
        Debug.DrawLine(transform.position, closestAlien.transform.position, Color.red);
    }

    private void HandleFleeing(float step)
    {
        evadingPredetor = true;
        Vector3 fleeDir = closestAlien.transform.position - transform.position;
        transform.position = Vector3.MoveTowards(transform.position, fleeDir, step);
        Debug.DrawLine(transform.position, fleeDir, Color.blue);
    }

    private void HandleLoveApproach(float step)
    {
        wantsToMate = true;
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

                alienPoolGo.transform.position = new Vector3(transform.position.x, 0.5f, transform.position.z) + Vector3.forward;
            }
        }
        lastClosestAlien = closestAlien;
        wantsToMate = false;
        closestAlien = null;
        closestAlienHandler = null;
        mateTimer = 0;
    }

    public GameObject FindClosestAlien()
    {
        int layerMask = 1 << 9; // Lyer 9 is Enemy
        Collider[] aliensInRange;
        aliensInRange = Physics.OverlapSphere(this.transform.position, lookRadius, layerMask);

        for (int i = 0; i < aliensInRange.Length; i++)  //list of gameObjects to search through
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
            evadingPredetor = false;
            closestAlien = null;
        }
        else if (otherAlien.currentSpecies < currentSpecies || (otherAlien.currentSpecies == 0 && currentSpecies == 3)) // 0:Sphere, 1:Square, 2:Triangle
        {
            // You eat
            otherAlien.gameObject.SetActive(false);
            chasingPrey = false;
            closestAlien = null;
        }
    }

    private void DeactivateAllModels()
    {
        foreach (var item in alienSpecies)
        {
            item.SetActive(false);
        }
    }
}
