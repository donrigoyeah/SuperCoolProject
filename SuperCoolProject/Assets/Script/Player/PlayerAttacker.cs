using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.VFX;
using Unity.VisualScripting;
using TMPro;
using UnityEditor.Experimental.GraphView;
using static AlienHandler;

public class PlayerAttacker : MonoBehaviour
{
    [Header("Lazer Sight Stuff")]
    [SerializeField] private LineRenderer laserSightLeft;
    [SerializeField] private LineRenderer laserSightRight;
    [SerializeField] private bool isLaserSight = true;
    public Transform lazerSpawnLocationRight;
    public Transform lazerSpawnLocationLeft;
    public bool isEnabled = false;
    public bool isDisabled = false;
    public float lazerSightRange = 20;
    public float lastTimeSinceLazer = 0;
    public float disableLazerAfterNoInput = 1f;
    public GameObject currentTargetEnemy;
    public GameObject AimTargetIndicatorGO;
    public RectTransform AimTargetIndicator;
    private Vector2 lastInput;
    private Vector3 AimTargetLocation;


    [Header("Lazer Gun Stuff")]
    [SerializeField] public GameObject overheatUIGO;
    [SerializeField] public Image overheatUI;
    [SerializeField] private float fireRate = 0.5f;
    [SerializeField] private float currentWeaponHeat = 0;
    [SerializeField] private float maxWeaponHeat = 100;
    [SerializeField] private float boostWeaponHeatThreshold = 70;
    [SerializeField] private float singleLazerHeat = 10;
    [SerializeField] private float gunCooldownSpeed = 0.003f;
    [SerializeField] private float gunOverHeatCooldownSpeed = 0.003f;
    [SerializeField] private bool gunOverheated = false;
    [SerializeField] private float nextFireTime = 0f;
    [SerializeField] private float bulletSpeed = 50;
    [SerializeField] private float bulletDamage = 10;
    [SerializeField] private float bulletDamageBoost = 2;
    [SerializeField] private bool leftRightSwitch;

    [Header("Grenade stuff")]
    [SerializeField] private Transform grenadeSpawnLocation;
    [SerializeField] public GameObject grenadeCooldownUIGO;
    [SerializeField] public Image grenadeCooldownUI;
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private GrenadeHandler grenadePrefab;
    [SerializeField] private float throwForce;
    [SerializeField] private float throwForceBuildUpSpeed = 0.3f;
    [SerializeField] private float maxthrowForce = 10f;
    [SerializeField] private float currentThrowForce = 0f;
    [SerializeField] private float currentGrenadeCooldownValue = 0f;
    [SerializeField] private float grenadeCooldownSpeed = 1;
    [SerializeField] private float grenadeCooldownMax = 10;
    [SerializeField] private bool grenadeAvailable;

    [Header("Grenade Trajectory Physics stuff")]
    [SerializeField] private int PhysicsFrame = 62;
    private Scene simulateScene;
    private PhysicsScene physicsScene;

    [Header("References")]
    private InputHandler inputHandler;
    private PlayerManager playerManager;

    // TODO: Imlement again
    //[Header("Camera Shake")]
    //[SerializeField] private CameraShake CameraShake;

