using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AlienManager : MonoBehaviour
{
    [Header("Current Alien Population")]
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
    public int minRadius = 30;
    public int maxRadius = 70;
    public int maxSleepDelay = 10;

    [Header("Kill Stuff")]
    public int totalKillCount = 0;
    public int currentPaidKillCount = 0;
    public int sphereKilled = 0;
    public int squareKilled = 0;
    public int triangleKilled = 0;

    public LoadingScreenHandler loadingScreenHandler;

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
        StartCoroutine(SpawnAliensWithDelay());
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

    IEnumerator SpawnAliensWithDelay()
    {
        yield return new WaitForSeconds(.5f);
        InitalSpawnAliens();
    }

    private void InitalSpawnAliens()
    {
        ClearResourceList();
        int oneSegmentOfPoulation = Mathf.RoundToInt(PoolManager.Instance.alienAmount / segmentAmount);
        int currentPopulationSegment = oneSegmentOfPoulation;
        int pieSliceSize = 360 / segmentAmount;
        int currentSlize = 0;
        int currentSpieziesForArea = 0;

        for (int i = 0; i < PoolManager.Instance.alienAmount; i++)
        {
            GameObject alienPoolGo = PoolManager.Instance.GetPooledAliens(false);
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
                alienPoolGo.transform.position = new Vector3(randPosX, 0.1f, randPosZ);
                alienPoolGo.SetActive(true);
            }
        }

        loadingScreenHandler.currentAwakeCalls++;
        return;
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


    public void KillAlien(int killedAlienIndex)
    {
        if (killedAlienIndex == 0) { sphereKilled++; totalKillCount++; }
        if (killedAlienIndex == 1) { squareKilled++; totalKillCount++; }
        if (killedAlienIndex == 2) { squareKilled++; totalKillCount++; }
    }

    private void SetPieChart(float[] valuesToSet)
    {
        float totalValues = 0;
        for (int i = 0; i < imagesPieChart.Length; i++)
        {
            totalValues += FindPercentage(valuesToSet, i);
            imagesPieChart[i].fillAmount = totalValues;
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
