using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public static TimeManager SharedInstance;

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
    public Gradient nightToSunrise;
    public Gradient sunriseToDay;
    public Gradient dayToSunSet;
    public Gradient sunsetToNight;

    private void Awake()
    {
        SharedInstance = this;
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
        }
        else if (hour == 12)
        {
            StartCoroutine(ChangeColor(sunriseToDay, 5f, 1f, 1.5f));
        }
        else if (hour == 18)
        {
            StartCoroutine(ChangeColor(dayToSunSet, 10f, 1.0f, 1f));
        }
        else if (hour == 24)
        {
            StartCoroutine(ChangeColor(sunsetToNight, 10f, 1f, 0.5f));
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
