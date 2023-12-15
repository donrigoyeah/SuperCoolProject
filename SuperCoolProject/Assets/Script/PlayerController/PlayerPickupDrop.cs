using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPickupDrop : MonoBehaviour
{
    private bool dragTrigger;
    private PlayerControls playerControls;
    private bool isDragDropActionPressed = false;

    private void OnEnable()
    {
        playerControls.Enable();
    }

    private void OnDisable()
    {
        playerControls.Disable();
    }

    private void Awake()
    {
        playerControls = new PlayerControls();

        playerControls.PlayerActionMap.DragDrop.started += ctx => isDragDropActionPressed = true;
        playerControls.PlayerActionMap.DragDrop.canceled += ctx => isDragDropActionPressed = false;
    }

    private void OnTriggerStay(Collider other)
    {
        if (isDragDropActionPressed && other.CompareTag("Player"))
        {
            other.gameObject.GetComponent<PlayerShoot>().enabled = false;
            other.gameObject.GetComponent<PlayerManager>().isCarryingPart = true;
            other.gameObject.GetComponent<PlayerManager>().currentPart = this.gameObject;
            this.transform.parent = other.transform;
        }
        else
        {
            other.gameObject.GetComponent<PlayerShoot>().enabled = true;
            other.gameObject.GetComponent<PlayerManager>().isCarryingPart = false;
            other.gameObject.GetComponent<PlayerManager>().currentPart = null;
            this.transform.parent = null;
        }
    }
}
