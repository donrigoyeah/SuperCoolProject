using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AlienManager : MonoBehaviour
{
    public static AlienManager SharedInstance;

    // 0:Sphere, 1:Square, 2:Triangle
    public int sphereCount;
    public int squareCount;
    public int triangleCount;

    public int totalActiveAliens;

    public Image[] imagesPieChart;
    public float[] values;

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
        for (int i = 0; i < PoolManager.SharedInstance.alienAmount; i++)
        {
            GameObject alienPoolGo = PoolManager.SharedInstance.GetPooledAliens();
            if (alienPoolGo != null)
            {
                int randomSpecies = Random.Range(0, 3);

                float r = Random.Range(30, 65);
                float angle = Random.Range(0, 360);

                float randPosX = r * Mathf.Cos(angle);
                float randPosZ = r * Mathf.Sin(angle);

                AlienHandler alienPoolGoHandler = alienPoolGo.GetComponent<AlienHandler>();
                alienPoolGoHandler.currentSpecies = randomSpecies;
                alienPoolGoHandler.lifeTime = Random.Range(0, 10) * -1;
                alienPoolGoHandler.HandleAging(0);
                alienPoolGo.SetActive(true);

                // TODO: Maybe have them spawn in groups of the same kind to make sure they have good start oppertunity
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
