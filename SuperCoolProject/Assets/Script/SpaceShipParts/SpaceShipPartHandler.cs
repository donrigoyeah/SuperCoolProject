using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class SpaceShipPartHandler : MonoBehaviour
{
    private InputHandler inputHandler;
    private PlayerManager playerManager;
    private PlayerLocomotion playerLocomotion;

    public SpaceShipScriptable spaceShipData;
    public float playerSpeedReduction = 0f;
    public float previousPlayerSpeed = 10f;
    public TextMeshProUGUI tmp;
    [SerializeField] private ParticleSystem draggingParticles;
    public bool particleSpawned = false;
    private AudioSource audioSource;
    [SerializeField] private AudioClip draggingAudio;

    public GameObject InteractionUIScreen;
    public TextMeshProUGUI InteractionButtonText;

    public bool isInteractingWithPlayer = false;


    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        InteractionUIScreen.SetActive(false);
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

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            tmp.text = spaceShipData.partName;

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