    [Header("Audio")]
    [SerializeField] private AudioClip coolingDownAudio;
    [SerializeField] private AudioClip gunReadyAudio;
    [SerializeField] private AudioClip gunOverheatedAudio;
    private bool hasOverheatedOnce = false;
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        inputHandler = GetComponent<InputHandler>();
        playerManager = GetComponent<PlayerManager>();
    }

    private void Start()
    {
        CreatePhysicsScene();
    }

    void Update()
    {
        HandleShootLazer();
        HandleGrenadeThrow();
    }

    private void FixedUpdate()
    {
        float delta = Time.deltaTime;
        HandleWeaponHeat(delta);
        HandleGrenadeCooldown(delta);
        HandleEnableLazerSight();
    }

    #region Handle Lazer

    private void HandleShootLazer()
    {
        if (gunOverheated == false && nextFireTime > fireRate)
        {
            overheatUI.color = Color.Lerp(Color.green, Color.red, overheatUI.fillAmount / 0.70f);
            if (inputHandler.inputPrimaryFire && !playerManager.isCarryingPart && !PauseMenu.SharedInstance.isPaused)
            {
                if (currentWeaponHeat > boostWeaponHeatThreshold)
                {
                    Debug.Log("Code Explanation for Extra Damage");
                    SpawnLazer(bulletDamage + bulletDamageBoost * currentWeaponHeat / boostWeaponHeatThreshold);
                }
                else
                {
                    SpawnLazer(bulletDamage);
                }

                // CameraShake.ShakeCamera();
                currentWeaponHeat += singleLazerHeat;
                nextFireTime = 0;
            }
            else
            {
                // CameraShake.ResetCameraPosition();
            }
        }
    }

    private void HandleWeaponHeat(float delta)
    {
        SetWepaonHeatSlider(currentWeaponHeat);
        nextFireTime += delta;

        // Overheated gun
        if (currentWeaponHeat > maxWeaponHeat)
        {
            overheatUI.color = Color.red;
            gunOverheated = true;
            isLaserSight = false;
            if (!audioSource.isPlaying)
            {
                audioSource.PlayOneShot(gunOverheatedAudio, 1f);
                audioSource.PlayOneShot(coolingDownAudio, 1f);
            }
            Debug.Log("Overheated");
            //CameraShake.ResetCameraPosition();
            hasOverheatedOnce = true;
        }

        if (currentWeaponHeat == 0)
        {
            if (!audioSource.isPlaying && hasOverheatedOnce)
            {
                audioSource.PlayOneShot(gunReadyAudio, 1f);
            }
            hasOverheatedOnce = false;
        }

        // Return if current heat is 0
        if (currentWeaponHeat <= 0)
        {
            gunOverheated = false;
            isLaserSight = true;
            currentWeaponHeat = 0;
            overheatUIGO.SetActive(false);
            //TODO: Hide UI if not needed?!
            return;
        }

        // Normale gun heat cooldown
        if (gunOverheated == false && currentWeaponHeat > 0)
        {
            currentWeaponHeat -= gunCooldownSpeed;
            overheatUIGO.SetActive(true);
        }

        // Gun is overheated and needs to cool down before use
        if (gunOverheated == true && currentWeaponHeat > 0)
        {
            currentWeaponHeat -= gunCooldownSpeed + gunOverHeatCooldownSpeed;
            gunOverheated = true;
        }
    }

    private void SetWepaonHeatSlider(float value)
    {
        overheatUI.fillAmount = value / maxWeaponHeat;
    }

    private void SpawnLazer(float damage)
    {
        //TODO: Add Recoil

        GameObject bulletPoolGo = PoolManager.SharedInstance.GetPooledBullets();
        GameObject muzzlePoolGo = PoolManager.SharedInstance.GetPooledMuzzle();

        if (bulletPoolGo != null)
        {
            if (leftRightSwitch == true)
            {
                // Instantiate Bullet left
                bulletPoolGo.transform.position = lazerSpawnLocationLeft.position;
                bulletPoolGo.transform.rotation = lazerSpawnLocationLeft.rotation;
                if (muzzlePoolGo != null)
                {
                    muzzlePoolGo.transform.position = lazerSpawnLocationLeft.position;
                    muzzlePoolGo.transform.rotation = lazerSpawnLocationLeft.rotation;
                    muzzlePoolGo.SetActive(true);
                    StartCoroutine(DisableAfterSeconds(1, muzzlePoolGo));
                }
            }
            else
            {
                // Instantiate Bullet right
                bulletPoolGo.transform.position = lazerSpawnLocationRight.position;
                bulletPoolGo.transform.rotation = lazerSpawnLocationRight.rotation;
                if (muzzlePoolGo != null)
                {
                    muzzlePoolGo.transform.position = lazerSpawnLocationRight.position;
                    muzzlePoolGo.transform.rotation = lazerSpawnLocationRight.rotation;
                    muzzlePoolGo.SetActive(true);
                    StartCoroutine(DisableAfterSeconds(1, muzzlePoolGo));
                }
            }
            if (AimTargetLocation != Vector3.zero)
            {
                bulletPoolGo.transform.LookAt(AimTargetLocation, Vector3.up);
            }
            leftRightSwitch = !leftRightSwitch;

            BulletHandler BH = bulletPoolGo.GetComponent<BulletHandler>();
            BH.isPlayerBullet = true;
            BH.bulletDamage = damage;
            bulletPoolGo.SetActive(true);
            BH.rb.velocity = Vector3.zero;
            BH.rb.velocity = bulletPoolGo.transform.forward * bulletSpeed;
        }
    }

    private void HandleEnableLazerSight()
    {
        //Debug.Log("Round inputHandler.inputAim.x): " + Mathf.RoundToInt(inputHandler.inputAim.x));
        //Debug.Log("Round lastInput.x): " + Mathf.RoundToInt(lastInput.x));


        if (isLaserSight)
        {
            lastInput = inputHandler.inputAim;
            lastTimeSinceLazer += Time.deltaTime;

            // Check if on Mouse and has not moved the mouse enough btw aimInput of controller
            if (inputHandler.inputAim != Vector2.zero ||
                    (inputHandler.isGamepad == false &&
                    Mathf.RoundToInt(inputHandler.inputAim.x) != Mathf.RoundToInt(lastInput.x) ||
                    Mathf.RoundToInt(inputHandler.inputAim.y) != Mathf.RoundToInt(lastInput.y))
                )
            {
                lastTimeSinceLazer = 0;
            }



            if (lastTimeSinceLazer < disableLazerAfterNoInput)
            {
                if (isEnabled == false)
                {
                    StartCoroutine(EnableLazers());
                    isEnabled = true;
                    isDisabled = false;
                }
                LaserSight();
                return;
            }
            else
            {
                if (isDisabled == false)
                {
                    StartCoroutine(DisableLazers());
                    isDisabled = true;
                    isEnabled = false;
                    return;
                }
            }
        }
        else
        {
            laserSightLeft.enabled = false;
            laserSightRight.enabled = false;
        }
    }

    IEnumerator EnableLazers()
    {
        laserSightLeft.enabled = true;
        laserSightRight.enabled = true;
        laserSightLeft.SetPosition(0, Vector3.zero);
        laserSightRight.SetPosition(0, Vector3.zero);
        laserSightLeft.SetPosition(1, Vector3.zero);
        laserSightRight.SetPosition(1, Vector3.zero);

        int steps = 30;
        float durationOfAnimation = 0.2f;
        for (int i = 0; i < steps; i++)
        {
            yield return new WaitForSeconds(durationOfAnimation / steps);

            laserSightLeft.SetPosition(1, (Vector3.forward * lazerSightRange * i) / steps);
            laserSightRight.SetPosition(1, (Vector3.forward * lazerSightRange * i) / steps);
        }

    }
    IEnumerator DisableLazers()
    {
        laserSightLeft.SetPosition(0, Vector3.zero);
        laserSightRight.SetPosition(0, Vector3.zero);

        int steps = 30;
        float durationOfAnimation = 0.2f;

        for (int i = 0; i < steps; i++)
        {
            yield return new WaitForSeconds(durationOfAnimation / steps);

            laserSightLeft.SetPosition(1, (Vector3.forward * lazerSightRange) - (Vector3.forward * lazerSightRange * i / steps));
            laserSightRight.SetPosition(1, (Vector3.forward * lazerSightRange) - (Vector3.forward * lazerSightRange * i / steps));
        }
        laserSightLeft.SetPosition(1, Vector3.zero);
        laserSightRight.SetPosition(1, Vector3.zero);

        laserSightLeft.enabled = false;
        laserSightRight.enabled = false;
    }

    private void LaserSight()
    {
        RaycastHit hit;

        // Use Spherecast here instead of ray to have a better detactionChance
        if (Physics.SphereCast(transform.position + Vector3.up / 2, 2, transform.forward, out hit, lazerSightRange))
        {
            if (hit.collider)
            {
                // If Alien or Cop
                if (hit.collider.gameObject.CompareTag("Alien") || hit.collider.gameObject.CompareTag("Cop"))
                {
                    laserSightLeft.useWorldSpace = true;
                    laserSightRight.useWorldSpace = true;

                    laserSightLeft.SetPosition(0, lazerSpawnLocationLeft.transform.position);
                    laserSightRight.SetPosition(0, lazerSpawnLocationRight.transform.position);
                    laserSightLeft.SetPosition(1, hit.transform.position);
                    laserSightRight.SetPosition(1, hit.transform.position);

                    AimTargetLocation = hit.transform.position;
                    AimTargetIndicator.position = AimTargetLocation;

                    if (currentTargetEnemy != hit.collider.gameObject)
                    {
                        currentTargetEnemy = hit.collider.gameObject;
                        StartCoroutine(CloseAimIndicator());
                    }

                    return;
                }
            }
        }
        currentTargetEnemy = null;
        AimTargetIndicatorGO.SetActive(false);

        laserSightLeft.useWorldSpace = false;
        laserSightRight.useWorldSpace = false;

        laserSightLeft.SetPosition(0, Vector3.zero);
        laserSightRight.SetPosition(0, Vector3.zero);
        laserSightLeft.SetPosition(1, Vector3.forward * lazerSightRange);
        laserSightRight.SetPosition(1, Vector3.forward * lazerSightRange);

        AimTargetLocation = Vector3.zero;
        return;
    }

    IEnumerator CloseAimIndicator()
    {
        AimTargetIndicatorGO.SetActive(true);
        AimTargetIndicator.localScale = Vector3.one;
        AimTargetIndicator.localRotation = Quaternion.Euler(0, 0, 0);

        int steps = 30;
        float durationOfAnimation = 0.5f;

        for (int i = 0; i < steps; i++)
        {
            yield return new WaitForSeconds(durationOfAnimation / steps);
            AimTargetIndicator.localScale = Vector3.one - (Vector3.one * i / (2 * steps)); // Results in Vector3.one/2
            AimTargetIndicator.localRotation = Quaternion.Euler(0, 0, 90 * i / steps);
        }
    }

    #endregion

    #region Handle Grenade

    private void HandleGrenadeThrow()
    {
        if (grenadeAvailable && GameManager.SharedInstance.hasAmmoBox)
        {
            if (inputHandler.inputSecondaryFire)
            {
                throwForce += throwForceBuildUpSpeed;
                currentThrowForce = Mathf.Min(maxthrowForce, throwForce);
                UpdateTrajectory();
            }
            else if (!inputHandler.inputSecondaryFire && throwForce > 0)
            {
                currentGrenadeCooldownValue = 0;
                lineRenderer.positionCount = 0;
                throwForce = 0;
                LaunchGrenade();
                grenadeAvailable = false;
            }
        }
    }

    private void HandleGrenadeCooldown(float delta)
    {
        SetGrenadeCooldownUI(currentGrenadeCooldownValue);
        if (currentGrenadeCooldownValue >= grenadeCooldownMax)
        {
            grenadeAvailable = true;
            currentGrenadeCooldownValue = grenadeCooldownMax;
            grenadeCooldownUIGO.SetActive(false);
            return;
        }

        if (currentGrenadeCooldownValue < grenadeCooldownMax)
        {
            grenadeAvailable = false;
            currentGrenadeCooldownValue += grenadeCooldownSpeed;
            currentGrenadeCooldownValue = Mathf.Min(grenadeCooldownMax, currentGrenadeCooldownValue);
            grenadeCooldownUIGO.SetActive(true);
        }
    }

    private void SetGrenadeCooldownUI(float currentChargeValue)
    {
        grenadeCooldownUI.fillAmount = currentChargeValue / grenadeCooldownMax;
    }

    private void CreatePhysicsScene()
    {
        string SimulationName = "Simulation" + inputHandler.playerIndex.ToString();
        simulateScene = SceneManager.CreateScene(SimulationName, new CreateSceneParameters(LocalPhysicsMode.Physics3D));
        physicsScene = simulateScene.GetPhysicsScene();
    }

    private void LaunchGrenade()
    {
        var spawned = Instantiate(grenadePrefab, grenadeSpawnLocation.position, grenadeSpawnLocation.rotation); //this is used to spawn grenede at spawnlocation with alloted throwForce
        spawned.Init(grenadeSpawnLocation.forward * currentThrowForce, false);
    }

    private void UpdateTrajectory()
    {

        if (lineRenderer.positionCount < PhysicsFrame)
        {
            lineRenderer.positionCount = PhysicsFrame;
        }

        GrenadeHandler grenadeTrajectory = Instantiate(grenadePrefab, grenadeSpawnLocation.position, Quaternion.identity); // this is used to simulate trejectory
        SceneManager.MoveGameObjectToScene(grenadeTrajectory.gameObject, simulateScene);

        grenadeTrajectory.Init(grenadeSpawnLocation.forward * currentThrowForce, true);

        for (var i = 0; i < PhysicsFrame; i++)
        {
            physicsScene.Simulate(Time.fixedDeltaTime);
            if (i < lineRenderer.positionCount)
            {
                lineRenderer.SetPosition(i, grenadeTrajectory.transform.position);
            }
        }

        Destroy(grenadeTrajectory.gameObject);
    }
    #endregion

    #region Handle Player / Alien interaction

    private void OnTriggerEnter(Collider other)
    {
        // Less resources on all the alien instances
        if (other.gameObject.CompareTag("Alien"))
        {
            AlienHandler AH = other.gameObject.GetComponent<AlienHandler>();
            if (AH.isDead) { return; }

            if (AH.currentAge == AlienAge.resource)
            {
                playerManager.HandleGainResource(AH.currentSpecies);
                AlienManager.SharedInstance.RemoveFromResourceList(AH);
                AH.gameObject.SetActive(false);
            }
            else
            {
                playerManager.HandleHit();
                AH.HandleDeath();
            }
        }
    }

    #endregion

    IEnumerator DisableAfterSeconds(int sec, GameObject objectToDeactivate)
    {
        yield return new WaitForSeconds(sec);
        objectToDeactivate.SetActive(false);
    }

}
