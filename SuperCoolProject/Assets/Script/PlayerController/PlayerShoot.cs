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
    public Vector3 firePoint;

    private void Awake()
    {
        playerControls = new PlayerControls();
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
    }

    private void Shoot()
    {
        Rigidbody rb;
        GameObject bulletPoolGo = PoolManager.SharedInstance.GetPooledBullets();
        if (bulletPoolGo != null)
        {
            bulletPoolGo.SetActive(true);
            bulletPoolGo.transform.position = firePoint;
            rb = bulletPoolGo.GetComponent<Rigidbody>();
            rb.AddForce(transform.forward * bulletSpeed, ForceMode.Impulse);
        }
        overheatSlider.value += gunOverheatingSpeed;
    }

}

