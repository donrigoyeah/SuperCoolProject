using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialSceneHandler : MonoBehaviour
{
    public GameObject AlienPrefab;

    public GameObject currentAlien;
    public AlienHandler currentAlienHandler;
    public Transform currentAlienTransform;

    public Vector3 alienStartPosition = new Vector3(-10, 0, 0);


    public static TutorialSceneHandler Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    public void ShowFoodCircleOrder()
    {
        StartCoroutine(DoTheFoodCircle());
    }

    private void SpawnAdultAlien(int species, bool attacking)
    {
        currentAlien = Instantiate(AlienPrefab, this.transform);
        currentAlienTransform = currentAlien.transform;
        currentAlienTransform.position = alienStartPosition;

        currentAlienHandler = currentAlien.GetComponent<AlienHandler>();
        currentAlienHandler.currentSpecies = species;
        currentAlienHandler.lifeTime = 100;

        if (attacking)
        {
            currentAlienHandler.currentState = AlienHandler.AlienState.hunting;
            currentAlienHandler.hungerTimer = 1000;
        }
        else
        {
            currentAlienHandler.currentState = AlienHandler.AlienState.roaming;
        }

        currentAlienHandler.targetPosition = Vector3.right;

        if (currentAlienTransform.position == Vector3.right)
        {
            StartCoroutine(currentAlienHandler.IdleSecsUntilNewState(30f, AlienHandler.AlienState.roaming));

        }
    }

    IEnumerator DoTheFoodCircle()
    {
        SpawnAdultAlien(0, false);
        yield return new WaitForSeconds(2);
        SpawnAdultAlien(1, true);
        yield return new WaitForSeconds(2);
        SpawnAdultAlien(2, true);
        yield return new WaitForSeconds(2);
        SpawnAdultAlien(0, true);

    }

}
