using System.Collections;
using System.Collections.Generic;
using System.Runtime.Versioning;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.UI;


public class TutorialHandler : MonoBehaviour
{
    [Header("UI Tutorial")]
    public GameObject TutorialGameObject;
    public GameObject[] tutorialSlides;
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
    public Vector3 cameraPositionForTut;

    public AlienHandler LoveAlien1;
    public AlienHandler LoveAlien2;


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

        TutorialGameObject.SetActive(false);
        currentTutorialSlide = 0;
        totalTutorialSlides = tutorialSlides.Length;
    }

    public void EnableEntireTutorial()
    {
        TutorialGameObject.SetActive(true);
        EventSystem.current.SetSelectedGameObject(nextButton.gameObject);
        Time.timeScale = 0;
    }

    public void SetHideTut()
    {
        PlayerPrefs.SetInt("hideTutorial", 1);
        TutorialGameObject.SetActive(false);
        Time.timeScale = 1;
        GameManager.Instance.UnFreezeAllPlayers();
    }

    public void NextSlide()
    {
        currentTutorialSlide++;
        if (currentTutorialSlide > totalTutorialSlides) // After last ok press
        {
            GameManager.Instance.UnFreezeAllPlayers();
            return;
        }
        else
        {
            if (currentTutorialSlide == 2) // After second slide
            {
                ShowFoodCircleOrder();
            }
            else if (currentTutorialSlide == 3)
            {
                ShowReproduction();
            }
            else if (currentTutorialSlide == 4)
            {
                ShowDefense();
            }
            else
            {
                EnableCertainSlide(currentTutorialSlide);
            }
        }
    }

    private void EnableCertainSlide(int slideIndex)
    {
        Time.timeScale = 0;

        foreach (var item in tutorialSlides)
        {
            item.SetActive(false);
        }
        tutorialSlides[slideIndex].SetActive(true);
    }

    private void ShowFoodCircleOrder()
    {
        TutorialGameObject.SetActive(false);
        Time.timeScale = 1;
        StartCoroutine(DoTheFoodCircle());
    }

    private void ShowReproduction()
    {
        TutorialGameObject.SetActive(false);
        Time.timeScale = 1;
        StartCoroutine(DoTheReproduction());
    }

    private void ShowDefense()
    {
        TutorialGameObject.SetActive(false);
        Time.timeScale = 1;
        StartCoroutine(DoTheDefense());
    }

    private AlienHandler SpawnAdultAlien(int species, bool isAttacking, bool isLoving)
    {
        GameObject alienPoolGo = PoolManager.Instance.GetPooledAliens(false);
        if (alienPoolGo != null)
        {
            currentAlienHandler = alienPoolGo.GetComponent<AlienHandler>();
            currentAlienHandler.BrainwashAlien();
            currentAlienHandler.currentSpecies = species;
            currentAlienHandler.currentAge = AlienHandler.AlienAge.fullyGrown;
            currentAlienHandler.MyTransform.localScale = Vector3.one;
            currentAlienHandler.targetPosition = alienEndPosition;

            currentAlienHandler.lustTimer = 20;
            currentAlienHandler.hasUterus = false;
            currentAlienHandler.hungerTimer = 20;
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
                currentAlienHandler.hungerTimer = 1000;
                currentAlienHandler.hasUterus = true;
                currentAlienHandler.maxAmountOfBabies = 2;
            }
        }
        if (currentAlienHandler == null)
        {
            return null;
        }
        return currentAlienHandler;
    }

    IEnumerator DoTheFoodCircle()
    {
        GameManager.Instance.CameraFollowSpot.position = cameraPositionForTut;
        SpawnAdultAlien(0, true, false);
        yield return new WaitForSeconds(spawnDelay);
        SpawnAdultAlien(1, true, false);
        yield return new WaitForSeconds(spawnDelay);
        SpawnAdultAlien(2, true, false);
        yield return new WaitForSeconds(spawnDelay);
        LoveAlien1 = SpawnAdultAlien(0, true, false);
        yield return new WaitForSeconds(spawnDelay + 1);

        EnableCertainSlide(currentTutorialSlide);
        TutorialGameObject.SetActive(true);
    }

    IEnumerator DoTheReproduction()
    {
        LoveAlien2 = SpawnAdultAlien(0, false, true);
        yield return new WaitForSeconds(spawnDelay + 1);

        EnableCertainSlide(currentTutorialSlide);
        TutorialGameObject.SetActive(true);
    }

    IEnumerator DoTheDefense()
    {
        //SpawnAdultAlien(0, false, true);
        yield return new WaitForSeconds(spawnDelay);
        LoveAlien1.targetAlien = GameManager.Instance.players[0].gameObject;
        LoveAlien2.targetAlien = GameManager.Instance.players[0].gameObject;
        LoveAlien1.currentState = AlienHandler.AlienState.hunting;
        LoveAlien2.currentState = AlienHandler.AlienState.hunting;
        EnableCertainSlide(currentTutorialSlide);
        TutorialGameObject.SetActive(true);
    }
}
