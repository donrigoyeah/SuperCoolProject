using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.UI;


public class TutorialHandler : MonoBehaviour
{
    public GameObject TutorialGameObject;
    public int hideTut;

    public Button nextButton;

    public GameObject[] tutorialSlides;
    public int currentTutorialSlide;
    public int totalTutorialSlides;



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
        TutorialSceneHandler.Instance.ShowFoodCircleOrder();
    }

    public void SkipTut()
    {
        TutorialGameObject.SetActive(false);
        Time.timeScale = 1;

        Debug.Log("remove this here in future");
        // Start with Alien behaviour Scene
        TutorialSceneHandler.Instance.ShowFoodCircleOrder();
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
            TutorialSceneHandler.Instance.ShowFoodCircleOrder();
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
}
