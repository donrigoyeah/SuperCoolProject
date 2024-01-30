using System;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class SpaceShipPartHandler : MonoBehaviour
{
    private InputHandler inputHandler;
    private PlayerManager playerManager;
    private PlayerLocomotion playerLocomotion;

    public SpaceShipScriptable spaceShipData;
    public float playerSpeedReduction = 0f;
    public float previousPlayerSpeed = 10f;
    public TextMeshProUGUI UpgradeName;
    public ParticleSystem draggingParticles;
    public bool particleSpawned = false;
    private AudioSource audioSource;
    public AudioClip draggingAudio;

    public GameObject InteractionUIScreen;
    public TextMeshProUGUI InteractionButtonText;

    private GameObject currentPart;
    public Vector3 pickupOffset;
    public bool isInteractingWithPlayer = false;

    private float speed = 0.2f;
    public float time;
    float radius = 0;
    float angle = 0;
    private float randPosZ;
    private float randPosX;
    private int distanceIncrease;
    public bool spaceshipPartReached = false;
    
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        InteractionUIScreen.SetActive(false);
    }

    private void Start()
    {
        currentPart = Instantiate(spaceShipData.model, this.transform);
        currentPart.transform.position = this.transform.position;

        time = 0f;
        
        distanceIncrease = Random.Range(-100, 100);
        radius = Random.Range(50 + distanceIncrease, 120);
        angle = 360 / Random.Range(1, 30);
        randPosX = radius * Mathf.Cos(angle);
        randPosZ = radius * Mathf.Sin(angle);
    }

    private void FixedUpdate()
    {
        if (inputHandler == null || playerLocomotion == null || playerManager == null || isInteractingWithPlayer == false) { return; }

        if (inputHandler.inputInteracting)
        {
            InteractionUIScreen.SetActive(false);
            playerSpeedReduction = spaceShipData.mass / 2.0f;

            if (!particleSpawned)
            {
                draggingParticles.gameObject.SetActive(true);
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
            this.transform.parent = inputHandler.transform;
            this.transform.localPosition = pickupOffset;
        }
        else if (!inputHandler.inputInteracting)
        {
            InteractionUIScreen.SetActive(true);
            draggingParticles.gameObject.SetActive(false);
            particleSpawned = false;

            playerLocomotion.playerSpeed = previousPlayerSpeed;
            playerManager.currentPart = null;
            playerManager.isCarryingPart = false;
            this.transform.parent = null;
        }
    }

    private void Update()
    {
        time += Time.deltaTime * speed;

        if (!spaceshipPartReached)
        {
            transform.position = GameManager.Instance.Trajectory(time, new Vector3(randPosX, 0, randPosZ));

        }
        
        if (time >= 1f)
        {
            spaceshipPartReached = true;
        }
        
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
            this.transform.parent = null;
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
            this.transform.parent = null;
            inputHandler = null;
            playerLocomotion = null;
            playerManager = null;
        }

    }

}
