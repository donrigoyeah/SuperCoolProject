using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using static AlienHandler;

public class PlayerAttacker : MonoBehaviour
{
    [Header("Lazer Sight Stuff")]
    [SerializeField] private LineRenderer laserSightLeft;
    [SerializeField] private LineRenderer laserSightRight;
    [SerializeField] private bool isLaserSight = true;
    [SerializeField] public Transform lazerSpawnLocationRight;
    [SerializeField] public Transform lazerSpawnLocationLeft;
    [SerializeField] public bool isEnabled = false;
    [SerializeField] public bool isDisabled = false;
    [SerializeField] public float lazerSightRange = 20;
    [SerializeField] public float lastTimeSinceLazer = 0;
    [SerializeField] public float disableLazerAfterNoInput = 1f;
    [SerializeField] public GameObject currentTargetEnemy;
    [SerializeField] public GameObject AimTargetIndicatorGO;
    [SerializeField] public RectTransform AimTargetIndicator;
    [SerializeField] private Vector2 lastInput;
    [SerializeField] private Vector3 AimTargetLocation;

    [Header("Lazer Gun Stuff")]
    [SerializeField] public GameObject overheatUIGO;
    [SerializeField] public Image overheatUI;
    private Vector3 overheatUIScaling = Vector3.one * 0.05f;
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
    private Transform myTransform;

    [Header("Grenade stuff")]
    [SerializeField] public GameObject grenadeCooldownUIGO;
    [SerializeField] public Image grenadeCooldownUI;
    [SerializeField] private LineRenderer grenadeLineRenderer;
    [SerializeField] private GameObject grenadePrefab;
    [SerializeField] private float currentGrenadeCooldownValue = 0f;
    [SerializeField] private float grenadeCooldownSpeed = 1;
    [SerializeField] private float grenadeCooldownMax = 10;
    [SerializeField] private bool grenadeAvailable;
    public bool grenadeKeyPressed = false;
    public Transform grenadespawnPoint;
    public Transform target;
    public Transform arcHeight;
    public Vector3 ac;
    public Vector3 cb;
    public Vector3 cachedResult;
    public bool isCachedGrenadeTrajectoryResultValid = false;
    public GameObject grenadeTrajectoryParent;
    public Sprite sprite;
    public float vertecCount = 12;

    [Header("Grenade Trajectory Physics stuff")]
    [SerializeField] private int PhysicsFrame = 62;
    private Scene simulateScene;
    private PhysicsScene physicsScene;

    [Header("References")]
    private InputHandler inputHandler;
    private PlayerManager playerManager;
    private Animator playerAnim;
    private CharacterController controller;



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
        laserSightLeft.enabled = false;
        laserSightRight.enabled = false;
        AimTargetIndicatorGO.SetActive(false);

        audioSource = GetComponent<AudioSource>();
        inputHandler = GetComponent<InputHandler>();
        playerManager = GetComponent<PlayerManager>();
        playerAnim = GetComponentInChildren<Animator>();
        controller = GetComponent<CharacterController>();

