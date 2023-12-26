using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerLocomotion : MonoBehaviour
{
    public float playerSpeed;
    [SerializeField] private float gravityValue;
    [SerializeField] private float jumpForce;

    private CharacterController controller;
    private Vector3 playerVelocity;
    public Vector3 targetAimPosition;

    private bool isJumping = true;
    private float jumpCooldownTimer = 0f;
    private float jumpCooldown = 2f;

    [SerializeField] private float dashCooldown = 5f;
    private bool isDashing = true;
    private float dashCooldownTimer = 0f;
    private float dashDuration = 0.3f;
    private float dashSpeed = 20f;

    [Header("Footstep Smoke Particle System")]
    [SerializeField] private float deltaFSS;
    private float currentFSSTimer;

    [Header("References")]
    public GameObject pauseMenu;
    private PlayerControls playerControls;
    private InputHandler inputHandler;

    private void Awake()
    {
        isJumping = false;
        controller = GetComponent<CharacterController>();
        inputHandler = GetComponent<InputHandler>();
    }

    void Update()
    {
        Movement();
        Rotation();

        if (inputHandler.inputJumping && isJumping)
        {
            Jump();
        }

        if (inputHandler.inputDashing && isDashing)
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

        if (inputHandler.inputPause)
        {
            PauseMenu.SharedInstance.Pause();
        }
    }

    void Movement()
    {
        Vector3 move = new Vector3(inputHandler.inputMovement.x, 0, inputHandler.inputMovement.y);
        controller.Move(move * Time.deltaTime * playerSpeed);

        if (currentFSSTimer < deltaFSS)
        {
            currentFSSTimer += Time.deltaTime;
        }

        //Dust during movement particles
        if (move.magnitude > 0.1f && currentFSSTimer >= deltaFSS)
        {
            // Spawn Footstep Smoke GO
            GameObject FSSGO = PoolManager.SharedInstance.GetPooledFSS();
            if (FSSGO != null)
            {
                FSSGO.transform.position = transform.position;
                FSSGO.transform.rotation = transform.rotation;
                FSSGO.SetActive(true);
                // Disable again after 1 second
                StartCoroutine(DisableAfterSeconds(1, FSSGO));
            }
            // Reset timer to limit spawns
            currentFSSTimer = 0;
        }

        // TODO: DO we need this? Maybe remove gravity at all and set y to fixed position (?!)
        playerVelocity.y += gravityValue * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);
    }

    void Rotation()
    {
        // Handle Gamepad rotation
        if (inputHandler.isGamepad)
        {
            float horizontal = inputHandler.inputAim.x;
            float vertical = inputHandler.inputAim.y;

            LookAt(new Vector3(transform.position.x + horizontal, 0, transform.position.z + vertical));
        }
        // Handle Mouse Input
        else
        {
            Ray ray = Camera.main.ScreenPointToRay(inputHandler.inputAim);
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero); // represent a plane in 3D space
            float rayDistance;

            if (groundPlane.Raycast(ray, out rayDistance))
            {
                Vector3 point = ray.GetPoint(rayDistance);
                LookAt(point);
            }
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
    IEnumerator DisableAfterSeconds(int sec, GameObject objectToDeactivate)
    {
        yield return new WaitForSeconds(sec);
        objectToDeactivate.SetActive(false);
    }
}
