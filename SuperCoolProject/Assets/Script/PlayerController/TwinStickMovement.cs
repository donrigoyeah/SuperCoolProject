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

    private Vector2 movement;
    private Vector2 aim;
    private Vector3 playerVelocity;
    public bool isJumping = true;
    public bool jumpTrigger;
    public float jumpCooldownTimer = 0f;
    public float jumpCooldown = 2f;


    private PlayerControls playerControls;

    private void Awake()
    {
        isJumping = true;
        controller = GetComponent<CharacterController>();
        playerControls = new PlayerControls();
    }

    private void OnEnable()
    {
        playerControls.Enable();
    }
    
    void Update()
    {
        Input();
        Movement();
        Rotation();

        if (jumpTrigger && isJumping)
        {
            Jump();
        }
        
        {
            jumpCooldownTimer -= Time.deltaTime;

            if (jumpCooldownTimer <= 0f)
            {
                isJumping = true; 
                jumpCooldownTimer = jumpCooldown; 
            }
        }
    }

    void Input()
    {
        movement = playerControls.PlayerActionMap.Movement.ReadValue<Vector2>();
        aim = playerControls.PlayerActionMap.Aim.ReadValue<Vector2>();
        jumpTrigger = playerControls.PlayerActionMap.Jump.triggered;

    }

    void Movement()
    {
        Vector3 move = new Vector3(movement.x, 0, movement.y);
        controller.Move(move * Time.deltaTime * playerSpeed);

        playerVelocity.y += gravityValue * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);
    }

    void Rotation()
    {
        Ray ray = Camera.main.ScreenPointToRay(aim);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        float rayDistance;

        if (groundPlane.Raycast(ray, out rayDistance))
        {
            Vector3 point = ray.GetPoint(rayDistance);
            LookAt(point);
        }
    }

    private void LookAt(Vector3 lookPoint)
    {
        Vector3 highCorrectPoint = new Vector3(lookPoint.x, transform.position.y, lookPoint.z);
        transform.LookAt(highCorrectPoint);
    }

    void Jump()
    {

        playerVelocity.y = jumpForce;
        isJumping = false;

    }
    
}
