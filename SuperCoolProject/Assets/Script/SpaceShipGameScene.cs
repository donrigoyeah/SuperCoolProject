using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class SpaceShipGameScene : MonoBehaviour
{
    [Header("Menu Scene Stuff")]
    public float bobbleAmplitude = 0.02f;
    public float bobbleSpeed = 0.5f;
    public float bobbleStart = 23.5f;

    [Header("Game Scene Stuff")]
    public float animationDurationStart = 1f;
    public float animationDurationWin = 3f;

    public Vector3 gamePosition = new Vector3(0, -1, 15);
    private Vector3 startPosition = new Vector3(-260, 130, 15);
    private Vector3 winPosition = new Vector3(-260, 130, 15);

    public GameObject SpaceShipCanvas;
    public GameObject DamageParticlesGO;
    public GameObject SpaceShipEntrance;
    private ParticleSystem DamageParticles;
    private ParticleSystem.MainModule DamageParticlesMain;

    public GameObject ExhaustParticlesGO;
    private ParticleSystem ExhaustParticles;
    private ParticleSystem.MainModule ExhaustParticlesMain;

    public GameObject LandingParticlesGO;

    bool isMainMenu;
    private float verticleBobMovement;
    private float elapsedTimeCrash;
    private float elapsedTimeWin;
    private Vector3 startingPos;
    private AlienHandler enteringAlien;

    public List<GameObject> shipLights;

    public static SpaceShipGameScene Instance;

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
        SpaceShipEntrance.SetActive(false);
    }

    private void Start()
    {
        isMainMenu = SceneManager.GetActiveScene().buildIndex == 0;
        if (isMainMenu)
        {
            TurnOffShipLights();
            return;
        }
        this.transform.position = startPosition;
    }

    public void TurnOffShipLights()
    {
        foreach (var item in shipLights)
        {
            item.SetActive(false);
        }
    }
    public void TurnOnShipLights()
    {
        foreach (var item in shipLights)
        {
            item.SetActive(true);
        }
    }


    public void StartIntroOfSpaceShip()
    {
        DamageParticles = DamageParticlesGO.GetComponent<ParticleSystem>();
        DamageParticlesMain = DamageParticles.main;

        ExhaustParticles = ExhaustParticlesGO.GetComponent<ParticleSystem>();
        ExhaustParticlesMain = ExhaustParticles.main;

        SpaceShipCanvas.SetActive(false);


        if (GameManager.Instance.devMode)
        {
            this.transform.position = gamePosition;
            GameManager.Instance.Clouds.SetActive(false);
            DamageParticlesGO.transform.rotation = Quaternion.Euler(-90, 90, 90);
            DamageParticlesMain.startSpeed = 0.5f;
            DamageParticlesMain.startLifetime = 6f;

            ExhaustParticlesGO.transform.rotation = Quaternion.Euler(-90, 90, 90);
            ExhaustParticlesMain.startSpeed = 0.5f;
            ExhaustParticlesMain.startLifetime = 6f;

            GameManager.Instance.HandleSpawnShipParts();
            PlayerInputManager.instance.EnableJoining();
            SpaceShipEntrance.SetActive(true);
        }
        else
        {
            StartCoroutine(CrashAnimation(animationDurationStart));
        }
    }
    private void FixedUpdate()
    {
        if (isMainMenu == false) { return; }

        HandleBobbing();
    }

    private void HandleBobbing()
    {
        verticleBobMovement = bobbleAmplitude * Mathf.Sin(Time.time / bobbleSpeed) + bobbleStart;
        transform.position = new Vector3(transform.position.x, verticleBobMovement, transform.position.z);
    }

    IEnumerator CrashAnimation(float seconds)
    {
        elapsedTimeCrash = 0;
        startingPos = this.transform.position;
        WaitForEndOfFrame frame = new WaitForEndOfFrame();

        while (elapsedTimeCrash < seconds)
        {
            this.transform.position = Vector3.Lerp(startingPos, gamePosition, (elapsedTimeCrash / seconds));
            elapsedTimeCrash += Time.deltaTime;
            yield return frame;
        }
        this.transform.position = gamePosition;

        SpaceShipEntrance.SetActive(true);


        LandingParticlesGO.SetActive(true);
        Destroy(LandingParticlesGO, 2);

        DamageParticlesGO.transform.rotation = Quaternion.Euler(-90, 90, 90);
        DamageParticlesMain.startSpeed = 0.5f;
        DamageParticlesMain.startLifetime = 6f;

        ExhaustParticlesGO.transform.rotation = Quaternion.Euler(-90, 90, 90);
        ExhaustParticlesMain.startSpeed = 0.5f;
        ExhaustParticlesMain.startLifetime = 6f;


        SpaceShipCanvas.SetActive(true);
        GameManager.Instance.HandleSpawnShipParts();
        PlayerInputManager.instance.EnableJoining();
        StartCoroutine(CameraShakeAfterCrash());
        SpaceShipEntrance.SetActive(true);
    }

    public IEnumerator WinAnimation()
    {
        elapsedTimeWin = 0;
        startingPos = this.transform.position;
        PlayerInputManager.instance.DisableJoining();
        DamageParticlesGO.SetActive(false);

        while (elapsedTimeWin < animationDurationWin)
        {
            this.transform.position = Vector3.Lerp(startingPos, winPosition, (elapsedTimeWin / animationDurationWin));
            elapsedTimeWin += Time.fixedDeltaTime;
            yield return new WaitForEndOfFrame();
        }

        // TODO: Add cameraShake
        this.transform.position = winPosition;

        //TODO: Rotate in the right direction on win
        ExhaustParticlesGO.transform.rotation = Quaternion.Euler(-90, 90, 90);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Alien"))
        {
            enteringAlien = other.gameObject.GetComponent<AlienHandler>();
            if (enteringAlien.brainWashed == true) { return; } // Interaction with player in TutorialScene, prevents HandleUpdateTarget error

            enteringAlien.SetTarget(this.gameObject); // this time its not an alienGO but the spaceship; true for isEvadingPlayer
            enteringAlien.currentState = AlienHandler.AlienState.evading; // this time its not an alienGO but the spaceship; true for isEvadingPlayer
        }
    }

    IEnumerator CameraShakeAfterCrash()
    {
        CameraShake.Instance.ShakeCamera(10);
        yield return new WaitForSeconds(1);
        CameraShake.Instance.ResetCameraPosition();

    }
}
