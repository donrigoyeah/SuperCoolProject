using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class InputHandler : MonoBehaviour
{
    private PlayerControls playerControls;
    public Vector2 inputMovement { get; private set; }
    public Vector2 inputAim { get; private set; }

    public bool inputPrimaryFire;
    public bool inputSecondaryFire;
    public bool inputSecondaryFireStarted;
    public bool inputInteracting;
    public bool inputJumping;
    public bool inputDashing;
    //public bool isDragDropActionPressed { get; private set; }

    //public bool jumpTriggered { get; private set; }
    //public bool dashTriggered { get; private set; }

    private void Awake()
    {
        playerControls = new PlayerControls();
    }

    private void HandleInput()
    {
        playerControls.PlayerActionMap.Movement.performed += ctx => inputMovement = ctx.ReadValue<Vector2>();
        playerControls.PlayerActionMap.Aim.performed += ctx => inputAim = ctx.ReadValue<Vector2>();

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
