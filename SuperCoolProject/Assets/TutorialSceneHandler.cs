using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialSceneHandler : MonoBehaviour
{
    public GameObject AlienPrefab;

    public GameObject currentAlien;
    public AlienHandler currentAlienHandler;
    public Transform currentAlienTransform;

    public float spawnDelay = 5;

    public Vector3 alienStartPosition;
    public Vector3 alienEndPosition;


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

    private void SpawnAdultAlien(int species, bool isAttacking, bool isLoving)
    {
        currentAlien = Instantiate(AlienPrefab, this.transform);
        currentAlienHandler = currentAlien.GetComponent<AlienHandler>();
        currentAlienHandler.BrainwashAlien();
        //currentAlienHandler.targetAlien = GameManager.Instance.players[0].gameObject;
        currentAlienHandler.currentSpecies = species;
        currentAlienHandler.currentAge = AlienHandler.AlienAge.sexualActive;
        currentAlienHandler.targetPosition = alienEndPosition * currentAlienHandler.sexualActiveScale;
        currentAlienHandler.lustTimer = 10;
        currentAlienHandler.hasUterus = false;
        currentAlienHandler.hungerTimer = 10;
        currentAlienHandler.ActivateCurrentModels(species);
        currentAlienHandler.lifeTime = 999; // Hackerman

        currentAlienTransform = currentAlien.transform;
        currentAlienTransform.position = alienStartPosition;

        if (isAttacking == true)
        {
            currentAlienHandler.currentState = AlienHandler.AlienState.hunting;
            currentAlienHandler.hungerTimer = 1000;
        }
        if (isLoving == true)
        {
            currentAlienHandler.currentState = AlienHandler.AlienState.loving;
            currentAlienHandler.hasUterus = true;
            currentAlienHandler.maxAmountOfBabies = 2;
        }
    }

    IEnumerator DoTheFoodCircle()
    {
        SpawnAdultAlien(0, true, false);
        yield return new WaitForSeconds(spawnDelay);
        SpawnAdultAlien(1, true, false);
        yield return new WaitForSeconds(spawnDelay);
        SpawnAdultAlien(2, true, false);
        yield return new WaitForSeconds(spawnDelay);
        SpawnAdultAlien(0, true, false);
        yield return new WaitForSeconds(spawnDelay);
        SpawnAdultAlien(0, false, true);
        yield return new WaitForSeconds(spawnDelay);
        GameManager.Instance.UnFreezeAllPlayers();

    }

}
