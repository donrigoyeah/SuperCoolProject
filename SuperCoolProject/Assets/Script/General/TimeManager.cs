using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static AlienHandler;

public class TimeManager : MonoBehaviour
{
    public enum DayState
    {
        nightToSunrise,
        sunriseToDay,
        dayToSunSet,
        sunsetToNight
    }
    [Header("Current Day State")]

    private DayState currentStateValue; //this holds the actual value 
    public DayState currentState
    {
        get
        {
            return currentStateValue;
        }
        set
        {
            currentStateValue = value;
            // Handle Behaviour
            switch (value)
            {
                case DayState.nightToSunrise:

                    GameManager.Instance.TurnOffAllPlayerLights();
                    StartCoroutine(TreeAndStoneHandler.Instance.TurnOffAllTreeLights());
                    break;
                case DayState.dayToSunSet:
                    GameManager.Instance.TurnOnAllPlayerLights();
                    StartCoroutine(TreeAndStoneHandler.Instance.TurnOnAllTreeLights());
                    break;
            }
        }
    } //this is public and accessible, and should be used to change "State"


    [Header("Time")]
    public int minutes;
    public int hours;
    private int days;
    private float timeCounter;
    private int secondsInMinutes = 60;
    private int minutesinHour = 60;
    public float timeBoost = 150f; //1min cycle completes in 25seconds

    [Header("Light")]
    public TextMeshProUGUI displayTime;

    [Header("Light")]
    public Light sun;

    [Header("Colors")]
    public Gradient nightToSunrise; // 6-12
    public Gradient sunriseToDay; // 12 - 18
    public Gradient dayToSunSet; // 18 - 0
    public Gradient sunsetToNight; // 0-6


    private float lerp;
    private string formattedTime;


    public static TimeManager Instance;

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

        // Set initial Time, State and Color
        hours = 6;
        currentState = DayState.sunriseToDay;
        StartCoroutine(ChangeColor(sunriseToDay, 1f, 1f, 1.5f));
    }

    private void FixedUpdate()
    {
        timeCounter += Time.deltaTime * timeBoost;

        if (timeCounter >= secondsInMinutes)
        {
            minutes++;
            timeCounter = 0;
        }

        if (minutes >= minutesinHour)
        {
            hours++;
            minutes = 0;
            HourSettings(hours);
        }

        DisplayTimeText();
    }

    private void HourSettings(int hour)
    {
        if (hour == 6)
        {
            StartCoroutine(ChangeColor(nightToSunrise, 5f, 0f, 1f));
            currentState = DayState.nightToSunrise;


        }
        else if (hour == 12)
        {
            StartCoroutine(ChangeColor(sunriseToDay, 5f, 1f, 1.5f));
            currentState = DayState.sunriseToDay;

        }
        else if (hour == 18)
        {
            StartCoroutine(ChangeColor(dayToSunSet, 5f, 1.0f, 1f));
            currentState = DayState.dayToSunSet;
        }
        else if (hour == 24)
        {
            StartCoroutine(ChangeColor(sunsetToNight, 5f, 1f, 0.5f));
            currentState = DayState.sunsetToNight;

            hours = 0;
            days++;
        }
    }

    private IEnumerator ChangeColor(Gradient sunColor, float time, float initialSunIntensity, float finalSunIntensity)
    {
        for (float i = 0; i < time; i += Time.deltaTime)
        {
            lerp = i / time;
            sun.color = sunColor.Evaluate(lerp);
            // sun.intensity = Mathf.Lerp(initialSunIntensity, finalSunIntensity, lerp); // Incase we have change of plans for intensity
            yield return null;
        }
    }

    private void DisplayTimeText()
    {
        //string formattedTime = $"{hours:D2}:{minutes:D2}";
        formattedTime = $"{hours:D2} h";
        displayTime.text = "DAY " + days + "\n" + formattedTime;
    }
}
