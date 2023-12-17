using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]
public class TwinStickMovement : MonoBehaviour
{
    [SerializeField] private float playerSpeed;
    [SerializeField] private float gravityValue;
    [SerializeField] private float jumpForce;
    
    private CharacterController controller;
    private Vector3 playerVelocity;
    public Vector3 targetAimPosition;

    private bool isJumping = true;
    private float jumpCooldownTimer = 0f;
    private float jumpCooldown = 2f;
    
    [SerializeField]private float dashCooldown = 5f;
    private bool isDashing = true;
    private float dashCooldownTimer = 0f;
    private float dashDuration = 0.3f;
    private float dashSpeed = 20f;
    
    private PlayerControls playerControls;
    private InputHandler inputHandler;

    private void Awake()
    {
        isJumping = true;
        controller = GetComponent<CharacterController>();
        inputHandler = GetComponent<InputHandler>();
    }
    
    void Update()
    {
        Movement();
        Rotation();

        if (inputHandler.jumpTriggered && isJumping)
        {
            Jump();
        }

        if (inputHandler.dashTriggered && isDashing)
        {
            StartCoroutine(Dash());
        }

        if (!isJumping)
        {
            jumpCooldownTimer -= Time.deltaTime;
            if (jumpCooldownTimer <= 0f)
            {
                isJumping = true;
                jumpCooldownTimer = jumpCooldown;
            }
        }

        if (!isDashing)
        {
            dashCooldownTimer -= Time.deltaTime;
            if (dashCooldownTimer <= 0f)
            {
                isDashing = true;
                dashCooldownTimer = dashCooldown;
            }
        }
    }
    
    void Movement()
    {
        Vector3 move = new Vector3( inputHandler.movementInput.x, 0,  inputHandler.movementInput.y);
        controller.Move(move * Time.deltaTime * playerSpeed);

        playerVelocity.y += gravityValue * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);

    }

    void Rotation()
    {
        Ray ray = Camera.main.ScreenPointToRay(inputHandler.aimInput);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero); // represent a plane in 3D space
        float rayDistance;

        if (groundPlane.Raycast(ray, out rayDistance))
        {
            Vector3 point = ray.GetPoint(rayDistance);
            LookAt(point);
        }
    }

    private void LookAt(Vector3 lookPoint)
    {
        targetAimPosition = new Vector3(lookPoint.x, transform.position.y, lookPoint.z);
        transform.LookAt(targetAimPosition);
    }

    private void Jump()
    {
        playerVelocity.y = jumpForce; 
        isJumping = false;
    }

    private IEnumerator Dash()
    {
        playerSpeed = dashSpeed;

        yield return new WaitForSeconds(dashDuration);

        playerSpeed = 10f;
        isDashing = false;
    }
}
