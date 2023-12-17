using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class PlayerAttacker : MonoBehaviour
{
    [Header("Lazer Gun Stuff")]
    [SerializeField] public Transform firePoint;
    [SerializeField] public Slider overheatSlider;
    [SerializeField] private float fireRate = 0.5f;
    [SerializeField] private float currentWeaponHeat = 0;
    [SerializeField] private float maxWeaponHeat = 100;
    [SerializeField] private float singleLazerHeat = 10;
    [SerializeField] private float gunCooldownSpeed = 0.003f;
    [SerializeField] private float gunOverHeatCooldownSpeed = 0.003f;
    private bool gunOverheated = false;
    private float nextFireTime = 0f;
    private float bulletSpeed = 50;

    [Header("Grenade stuff")]
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private Grenade grenadePrefab;
    [SerializeField] private Transform spawnLocation;
    [SerializeField] private float force;
    [SerializeField] private float forceBuildUpSpeed = 0.3f;
    [SerializeField] private float maxForce = 10f;
    [SerializeField] private float currentForce = 0f;

    [Header("Grenade Trajectory Physics stuff")]
    [SerializeField] private int PhysicsFrame = 62;
    private Scene simulateScene;
    private PhysicsScene physicsScene;

    [Header("References")]
    private InputHandler inputHandler;
    private PlayerManager playerManager;
    private Image overHeatUIImage;
    // TODO: Add recharge slider
    private Image grenadeRechargeUIImage;

    private void Awake()
    {
        inputHandler = GetComponent<InputHandler>();
        playerManager = GetComponent<PlayerManager>();
        overHeatUIImage = overheatSlider.fillRect.GetComponent<Image>();
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
    }

    #region Handle Lazer

    private void HandleShootLazer()
    {
        if (!gunOverheated && nextFireTime > fireRate)
        {
            if (inputHandler.inputPrimaryFire && !playerManager.isCarryingPart)
            {
                SpawnLazer();
                currentWeaponHeat += singleLazerHeat;
                nextFireTime = 0;
            }
        }

        overHeatUIImage.color = Color.Lerp(Color.green, Color.red, overheatSlider.value / 0.70f);
    }

    private void HandleWeaponHeat(float delta)
    {
        SetWepaonHeatSlider(currentWeaponHeat);
        nextFireTime += delta;
        if (currentWeaponHeat <= 0) { currentWeaponHeat = 0; return; }

        if (currentWeaponHeat > 0 && gunOverheated == false)
        {
            currentWeaponHeat -= gunCooldownSpeed;
        }
        else if (currentWeaponHeat >= maxWeaponHeat)
        {
            gunOverheated = true;
        }
        else if (gunOverheated == true && currentWeaponHeat > 0)
        {
            currentWeaponHeat -= gunCooldownSpeed + gunOverHeatCooldownSpeed;
        }
        else if (gunOverheated == true && currentWeaponHeat <= 0)
        {
            gunOverheated = false;
        }
    }

    private void SetWepaonHeatSlider(float value)
    {
        overheatSlider.value = value / maxWeaponHeat;
    }

    private void SpawnLazer()
    {
        Debug.Log("Shoot lazer");
        GameObject bulletPoolGo = PoolManager.SharedInstance.GetPooledBullets();
        if (bulletPoolGo != null)
        {
            bulletPoolGo.transform.position = firePoint.position;
            bulletPoolGo.transform.rotation = firePoint.rotation;

            bulletPoolGo.SetActive(true);


            Rigidbody rb = bulletPoolGo.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = firePoint.forward * bulletSpeed;
            }
        }
    }

    #endregion




    #region Handle Grenade

    private void HandleGrenadeThrow()
    {
        if (inputHandler.inputSecondaryFire)
        {
            force += forceBuildUpSpeed;
            currentForce = Mathf.Min(maxForce, force);
            UpdateTrajectory();
        }

        if (!inputHandler.inputSecondaryFire && force > 0)
        {
            LaunchGrenade();
            lineRenderer.positionCount = 0;
            force = 0;
        }

    }

    private void CreatePhysicsScene()
    {
        simulateScene = SceneManager.CreateScene("Simulation", new CreateSceneParameters(LocalPhysicsMode.Physics3D));
        physicsScene = simulateScene.GetPhysicsScene();
    }

    private void LaunchGrenade()
    {
        var spawned = Instantiate(grenadePrefab, spawnLocation.position, spawnLocation.rotation); //this is used to spawn grenede at spawnlocation with alloted force
        spawned.Init(spawnLocation.forward * currentForce, false);
    }

    private void UpdateTrajectory()
    {

        if (lineRenderer.positionCount < PhysicsFrame)
        {
            lineRenderer.positionCount = PhysicsFrame;
        }

        Grenade grenadeTrajectory = Instantiate(grenadePrefab, spawnLocation.position, Quaternion.identity); // this is used to simulate trejectory
        SceneManager.MoveGameObjectToScene(grenadeTrajectory.gameObject, simulateScene);

        grenadeTrajectory.Init(spawnLocation.forward * currentForce, true);

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

}
