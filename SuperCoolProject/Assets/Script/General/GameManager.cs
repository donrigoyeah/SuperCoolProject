using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    [Header("General")]
    public bool devMode;
    public int hideTut;

    [Header("World")]
    public int worldRadius = 150;
    public bool hasLost;
    public int numberOfPlayers;
    public int maxPlayers = 2;

    [Header("Clone Juice")]
    public Image cloneJuiceUI;
    public float cloneCost = 20;
    public float currentCloneJuice;
    public float maxCloneJuice;

    [Header("SpaceshipParts")]
    SpaceShipPartHandler CurrentPartHandler;
    GameObject CurrentPartGO;
    public int totalSpaceShipParts;
    public Vector3 AntennaSpawnLocation;
    public GameObject SpaceShipPart;
    public Transform SpaceShipPartContainer;
    public int currentSpaceShipParts;
    public TextMeshProUGUI spaceShipPartsDisplay;
    public SpaceShipScriptable[] spaceShipScriptable;

    [Header("SpaceShipPartsBoolValues")]
    public bool hasAmmoBox = false;
    public bool hasAntenna = false;
    public bool hasCloneJuicer = false;
    public bool hasFuelCanister = false;
    public bool hasLightmachine = false;
    public bool hasRadar = false;
    public bool hasShieldGenerator = false;
    public bool hasAimAssist = false;
    public Transform start;
    public Transform arc;
    
    [Header("References")]
    public PlayerInputManager playerInputManager;
    public List<PlayerManager> players;
    public List<PlayerLocomotion> playersLocos;
    public Transform CameraFollowSpot; // For Cinemachine
    public LoadingScreenHandler loadingScreenHandler;

    [Header("UI Elements")]
    public GameObject DeathScreen;
    public TextMeshProUGUI respawnButton;
    public GameObject GameOverScreen;
    public Image DeathScreenCloneJuiceUI;
    public GameObject Clouds;


    [Header("Camera")]
    public int cameraSpeed = 1;
    public int cameraSpeedRaiseBuffer = 2;
    public int cameraSpeedRaiseDuration = 2;
    public int cameraSpeedMultiplier = 3;

    [Header("Dead Body")]
    public bool playerDeadBody = false;


    public static GameManager Instance;

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

        players = new List<PlayerManager>();
        playersLocos = new List<PlayerLocomotion>();
        totalSpaceShipParts = spaceShipScriptable.Length;
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
        if (players.Count == 0) { return; }
        if (players[0].isInteracting == true) { return; }

        HandleCameraTarget();
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
        CameraFollowSpot.position = Vector3.Lerp(CameraFollowSpot.transform.position, new Vector3(targetXNorm, targetYNorm, targetZNorm), Time.fixedDeltaTime * cameraSpeed);
    }

    public void HideMouseCursor()
    {
        Cursor.visible = false;
    }

    public void ShowMouseCursor()
    {
        Cursor.visible = true;
    }

    #region Handle Add Players

    public void AddPlayer(PlayerManager pm)
    {
        numberOfPlayers++;
        players.Add(pm);

        PlayerLocomotion currentPlayerLoco = pm.GetComponent<PlayerLocomotion>();
        playersLocos.Add(currentPlayerLoco);

        pm.gameObject.transform.LookAt(TutorialHandler.Instance.alienEndPosition);

        HUDHandler.Instance.HUDSystemGO.SetActive(true);
        HUDHandler.Instance.EnableCurrentHUD(2); // Enable Time Display

        // Enable Light Beams on Player
        if ((TimeManager.Instance.currentState == TimeManager.DayState.sunsetToNight || TimeManager.Instance.currentState == TimeManager.DayState.dayToSunSet) && hasLightmachine == true)
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
            CameraFollowSpot.position = Vector3.zero;
            StartCoroutine(RaiseCameraSpeed(cameraSpeedRaiseDuration));
            //StartCoroutine(WaitSecBeforeTut(cameraSpeedRaiseDuration));
        }
    }

    IEnumerator WaitSecBeforeTut(float duration)
    {
        yield return new WaitForSeconds(duration);
        HandleTutorialStart();
    }

    private void HandleTutorialStart()
    {
        Debug.Log("start Turtorial here. Uncomment the foloowing line up to return");
        FreezeAllPlayers();
        TutorialHandler.Instance.EnableEntireTutorial();
        return;

        // Folowing code only runs if playerPrefs exist, and they only do in builds
        if (PlayerPrefs.HasKey("hideTutorial"))
        {
            hideTut = PlayerPrefs.GetInt("hideTutorial");

            if (hideTut == 1 || devMode == true)
            {
                UnFreezeAllPlayers();
                return;
            }
            else
            {
                FreezeAllPlayers();
                TutorialHandler.Instance.EnableEntireTutorial();
                return;
            }
        }

        Debug.Log("has no PlayerPrefs");
        FreezeAllPlayers();
        TutorialHandler.Instance.EnableEntireTutorial();
        return;
    }

    public void FreezeAllPlayers()
    {
        foreach (PlayerLocomotion player in playersLocos)
        {
            player.canMove = false;
        }
        foreach (PlayerManager player in players)
        {
            player.isInteracting = true;
        }
    }
    public void UnFreezeAllPlayers()
    {
        foreach (PlayerLocomotion player in playersLocos)
        {
            player.canMove = true;
        }
        foreach (PlayerManager player in players)
        {
            player.isInteracting = false;
        }
    }

    public void TurnOnAllPlayerLights()
    {
        if (hasLightmachine == false) { return; }
        else
        {
            foreach (PlayerManager player in players)
            {
                player.LightBeam.SetActive(true);
            }
        }
    }
    public void TurnOffAllPlayerLights()
    {
        foreach (PlayerManager player in players)
        {
            player.LightBeam.SetActive(false);
        }
    }

    public IEnumerator RaiseCameraSpeed(float duration)
    {
        int steps = 10;

        yield return new WaitForSeconds(cameraSpeedRaiseBuffer);

        for (int i = 0; i <= steps; i++)
        {
            yield return new WaitForSeconds(duration / steps);
            cameraSpeed = 1 + cameraSpeedMultiplier * i / steps;
        }

        Clouds.SetActive(false);
    }

    public void HandleDrainCloneJuice()
    {
        currentCloneJuice -= cloneCost;
        cloneJuiceUI.fillAmount = currentCloneJuice / maxCloneJuice;
        if (currentCloneJuice < 0)
        {
            HandleLoss();
        }
    }

    public void HandleGainCloneJuivce(float gain)
    {
        if (hasCloneJuicer)
        {
            gain = gain * 2;
        }
        currentCloneJuice += gain;
    }

    #endregion

    #region Handle Game State

    private void HandleWin()
    {
        Debug.Log("Player won");

        CopManager.Instance.HandleSpawnCopCar(AlienManager.Instance.totalKillCount);

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
        for (int i = 0; i < totalSpaceShipParts - 1; i++)
        {
            int distanceIncrease = i * 10;

            radius = Random.Range(50 + distanceIncrease, 80 + distanceIncrease);

            float randPosX = radius * Mathf.Cos(angle);
            float randPosZ = radius * Mathf.Sin(angle);

            angle += 360 / totalSpaceShipParts;
            CurrentPartGO = Instantiate(SpaceShipPart, SpaceShipPartContainer);
            // CurrentPartGO.transform.position = new Vector3(randPosX, 0, randPosZ);
            // CurrentPartGO.transform.position = new Vector3(30, 0, 30);

            CurrentPartHandler = CurrentPartGO.GetComponent<SpaceShipPartHandler>();
            CurrentPartHandler.UpgradeName.text = spaceShipScriptable[i].name;
            CurrentPartHandler.spaceShipData = spaceShipScriptable[i];
        }

        // Spawn Antenna last and in front of player
        CurrentPartGO = Instantiate(SpaceShipPart, SpaceShipPartContainer);
        CurrentPartGO.transform.position = AntennaSpawnLocation;

        CurrentPartHandler = CurrentPartGO.GetComponent<SpaceShipPartHandler>();
        CurrentPartHandler.UpgradeName.text = spaceShipScriptable[totalSpaceShipParts - 1].name;
        CurrentPartHandler.spaceShipData = spaceShipScriptable[totalSpaceShipParts - 1];

        TutorialHandler.Instance.totalAmountOfSpaceShpParts.text = totalSpaceShipParts.ToString();

        // After loading all aliens sent finished state to Loading Screen
        loadingScreenHandler.currentAwakeCalls++;
    }

    //Space Ships parts are collected and abilities are unlocked here
    public void SpaceShipPartUpdate()
    {
        spaceShipPartsDisplay.text = currentSpaceShipParts.ToString() + "/" + totalSpaceShipParts.ToString();

        if (hasFuelCanister)
        {
            foreach (var item in playersLocos)
            {
                item.canDash = true;
            }
        }

        if (hasShieldGenerator)
        {
            foreach (var item in players)
            {
                item.shieldRechargeTime = item.shieldRechargeTimeWithUpgrade;
            }
        }

        if (hasAntenna)
        {
            HUDHandler.Instance.UnlockPopulation.SetActive(false);

        }

        if (hasRadar)
        {
            HUDHandler.Instance.UnlockMiniMap.SetActive(false);
        }

        if (hasAimAssist)
        {
            foreach (var item in players)
            {
                item.canAim = true;
            }
        }


        if (currentSpaceShipParts == totalSpaceShipParts)
        {
            HandleWin();
        }
    }

    public Vector3 Trajectory(float t, Vector3 end)
    {
        Vector3 ab = Vector3.Lerp(start.position, arc.position, t);
        Vector3 bc = Vector3.Lerp(arc.position, end, t);
        return Vector3.Lerp(ab, bc, t);
    }

    // private void OnDrawGizmos()
    // {
    //     for (int i = 0; i < 40; i++)
    //     {
    //         Gizmos.DrawWireSphere(Trajectory(i/40f), 0.1f);
    //     }
    //     Debug.Log("draw");
    // }

    #endregion



    // TODO: Handle stuff like day/night cycle here
    // Handle spaceship parts collected
    // Handle gametime
}