        myTransform = this.transform;
    }

    void Update()
    {


        if (playerManager.isInteracting == true || playerManager.isAlive == false) { return; }
        else
        {
            HandleShootLazer();
            HandleGrenadeThrow();
        }
    }

    private void FixedUpdate()
    {
        if (playerManager.isInteracting == true || playerManager.isAlive == false) { return; }
        else
        {
            float delta = Time.deltaTime;
            HandleWeaponHeat(delta);
            HandleGrenadeCooldown(delta);
            //HandleEnableLazerSight();
            if (playerManager.canAim || GameManager.Instance.devMode)
            {
                AimLockTarget();
            }
        }
    }

    #region Handle Lazer

    private void HandleShootLazer()
    {
        if (gunOverheated == false && nextFireTime > fireRate)
        {
            //overheatUI.color = Color.Lerp(Color.green, Color.red, overheatUI.fillAmount / 0.70f);
            overheatUI.color = Color.red;
            if (inputHandler.inputPrimaryFire && !playerManager.isCarryingPart && !PauseMenu.Instance.isPaused)
            {
                playerAnim.SetBool("IsShooting", true);

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
                return;
            }
            else
            {
                playerAnim.SetBool("IsShooting", false);
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
            overheatUI.color = Color.white;
            gunOverheated = true;
            isLaserSight = false;
            if (!audioSource.isPlaying)
            {
                audioSource.PlayOneShot(gunOverheatedAudio, 1f);
                audioSource.PlayOneShot(coolingDownAudio, 1f);
            }
            //CameraShake.ResetCameraPosition();
            hasOverheatedOnce = true;
        }

        if (currentWeaponHeat == 0 && hasOverheatedOnce == true)
        {
            if (!audioSource.isPlaying && hasOverheatedOnce)
            {
                audioSource.PlayOneShot(gunReadyAudio, 1f);
            }
            hasOverheatedOnce = false;
            overheatUI.color = Color.red;
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
            overheatUIGO.transform.localScale = overheatUIScaling * currentWeaponHeat / maxWeaponHeat;
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

        GameObject bulletPoolGo = PoolManager.Instance.GetPooledBullets();
        GameObject muzzlePoolGo = PoolManager.Instance.GetPooledMuzzle();

        if (bulletPoolGo != null)
        {
            if (leftRightSwitch == true)
            {
                playerAnim.SetBool("IsLeftShooting", true);

                // Instantiate Bullet left
                bulletPoolGo.transform.position = lazerSpawnLocationLeft.position;
                bulletPoolGo.transform.rotation = myTransform.rotation;
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
                playerAnim.SetBool("IsLeftShooting", false);
                // Instantiate Bullet right
                bulletPoolGo.transform.position = lazerSpawnLocationRight.position;
                bulletPoolGo.transform.rotation = myTransform.rotation;
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

            // Check if the player is shooting
            bool isShooting = inputHandler.inputPrimaryFire && !playerManager.isCarryingPart && !PauseMenu.Instance.isPaused;

            if (isShooting)
            {
                lastTimeSinceLazer = 0;
            }
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
                //LaserSight(); // now AimLockTarget();
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

        int steps = 10;
        float durationOfAnimation = 0.1f;
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
        laserSightLeft.SetPosition(1, (Vector3.forward * lazerSightRange));
        laserSightRight.SetPosition(1, (Vector3.forward * lazerSightRange));

        int steps = 10;
        float durationOfAnimation = 0.1f;

        for (int i = 0; i < steps; i++)
        {
            yield return new WaitForSeconds(durationOfAnimation / steps);
            laserSightLeft.SetPosition(0, (Vector3.forward * lazerSightRange * i) / steps);
            laserSightRight.SetPosition(0, (Vector3.forward * lazerSightRange * i) / steps);

            // Remove Laser from end to start
            //laserSightLeft.SetPosition(1, (Vector3.forward * lazerSightRange) - (Vector3.forward * lazerSightRange * i / steps));
            //laserSightRight.SetPosition(1, (Vector3.forward * lazerSightRange) - (Vector3.forward * lazerSightRange * i / steps));
        }
        laserSightLeft.SetPosition(1, Vector3.zero);
        laserSightRight.SetPosition(1, Vector3.zero);

        laserSightLeft.enabled = false;
        laserSightRight.enabled = false;
    }

    private void AimLockTarget()
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
                    laserSightLeft.enabled = true;
                    laserSightRight.enabled = true;
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
        laserSightLeft.enabled = false;
        laserSightRight.enabled = false;

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
            AimTargetIndicator.localRotation = Quaternion.Euler(0, 0, 180 * i / steps);
        }
    }

    #endregion

    #region Handle Grenade

    private void DrawTrajectory()
    {
        grenadeLineRenderer.enabled = true;
        var pointList = new List<Vector3>();

        for (float ratio = 0; ratio <= 1; ratio += 1 / vertecCount)
        {
            var pos1 = Vector3.Lerp(grenadespawnPoint.position, arcHeight.position, ratio);
            var post2 = Vector3.Lerp(arcHeight.position, target.position, ratio);
            var pos3 = Vector3.Lerp(pos1, post2, ratio);

            pointList.Add(pos3);
        }

        grenadeLineRenderer.positionCount = pointList.Count;
        grenadeLineRenderer.SetPositions(pointList.ToArray());
    }

    public Vector3 Evaluate(float t)
    {
        ac = Vector3.Lerp(grenadespawnPoint.position, arcHeight.position, t);
        cb = Vector3.Lerp(arcHeight.position, target.position, t);
        cachedResult = Vector3.Lerp(ac, cb, t);

        return cachedResult;
    }

    private void HandleGrenadeThrow()
    {
        if (grenadeAvailable && GameManager.Instance.hasAmmoBox || GameManager.Instance.devMode)
        {
            if (inputHandler.inputSecondaryFire)
            {
                DrawTrajectory();
                grenadeKeyPressed = true;
                if (arcHeight.localPosition.z <= 10f)
                {
                    arcHeight.localPosition += new Vector3(0f, 0f, 0.2f * 0.5f);
                }

                if (target.localPosition.z + 0.2f <= 20f)
                {
                    target.localPosition += new Vector3(0f, 0f, 0.2f);
                }
            }
            else if (!inputHandler.inputSecondaryFire && grenadeAvailable && grenadeKeyPressed)
            {
                grenadeLineRenderer.enabled = false;
                currentGrenadeCooldownValue = 0;
                // lineRenderer.positionCount = 0;
                // throwForce = 0;
                grenadeTrajectoryParent.transform.SetParent(null);
                StartCoroutine(ResetGrenadeTransform());
                grenadeAvailable = false;
                grenadeKeyPressed = false;
                LaunchGrenade();
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

    private void LaunchGrenade()
    {
        // GameObject NewGrenade = Instantiate(grenadePrefab, grenadespawnPoint.transform.position, transform.rotation);
        GameObject grenadePoolGo = PoolManager.Instance.GetPooledGrenade();
        grenadePoolGo.SetActive(true);
        grenadePoolGo.transform.position = grenadespawnPoint.transform.position;
        grenadePoolGo.transform.rotation = grenadespawnPoint.transform.rotation;
        GrenadeHandler currentGH = grenadePoolGo.GetComponent<GrenadeHandler>();
        currentGH.time = 0;
        currentGH.playerAttacker = this;
        StartCoroutine(DisableAfterSeconds(2, grenadePoolGo));
        // GrenadeHandler currentGH = NewGrenade.GetComponent<GrenadeHandler>();
        // currentGH.playerAttacker = this;
    }

    #endregion

    #region Handle Player / Alien interaction

    private void OnTriggerEnter(Collider other)
    {
        // Less resources on all the alien instances
        if (other.gameObject.CompareTag("Alien"))
        {
            AlienHandler CurrentCollidingAH = other.gameObject.GetComponent<AlienHandler>();
            if (CurrentCollidingAH.isDead) { return; }

            if (CurrentCollidingAH.currentAge == AlienAge.resource)
            {
                playerManager.HandleGainResource(CurrentCollidingAH.currentSpecies);
                AlienManager.Instance.RemoveFromResourceList(CurrentCollidingAH);
                CurrentCollidingAH.HandleDeath();
            }
            else
            {
                if (CurrentCollidingAH.currentState == AlienState.hunting)
                {
                    playerManager.HandleHit();
                    if (other.gameObject.activeInHierarchy)
                    {
                        CurrentCollidingAH.HandleDeathByCombat();
                    }
                }
            }
        }
    }

    #endregion

    IEnumerator DisableAfterSeconds(int sec, GameObject objectToDeactivate)
    {
        yield return new WaitForSeconds(sec);
        objectToDeactivate.SetActive(false);
    }

    IEnumerator ResetGrenadeTransform()
    {
        yield return new WaitForSeconds(2f);
        Debug.Log("ff");

        grenadeTrajectoryParent.transform.SetParent(transform);

        grenadeTrajectoryParent.transform.localRotation = Quaternion.identity;
        grenadeTrajectoryParent.transform.localPosition = Vector3.zero;

        target.localPosition = new Vector3(0, 2, 0);
        target.localRotation = Quaternion.Euler(0, -90, 0);


    }
}
