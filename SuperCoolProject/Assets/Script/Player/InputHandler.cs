using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class InputHandler : MonoBehaviour
{
    private PlayerControls playerControls;
    public Vector2 inputMovement;
    public Vector2 inputAim;
    public bool inputPrimaryFire;
    public bool inputSecondaryFire;
    public bool inputInteracting;
    public bool inputJumping;
    public bool inputDashing;
    public bool isGamepad;

    private void Awake()
    {
        playerControls = new PlayerControls();
    }

    private void HandleInput()
    {
        playerControls.PlayerActionMap.Movement.performed += ctx => inputMovement = ctx.ReadValue<Vector2>();
        playerControls.PlayerActionMap.Aim.performed += ctx =>
        {
            inputAim = ctx.ReadValue<Vector2>();
            if (ctx.control.device is Gamepad)
            {
                isGamepad = true;
            }
        };

        playerControls.PlayerActionMap.Jump.performed += _ => inputJumping = true;
        playerControls.PlayerActionMap.Jump.canceled += _ => inputJumping = false;

        playerControls.PlayerActionMap.Dash.performed += _ => inputDashing = true;
        playerControls.PlayerActionMap.Dash.canceled += _ => inputDashing = false;

        playerControls.PlayerActionMap.Interaction.performed += ctx => inputInteracting = true;
        playerControls.PlayerActionMap.Interaction.canceled += ctx => inputInteracting = false;

        playerControls.PlayerActionMap.PrimaryFire.performed += ctx => inputPrimaryFire = true;
        playerControls.PlayerActionMap.PrimaryFire.canceled += ctx => inputPrimaryFire = false;

        playerControls.PlayerActionMap.SecondaryFire.performed += ctx => inputSecondaryFire = true;
        playerControls.PlayerActionMap.SecondaryFire.canceled += ctx => inputSecondaryFire = false;
    }

    private void OnEnable()
    {
        playerControls.Enable();
        HandleInput();
    }

    private void OnDisable()
    {
        playerControls.Disable();
    }
}
