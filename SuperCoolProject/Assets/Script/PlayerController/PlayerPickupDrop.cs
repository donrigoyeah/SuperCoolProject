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
            GameObject.Find("Player").GetComponent<PlayerShoot>().enabled = false;
            this.transform.parent = other.transform;
        }
        else
        {
            GameObject.Find("Player").GetComponent<PlayerShoot>().enabled = true;
            this.transform.parent = null;
        }
    }
}
