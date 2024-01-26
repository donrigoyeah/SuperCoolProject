using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerLocomotion : MonoBehaviour
{
    private Transform myTransform;
    private Camera MainCamera;
    private float horizontalInput;
    private float verticalInput;
    public bool canMove = true;

    public float playerSpeed;
    public float gravityValue;
    public float jumpForce;

    private CharacterController controller;
    private Vector3 playerVelocity;
    public Vector3 targetAimPosition;

    [Header("PlayerMovement")]
    private float rayDistance;
    private Plane groundPlane;
    private Ray ray;
    private Vector3 move;
    private Vector3 point;

    [Header("Dash")]
    public GameObject dashUiGO;
    public Image dashUi;
    public float dashMaxValue = 100;
    public float dashCurrentCharge = 0;
    public float dashCost = 33;
    public float dashRechargeSpeed = 3;
    public GameObject dashParticle;
    public bool canDash;
    public bool isDashing = false;
    private float dashDuration = 0.3f;
    private float dashExtraSpeed = 20f;

    [Header("Foot step Smoke Particle System")]
    private GameObject FSSGO;
    private Transform FSSTransform;
    public float deltaFSS;
    private float currentFSSTimer;

    [Header("References")]
    private PlayerControls playerControls;
    private PlayerInput playerInput;
    private PlayerManager playerManager;
    private InputHandler inputHandler;
    private Animator playerAnim;

    [Header("Audio")]
    public AudioClip footstepAudio;
    private AudioSource audioSource;


    private void OnEnable()
    {
        audioSource = GetComponent<AudioSource>();
        controller = GetComponent<CharacterController>();
        inputHandler = GetComponent<InputHandler>();
        playerManager = GetComponent<PlayerManager>();
        playerAnim = GetComponentInChildren<Animator>();
        myTransform = GetComponent<Transform>();
        MainCamera = Camera.main;
        dashParticle.SetActive(false);
    }

    void Update()
    {
        // Player can not move!
        if (canMove == false) { return; }

        // Player is Alive
        if (playerManager.isAlive)
        {
            if (inputHandler.inputMovement != Vector2.zero && !playerManager.isInteracting)
            {
                playerAnim.SetBool("IsWalking", true);
                Movement();
                Rotation();
            }
            else
            {
                playerAnim.SetBool("IsWalking", false);
            }

            if (inputHandler.inputDashing && dashCurrentCharge > 0)
            {
                if (isDashing == false && canDash == true || GameManager.Instance.devMode)
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
        }


        // Handle Pause Input
        if (inputHandler.inputPause)
        {
            // TODO: Check that it does not trigger double
            if (PauseMenu.Instance.isPaused)
            {
                StartCoroutine(PauseMenu.Instance.Resume());
                PauseMenu.Instance.Resume();
            }

            if (PauseMenu.Instance.isPaused == false)
            {
                StartCoroutine(PauseMenu.Instance.Pause());
                PauseMenu.Instance.Pause();
            }
        }

        // Handle toggle HUD 
        if (inputHandler.inputNavToggle)
        {
            HUDHandler.Instance.ChangeHUD();
        }
    }

    void Movement()
    {
        move = new Vector3(inputHandler.inputMovement.x, 0, inputHandler.inputMovement.y);
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
            FSSGO = PoolManager.Instance.GetPooledFSS();
            if (FSSGO != null)
            {
                FSSTransform = FSSGO.GetComponent<Transform>();
                FSSTransform.position = myTransform.position;
                FSSTransform.rotation = myTransform.rotation;
                FSSGO.SetActive(true);
                StartCoroutine(DisableAfterSeconds(1, FSSGO));
                FSSGO = null;
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
            horizontalInput = inputHandler.inputAim.x;
            verticalInput = inputHandler.inputAim.y;

            LookAt(new Vector3(myTransform.position.x + horizontalInput, 0, myTransform.position.z + verticalInput));
        }
        // Handle Mouse Input
        else
        {
            ray = MainCamera.ScreenPointToRay(inputHandler.inputAim);
            groundPlane = new Plane(Vector3.up, Vector3.zero); // represent a plane in 3D space

            if (groundPlane.Raycast(ray, out rayDistance))
            {
                point = ray.GetPoint(rayDistance);
                LookAt(point);
            }
        }
    }

    private void LookAt(Vector3 lookPoint)
    {
        targetAimPosition = new Vector3(lookPoint.x, myTransform.position.y, lookPoint.z);
        myTransform.LookAt(targetAimPosition);
    }

    private IEnumerator Dash()
    {
        Debug.Log("Particle system for dash added here");
        isDashing = true;
        playerSpeed += dashExtraSpeed;
        dashCurrentCharge -= dashCost;

        //Instantiate particle system for dash
        // TODO: face the direction of travel, not where player is looking at
        dashParticle.transform.rotation = myTransform.rotation;
        dashParticle.SetActive(true);
        yield return new WaitForSeconds(dashDuration);
        dashParticle.SetActive(false);

        isDashing = false;
        playerSpeed -= dashExtraSpeed;
    }

    IEnumerator DisableAfterSeconds(int sec, GameObject objectToDeactivate)
    {
        yield return new WaitForSeconds(sec);
        objectToDeactivate.SetActive(false);
    }
}
