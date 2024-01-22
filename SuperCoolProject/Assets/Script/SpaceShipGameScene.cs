using System.Collections;
using System.Collections.Generic;
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
    public int animationSteps = 100;
    public float animationDuration = 3f;

    Vector3 endPosition = new Vector3(0, 0, 15);
    Vector3 startPosition = new Vector3(-260, 130, 15);

    public GameObject SpaceShipCanvas;
    public GameObject DamageParticlesGO;
    ParticleSystem DamageParticles;
    private ParticleSystem.MainModule DamageParticlesMain;

    public GameObject ExhaustParticlesGO;
    ParticleSystem ExhaustParticles;
    private ParticleSystem.MainModule ExhaustParticlesMain;

    public GameObject LandingParticlesGO;

    public PlayerInputManager playerInputManager;

    bool isMainMenu;


    private void Start()
    {
        this.transform.position = startPosition;

        isMainMenu = SceneManager.GetActiveScene().buildIndex == 0;

        DamageParticles = DamageParticlesGO.GetComponent<ParticleSystem>();
        DamageParticlesMain = DamageParticles.main;

        ExhaustParticles = ExhaustParticlesGO.GetComponent<ParticleSystem>();
        ExhaustParticlesMain = ExhaustParticles.main;

        SpaceShipCanvas.SetActive(false);

        if (GameManager.Instance.devMode)
        {
            this.transform.position = endPosition;
            GameManager.Instance.Clouds.SetActive(false);
            DamageParticlesGO.transform.rotation = Quaternion.Euler(-90, 90, 90);
            DamageParticlesMain.startSpeed = 0.5f;
            DamageParticlesMain.startLifetime = 6f;

            ExhaustParticlesGO.transform.rotation = Quaternion.Euler(-90, 90, 90);
            ExhaustParticlesMain.startSpeed = 0.5f;
            ExhaustParticlesMain.startLifetime = 6f;
        }
        else
        {
            playerInputManager.DisableJoining();
            StartCoroutine(CrashAnimation(animationDuration));
        }

    }
    private void FixedUpdate()
    {
        if (isMainMenu == false) { return; }

        HandleBobbing();
    }

    private void HandleBobbing()
    {
        float verticleBobMovement = bobbleAmplitude * Mathf.Sin(Time.time / bobbleSpeed) + bobbleStart;
        transform.position = new Vector3(transform.position.x, verticleBobMovement, transform.position.z);
    }

    IEnumerator CrashAnimation(float seconds)
    {
        float elapsedTime = 0;
        Vector3 startingPos = this.transform.position;
        while (elapsedTime < seconds)
        {
            this.transform.position = Vector3.Lerp(startingPos, endPosition, (elapsedTime / seconds));
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        // TODO: Add cameraShake
        this.transform.position = endPosition;
        LandingParticlesGO.SetActive(true);
        Destroy(LandingParticlesGO, 2);

        SpaceShipCanvas.SetActive(true);
        playerInputManager.EnableJoining();

        DamageParticlesGO.transform.rotation = Quaternion.Euler(-90, 90, 90);
        DamageParticlesMain.startSpeed = 0.5f;
        DamageParticlesMain.startLifetime = 6f;

        ExhaustParticlesGO.transform.rotation = Quaternion.Euler(-90, 90, 90);
        ExhaustParticlesMain.startSpeed = 0.5f;
        ExhaustParticlesMain.startLifetime = 6f;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Alien"))
        {
            AlienHandler enteringAlien = other.gameObject.GetComponent<AlienHandler>();
            enteringAlien.HandleFleeing(this.gameObject, true); // this time its not an alienGO but the spaceship; true for isEvadingPlayer
        }
    }
}
