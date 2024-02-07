using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class HUDHandler : MonoBehaviour
{
    [Header("Reference")]
    public GameObject HUDSystemGO;
    public int currentHUD;
    public GameObject[] HUDS; // 0: Population 1: Minimap 2: Time
    public RectTransform HUDScaler;
    public GameObject UnlockPopulation;
    public Image UnlockPopulationImage;
    public GameObject UnlockMiniMap;
    public Image UnlockMiniMapImage;

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
    public float lastInputTimer = 100;
    public float timeThreshold = 3;
    private float minWaitDuration = .5f;
    private bool isResizing = false;
    public float scalingTransitionDuration = .5f;
    private int scalingTransitionSteps = 30;
    public float whiteNoiseSpeed;
    public float whiteNoiseRotationSpeed;
    public Material color;

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
        lastInputTimer += Time.fixedDeltaTime;

        if (lastInputTimer > timeThreshold && HUDScaler.localScale == Vector3.one)
        {
            StartCoroutine(ScaleDown());
        }

        if (currentHUD == 2) { HandleDisplayTimeOfDay(); }

        if (currentHUD == 0 && GameManager.Instance.hasAntenna == false)
        {
            Color newColor = UnlockPopulationImage.color;
            newColor.r = Mathf.PingPong(Time.time * whiteNoiseSpeed, 1);
            newColor.g = Mathf.PingPong(Time.time * whiteNoiseSpeed, 1);
            newColor.b = Mathf.PingPong(Time.time * whiteNoiseSpeed, 1);
            UnlockPopulationImage.color = newColor;
            UnlockPopulationImage.rectTransform.Rotate(Vector3.forward, Time.time * whiteNoiseRotationSpeed);
        }
        if (currentHUD == 1 && GameManager.Instance.hasRadar == false)
        {
            Color newColor = UnlockPopulationImage.color;
            newColor.r = Mathf.PingPong(Time.time * whiteNoiseSpeed, 1);
            newColor.g = Mathf.PingPong(Time.time * whiteNoiseSpeed, 1);
            newColor.b = Mathf.PingPong(Time.time * whiteNoiseSpeed, 1);
            UnlockMiniMapImage.color = newColor;
            UnlockMiniMapImage.rectTransform.Rotate(Vector3.forward, Time.time * whiteNoiseRotationSpeed);
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
            // This here is just to zoom on the 
            if (currentHUD == 1) { StartCoroutine(MiniMapCameraZoomOut()); } // Is MiniMap
            if (currentHUD == 3) { currentHUD = 0; } // loop back to 0
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

        // TODO: Make variable for zooming
        float delta = 0;
        Vector3 half = Vector3.one / 2f;
        WaitForEndOfFrame frame = new WaitForEndOfFrame();
        while (delta < scalingTransitionDuration)
        {
            delta += Time.deltaTime;
            HUDScaler.localScale = Vector3.Lerp(half, Vector3.one, delta / scalingTransitionDuration);
            yield return frame;
        }

        isResizing = false;
    }

    IEnumerator ScaleDown()
    {
        isResizing = true;

        float delta = 0;
        Vector3 half = Vector3.one / 2f;
        WaitForEndOfFrame frame = new WaitForEndOfFrame();
        while (delta < scalingTransitionDuration)
        {
            delta += Time.deltaTime;
            HUDScaler.localScale = Vector3.Lerp(Vector3.one, half, delta / scalingTransitionDuration);
            //TODO HUDScaler.localScale = Vector3.Lerp(Vector3.one, half, animationCurve.Evaluate(delta / scalingTransitionDuration));
            yield return frame;
        }

        isResizing = false;
    }
}
