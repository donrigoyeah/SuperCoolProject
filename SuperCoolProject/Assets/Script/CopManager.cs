using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static UnityEditor.Progress;

public class CopManager : MonoBehaviour
{
    public static CopManager SharedInstance;

    public bool hasBeenServed = false;
    public int copAmount = 0;
    public bool hasLanded = false;
    public int paidFine = 0;
    public bool isFineEvading = false;
    public int currentFineRequested;
    public List<CopHandler> currentCops;
    public GameObject CopCar;
    public GameObject CopCarCurrent;
    public GameObject CopScreenGO;
    public TextMeshProUGUI fineDescribtion;
    public TextMeshProUGUI fineCost;
    public int costPerKill = 50;
    public float copCarSpeed = 100;

    private void Awake()
    {
        SharedInstance = this;
    }


    private void FixedUpdate()
    {
        if (CopCarCurrent == null) { return; }

        if (hasLanded == false)
        {
            HandleLandCopCar();
            return;
        }
        if (currentCops.Count > 0 && hasBeenServed == true)
        {
            HandleReturnCops();
            return;
        }
        if (currentCops.Count == 0 && CopCarCurrent != null)
        {
            HandleReturnCopCar();
            return;
        }
    }

    public void HandleLandCopCar()
    {
        CopCarCurrent.transform.position = Vector3.MoveTowards(CopCarCurrent.transform.position, new Vector3(CopCarCurrent.transform.position.x, 0, CopCarCurrent.transform.position.z), Time.deltaTime * copCarSpeed);
        if (CopCarCurrent.transform.position.y <= 0)
        {
            hasLanded = true;
            HandleSpawnCops();
        }
    }

    public void HandleSpawnCops()
    {
        for (int i = 0; i < copAmount; i++)
        {
            GameObject copPoolGo = PoolManager.SharedInstance.GetPooledCop();
            if (copPoolGo != null)
            {

                float r = Random.Range(1, 3);
                float angle = i * (360 / copAmount);

                float randPosX = r * Mathf.Cos(Mathf.Deg2Rad * angle);
                float randPosZ = r * Mathf.Sin(Mathf.Deg2Rad * angle);

                copPoolGo.transform.position = CopCarCurrent.transform.position + new Vector3(randPosX, 0, randPosZ);

                CopHandler CH = copPoolGo.GetComponent<CopHandler>();
                currentCops.Add(CH);
                CH.CopCar = CopCarCurrent.transform;
                CH.isAggro = false;
                CH.gameObject.SetActive(true);
            }
        }
    }

    public void HandleSpawnCopCar(int newCopAmount)
    {
        hasBeenServed = false;
        hasLanded = false;
        copAmount = newCopAmount;

        int amountKilled = GameManager.SharedInstance.sphereKilled + GameManager.SharedInstance.squareKilled + GameManager.SharedInstance.triangleKilled;
        currentFineRequested = (amountKilled) * costPerKill;
        fineCost.text = currentFineRequested.ToString();
        fineDescribtion.text = "You killed " + amountKilled + " Aliens";

        CopCarCurrent = Instantiate(CopCar);
        CopCarCurrent.gameObject.transform.SetParent(this.transform);
        float rCar = Random.Range(20, 30);
        float angleCar = Random.Range(0, 360);

        float randPosXCar = rCar * Mathf.Cos(Mathf.Deg2Rad * angleCar);
        float randPosZCar = rCar * Mathf.Sin(Mathf.Deg2Rad * angleCar);

        CopCarCurrent.transform.position = new Vector3(randPosXCar, 100, randPosZCar);
    }

    public void HandleReturnCops()
    {
        float totalDistanceOfCopsFromCar = 0;

        for (int i = 0; i < currentCops.Count; i++)
        {
            totalDistanceOfCopsFromCar += Vector3.Distance(currentCops[i].transform.position, CopCarCurrent.transform.position);
        }

        if (totalDistanceOfCopsFromCar < .5f)
        {
            for (int i = 0; i < currentCops.Count; i++)
            {
                currentCops[i].gameObject.SetActive(false);
                currentCops.Remove(currentCops[i]);

            }

            return;
        }
    }

    public void HandleReturnCopCar()
    {
        if (CopCarCurrent.transform.position.y >= 100)
        {
            Destroy(CopCarCurrent.gameObject);
            CopCarCurrent = null;
            return;
        }

        CopCarCurrent.transform.position = Vector3.MoveTowards(CopCarCurrent.transform.position, CopCarCurrent.transform.position + Vector3.up * 100, Time.deltaTime * copCarSpeed);
    }


    public void FineServed()
    {
        hasBeenServed = true;
    }

    public void FinePay()
    {
        paidFine += currentFineRequested;
    }

    public void FineNotPaying()
    {
        isFineEvading = true;
        foreach (var cop in currentCops)
        {
            cop.isAggro = true;
        }
    }

}
