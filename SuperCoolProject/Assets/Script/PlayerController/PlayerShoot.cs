using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerShoot : MonoBehaviour
{
    [SerializeField] private Slider overheatSlider;
    public float fireRate = 0.5f;
    public float nextFireTime = 0f;
    public float bulletSpeed = 150;
    public float gunCooldownSpeed = 0.02f;
    public float gunOverheatingSpeed = 0.10f;

    private bool shootTrigger;
    private bool isShooting = false;
    private bool gunOverheated = false;
    private PlayerControls playerControls;
    private TwinStickMovement twinStickMovement;
    public Transform firePoint;

    private void Awake()
    {
        playerControls = new PlayerControls();
        twinStickMovement = GetComponent<TwinStickMovement>();
        playerControls.PlayerActionMap.Shoot.performed += ctx => isShooting = true;
        playerControls.PlayerActionMap.Shoot.canceled += ctx => isShooting = false;
    }

    private void OnEnable()
    {
        playerControls.Enable();
    }

    private void OnDisable()
    {
        playerControls.Disable();
    }

    void Update()
    {
        overheatSlider.value -= gunCooldownSpeed;
        if (overheatSlider.value >= 0.97f)
        {
            gunOverheated = true;
            overheatSlider.value -= gunCooldownSpeed + 0.003f;
        }
        else if (overheatSlider.value == 0f)
        {
            gunOverheated = false;
        }

        if (nextFireTime >= 0f)
        {
            nextFireTime -= Time.deltaTime;
        }

        if (isShooting && nextFireTime <= 0f && !gunOverheated)
        {
            Shoot();
            nextFireTime = fireRate;
            isShooting = false;
        }

        overheatSlider.gameObject.transform.Find("Fill Area").Find("Fill").GetComponent<Image>().color =
            Color.Lerp(Color.green, Color.red, overheatSlider.value / 0.70f);
    }

    private void Shoot()
    {
        Rigidbody rb;
        GameObject bulletPoolGo = PoolManager.SharedInstance.GetPooledBullets();
        if (bulletPoolGo != null)
        {
            bulletPoolGo.transform.position = firePoint.position;
            bulletPoolGo.transform.rotation = firePoint.rotation;

            bulletPoolGo.SetActive(true);

            rb = bulletPoolGo.GetComponent<Rigidbody>();
            rb.velocity = firePoint.forward * bulletSpeed;
        }
        overheatSlider.value += gunOverheatingSpeed;
    }

}

