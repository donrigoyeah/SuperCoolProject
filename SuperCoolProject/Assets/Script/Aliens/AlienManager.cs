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

    [Header("Alien Settings")]
    public int alienLifeResource = 1;
    public int alienLifeChild = 30;
    public int alienLifeSexual = 40;
    public int alienLifeFullGrown = 50;
    public int timeToSexual = 15;
    public int timeToFullGrown = 25;
    public float alienSpeed;
    public float alienSpeedHunting;
    public float lookRadius = 10;
    public float resourceScale = 0.7f;
    public float childScale = 0.6f;
    public float sexualActiveScale = 0.8f;
    public float fullGrownScale = 1f;
    public float lustTimerThreshold = 5;
    public int maxAmountOfBabies = 10;
    public float hungerTimerThreshold = 5;
    public float dissolveRate = 0.0125f;
    public float refreshRate = 0.025f;
    public float renderDistance = 30;

    [Header("Alien Audio")]
    public List<AudioClip> aliensEating;
    public List<AudioClip> aliensLoving;

    [Header("Water / Sphere Alien Audio")]
    public AudioClip[] sphereAttackAudio;
    public AudioClip[] sphereDyingAudio;
    public AudioClip[] sphereBeingAttackedAudio;
    public AudioClip[] sphereLovemakingAudio;
    public AudioClip[] sphereEvadingAudio;

    [Header("Oxygen / Square Alien Audio")]
    public AudioClip[] squareAttackAudio;
    public AudioClip[] squareDyingAudio;
    public AudioClip[] squareBeingAttackedAudio;
    public AudioClip[] squareLovemakingAudio;
    public AudioClip[] squareEvadingAudio;

    [Header("Meat / Triangle Alien Audio")]
    public AudioClip[] triangleAttackAudio;
    public AudioClip[] triangleDyingAudio;
    public AudioClip[] triangleBeingAttackedAudio;
    public AudioClip[] triangleLovemakingAudio;
    public AudioClip[] triangleEvadingAudio;

    [Header("Array of all alien state")]
    public List<AudioClip[]> attackAudioList = new List<AudioClip[]>();
    public List<AudioClip[]> dyingAudioList = new List<AudioClip[]>();
    public List<AudioClip[]> beingAttackedAudioList = new List<AudioClip[]>();
    public List<AudioClip[]> lovemakingAudioList = new List<AudioClip[]>();
    public List<AudioClip[]> evadingAudioList = new List<AudioClip[]>();


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

        alienSpeedHunting = alienSpeed + 2;
        loadingScreenHandler.totalAwakeCalls++;
    }

    private void Start()
    {
        attackAudioList.Add(sphereAttackAudio);
        attackAudioList.Add(squareAttackAudio);
        attackAudioList.Add(triangleAttackAudio);

        dyingAudioList.Add(sphereDyingAudio);
        dyingAudioList.Add(squareDyingAudio);
        dyingAudioList.Add(triangleDyingAudio);

        beingAttackedAudioList.Add(sphereBeingAttackedAudio);
        beingAttackedAudioList.Add(squareBeingAttackedAudio);
        beingAttackedAudioList.Add(triangleBeingAttackedAudio);

        lovemakingAudioList.Add(sphereLovemakingAudio);
        lovemakingAudioList.Add(squareLovemakingAudio);
        lovemakingAudioList.Add(triangleLovemakingAudio);

        evadingAudioList.Add(sphereEvadingAudio);
        evadingAudioList.Add(squareEvadingAudio);
        evadingAudioList.Add(triangleEvadingAudio);

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

                alienPoolGoHandler = alienPoolGo.GetComponent<AlienHandler>();
                allAlienHandlers.Add(alienPoolGoHandler);
                alienPoolGoHandler.currentSpecies = currentSpieziesForArea;
                AddToResourceList(alienPoolGoHandler);
                alienPoolGoHandler.spawnAsAdults = true;

                r = Random.Range(minSpawnRadius, maxSpawnRadius);
                angle = Random.Range(currentSlize - segmentWidthRange, currentSlize + segmentWidthRange);
                randPosX = r * Mathf.Cos(Mathf.Deg2Rad * angle);
                randPosZ = r * Mathf.Sin(Mathf.Deg2Rad * angle);

                while (Physics.OverlapSphere(new Vector3(randPosX, .5f, randPosZ), 0.1f).Length != 0)
                {
                    r = Random.Range(minSpawnRadius, maxSpawnRadius);
                    angle = Random.Range(currentSlize - segmentWidthRange, currentSlize + segmentWidthRange);
                    randPosX = r * Mathf.Cos(Mathf.Deg2Rad * angle);
                    randPosZ = r * Mathf.Sin(Mathf.Deg2Rad * angle);
                };

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
