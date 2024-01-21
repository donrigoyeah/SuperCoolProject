using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.UI;


public class TutorialHandler : MonoBehaviour
{
    [Header("UI Tutorial")]
    public GameObject TutorialGameObject;
    public GameObject[] tutorialSlides;
    public int hideTut;
    public int currentTutorialSlide;
    public int totalTutorialSlides;
    public Button nextButton;

    [Header("Tutorial Scene")]
    public GameObject AlienPrefab;
    public GameObject currentAlien;
    public AlienHandler currentAlienHandler;
    public Transform currentAlienTransform;
    public float spawnDelay = 5;
    public Vector3 alienStartPosition;
    public Vector3 alienEndPosition;


    public static TutorialHandler Instance;

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

    private void OnEnable()
    {
        currentTutorialSlide = 0;
        totalTutorialSlides = tutorialSlides.Length;
    }

    public void EnableEntireTutorial()
    {
        // Folowing code only runs if playerPrefs exist, and they only do in builds
        if (PlayerPrefs.HasKey("hideTutorial"))
        {
            Debug.Log("has PlayerPrefs, workaround here with return");
            TutorialGameObject.SetActive(true);
            EventSystem.current.SetSelectedGameObject(nextButton.gameObject);
            Time.timeScale = 0;
            return;


            hideTut = PlayerPrefs.GetInt("hideTutorial");
            if (hideTut == 1 || GameManager.Instance.devMode)
            {
                TutorialGameObject.SetActive(false);
            }
            else
            {
                TutorialGameObject.SetActive(true);
                EventSystem.current.SetSelectedGameObject(nextButton.gameObject);
                Time.timeScale = 0;
            }
        }
        else
        {
            Debug.Log("has no PlayerPrefs");
            TutorialGameObject.SetActive(true);
            EventSystem.current.SetSelectedGameObject(nextButton.gameObject);
            Time.timeScale = 0;
        }
    }


    public void SetHideTut()
    {
        PlayerPrefs.SetInt("hideTutorial", 1);
        TutorialGameObject.SetActive(false);
        Time.timeScale = 1;

        Debug.Log("remove this here in future");
        // Start with Alien behaviour Scene
        ShowFoodCircleOrder();
    }

    public void SkipTut()
    {
        TutorialGameObject.SetActive(false);
        Time.timeScale = 1;

        Debug.Log("remove this here in future");
        // Start with Alien behaviour Scene
        ShowFoodCircleOrder();
    }

    public void NextSlide()
    {
        currentTutorialSlide++;

        if (currentTutorialSlide == totalTutorialSlides)
        {
            // Completed tutorial
            TutorialGameObject.SetActive(false);
            Time.timeScale = 1;

            // Start with Alien behaviour Scene
            ShowFoodCircleOrder();
        }
        else
        {
            foreach (var item in tutorialSlides)
            {
                item.SetActive(false);
            }
            tutorialSlides[currentTutorialSlide].SetActive(true);
        }
    }


    public void ShowFoodCircleOrder()
    {
        StartCoroutine(DoTheFoodCircle());
    }

    private void SpawnAdultAlien(int species, bool isAttacking, bool isLoving)
    {
        GameObject alienPoolGo = PoolManager.Instance.GetPooledAliens(false);
        if (alienPoolGo != null)
        {
            currentAlienHandler = alienPoolGo.GetComponent<AlienHandler>();
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

            currentAlienTransform = alienPoolGo.transform;
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
