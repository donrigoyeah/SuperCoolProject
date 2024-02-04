using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AlienManager : MonoBehaviour
{
    [Header("Current Alien Population")]
    public List<AlienHandler> allAlienHandlers = new List<AlienHandler>(300);
    // 0:Sphere, 1:Square, 2:Triangle
    public int sphereCount;
    public int squareCount;
    public int triangleCount;

    public int totalActiveAliens;

    public Image[] imagesPieChart;
    public float[] values;

    [Header("Current Alien Resources")]
    public List<AlienHandler> resourceSphere = new List<AlienHandler>(100);
    public List<AlienHandler> resourceSquare = new List<AlienHandler>(100);
    public List<AlienHandler> resourceTriangle = new List<AlienHandler>(100);

    // TODO: List of List better?!

    [Header("Spawn Settings")]
    public int segmentAmount = 6;
    public int segmentWidthRange = 10;
    public int minSpawnRadius = 30;
    public int maxSpawnRadius = 70;
    public int maxInitialLifeTime = 10;
    public int totalTimeToSpawnAliens = 3;

    [Header("Kill Stuff")]
    public int totalKillCount = 0;
    public int currentPaidKillCount = 0;
    public int sphereKilled = 0;
    public int squareKilled = 0;
    public int triangleKilled = 0;

    public LoadingScreenHandler loadingScreenHandler;
    private GameObject alienPoolGo;
    private AlienHandler alienPoolGoHandler;
    private int oneSegmentOfPoulation;
    private int currentPopulationSegment;
    private int pieSliceSize;
    private int currentSlize;
    private int currentSpieziesForArea;
    private float r;
    private float angle;
    private float randPosX;
    private float randPosZ;
    private float totalValues;
    private float totalAmount;

    public Material[] alienColors; // 0:Blue > 1:Green > 2:Red  


    private AlienHandler PopulationUIAH;

    public static AlienManager Instance;

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

        loadingScreenHandler.totalAwakeCalls++;
    }

    private void Start()
    {
        StartCoroutine(SpawnAliens());
    }


    private void FixedUpdate()
    {
        PopulationUI();
    }

    private void ClearResourceList()
    {
        // When spawned in object Pool, The list is already filled with only spheres.
        resourceSphere.Clear();
        resourceSquare.Clear();
        resourceTriangle.Clear();
    }

    IEnumerator SpawnAliens()
    {
        if (PoolManager.Instance.alienAmount == 0) { yield return null; }

        ClearResourceList();
        oneSegmentOfPoulation = Mathf.RoundToInt(PoolManager.Instance.alienAmount / segmentAmount);
        currentPopulationSegment = oneSegmentOfPoulation;
        pieSliceSize = 360 / segmentAmount;
        currentSlize = 0;
        currentSpieziesForArea = 0;

        for (int i = 0; i < PoolManager.Instance.alienAmount; i++)
        {
            alienPoolGo = PoolManager.Instance.GetPooledAliens(false);
            if (alienPoolGo != null)
            {
                if (i > currentPopulationSegment)
                {
                    currentPopulationSegment += oneSegmentOfPoulation;
                    currentSlize += pieSliceSize;

                    currentSpieziesForArea++;
                    if (currentSpieziesForArea == 3) { currentSpieziesForArea = 0; };

                }

                r = Random.Range(minSpawnRadius, maxSpawnRadius);
                angle = Random.Range(currentSlize - segmentWidthRange, currentSlize + segmentWidthRange);

                randPosX = r * Mathf.Cos(Mathf.Deg2Rad * angle);
                randPosZ = r * Mathf.Sin(Mathf.Deg2Rad * angle);

                alienPoolGoHandler = alienPoolGo.GetComponent<AlienHandler>();
                allAlienHandlers.Add(alienPoolGoHandler);
                alienPoolGoHandler.currentSpecies = currentSpieziesForArea;
                AddToResourceList(alienPoolGoHandler);
                alienPoolGoHandler.spawnAsAdults = true;
                alienPoolGo.transform.position = new Vector3(randPosX, 0.1f, randPosZ);
                alienPoolGo.SetActive(true);
            }
            yield return new WaitForSeconds((1 / PoolManager.Instance.alienAmount));//* totalTimeToSpawnAliens
        }

        loadingScreenHandler.currentAwakeCalls++;
    }

    public void AddToResourceList(AlienHandler currentAlien)
    {
        if (currentAlien.currentSpecies == 0)
        {
            resourceSphere.Add(currentAlien);
            return;
        }
        else if (currentAlien.currentSpecies == 1)
        {
            resourceSquare.Add(currentAlien);
            return;
        }
        else if (currentAlien.currentSpecies == 2)
        {
            resourceTriangle.Add(currentAlien);
            return;
        }
    }

    public void RemoveFromResourceList(AlienHandler currentAlien)
    {
        if (currentAlien.currentSpecies == 0)
        {
            resourceSphere.Remove(currentAlien);
            return;
        }
        else if (currentAlien.currentSpecies == 1)
        {
            resourceSquare.Remove(currentAlien);
            return;
        }
        else if (currentAlien.currentSpecies == 2)
        {
            resourceTriangle.Remove(currentAlien);
            return;
        }
    }


    private void PopulationUI()
    {
        sphereCount = 0;
        squareCount = 0;
        triangleCount = 0;
        totalActiveAliens = 0;

        foreach (var item in PoolManager.Instance.AlienPool)
        {
            if (item.activeInHierarchy)
            {
                PopulationUIAH = item.GetComponent<AlienHandler>();
                if (PopulationUIAH.currentSpecies == 0)
                {
                    sphereCount++;
                }
                else if (PopulationUIAH.currentSpecies == 1)
                {
                    squareCount++;
                }
                else if (PopulationUIAH.currentSpecies == 2)
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


    public void KillAlien(int killedAlienIndex)
    {
        if (killedAlienIndex == 0) { sphereKilled++; totalKillCount++; }
        if (killedAlienIndex == 1) { squareKilled++; totalKillCount++; }
        if (killedAlienIndex == 2) { squareKilled++; totalKillCount++; }
    }

    private void SetPieChart(float[] valuesToSet)
    {
        totalValues = 0;
        for (int i = 0; i < imagesPieChart.Length; i++)
        {
            totalValues += FindPercentage(valuesToSet, i);
            imagesPieChart[i].fillAmount = totalValues;
        }
    }

    private float FindPercentage(float[] valuesToSet, int index)
    {
        totalAmount = 0;
        for (int i = 0; i < valuesToSet.Length; i++)
        {
            totalAmount += valuesToSet[i];
        }
        return valuesToSet[index] / totalAmount;
    }
}
