using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AlienManager : MonoBehaviour
{
    public static AlienManager SharedInstance;

    [Header("Current Alien Population")]
    // 0:Sphere, 1:Square, 2:Triangle
    public int sphereCount;
    public int squareCount;
    public int triangleCount;

    public int totalActiveAliens;

    public Image[] imagesPieChart;
    public Slider[] menuResourcesSlider;
    public float[] values;

    [Header("Spawn Settings")]
    public int segmentAmount = 6;
    public int segmentWidthRange = 10;
    public int minRadius = 30;
    public int maxRadius = 70;
    public int maxSleepDelay = 10;


    private void Awake()
    {
        SharedInstance = this;
    }

    private void Start()
    {
        SpawnAlien();
    }

    private void FixedUpdate()
    {
        PopulationUI();
    }
    private void SpawnAlien()
    {
        int oneSegmentOfPoulation = Mathf.RoundToInt(PoolManager.SharedInstance.alienAmount / segmentAmount);
        int currentPopulationSegment = oneSegmentOfPoulation;
        int pieSliceSize = 360 / segmentAmount;
        int currentSlize = 0;
        int currentSpieziesForArea = 0;

        for (int i = 0; i < PoolManager.SharedInstance.alienAmount; i++)
        {
            GameObject alienPoolGo = PoolManager.SharedInstance.GetPooledAliens();
            if (alienPoolGo != null)
            {
                if (i > currentPopulationSegment)
                {
                    currentPopulationSegment += oneSegmentOfPoulation;
                    currentSlize += pieSliceSize;

                    currentSpieziesForArea++;
                    if (currentSpieziesForArea == 3) { currentSpieziesForArea = 0; };

                }

                float r = Random.Range(minRadius, maxRadius);
                float angle = Random.Range(currentSlize - segmentWidthRange, currentSlize + segmentWidthRange);

                float randPosX = r * Mathf.Cos(Mathf.Deg2Rad * angle);
                float randPosZ = r * Mathf.Sin(Mathf.Deg2Rad * angle);

                AlienHandler alienPoolGoHandler = alienPoolGo.GetComponent<AlienHandler>();
                alienPoolGoHandler.currentSpecies = currentSpieziesForArea;
                alienPoolGoHandler.lifeTime = Random.Range(0, maxSleepDelay);
                alienPoolGoHandler.transform.localScale = Vector3.one * 0.2f; // Resource scale
                alienPoolGo.SetActive(true);

                alienPoolGo.transform.position = new Vector3(randPosX, 0.1f, randPosZ);
            }
        }
    }

    private void PopulationUI()
    {
        sphereCount = 0;
        squareCount = 0;
        triangleCount = 0;
        totalActiveAliens = 0;

        foreach (var item in PoolManager.SharedInstance.AlienPool)
        {
            if (item.activeInHierarchy)
            {
                AlienHandler itemAH = item.GetComponent<AlienHandler>();
                if (itemAH.currentSpecies == 0)
                {
                    sphereCount++;
                }
                else if (itemAH.currentSpecies == 1)
                {
                    squareCount++;
                }
                else if (itemAH.currentSpecies == 2)
                {
                    triangleCount++;
                }
                totalActiveAliens++;
            }
        }

        values = new float[3];
        values[0] = sphereCount;
        values[1] = squareCount;
        values[2] = triangleCount;
        SetPieChart(values);
    }


    private void SetPieChart(float[] valuesToSet)
    {
        float totalValues = 0;
        for (int i = 0; i < imagesPieChart.Length; i++)
        {
            totalValues += FindPercentage(valuesToSet, i);
            imagesPieChart[i].fillAmount = totalValues;
            menuResourcesSlider[i].value = totalValues;
        }
    }

    private float FindPercentage(float[] valuesToSet, int index)
    {
        float totalAmount = 0;
        for (int i = 0; i < valuesToSet.Length; i++)
        {
            totalAmount += valuesToSet[i];
        }
        return valuesToSet[index] / totalAmount;
    }
}
