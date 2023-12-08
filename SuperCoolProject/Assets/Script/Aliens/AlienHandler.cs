using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class AlienHandler : MonoBehaviour
{
    public float alienSpeed = 5;
    public float lookRadius = 10;
    public float lifeTime = 0;

    public GameObject[] alienSpecies;
    public int currentSpecies;

    public GameObject closestObject = null;
    int closestObjectIndex;

    private void Awake()
    {
        foreach (var item in alienSpecies)
        {
            item.SetActive(false);
        }
    }


    private void OnEnable()
    {
        lifeTime = 0;
        closestObject = null;
    }

    private void FixedUpdate()
    {
        lifeTime += Time.deltaTime;
        if (closestObject == null)
        {
            Idle();
            FindClosestAlien();
        }
        else
        {
            float step = alienSpeed * Time.deltaTime; // calculate distance to move

            if (closestObjectIndex == currentSpecies || lifeTime > 2)
            {
                Debug.Log("Lets Mate");
                // Handle mating
            }
            else if (closestObjectIndex > currentSpecies || (currentSpecies == 3 && closestObjectIndex == 0))
            {
                Debug.Log("Ohh Shit");
                // Handle running away
                HandleFleeing(step);
            }
            else if (closestObjectIndex < currentSpecies || (currentSpecies == 0 && closestObjectIndex == 3))
            {
                Debug.Log("i want to eat you");
                // Handle attacking
                HandleAttacking(step);
            }
        }
    }
    private void LateUpdate()
    {
        if (transform.position.x > GameManager.SharedInstance.worldBoundaryX) { transform.position = new Vector3(GameManager.SharedInstance.worldBoundaryX, transform.position.y, transform.position.z); }
        if (transform.position.x < GameManager.SharedInstance.worldBoundaryMinusX) { transform.position = new Vector3(GameManager.SharedInstance.worldBoundaryMinusX, transform.position.y, transform.position.z); }
        if (transform.position.z > GameManager.SharedInstance.worldBoundaryZ) { transform.position = new Vector3(transform.position.x, transform.position.y, GameManager.SharedInstance.worldBoundaryZ); }
        if (transform.position.z < GameManager.SharedInstance.worldBoundaryMinusZ) { transform.position = new Vector3(transform.position.x, transform.position.y, GameManager.SharedInstance.worldBoundaryMinusZ); }
    }

    void Idle()
    {
        float randDirX = Random.Range(0, 2) - .5f;
        float randDirY = Random.Range(0, 2) - .5f;
        transform.position += new Vector3(randDirX, 0, randDirY) * alienSpeed;
    }

    private void HandleAttacking(float step)
    {
        transform.position = Vector3.MoveTowards(transform.position, closestObject.transform.position, step);
    }

    private void HandleFleeing(float step)
    {
        Vector3 fleeDir = transform.position - closestObject.transform.position;
        transform.position = Vector3.MoveTowards(transform.position, fleeDir, step);
    }


    public GameObject FindClosestAlien()
    {
        for (int i = 0; i < PoolManager.SharedInstance.AlienPool.Count; i++)  //list of gameObjects to search through
        {
            if (PoolManager.SharedInstance.AlienPool[i] == this.gameObject) continue;

            float dist = Vector3.Distance(PoolManager.SharedInstance.AlienPool[i].transform.position, transform.position);
            if (dist < lookRadius)
            {
                closestObject = PoolManager.SharedInstance.AlienPool[i];
                closestObjectIndex = closestObject.GetComponent<AlienHandler>().currentSpecies;
            }
        }
        return closestObject;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<AlienHandler>().currentSpecies == currentSpecies)
        {
            // Spawn new Species
            Debug.Log("Mating");
            GameObject alienPoolGo = PoolManager.SharedInstance.GetPooledAliens();
            if (alienPoolGo != null)
            {
                alienPoolGo.SetActive(true);
                alienPoolGo.GetComponent<AlienHandler>().alienSpecies[currentSpecies].SetActive(true);
                alienPoolGo.GetComponent<AlienHandler>().currentSpecies = currentSpecies;
                alienPoolGo.transform.position = this.transform.position;
            }
            else if (collision.gameObject.GetComponent<AlienHandler>().currentSpecies > currentSpecies || (collision.gameObject.GetComponent<AlienHandler>().currentSpecies == 3 && currentSpecies == 0))
            {
                // Got eaten
                Debug.Log("Got eaten");

                this.gameObject.SetActive(false);
            }
            else if (collision.gameObject.GetComponent<AlienHandler>().currentSpecies < currentSpecies || (collision.gameObject.GetComponent<AlienHandler>().currentSpecies == 0 && currentSpecies == 3))
            {
                // You eat
                Debug.Log("Eat");

                closestObject = null;
            }
        }
    }

}
