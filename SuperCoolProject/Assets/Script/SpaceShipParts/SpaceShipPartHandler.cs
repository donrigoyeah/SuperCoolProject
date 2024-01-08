using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
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
    [SerializeField] private ParticleSystem collectingResourcesParticle;
    public bool particleSpawned = false;
    private AudioSource audioSource;
    [SerializeField] private AudioClip draggingAudio;



    // public LineRenderer linerenderer;
    // public Transform position1;
    // public Transform position2;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            inputHandler = other.gameObject.GetComponent<InputHandler>();
            playerLocomotion = other.gameObject.GetComponent<PlayerLocomotion>();
            playerManager = other.gameObject.GetComponent<PlayerManager>();

            playerSpeedReduction = spaceShipData.mass / 2.0f;

            if (inputHandler.inputInteracting)
            {
                if (!particleSpawned)
                {
                    collectingResourcesParticle.gameObject.SetActive(true);
                    particleSpawned = true;
                }

                if (!audioSource.isPlaying)
                {
                    audioSource.PlayOneShot(draggingAudio, 1f);
                }
                playerManager.currentPart = this.gameObject;
                previousPlayerSpeed = playerLocomotion.playerSpeed;
                playerLocomotion.playerSpeed = playerSpeedReduction;
                playerManager.isCarryingPart = true;
                this.transform.parent = other.transform;
            }
            else if (!inputHandler.inputInteracting)
            {
                collectingResourcesParticle.gameObject.SetActive(false);
                particleSpawned = false;
                if (playerManager == null)
                {
                    playerManager = other.gameObject.GetComponent<PlayerManager>();
                }
                playerLocomotion.playerSpeed = previousPlayerSpeed;
                playerManager.currentPart = null;
                playerManager.isCarryingPart = false;
                this.transform.parent = null;

            }
        }
    }

    private void Update()
    {
        tmp.text = spaceShipData.partName;
    }

}
