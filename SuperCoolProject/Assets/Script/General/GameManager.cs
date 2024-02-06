using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;
using UnityEngine.EventSystems;

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
    public Vector3 playerSpawnLocation;

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


    [Header("References")]
    public PlayerInputManager playerInputManager;
    public List<PlayerManager> players;
    public List<PlayerLocomotion> playersLocos;
    public Transform CameraFollowSpot; // For Cinemachine
    public LoadingScreenHandler loadingScreenHandler;
    public SpaceShipGameScene spaceShipGameScene;

    [Header("UI Elements")]
    public GameObject DeathScreen;
    public Button respawnButton;
    public Button restartButton;
    public GameObject GameOverScreen;
    public Image DeathScreenCloneJuiceUI;
    public TextMeshProUGUI deathScreenReason;

    public GameObject Clouds;


    [Header("Camera")]
    public int cameraSpeed = 1;
    public int cameraSpeedRaiseBuffer = 2;
    public int cameraSpeedRaiseDuration = 2;
    public int cameraSpeedMultiplier = 3;
    public float cameraZOffset = 2;

    [Header("Dead Body")]
    public bool playerDeadBody = false;

    [Header("Player/Spaceship Parts")]
    public GameObject spaceshipAntenna;
    public GameObject spaceshipShield;
    public GameObject spaceshipGrenadeModule;

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

        playerInputManager.DisableJoining();
    }

    private void FixedUpdate()
    {
        if (players.Count == 0) { return; }
        if (players[0].isInteracting == true) { return; }

        HandleCameraTarget();
        HandleCameraZoom();
    }

    private void HandleCameraTarget()
    {
        float targetX = 0;
        float targetY = 0;
        float targetZ = 0;

        foreach (var pm in players)
        {
            targetX += pm.transform.position.x;
            targetY += pm.transform.position.y;
            targetZ += pm.transform.position.z;
        }

        float targetXNorm = targetX / players.Count;
        float targetYNorm = targetY / players.Count;
        float targetZNorm = (targetZ / players.Count) - cameraZOffset;

        CameraFollowSpot.position = Vector3.Lerp(CameraFollowSpot.transform.position, new Vector3(targetXNorm, targetYNorm, targetZNorm), Time.deltaTime * cameraSpeed);
    }

    private void HandleCameraZoom()
    {
        if (playersLocos.TrueForAll(a => a.move != Vector3.zero))
        {
            // Zoom IN
            CameraShake.Instance.ZoomIn();
        }
        else
        {
            // Zoom Out
            CameraShake.Instance.ZoomOut();
        }
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

        pm.gameObject.transform.position = playerSpawnLocation;
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
            if (devMode == false)
            {
                StartCoroutine(WaitSecBeforeTut(cameraSpeedRaiseDuration));
            }
        }
    }

    IEnumerator WaitSecBeforeTut(float duration)
    {
        yield return new WaitForSeconds(duration);
        HandleTutorialStart();
    }

    private void HandleTutorialStart()
    {
        // Folowing code only runs if playerPrefs exist, and they only do in builds
        if (PlayerPrefs.HasKey("hideTutorial"))
        {
            hideTut = PlayerPrefs.GetInt("hideTutorial");

            if (hideTut == 1)
            {
                UnFreezeAllPlayers();
                UnFreezeAllAliens();
                return;
            }
            else
            {
                FreezeAllPlayers();
                FreezeAllAliens();
                TutorialHandler.Instance.EnableEntireTutorial();
                return;
            }
        }

        FreezeAllPlayers();
        FreezeAllAliens();
        TutorialHandler.Instance.EnableEntireTutorial();
        return;
    }

    public void FreezeAllAliens()
    {
        foreach (var item in AlienManager.Instance.allAlienHandlers)
        {
            item.canAct = false;
        }
    }

    public void UnFreezeAllAliens()
    {
        foreach (var item in AlienManager.Instance.allAlienHandlers)
        {
            item.canAct = true;
        }
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

    // Activated vai button on ui
    public void RespawnAllPlayers()
    {
        foreach (var item in players)
        {
            item.HandleRespawn();
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
        //if (currentCloneJuice < 0)
        //{
        //    HandleLoss();
        //}
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

        foreach (var item in players)
        {
            item.gameObject.SetActive(false);
        }

        if (spaceShipGameScene != null)
        {
            StartCoroutine(spaceShipGameScene.WinAnimation());
        }
        //CopManager.Instance.HandleSpawnCopCar(AlienManager.Instance.totalKillCount);

    }

    public void HandleLoss()
    {
        hasLost = true;
        foreach (var item in players)
        {
            item.isAlive = false;
        }
        GameOverScreen.SetActive(true);
        EventSystem.current.SetSelectedGameObject(respawnButton.gameObject);
    }

    #endregion

    #region Handle SpaceShip Parts

    public void HandleSpawnShipParts()
    {
        float radius = 0;
        float angle = 0;
        int distanceIncrease;
        float randPosPartX;
        float randPosPartZ;

        for (int i = 0; i < totalSpaceShipParts - 1; i++)
        {
            angle += 360 / totalSpaceShipParts;
            distanceIncrease = i * 10;
            radius = Random.Range(50 + distanceIncrease, 80 + distanceIncrease);
            randPosPartX = radius * Mathf.Cos(angle);
            randPosPartZ = radius * Mathf.Sin(angle);

            while (Physics.OverlapSphere(new Vector3(randPosPartX, .5f, randPosPartZ), 0.1f).Length > 0)
            {
                radius = Random.Range(50 + distanceIncrease, 80 + distanceIncrease);
                randPosPartX = radius * Mathf.Cos(angle);
                randPosPartZ = radius * Mathf.Sin(angle);
            };

            CurrentPartGO = Instantiate(SpaceShipPart, SpaceShipPartContainer);
            CurrentPartHandler = CurrentPartGO.GetComponent<SpaceShipPartHandler>();
            CurrentPartHandler.targetPositionX = randPosPartX;
            CurrentPartHandler.targetPositionZ = randPosPartZ;
            CurrentPartHandler.UpgradeName.text = spaceShipScriptable[i].name;
            CurrentPartHandler.spaceShipData = spaceShipScriptable[i];
            StartCoroutine(CurrentPartHandler.HandleFlyingParts());

        }

        // Spawn Antenna last and in front of player
        CurrentPartGO = Instantiate(SpaceShipPart, SpaceShipPartContainer);
        CurrentPartHandler = CurrentPartGO.GetComponent<SpaceShipPartHandler>();
        CurrentPartHandler.targetPositionX = AntennaSpawnLocation.x;
        CurrentPartHandler.targetPositionZ = AntennaSpawnLocation.z;
        CurrentPartHandler.UpgradeName.text = spaceShipScriptable[totalSpaceShipParts - 1].name;
        CurrentPartHandler.spaceShipData = spaceShipScriptable[totalSpaceShipParts - 1];
        StartCoroutine(CurrentPartHandler.HandleFlyingParts());

        TutorialHandler.Instance.totalAmountOfSpaceShpParts.text = totalSpaceShipParts.ToString();
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
            spaceshipShield.SetActive(true);
            foreach (var item in players)
            {
                item.shieldRechargeTime = item.shieldRechargeTimeWithUpgrade;
            }

            foreach (PlayerManager playerAntennaa in players)
            {
                Debug.Log("aaa");
                playerAntennaa.playerShield.gameObject.SetActive(true);
            }
        }

        if (hasAntenna)
        {
            spaceshipAntenna.SetActive(true);

            foreach (PlayerManager playerAntennaa in players)
            {
                playerAntennaa.playerAntenna.gameObject.SetActive(true);
            }

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

        if (hasAmmoBox)
        {
            spaceshipGrenadeModule.SetActive(true);
        }

        if (currentSpaceShipParts == totalSpaceShipParts)
        {
            HandleWin();
        }
    }
    #endregion
}
