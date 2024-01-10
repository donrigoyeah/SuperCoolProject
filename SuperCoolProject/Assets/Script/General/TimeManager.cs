using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public static TimeManager SharedInstance;


    public enum DayState
    {
        nightToSunrise,
        sunriseToDay,
        dayToSunSet,
        sunsetToNight
    }
    [Header("Current Day State")]
    public DayState currentState;


    [Header("Time")]
    public int minutes;
    public int hours;
    private int days = 1;
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

    private void Awake()
    {
        SharedInstance = this;
        currentState = DayState.sunsetToNight;
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

            foreach (var player in GameManager.SharedInstance.players)
            {
                player.LightBeam.SetActive(false);
            }
        }
        else if (hour == 12)
        {
            StartCoroutine(ChangeColor(sunriseToDay, 5f, 1f, 1.5f));
            currentState = DayState.sunriseToDay;

        }
        else if (hour == 18)
        {
            StartCoroutine(ChangeColor(dayToSunSet, 10f, 1.0f, 1f));
            currentState = DayState.dayToSunSet;

            foreach (var player in GameManager.SharedInstance.players)
            {
                player.LightBeam.SetActive(true);
            }
        }
        else if (hour == 24)
        {
            StartCoroutine(ChangeColor(sunsetToNight, 10f, 1f, 0.5f));
            currentState = DayState.sunsetToNight;

            hours = 0;
            days++;
        }
    }

    private IEnumerator ChangeColor(Gradient sunColor, float time, float initialSunIntensity, float finalSunIntensity)
    {
        for (float i = 0; i < time; i += Time.deltaTime)
        {
            float lerp = i / time;
            sun.color = sunColor.Evaluate(lerp);
            // sun.intensity = Mathf.Lerp(initialSunIntensity, finalSunIntensity, lerp); // Incase we have change of plans for intensity
            yield return null;
        }
    }

    private void DisplayTimeText()
    {
        string formattedTime = $"{hours:D2}:{minutes:D2}";
        displayTime.text = "Day: " + days + "\n" + formattedTime;
    }
}
