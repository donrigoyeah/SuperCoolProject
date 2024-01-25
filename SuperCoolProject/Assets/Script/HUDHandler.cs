using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUDHandler : MonoBehaviour
{
    [Header("Reference")]
    public GameObject HUDSystemGO;
    public int currentHUD;
    public GameObject[] HUDS; // 0: Population 1: Minimap 2: Time
    public RectTransform HUDScaler;
    public GameObject UnlockPopulation;
    public GameObject UnlockMiniMap;

    [Header("MiniMap")]
    public Camera MiniMapCamera;
    public float cameraZoomOut = 200; // TODO: This is entire Island. Maybe Have it follow the player instead
    public float cameraZoomIn = 50;
    private int zoomiSteps;

    [Header("DayTime")]
    public RectTransform DayNightCircle;
    public RectTransform SunNMoonCircle;
    public int currentMinute = 0;
    public int currentHours = 0;
    public int currentTotalMinutes = 0;
    public float currentPercentage = 0;

    [Header("General")]
    private float lastInputTimer = 100;
    private float minWaitDuration = .5f;
    private float timeThreshold = 2;
    private bool isResizing = false;
    private float transitionDuration = .5f;


    public static HUDHandler Instance;

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

    private void Start()
    {
        // Set right day/night indicator
        currentMinute = TimeManager.Instance.minutes;
        currentHours = TimeManager.Instance.hours;
        currentTotalMinutes = currentMinute + currentHours * 60;
        currentPercentage = (currentTotalMinutes * 100) / (24 * 60);
        DayNightCircle.transform.rotation = Quaternion.Euler(0, 0, 180);


        currentHUD = 2;
        if (GameManager.Instance.devMode)
        {
            UnlockMiniMap.SetActive(true);
            UnlockPopulation.SetActive(true);
        }
        DisbaleAllHUDS();
    }

    private void FixedUpdate()
    {
        lastInputTimer += Time.deltaTime;

        if (lastInputTimer > timeThreshold)
        {
            if (HUDScaler.localScale == Vector3.one)
            {
                StartCoroutine(ScaleDown());
            }
        }
        if (currentHUD == 2)
        {
            HandleDisplayTimeOfDay();
        }
    }

    public void ChangeHUD()
    {
        // Prevent double clicking
        if (isResizing) { return; }
        if (lastInputTimer < minWaitDuration) return;

        lastInputTimer = 0;

        if (HUDScaler.localScale != Vector3.one)
        {
            if (currentHUD == 1) { StartCoroutine(MiniMapCameraZoomOut()); } // Is MiniMap
            StartCoroutine(ScaleUp());
            EnableCurrentHUD(currentHUD);
        }
        else
        {
            currentHUD++;
            if (currentHUD == 1) { StartCoroutine(MiniMapCameraZoomOut()); } // Is MiniMap
            else if (currentHUD == HUDS.Length) { currentHUD = 0; } // loop back to 0
            EnableCurrentHUD(currentHUD);
        }
    }

    private void DisbaleAllHUDS()
    {
        foreach (var item in HUDS)
        {
            item.SetActive(false);
        }
    }

    public void EnableCurrentHUD(int index)
    {
        DisbaleAllHUDS();
        HUDS[index].SetActive(true);
    }

    private void HandleDisplayTimeOfDay()
    {
        currentMinute = TimeManager.Instance.minutes;
        currentHours = TimeManager.Instance.hours;

        currentTotalMinutes = currentMinute + currentHours * 60;
        currentPercentage = (currentTotalMinutes * 100) / (24 * 60);

        SunNMoonCircle.transform.rotation = Quaternion.Euler(0, 0, 360 * currentPercentage / 100);

        if (currentTotalMinutes > (5 * 60) && currentTotalMinutes <= 6 * 60)
        {
            DayNightCircle.transform.rotation = Quaternion.Euler(0, 0, (180 * (currentTotalMinutes - (5 * 60)) / 60) + 180);
        }
        if (currentTotalMinutes > (17 * 60) && currentTotalMinutes <= 18 * 60)
        {
            DayNightCircle.transform.rotation = Quaternion.Euler(0, 0, 180 * (currentTotalMinutes - (17 * 60)) / 60);
        }
    }


    IEnumerator MiniMapCameraZoomOut()
    {
        zoomiSteps = 40;
        for (int i = 0; i <= zoomiSteps; i++)
        {
            yield return new WaitForSeconds(timeThreshold / zoomiSteps);
            MiniMapCamera.orthographicSize = cameraZoomIn + ((cameraZoomOut - cameraZoomIn) * i / zoomiSteps);
        }
    }

    IEnumerator ScaleUp()
    {
        isResizing = true;

        for (int i = 0; i <= 10; i++)
        {
            yield return new WaitForSeconds(transitionDuration / 10);
            HUDScaler.localScale = Vector3.one * .5f + Vector3.one * i * transitionDuration / 10;
        }

        isResizing = false;
    }

    IEnumerator ScaleDown()
    {
        isResizing = true;

        for (int i = 0; i <= 10; i++)
        {
            yield return new WaitForSeconds(transitionDuration / 10);
            HUDScaler.localScale = Vector3.one - Vector3.one * i * transitionDuration / 10;
        }

        isResizing = false;
    }
}
