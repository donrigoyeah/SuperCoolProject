using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;


public class TutorialHandler : MonoBehaviour
{
    public static TutorialHandler SharedInstance;

    public GameObject TutorialGameObject;
    public int hideTut;

    public Button nextButton;

    public GameObject[] tutorialSlides;
    public int currentTutorialSlide;
    public int totalTutorialSlides;



    private void Awake()
    {
        SharedInstance = this;
    }

    private void OnEnable()
    {
        currentTutorialSlide = 0;
        totalTutorialSlides = tutorialSlides.Length;

        if (PlayerPrefs.HasKey("hideTutorial"))
        {
            hideTut = PlayerPrefs.GetInt("hideTutorial");
            if (hideTut == 1)
            {
                TutorialGameObject.SetActive(false);
            }
            else
            {
                TutorialGameObject.SetActive(true);
                nextButton.Select();
                Time.timeScale = 0;
            }
        }
    }

    public void SetHideTut()
    {
        PlayerPrefs.SetInt("hideTutorial", 1);
    }

    public void NextSlide()
    {
        currentTutorialSlide++;

        if (currentTutorialSlide == totalTutorialSlides)
        {
            // Completed tutorial
            TutorialGameObject.SetActive(false);
            Time.timeScale = 1;
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
