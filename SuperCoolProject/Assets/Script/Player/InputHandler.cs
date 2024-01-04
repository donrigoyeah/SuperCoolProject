using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class InputHandler : MonoBehaviour
{
    private PlayerControls playerControls;
    private PlayerInput playerInput;
    private PlayerManager playerManager;
    public Vector2 inputMovement;
    public Vector2 inputAim;
    public bool inputPrimaryFire;
    public bool inputSecondaryFire;
    public bool inputInteracting;
    public bool inputJumping;
    public bool inputDashing;
    public bool inputPause;
    public bool inputNavToggle;
    public bool inputDevKit;
    public bool isGamepad;
    public int playerIndex;

    //public void OnEnable()
    //{
    //    if (playerControls == null)
    //    {
    //        playerControls = new PlayerControls();

    //        playerInput = GetComponent<PlayerInput>();
    //        playerIndex = playerInput.playerIndex;
    //        playerManager = GetComponent<PlayerManager>();
    //        GameManager.SharedInstance.AddPlayer(playerManager);
    //        Debug.Log("Player Joined: " + playerIndex.ToString());

    //    playerControls.PlayerActionMap.Dash.performed += ctx => inputDashing = true;
    //    playerControls.PlayerActionMap.Dash.canceled += ctx => inputDashing = false;

    //    playerControls.PlayerActionMap.Interaction.performed += ctx => inputInteracting = true;
    //    playerControls.PlayerActionMap.Interaction.canceled += ctx => inputInteracting = false;

    //    playerControls.PlayerActionMap.PrimaryFire.performed += ctx => inputPrimaryFire = true;
    //    playerControls.PlayerActionMap.PrimaryFire.canceled += ctx => inputPrimaryFire = false;

    //    playerControls.PlayerActionMap.SecondaryFire.performed += ctx => inputSecondaryFire = true;
    //    playerControls.PlayerActionMap.SecondaryFire.canceled += ctx => inputSecondaryFire = false;

    //    playerControls.PlayerActionMap.Pause.performed += ctx => inputPause = true;
    //    playerControls.PlayerActionMap.Pause.canceled += ctx => inputPause = false;
    //}


    private void OnEnable()
    {
        if (playerControls == null)
        {
            playerControls = new PlayerControls();
        }

        playerInput = GetComponent<PlayerInput>();
        playerIndex = playerInput.playerIndex;
        playerManager = GetComponent<PlayerManager>();
        GameManager.SharedInstance.AddPlayer(playerManager);
        Debug.Log("Player Joined: " + playerIndex.ToString());
        playerControls.Enable();
    }

    // private void OnEnable()
    // {
    //     playerControls.Enable();
    // }

    private void OnDisable()
    {
        playerControls.Disable();
    }

    public void MoveInput(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            inputMovement = ctx.ReadValue<Vector2>();
        }
        if (ctx.canceled)
        {
            inputMovement = Vector2.zero;
        }

        //playerControls.PlayerActionMap.Movement.performed += ctx => inputMovement = ctx.ReadValue<Vector2>();
        //playerControls.PlayerActionMap.Movement.canceled += ctx => inputMovement = Vector2.zero;
    }

    public void AimInput(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            inputAim = ctx.ReadValue<Vector2>();
        }
        if (ctx.canceled)
        {
            inputAim = Vector2.zero;
        }


        //playerControls.PlayerActionMap.Aim.performed += ctx => inputAim = ctx.ReadValue<Vector2>();
        //playerControls.PlayerActionMap.Aim.canceled += ctx => inputAim = Vector2.zero;

        if (ctx.control.device is Gamepad)
        {
            isGamepad = true;
        }
    }

    public void DashInput(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            inputDashing = true;
        }
        if (ctx.canceled)
        {
            inputDashing = false;
        }

        //playerControls.PlayerActionMap.Dash.performed += ctx => inputDashing = true;
        //playerControls.PlayerActionMap.Dash.canceled += ctx => inputDashing = false;
    }

    public void JumpInput(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            inputJumping = true;
        }
        if (ctx.canceled)
        {
            inputJumping = false;
        }

        //playerControls.PlayerActionMap.Jump.performed += ctx => inputJumping = true;
        //playerControls.PlayerActionMap.Jump.canceled += ctx => inputJumping = false;
    }

    public void InteractionInput(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            inputInteracting = true;
        }
        if (ctx.canceled)
        {
            inputInteracting = false;
        }
        //playerControls.PlayerActionMap.Interaction.performed += ctx => inputInteracting = true;
        //playerControls.PlayerActionMap.Interaction.canceled += ctx => inputInteracting = false;
    }

    public void PrimaryFireInput(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            inputPrimaryFire = true;
        }
        if (ctx.canceled)
        {
            inputPrimaryFire = false;
        }

        //playerControls.PlayerActionMap.PrimaryFire.performed += ctx => inputPrimaryFire = true;
        //playerControls.PlayerActionMap.PrimaryFire.canceled += ctx => inputPrimaryFire = false;
    }

    public void SecondaryFireInput(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            inputSecondaryFire = true;
        }
        if (ctx.canceled)
        {
            inputSecondaryFire = false;
        }

        //playerControls.PlayerActionMap.SecondaryFire.performed += ctx => inputSecondaryFire = true;
        //playerControls.PlayerActionMap.SecondaryFire.canceled += ctx => inputSecondaryFire = false;
    }

    public void PauseInput(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            inputPause = true;
        }
        if (ctx.canceled)
        {
            inputPause = false;
        }

        //playerControls.PlayerActionMap.Pause.performed += ctx => inputPause = true;
        //playerControls.PlayerActionMap.Pause.canceled += ctx => inputPause = false;
    }

    public void NavToggleInput(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            inputNavToggle = true;
        }
        if (ctx.canceled)
        {
            inputNavToggle = false;
        }

        //playerControls.PlayerActionMap.Pause.performed += ctx => inputPause = true;
        //playerControls.PlayerActionMap.Pause.canceled += ctx => inputPause = false;
    }
}
