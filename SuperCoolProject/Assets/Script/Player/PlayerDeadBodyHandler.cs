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
    public BoxCollider boxCollider;
    public GameObject deadPlayer;

    private void Update()
    {
        deadPlayer.transform.localPosition = this.gameObject.transform.position;
        
        if(inputHandler == null || playerLocomotion == null || playerManager == null){return;}

        if (inputHandler.inputInteracting)
        {
            playerManager.currentPart = this.gameObject;
            playerLocomotion.playerSpeed = playerSpeedReduction;
            playerManager.isCarryingPart = true;
        }
        else if (!inputHandler.inputInteracting)
        {
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
            Debug.Log("player");
            inputHandler = other.gameObject.GetComponent<InputHandler>();
            playerLocomotion = other.gameObject.GetComponent<PlayerLocomotion>();
            playerManager = other.gameObject.GetComponent<PlayerManager>();
            
            if (inputHandler.inputInteracting)
            {
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
