using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.VFX;
using static UnityEngine.Rendering.DebugUI;
using Unity.VisualScripting;
using static AlienHandler;

public class PlayerAttacker : MonoBehaviour
{
    [Header("Lazer Gun Stuff")]
    [SerializeField] public Transform lazerSpawnLocation;
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
    [SerializeField] private LineRenderer laserSight;
    [SerializeField] private bool isLaserSight = true;

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

    [Header("Camera Shake")]
    [SerializeField] private CameraShake CameraShake;

    [Header("Audio")]
    private AudioSource audioSource;
    [SerializeField] private AudioClip coolingDownAudio;
    [SerializeField] private AudioClip gunReadyAudio;
    [SerializeField] private AudioClip gunOverheatedAudio;
    private bool hasOverheatedOnce = false;


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

        if (isLaserSight)
        {
            laserSight.enabled = true;
            LaserSight();
        }
        else
        {
            laserSight.enabled = false;
        }
    }

    private void FixedUpdate()
    {
        float delta = Time.deltaTime;
        HandleWeaponHeat(delta);
        HandleGrenadeCooldown(delta);
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

                CameraShake.ShakeCamera();
                currentWeaponHeat += singleLazerHeat;
                nextFireTime = 0;
            }
            else
            {
                CameraShake.ResetCameraPosition();
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
            CameraShake.ResetCameraPosition();
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
        GameObject bulletPoolGo = PoolManager.SharedInstance.GetPooledBullets();
        if (bulletPoolGo != null)
        {
            bulletPoolGo.transform.position = lazerSpawnLocation.position;
            bulletPoolGo.transform.rotation = lazerSpawnLocation.rotation;
            BulletHandler BH = bulletPoolGo.GetComponent<BulletHandler>();
            BH.bulletDamage = damage;
            bulletPoolGo.SetActive(true);
            Debug.Log("Moved to BulletHandler:");
            BH.rb.velocity = lazerSpawnLocation.forward * bulletSpeed;
            //Rigidbody rb = bulletPoolGo.GetComponent<Rigidbody>();
            //if (rb != null)
            //{
            //    rb.velocity = lazerSpawnLocation.forward * bulletSpeed;
            //}

        }
        GameObject muzzlePoolGo = PoolManager.SharedInstance.GetPooledMuzzle();
        if (muzzlePoolGo != null)
        {
            muzzlePoolGo.transform.position = lazerSpawnLocation.position;
            muzzlePoolGo.transform.rotation = lazerSpawnLocation.rotation;
            muzzlePoolGo.SetActive(true);
            StartCoroutine(DisableAfterSeconds(1, muzzlePoolGo));
        } 
    }

    void LaserSight()
    {
        // Debug.Log("Laser Sight");
        //TODO: Add Recoil
        
        RaycastHit hit;
        
        if(Physics.Raycast(transform.position, transform.forward, out hit))
        {
            if (hit.collider)
            {
                laserSight.SetPosition(1, new Vector3(0, 0, hit.distance));
            }
            else
            {
                laserSight.SetPosition(0, transform.position);
                laserSight.SetPosition(1, transform.position + transform.forward * 40);
            }
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
        simulateScene = SceneManager.CreateScene("Simulation", new CreateSceneParameters(LocalPhysicsMode.Physics3D));
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
        Debug.Log("Moved this to player so alien script get shortend");
        // Less resources on all the alien instances
        if (other.gameObject.CompareTag("Alien"))
        {
            AlienHandler AH = other.gameObject.GetComponent<AlienHandler>();
            if (AH.currentAge == AlienAge.resource)
            {
                playerManager.HandleGainResource(AH.currentSpecies);
            }
        }

        //if (hasInteractedWithPlayer == false)
        //{
        //    PlayerManager PM = other.gameObject.GetComponent<PlayerManager>();
        //    // Handle Gathering resource
        //    if (currentAge == AlienAge.resource)
        //    {
        //        // Check so script only runce once
        //        hasInteractedWithPlayer = true;
        //        PM.HandleGainResource(currentSpecies);
        //        this.gameObject.SetActive(false);
        //    }
        //    else
        //    {
        //        hasInteractedWithPlayer = true;
        //        PM.HandleHit();
        //        this.gameObject.SetActive(false);
        //    }
        //}

    }
    #endregion

    IEnumerator DisableAfterSeconds(int sec, GameObject objectToDeactivate)
    {
        yield return new WaitForSeconds(sec);
        objectToDeactivate.SetActive(false);
    }

}
