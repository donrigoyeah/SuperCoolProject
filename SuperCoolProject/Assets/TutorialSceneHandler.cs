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

    private void SpawnAdultAlien(int species, bool isAttacking, bool isIdleing, bool isLoving)
    {
        currentAlien = Instantiate(AlienPrefab, this.transform);
        currentAlienHandler = currentAlien.GetComponent<AlienHandler>();
        currentAlienHandler.BrainwashAlien();
        currentAlienHandler.targetAlien = GameManager.Instance.players[0].gameObject;
        currentAlienHandler.currentSpecies = species;
        currentAlienHandler.currentAge = AlienHandler.AlienAge.sexualActive;
        Debug.Log("SpawnAdultAlien, targetPosition : " + alienEndPosition);
        Debug.Log("SpawnAdultAlien, targetPosition * scale: " + alienEndPosition * currentAlienHandler.sexualActiveScale);
        currentAlienHandler.targetPosition = alienEndPosition * currentAlienHandler.sexualActiveScale;
        currentAlienHandler.lustTimer = 10;
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
        if (isIdleing == true)
        {
            currentAlienHandler.currentState = AlienHandler.AlienState.roaming;
        }
        if (isLoving == true)
        {
            currentAlienHandler.currentState = AlienHandler.AlienState.loving;
        }

        //currentAlienHandler.targetPosition = Vector3.right;

        if (currentAlienTransform.position == Vector3.right)
        {
            StartCoroutine(currentAlienHandler.IdleSecsUntilNewState(30f, AlienHandler.AlienState.roaming));

        }
    }

    IEnumerator DoTheFoodCircle()
    {
        SpawnAdultAlien(0, false, true, false);
        yield return new WaitForSeconds(spawnDelay);
        SpawnAdultAlien(1, true, false, false);
        yield return new WaitForSeconds(spawnDelay);
        SpawnAdultAlien(2, true, false, false);
        yield return new WaitForSeconds(spawnDelay);
        SpawnAdultAlien(0, true, false, false);
        yield return new WaitForSeconds(spawnDelay);
        SpawnAdultAlien(0, true, false, true);
        yield return new WaitForSeconds(spawnDelay);
        GameManager.Instance.UnFreezeAllPlayers();

    }

}
