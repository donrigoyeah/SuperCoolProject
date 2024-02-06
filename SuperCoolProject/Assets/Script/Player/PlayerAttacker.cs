using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using static AlienHandler;

public class PlayerAttacker : MonoBehaviour
{
    private Transform myTransform;

    [Header("Lazer Sight Stuff")]
    public LineRenderer laserSightLeft;
    public LineRenderer laserSightRight;
    public bool isLaserSight = true;
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
    public Vector2 lastInput;
    public Vector3 AimTargetLocation;
    private int stepsEnableLazer;
    private float durationOfAnimationEnableLazer;
    private int stepsDisableLazer;
    private float durationOfAnimationDisableLazer;
    private int stepsCloseAim;
    private float durationOfAnimationCloseAim;
    private RaycastHit hit;

    [Header("Lazer Gun Stuff")]
    public GameObject overheatUIGO;
    public Image overheatUI;
    private Vector3 overheatUIScaling = Vector3.one * 0.2f;
    public float fireRate = 0.5f;
    public float currentWeaponHeat = 0;
    public float maxWeaponHeat = 100;
    public float boostWeaponHeatThreshold = 70;
    public float singleLazerHeat = 10;
    public float gunCooldownSpeed = 0.003f;
    public float gunOverHeatCooldownSpeed = 0.003f;
    public bool gunOverheated = false;
    public float nextFireTime = 0f;
    public float bulletSpeed = 50;
    public float bulletDamage = 10;
    public float bulletDamageBoost = 2;
    public bool leftRightSwitch;
    private GameObject bulletPoolGo;
    private GameObject muzzlePoolGo;
    private BulletHandler BH;

    [Header("Grenade stuff")]
    public GameObject grenadeCooldownUIGO;
    public Image grenadeCooldownUI;
    public LineRenderer grenadeLineRenderer;
    public GameObject grenadePrefab;
    public float currentGrenadeCooldownValue = 0f;
    public float grenadeCooldownSpeed = 1;
    public float grenadeCooldownMax = 10;
    public bool grenadeAvailable;
    public bool grenadeKeyPressed = false;
    public Transform grenadespawnPoint;
    public Transform target;
    public Transform arcHeight;
    public float trajectorySpeed = 2;
    public Vector3 ac;
    public Vector3 cb;
    public Vector3 cachedResult;
    public bool isCachedGrenadeTrajectoryResultValid = false;
    public GameObject grenadeTrajectoryParent;
    public Sprite sprite;
    public float vertecCount = 12;
    private GameObject grenadePoolGo;
    private GrenadeHandler currentGH;
    private Vector3 pos1;
    private Vector3 post2;
    private Vector3 pos3;


    [Header("References")]
    private InputHandler inputHandler;
    private PlayerManager playerManager;
    private Animator playerAnim;
    private CharacterController controller;
    private float delta;
    private AlienHandler CurrentCollidingAH;

    [Header("Audio")]
    public AudioClip coolingDownAudio;
    public AudioClip gunReadyAudio;
    public AudioClip gunOverheatedAudio;
    public AudioClip collectingResource;
    public List<AudioClip> shootLazer;
    public AudioClip canNotShootLazer;
    public AudioClip shootGrenade;
    private bool hasOverheatedOnce = false;
    private AudioSource audioSource;

    private void Awake()
    {
        laserSightLeft.enabled = false;
        laserSightRight.enabled = false;
        AimTargetIndicatorGO.SetActive(false);
        currentGrenadeCooldownValue = grenadeCooldownMax;
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

        HandleShootLazer();
        HandleGrenadeThrow();
    }

    private void FixedUpdate()
    {
        if (playerManager.isInteracting == true || playerManager.isAlive == false) { return; }

        delta = Time.deltaTime;
        HandleWeaponHeat(delta);
        HandleGrenadeCooldown(delta);
        //HandleEnableLazerSight();
        if (playerManager.canAim || GameManager.Instance.devMode)
        {
            AimLockTarget();
        }
    }

    #region Handle Lazer

