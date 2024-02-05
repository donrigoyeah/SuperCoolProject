using System;
using System.Collections;
using TMPro;
using Unity.Burst.Intrinsics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class SpaceShipPartHandler : MonoBehaviour
{
    private InputHandler inputHandler;
    private PlayerManager playerManager;
    private PlayerLocomotion playerLocomotion;
    private Transform myTransform;
    public GameObject draggingParticles;
    public GameObject flyingParticles;
    public GameObject landingParticles;

    public SpaceShipScriptable spaceShipData;
    public float playerSpeedReduction = 0f;
    public float previousPlayerSpeed = 10f;
    public TextMeshProUGUI UpgradeName;
    public bool particleSpawned = false;
    private AudioSource audioSource;
    public AudioClip draggingAudio;

    public GameObject InteractionUIScreen;
    public TextMeshProUGUI InteractionButtonText;

    private GameObject currentPart;
    public Vector3 pickupOffset;
    public bool isInteractingWithPlayer = false;

    private float flyingSpeed = 0.2f;
    public float time;
    public bool hasLanded = false;

    public float targetPositionX;
    public float targetPositionZ;

    public Transform arc;
    private Vector3 ab;
    private Vector3 bc;

    public Transform[] childrenList;
    public float rotateSpeed = 75;
    public Transform rotatingPart;
    public bool noRotationRequired = false;

    private void Awake()
    {
        myTransform = this.transform;
        audioSource = GetComponent<AudioSource>();
        InteractionUIScreen.SetActive(false);
        draggingParticles.SetActive(false);
        flyingParticles.SetActive(true);

    }

    private void Start()
    {
        currentPart = Instantiate(spaceShipData.model, this.transform);
        currentPart.transform.position = this.transform.position;

        Rotator();

        if (spaceShipData.partName == "AmmoBox" || spaceShipData.partName == "FuelCanister" || spaceShipData.partName == "Antenna")
        {
            noRotationRequired = true;
        }

    }

    private void Update()
    {


        if (!noRotationRequired)
        {
            rotatingPart.Rotate(0, rotateSpeed * Time.deltaTime, 0, Space.World);
        }

        if (spaceShipData.partName == "Antenna")
        {

            rotatingPart.Rotate(0, Mathf.Sin(Time.time) * 1f, 0, Space.World);
        }
    }

    private void FixedUpdate()
    {

        if (inputHandler == null || playerLocomotion == null || playerManager == null || isInteractingWithPlayer == false || hasLanded == false) { return; }

        if (inputHandler.inputInteracting)
        {
            InteractionUIScreen.SetActive(false);
            playerSpeedReduction = spaceShipData.mass / 2.0f;

            if (!particleSpawned)
            {
                draggingParticles.SetActive(true);
                particleSpawned = true;
            }

            if (!audioSource.isPlaying)
            {
                audioSource.PlayOneShot(draggingAudio, 1f);
            }

            playerManager.currentPart = this.gameObject;
            playerLocomotion.playerSpeed = previousPlayerSpeed;
            playerLocomotion.playerSpeed -= playerSpeedReduction;
            playerManager.isCarryingPart = true;
            myTransform.parent = inputHandler.transform;
            myTransform.localPosition = pickupOffset;
        }
        else if (!inputHandler.inputInteracting)
        {
            InteractionUIScreen.SetActive(true);
            draggingParticles.SetActive(false);
            particleSpawned = false;

            playerLocomotion.playerSpeed = previousPlayerSpeed;
            playerManager.currentPart = null;
            playerManager.isCarryingPart = false;
            myTransform.parent = null;
        }
    }

    public IEnumerator HandleFlyingParts()
    {
        flyingParticles.SetActive(true);
        Vector3 targetPositon = new Vector3(targetPositionX, -1, targetPositionZ);

        float distanceToLandingPosition = 100;

        float delta = 0;
        WaitForEndOfFrame frame = new WaitForEndOfFrame();

        while (distanceToLandingPosition > 0)
        {
            delta += Time.deltaTime;
            ab = Vector3.Lerp(Vector3.zero, arc.position, delta);
            bc = Vector3.Lerp(arc.position, targetPositon, delta);
            myTransform.position = Vector3.Lerp(ab, bc, delta);
            distanceToLandingPosition = Vector3.Distance(myTransform.position, targetPositon);
            yield return frame;
        }

        myTransform.position = targetPositon;
        flyingParticles.SetActive(false);
        hasLanded = true;

        StartCoroutine(PlayLandingParticles());
    }

    IEnumerator PlayLandingParticles()
    {
        landingParticles.SetActive(true);
        yield return new WaitForSeconds(2);
        landingParticles.SetActive(false);
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            inputHandler = other.gameObject.GetComponent<InputHandler>();
            playerLocomotion = other.gameObject.GetComponent<PlayerLocomotion>();
            playerManager = other.gameObject.GetComponent<PlayerManager>();

            isInteractingWithPlayer = true;
            InteractionButtonText.text = TutorialHandler.Instance.interactionButton;
            playerLocomotion.playerSpeed = previousPlayerSpeed;
            InteractionUIScreen.SetActive(true);
            myTransform.parent = null;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isInteractingWithPlayer = false;
            InteractionUIScreen.SetActive(false);
            draggingParticles.gameObject.SetActive(false);
            playerManager.currentPart = null;
            playerManager.isCarryingPart = false;
            myTransform.parent = null;
            inputHandler = null;
            playerLocomotion = null;
            playerManager = null;
        }

    }

    private void Rotator()
    {
        childrenList = gameObject.GetComponentsInChildren<Transform>();

        foreach (Transform radarTransform in childrenList)
        {
            if (radarTransform.gameObject.CompareTag("Rotate"))
            {
                rotatingPart = radarTransform;
            }
        }
    }

}
