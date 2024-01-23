using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
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

    public TextMeshProUGUI primaryFireButtonText;
    public TextMeshProUGUI totalAmountOfSpaceShpParts;

    public string primaryShootButton;
    public string secondaryShootButton;
    public string toggleNavButton;
    public string interactionButton;
    public string dashButton;


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


        primaryFireButtonText.text = primaryShootButton;
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
            TutorialGameObject.SetActive(false);
            GameManager.Instance.UnFreezeAllPlayers();
            Time.timeScale = 1;
            return;
        }

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
        else if (currentTutorialSlide < totalTutorialSlides)
        {
            EnableCertainSlide(currentTutorialSlide);
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

    private AlienHandler SpawnAdultAlien(int species, bool isLoving)
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
            currentAlienHandler.currentState = AlienHandler.AlienState.idle;
            currentAlienHandler.HandleStateIcon(AlienHandler.AlienState.hunting);
            currentAlienHandler.hungerTimer = 1000;

            currentAlienTransform = alienPoolGo.transform;
            currentAlienTransform.position = alienStartPosition;
            if (isLoving == true)
            {
                currentAlienHandler.hasUterus = true;
                currentAlienHandler.maxAmountOfBabies = 2;
                currentAlienHandler.HandleStateIcon(AlienHandler.AlienState.loving);
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
        SpawnAdultAlien(0, false);
        yield return new WaitForSeconds(spawnDelay);
        SpawnAdultAlien(1, false);
        yield return new WaitForSeconds(spawnDelay);
        SpawnAdultAlien(2, false);
        yield return new WaitForSeconds(spawnDelay);
        LoveAlien1 = SpawnAdultAlien(0, false);
        yield return new WaitForSeconds(spawnDelay + 1);

        EnableCertainSlide(currentTutorialSlide);
        TutorialGameObject.SetActive(true);
    }

    IEnumerator DoTheReproduction()
    {
        LoveAlien2 = SpawnAdultAlien(0, true);
        LoveAlien1.HandleStateIcon(AlienHandler.AlienState.loving);
        LoveAlien1.targetAlien = LoveAlien2.gameObject;
        LoveAlien1.transform.LookAt(LoveAlien2.transform);
        yield return new WaitForSeconds(spawnDelay + 1);
        EnableCertainSlide(currentTutorialSlide);
        TutorialGameObject.SetActive(true);
    }

    IEnumerator DoTheDefense()
    {
        //SpawnAdultAlien(0, false, true);
        LoveAlien1.currentState = AlienHandler.AlienState.hunting;
        LoveAlien2.currentState = AlienHandler.AlienState.hunting;
        LoveAlien2.targetAlien = GameManager.Instance.players[0].gameObject;
        LoveAlien1.targetAlien = GameManager.Instance.players[0].gameObject;
        LoveAlien1.transform.LookAt(GameManager.Instance.players[0].gameObject.transform);
        LoveAlien2.transform.LookAt(GameManager.Instance.players[0].gameObject.transform);
        yield return new WaitForSeconds(1);
        LoveAlien1.brainWashed = false;
        LoveAlien2.brainWashed = false;
        EnableCertainSlide(currentTutorialSlide);
        TutorialGameObject.SetActive(true);
    }
}
