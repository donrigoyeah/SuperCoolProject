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

    public Vector2 movement;
    public Vector2 aim;
    private Vector3 playerVelocity;
    public Vector3 targetAimPosition;

    private bool isJumping = true;
    private bool jumpTrigger;
    private float jumpCooldownTimer = 0f;
    private float jumpCooldown = 2f;

    public bool isDashing = true;
    public bool dashTrigger;
    public float dashCooldownTimer = 0f;
    public float dashCooldown = 5f;
    public float dashDuration = 0.3f;
    public float dashSpeed = 20f;


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

        if (dashTrigger && isDashing)
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

    void Input()
    {
        movement = playerControls.PlayerActionMap.Movement.ReadValue<Vector2>();
        aim = playerControls.PlayerActionMap.Aim.ReadValue<Vector2>();
        jumpTrigger = playerControls.PlayerActionMap.Jump.triggered;
        dashTrigger = playerControls.PlayerActionMap.Dash.triggered;
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
        Debug.Log("Hey");

        float originalSpeed = playerSpeed;
        playerSpeed = dashSpeed;

        yield return new WaitForSeconds(dashDuration);

        playerSpeed = originalSpeed;
        isDashing = false;
    }
}
