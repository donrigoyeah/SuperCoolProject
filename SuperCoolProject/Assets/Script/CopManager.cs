using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CopManager : MonoBehaviour
{
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

    public int amountOfKilledAliensPaid = 0;
    public int currentAmountOfKilledAliens = 0;

    private float rCop;
    private float angleCop;
    private float randPosXCop;
    private float randPosZCop;
    private float rCar;
    private float angleCar;
    private float randPosXCar;
    private float randPosZCar;

    private GameObject copPoolGo;
    private CopHandler CH;
    private int copCarCount;
    private float totalDistanceOfCopsFromCar;

    public Button payButton;

    public static CopManager Instance;

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
        CopCarCurrent.transform.position = Vector3.MoveTowards(CopCarCurrent.transform.position, new Vector3(CopCarCurrent.transform.position.x, 0, CopCarCurrent.transform.position.z), Time.fixedDeltaTime * copCarSpeed);
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
            copPoolGo = PoolManager.Instance.GetPooledCop();
            if (copPoolGo != null)
            {

                rCop = Random.Range(1, 3);
                angleCop = i * (360 / copAmount);

                randPosXCop = rCop * Mathf.Cos(Mathf.Deg2Rad * angleCop);
                randPosZCop = rCop * Mathf.Sin(Mathf.Deg2Rad * angleCop);

                copPoolGo.transform.position = CopCarCurrent.transform.position + new Vector3(randPosXCop, 0, randPosZCop);

                CH = copPoolGo.GetComponent<CopHandler>();
                currentCops.Add(CH);
                CH.CopCar = CopCarCurrent.transform;
                CH.isAggro = false;
                CH.gameObject.SetActive(true);
            }
        }
    }

    public void HandleSpawnCopCar(int copCount)
    {
        hasBeenServed = false;
        hasLanded = false;

        currentAmountOfKilledAliens = AlienManager.Instance.totalKillCount - amountOfKilledAliensPaid;
        currentFineRequested = (currentAmountOfKilledAliens) * costPerKill;
        fineCost.text = currentFineRequested.ToString();
        fineDescribtion.text = "You killed " + currentAmountOfKilledAliens + " Aliens";


        copCarCount = Mathf.RoundToInt(copCount / 3);

        for (int i = 0; i < copCarCount; i++)
        {

        }
        CopCarCurrent = Instantiate(CopCar);
        CopCarCurrent.gameObject.transform.SetParent(this.transform);
        rCar = Random.Range(20, 25);
        angleCar = Random.Range(0, 360);

        randPosXCar = rCar * Mathf.Cos(Mathf.Deg2Rad * angleCar);
        randPosZCar = rCar * Mathf.Sin(Mathf.Deg2Rad * angleCar);

        CopCarCurrent.transform.position = new Vector3(randPosXCar, 100, randPosZCar);
    }


    public void HandleReturnCops()
    {
        totalDistanceOfCopsFromCar = 0;

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

        CopCarCurrent.transform.position = Vector3.MoveTowards(CopCarCurrent.transform.position, CopCarCurrent.transform.position + Vector3.up * 100, Time.fixedDeltaTime * copCarSpeed);
    }

    public void FineServed()
    {
        hasBeenServed = true;
    }

    public void FinePay()
    {
        paidFine += currentFineRequested;
        amountOfKilledAliensPaid = currentAmountOfKilledAliens;
        Time.timeScale = 1;
    }

    public void FineNotPaying()
    {
        isFineEvading = true;
        foreach (var cop in currentCops)
        {
            cop.isAggro = true;
        }
        Time.timeScale = 1;
    }

}