    private void HandleShootLazer()
    {
        if (gunOverheated == false && nextFireTime > fireRate)
        {
            overheatUI.color = Color.red;

            if (inputHandler.inputPrimaryFire && !playerManager.isCarryingPart && !PauseMenu.Instance.isPaused)
            {
                playerAnim.SetBool("IsShooting", true);

                if (currentWeaponHeat > boostWeaponHeatThreshold)
                {
                    SpawnLazer(bulletDamage + bulletDamageBoost * currentWeaponHeat / boostWeaponHeatThreshold);
                }
                else
                {
                    SpawnLazer(bulletDamage);
                }

                CameraShake.Instance.ShakeCamera(0);
                currentWeaponHeat += singleLazerHeat;
                nextFireTime = 0;
            }
            else
            {
                playerAnim.SetBool("IsShooting", false);
                CameraShake.Instance.ResetCameraPosition();
            }
        }
        if (inputHandler.inputPrimaryFire && gunOverheated == true)
        {
            audioSource.PlayOneShot(canNotShootLazer);
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
            CameraShake.Instance.ResetCameraPosition();
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
        bulletPoolGo = PoolManager.Instance.GetPooledBullets();
        muzzlePoolGo = PoolManager.Instance.GetPooledMuzzle();

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

            BH = bulletPoolGo.GetComponent<BulletHandler>();
            BH.shootinPlayerAttacker = this;
            BH.isPlayerBullet = true;
            BH.bulletDamage = damage;
            bulletPoolGo.SetActive(true);
            BH.rb.velocity = Vector3.zero;
            BH.rb.velocity = bulletPoolGo.transform.forward * bulletSpeed;

            audioSource.PlayOneShot(RandomAudioSelector(shootLazer), 1f);

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

        stepsEnableLazer = 10;
        durationOfAnimationEnableLazer = 0.1f;
        for (int i = 0; i < stepsEnableLazer; i++)
        {
            yield return new WaitForSeconds(durationOfAnimationEnableLazer / stepsEnableLazer);

            laserSightLeft.SetPosition(1, (Vector3.forward * lazerSightRange * i) / stepsEnableLazer);
            laserSightRight.SetPosition(1, (Vector3.forward * lazerSightRange * i) / stepsEnableLazer);
        }


    }

    IEnumerator DisableLazers()
    {
        laserSightLeft.SetPosition(0, Vector3.zero);
        laserSightRight.SetPosition(0, Vector3.zero);
        laserSightLeft.SetPosition(1, (Vector3.forward * lazerSightRange));
        laserSightRight.SetPosition(1, (Vector3.forward * lazerSightRange));

        stepsDisableLazer = 10;
        durationOfAnimationDisableLazer = 0.1f;

        for (int i = 0; i < stepsDisableLazer; i++)
        {
            yield return new WaitForSeconds(durationOfAnimationDisableLazer / stepsDisableLazer);
            laserSightLeft.SetPosition(0, (Vector3.forward * lazerSightRange * i) / stepsDisableLazer);
            laserSightRight.SetPosition(0, (Vector3.forward * lazerSightRange * i) / stepsDisableLazer);

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

        stepsCloseAim = 30;
        durationOfAnimationCloseAim = 0.5f;

        for (int i = 0; i < stepsCloseAim; i++)
        {
            yield return new WaitForSeconds(durationOfAnimationCloseAim / stepsCloseAim);
            AimTargetIndicator.localScale = Vector3.one - (Vector3.one * i / (2 * stepsCloseAim)); // Results in Vector3.one/2
            AimTargetIndicator.localRotation = Quaternion.Euler(0, 0, 180 * i / stepsCloseAim);
        }
    }

    #endregion

    #region Handle Grenade

    private void DrawTrajectory()
    {
        Debug.Log("I am sorry if you made any changes to this function I dont remember so I redid it");
        grenadeLineRenderer.enabled = true;
        var pointList = new List<Vector3>();

        for (float t = 0; t <= 1; t += 1 / vertecCount)
        {
            Vector3 point = Evaluate(t);
            pointList.Add(point);
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
            if (inputHandler.inputSecondaryFire && grenadeAvailable)
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
        grenadePoolGo = PoolManager.Instance.GetPooledGrenade();
        if (grenadePoolGo != null && grenadeAvailable)
        {
            grenadeAvailable = false;
            grenadePoolGo.transform.position = grenadespawnPoint.transform.position;
            grenadePoolGo.transform.rotation = grenadespawnPoint.transform.rotation;
            currentGH = grenadePoolGo.GetComponent<GrenadeHandler>();
            currentGH.time = 0;
            currentGH.playerAttacker = this;
            grenadePoolGo.SetActive(true);
            StartCoroutine(DisableAfterSeconds(1, grenadePoolGo));
            // GrenadeHandler currentGH = NewGrenade.GetComponent<GrenadeHandler>();
            // currentGH.playerAttacker = this;
        }
        audioSource.PlayOneShot(shootGrenade);
    }

    #endregion

    #region Handle Player / Alien interaction

    private void OnTriggerEnter(Collider other)
    {
        // Less resources on all the alien instances
        if (other.gameObject.CompareTag("Alien"))
        {

            CurrentCollidingAH = other.gameObject.GetComponent<AlienHandler>();
            if (CurrentCollidingAH.isDead) { return; }

            if (CurrentCollidingAH.currentAge == AlienAge.resource)
            {
                audioSource.PlayOneShot(collectingResource, 1f);

                playerManager.HandleGainResource(CurrentCollidingAH.currentSpecies);
                AlienManager.Instance.RemoveFromResourceList(CurrentCollidingAH);
                CurrentCollidingAH.HandleDeath();
            }
            else
            {
                if (CurrentCollidingAH.currentState == AlienHandler.AlienState.hunting)
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

        grenadeTrajectoryParent.transform.SetParent(transform);

        grenadeTrajectoryParent.transform.localRotation = Quaternion.identity;
        grenadeTrajectoryParent.transform.localPosition = Vector3.zero;

        target.localPosition = new Vector3(0, 2, 0);
        target.localRotation = Quaternion.Euler(0, -90, 0);
    }

    AudioClip RandomAudioSelector(List<AudioClip> audioList) // incase we plan to add more audio for each state
    {
        int randomIndex = Random.Range(0, audioList.Count);
        AudioClip selectedAudio = audioList[randomIndex];

        return selectedAudio;
    }
}
