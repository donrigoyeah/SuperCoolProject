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
    public bool hasDashPart = false;


    [Header("References")]
    [SerializeField] private GameObject map;
    [SerializeField] private PlayerInputManager playerInputManager;
    public List<PlayerManager> players;
    public Transform CameraFollowSpot; // For Cinemachine
    public LoadingScreenHandler loadingScreenHandler;

    public GameObject DeathScreen;
    public GameObject GameOverScreen;
    public Image DeathScreenCloneJuiceUI;

    [Header("Camera")]
    public int cameraSpeed = 1;
    public int cameraSpeedRaiseBuffer = 2;
    public int cameraSpeedRaiseDuration = 2;
    public int cameraSpeedMultiplier = 3;

    [Header("Dead Body")]
    public bool playerDeadBody = false;

    private void Awake()
    {
        SharedInstance = this;
        players = new List<PlayerManager>();
        currentSpaceShipParts = 0;
        spaceShipPartsDisplay.text = currentSpaceShipParts.ToString() + "/" + totalSpaceShipParts.ToString();
        currentCloneJuice = maxCloneJuice;
        cloneJuiceUI.fillAmount = currentCloneJuice / maxCloneJuice;
        loadingScreenHandler.totalAwakeCalls++;
    }

    private void Start()
    {
        HandleSpawnShipParts();
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
        CameraFollowSpot.position = Vector3.Lerp(CameraFollowSpot.transform.position, new Vector3(targetXNorm, targetYNorm, targetZNorm), Time.deltaTime * cameraSpeed);
    }


    #region Handle Add Players

    public void AddPlayer(PlayerManager pm)
    {
        numberOfPlayers++;
        players.Add(pm);
        HUDHandler.SharedInstance.HUDSystemGO.SetActive(true);
        HUDHandler.SharedInstance.EnableCurrentHUD(2); // Enable Time Display

        // Enable Light Beams on Player
        if (TimeManager.SharedInstance.currentState == TimeManager.DayState.sunsetToNight || TimeManager.SharedInstance.currentState == TimeManager.DayState.dayToSunSet)
        {
            pm.LightBeam.SetActive(true);
        }
        else
        {
            pm.LightBeam.SetActive(false);
        }

        if (numberOfPlayers == maxPlayers)
        {
            playerInputManager.DisableJoining();
        }
        if (numberOfPlayers == 1)
        {
            StartCoroutine(RaiseCameraSpeed(cameraSpeedRaiseDuration));
        }

    }

    IEnumerator RaiseCameraSpeed(float duration)
    {
        int steps = 10;

        yield return new WaitForSeconds(cameraSpeedRaiseBuffer);

        for (int i = 0; i <= steps; i++)
        {
            yield return new WaitForSeconds(duration / steps);
            cameraSpeed = 1 + cameraSpeedMultiplier * i / steps;
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

            radius = Random.Range(50 + distanceIncrease, 80 + distanceIncrease);

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
        // After loading all aliens sent finished state to Loading Screen
        loadingScreenHandler.currentAwakeCalls++;
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



    // TODO: Handle stuff like day/night cycle here
    // Handle spaceship parts collected
    // Handle gametime
}
