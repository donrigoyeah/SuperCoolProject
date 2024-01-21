using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class LoadingScreenHandler : MonoBehaviour
{
    public static LoadingScreenHandler SharedInstance;

    // References for the loading scsreen
    [Header("LoadingScreen")]
    public GameObject LoadingScreen;
    public Image LoadingBar;
    public int numberOfPools = 10;
    public int currentLoadedPools = 0;
    public int totalAwakeCalls = 0;
    public int currentAwakeCalls = 0;

    public bool hasFinishedLoading;

    private void Awake()
    {
        SharedInstance = this;
        hasFinishedLoading = false;
        LoadingScreen.SetActive(true);
    }

    private void Update()
    {
        if (hasFinishedLoading)
        {
            LoadingScreen.SetActive(false);
        }

        float loadingAmount = (currentLoadedPools / numberOfPools + currentAwakeCalls / totalAwakeCalls) / 2;
        LoadingBar.fillAmount = loadingAmount;

        if (loadingAmount >= 1)
        {
            hasFinishedLoading = true;
        }
    }
}
