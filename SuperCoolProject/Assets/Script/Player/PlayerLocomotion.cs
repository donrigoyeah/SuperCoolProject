using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;

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

    [Header("Dash")]
    [SerializeField] public GameObject dashUiGO;
    [SerializeField] public Image dashUi;
    public float dashMaxValue = 100;
    public float dashCurrentCharge = 0;
    public float dashCost = 33;
    public float dashRechargeSpeed = 3;
    [SerializeField] private ParticleSystem dashParticle;
    private ParticleSystem dashParticleSystemInstantiate;

    public bool isDashing = false;
    private float dashDuration = 0.3f;
    private float dashExtraSpeed = 20f;

    [Header("Footstep Smoke Particle System")]
    [SerializeField] private float deltaFSS;
    private float currentFSSTimer;

    [Header("References")]
    private PlayerControls playerControls;
    private PlayerInput playerInput;
    private PlayerManager playerManager;
    private InputHandler inputHandler;

    [Header("Audio")]
    [SerializeField] private AudioClip footstepAudio;
    private AudioSource audioSource;

    private void OnEnable()
    {
        isJumping = false;
        audioSource = GetComponent<AudioSource>();
        controller = GetComponent<CharacterController>();
        inputHandler = GetComponent<InputHandler>();
        playerManager = GetComponent<PlayerManager>();
    }

    void Update()
    {
        // Player is Alive
        if (playerManager.isAlive)
        {
            if (inputHandler.inputMovement != Vector2.zero && !playerManager.isInteracting)
            {
                Movement();
                Rotation();
            }

            if (inputHandler.inputJumping && isJumping)
            {
                Jump();
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

            if (inputHandler.inputDashing && dashCurrentCharge > 0)
            {
                if (isDashing == false)
                {
                    StartCoroutine(Dash());
                }
            }

            if (dashCurrentCharge < dashMaxValue)
            {
                dashUiGO.SetActive(true);
                dashCurrentCharge += Time.deltaTime * dashRechargeSpeed;
                dashUi.fillAmount = dashCurrentCharge / dashMaxValue;
            }
            else
            {
                dashUiGO.SetActive(false);
            }

            //This is to make dash partcle system follow player for proper trail
            if (dashParticleSystemInstantiate != null)
            {
                dashParticleSystemInstantiate.transform.position = this.transform.position;
            }
        }

        // Handle Pause Input
        if (inputHandler.inputPause)
        {
            // TODO: Check that it does not trigger double
            if (PauseMenu.SharedInstance.isPaused)
            {
                StartCoroutine(PauseMenu.SharedInstance.Resume());
                PauseMenu.SharedInstance.Resume();
            }

            if (PauseMenu.SharedInstance.isPaused == false)
            {
                StartCoroutine(PauseMenu.SharedInstance.Pause());
                PauseMenu.SharedInstance.Pause();
            }
        }
        if (inputHandler.inputNavToggle)
        {
            HUDHandler.SharedInstance.ChangeHUD();
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
            audioSource.PlayOneShot(footstepAudio, 1f);

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
        Debug.Log("Particle system for dash added here");
        isDashing = true;
        playerSpeed += dashExtraSpeed;
        dashCurrentCharge -= dashCost;

        //Instantiate particle system for dash
        dashParticleSystemInstantiate = Instantiate(dashParticle, this.transform.position, quaternion.identity);
        yield return new WaitForSeconds(dashDuration);
        Destroy(dashParticleSystemInstantiate.gameObject);

        isDashing = false;
        playerSpeed -= dashExtraSpeed;
    }
    IEnumerator DisableAfterSeconds(int sec, GameObject objectToDeactivate)
    {
        yield return new WaitForSeconds(sec);
        objectToDeactivate.SetActive(false);
    }
}
