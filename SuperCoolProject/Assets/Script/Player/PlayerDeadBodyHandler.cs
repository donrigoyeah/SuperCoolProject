using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDeadBodyHandler : MonoBehaviour
{
    private InputHandler inputHandler;
    private PlayerManager playerManager;
    private PlayerLocomotion playerLocomotion;
    
    public float playerSpeedReduction = 2f;
    public float previousPlayerSpeed = 10f;
    public GameObject deadPlayer;
    public GameObject interactionUI;
    
    private void FixedUpdate()
    {
        if (deadPlayer != null)
        {
            deadPlayer.transform.localPosition = this.gameObject.transform.position;
        }
        
        if(inputHandler == null || playerLocomotion == null || playerManager == null){return;}

        if (inputHandler.inputInteracting)
        {
            interactionUI.SetActive(false);
            playerManager.currentPart = this.gameObject;
            playerLocomotion.playerSpeed = playerSpeedReduction;
            playerManager.isCarryingPart = true;
        }
        else if (!inputHandler.inputInteracting)
        {
            interactionUI.SetActive(true);
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
            inputHandler = other.gameObject.GetComponent<InputHandler>();
            playerLocomotion = other.gameObject.GetComponent<PlayerLocomotion>();
            playerManager = other.gameObject.GetComponent<PlayerManager>();
            
            if (inputHandler.inputInteracting)
            {
                interactionUI.SetActive(false);
                playerManager.currentPart = this.gameObject;
                playerLocomotion.playerSpeed = playerSpeedReduction;
                playerManager.isCarryingPart = true;
                this.transform.parent = other.transform;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (inputHandler == null)
            {
                inputHandler = other.gameObject.GetComponent<InputHandler>();
            }
            
            if (!inputHandler.inputInteracting)
            {
                if (playerManager == null)
                {
                    playerManager = other.gameObject.GetComponent<PlayerManager>();
                }
                if (playerLocomotion == null)
                {
                    playerLocomotion = other.gameObject.GetComponent<PlayerLocomotion>();
                }
                interactionUI.SetActive(false);
                playerLocomotion.playerSpeed = previousPlayerSpeed;
                playerManager.currentPart = null;
                playerManager.isCarryingPart = false;
                this.transform.parent = null;
                inputHandler = null;
                playerLocomotion = null;
                playerManager = null;
            }
        }
    }
}
