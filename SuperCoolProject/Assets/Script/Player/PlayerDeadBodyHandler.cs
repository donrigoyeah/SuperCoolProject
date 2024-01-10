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


    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
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
            else if (!inputHandler.inputInteracting)
            {
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
}
