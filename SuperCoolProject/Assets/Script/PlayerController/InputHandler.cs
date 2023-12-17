using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class InputHandler : MonoBehaviour
{
    private PlayerControls playerControls;
    public bool isShooting;
    public bool isDragDropActionPressed { get; private set; }

    public Vector2 movementInput { get; private set; }
    public Vector2 aimInput { get; private set; }
    public bool jumpTriggered { get; private set; }
    public bool dashTriggered { get; private set; }
    
    private void Awake()
    {
        playerControls = new PlayerControls();
    }

    private void PickDrop()
    {
        playerControls.PlayerActionMap.DragDrop.performed += ctx => isDragDropActionPressed = true;
        playerControls.PlayerActionMap.DragDrop.canceled += ctx => isDragDropActionPressed = false;
    }
    
    public void PlayerShoot()
    {
        playerControls.PlayerActionMap.Shoot.performed += ctx => isShooting = true;
        playerControls.PlayerActionMap.Shoot.canceled += ctx => isShooting = false;
    }
    
    private void TwinStickMovement()
    {
        playerControls.PlayerActionMap.Movement.performed += ctx => movementInput = ctx.ReadValue<Vector2>();
        playerControls.PlayerActionMap.Aim.performed += ctx => aimInput = ctx.ReadValue<Vector2>();
        playerControls.PlayerActionMap.Jump.performed += _ => jumpTriggered = true;
        playerControls.PlayerActionMap.Jump.canceled += _ => jumpTriggered = false;
        playerControls.PlayerActionMap.Dash.performed += _ => dashTriggered = true;
        playerControls.PlayerActionMap.Dash.canceled += _ => dashTriggered = false;
    }
    
    private void OnEnable()
    {
        playerControls.Enable();
        TwinStickMovement();
        PlayerShoot();
        PickDrop();
    }

    private void OnDisable()
    {
        playerControls.Disable();
    }
}
