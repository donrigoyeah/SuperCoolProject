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
    [Header("General")]
    public bool devMode;

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
    public List<PlayerLocomotion> playersLocos;
    public Transform CameraFollowSpot; // For Cinemachine
    public LoadingScreenHandler loadingScreenHandler;

    public GameObject DeathScreen;
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
        CameraFollowSpot.position = Vector3.Lerp(CameraFollowSpot.transform.position, new Vector3(targetXNorm, targetYNorm, targetZNorm), Time.deltaTime * cameraSpeed);
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
        if (TimeManager.Instance.currentState == TimeManager.DayState.sunsetToNight || TimeManager.Instance.currentState == TimeManager.DayState.dayToSunSet)
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
            // ???           // TODO: Change this here
            if (devMode == false)
            {
                Debug.Log("Tutorial STart");
                FreezeAllPlayers();
            }
        }
    }

    public void FreezeAllPlayers()
    {
        foreach (PlayerLocomotion player in playersLocos)
        {
            player.canMove = false;
        }
    }
    public void UnFreezeAllPlayers()
    {
        foreach (PlayerLocomotion player in playersLocos)
        {
            player.canMove = true;
        }
    }


    public void TurnOnAllPlayerLights()
    {
        foreach (PlayerManager player in players)
        {
            player.LightBeam.SetActive(true);
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
