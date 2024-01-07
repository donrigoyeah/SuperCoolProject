using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Serialization;
using UnityEngine.InputSystem;
using Cinemachine;

public class GameManager : MonoBehaviour
{
    public static GameManager SharedInstance;

    [Header("World")]
    public int worldRadius = 150;
    public bool hasLost;
    public int numberOfPlayers;
    public int maxPlayers = 2;

    [Header("Clone Juice")]
    [SerializeField] public Image cloneJuiceUI;
    public float cloneCost = 20;
    public float currentCloneJuice;
    public float maxCloneJuice;

    [Header("Cops & Taxes")]
    public bool hasBeenServed = false;
    public int paidFine = 0;
    public bool isFineEvading = false;
    public int currentFineRequested;
    public List<CopHandler> currentCops;
    public GameObject CopCar;
    public GameObject CopScreenGO;
    public TextMeshProUGUI fineDescribtion;
    public TextMeshProUGUI fineCost;

    [Header("SpaceshipParts")]
    public GameObject SpaceShipPart;
    public Transform SpaceShipPartContainer;
    public int totalSpaceShipParts = 5;
    public int currentSpaceShipParts;
    public TextMeshProUGUI spaceShipPartsDisplay;
    [SerializeField] private SpaceShipScriptable[] spaceShipScriptable;

    [Header("SpaceShipPartsBoolValues")]
    public bool hasFuelCanister = false;
    public bool hasAmmoBox = false;
    public bool hasShieldGenerator = false;
    public bool hasAntenna = false;

    [Header("KillCounter")]
    public int sphereKilled = 0;
    public int squareKilled = 0;
    public int triangleKilled = 0;

    [Header("References")]
    [SerializeField] private GameObject map;
    [SerializeField] private PlayerInputManager playerInputManager;
    public List<PlayerManager> players;
    public Transform CameraFollowSpot; // For Cinemachine
    public GameObject PlayerHUD;
    public CopScreenHandler CopScreenHandler;

    public GameObject DeathScreen;
    public GameObject GameOverScreen;
    public Image DeathScreenCloneJuiceUI;

    private void Awake()
    {
        players = new List<PlayerManager>();
        SharedInstance = this;
        HandleSpawnShipParts();
        currentSpaceShipParts = 0;
        spaceShipPartsDisplay.text = currentSpaceShipParts.ToString() + "/" + totalSpaceShipParts.ToString();
        currentCloneJuice = maxCloneJuice;
        cloneJuiceUI.fillAmount = currentCloneJuice / maxCloneJuice;
    }

    private void FixedUpdate()
    {
        if (players.Count != 0)
        {
            HandleCameraTarget();
        }
    }

    private void HandleCameraTarget()
    {
        float targetX = 0;
        float targetY = 0;
        float targetZ = 0;

        foreach (var player in players)
        {
            targetX += player.transform.position.x;
            targetY += player.transform.position.y;
            targetZ += player.transform.position.z;
        }

        float targetXNorm = targetX / players.Count;
        float targetYNorm = targetY / players.Count;
        float targetZNorm = targetZ / players.Count;

        //CameraFollowSpot.position = new Vector3(targetXNorm, targetYNorm, targetZNorm);
        CameraFollowSpot.position = Vector3.Lerp(CameraFollowSpot.transform.position, new Vector3(targetXNorm, targetYNorm, targetZNorm), Time.deltaTime);
    }


    #region Handle Add Players

    public void AddPlayer(PlayerManager pm)
    {
        numberOfPlayers++;
        players.Add(pm);
        PlayerHUD.SetActive(true);

        if (numberOfPlayers == maxPlayers)
        {
            playerInputManager.DisableJoining();
        }
    }

    public void HandleCloneJuiceDrain()
    {
        currentCloneJuice -= cloneCost;
        cloneJuiceUI.fillAmount = currentCloneJuice / maxCloneJuice;
        if (currentCloneJuice < 0)
        {
            HandleLoss();
        }
    }

    #endregion

    #region Handle Game State

    private void HandleWin()
    {
        Debug.Log("Player won");
    }

    private void HandleLoss()
    {
        Debug.Log("You Lost");
        hasLost = true;
        GameOverScreen.SetActive(true);
    }

    #endregion

    #region Handle SpaceShip Parts

    private void HandleSpawnShipParts()
    {
        float radius = 0;
        float angle = 0;
        for (int i = 0; i < totalSpaceShipParts; i++)
        {
            int distanceIncrease = i * 10;

            radius = Random.Range(40 + distanceIncrease, 60 + distanceIncrease);

            float randPosX = radius * Mathf.Cos(angle);
            float randPosZ = radius * Mathf.Sin(angle);

            angle += 360 / totalSpaceShipParts;
            GameObject Go = Instantiate(SpaceShipPart, SpaceShipPartContainer);
            Go.transform.position = new Vector3(randPosX, 0, randPosZ);

            SpaceShipPartHandler DataAssign = Go.GetComponent<SpaceShipPartHandler>();

            if (spaceShipScriptable.Length > i)
            {
                DataAssign.spaceShipData = spaceShipScriptable[i];
            }
        }
    }

    //Space Ships parts are collected and abilities are unlocked here
    public void SpaceShipPartUpdate()
    {
        spaceShipPartsDisplay.text = currentSpaceShipParts.ToString() + "/" + totalSpaceShipParts.ToString();

        if (hasFuelCanister)
        {
            foreach (var item in players)
            {
                item.GetComponent<PlayerLocomotion>().playerSpeed = 13f;
            }
        }

        if (hasAntenna) { Debug.Log("Found Antenna"); map.SetActive(true); }

        if (currentSpaceShipParts == totalSpaceShipParts)
        {
            HandleWin();
        }
    }


    #endregion

    #region Handle Cops

    public void HandleSpawnCopCar(int copAmount)
    {
        hasBeenServed = false;

        currentFineRequested = (sphereKilled + squareKilled + triangleKilled) * 50;
        fineCost.text = currentFineRequested.ToString();
        fineDescribtion.text = "You killed " + sphereKilled + squareKilled + triangleKilled + " Aliens";

        GameObject CurrentCopCar = Instantiate(CopCar);

        if (CurrentCopCar.transform.position.y <= 0)
        {
            for (var i = 0; i < copAmount; i++)
            {
                GameObject copPoolGo = PoolManager.SharedInstance.GetPooledCop();
                if (copPoolGo != null)
                {
                    copPoolGo.transform.position = lazerSpawnLocation.position;
                    CopHandler CH = copPoolGo.GetComponent<CopHandler>();
                    CH.isAggro = false;

                }
            }
        }

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

    #endregion

    // TODO: Handle stuff like day/night cycle here
    // Handle spaceship parts collected
    // Handle gametime
}
