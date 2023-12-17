using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPickupDrop : MonoBehaviour
{
    private InputHandler inputHandler;
    private PlayerManager playerManager;
    
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            inputHandler = other.gameObject.GetComponent<InputHandler>();
            if (inputHandler.isDragDropActionPressed)
            {
                playerManager = other.gameObject.GetComponent<PlayerManager>();
                playerManager.currentPart = this.gameObject;
                playerManager.isCarryingPart = true;
                this.transform.parent = other.transform;
            }
            else if (!inputHandler.isDragDropActionPressed)
            {
                Debug.Log("3");
                if (playerManager == null)
                {
                    playerManager = other.gameObject.GetComponent<PlayerManager>();
                }
                playerManager.currentPart = null;
                playerManager.isCarryingPart = false;
                this.transform.parent = null;
            }
        }

    }
}
