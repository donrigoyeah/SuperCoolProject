using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerShoot : MonoBehaviour
{
    [SerializeField] private Slider overheatSlider;
    [SerializeField] private float fireRate = 0.5f;
    private float nextFireTime = 0f;
    private float bulletSpeed = 50;
    [SerializeField] private float gunCooldownSpeed = 0.003f;
    private float gunOverheatingSpeed = 0.10f;

    private bool shootTrigger;
    private bool gunOverheated = false;
    private PlayerControls playerControls;
    [SerializeField] public Transform firePoint;
    private InputHandler inputHandler;
    private PlayerManager playerManager;

    private void Awake()
    {
        inputHandler = GetComponent<InputHandler>();
        playerManager = GetComponent<PlayerManager>();
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
            Debug.Log("gg");
            nextFireTime -= Time.deltaTime;
        }
        
        if (inputHandler.isShooting && nextFireTime <= 0f && !gunOverheated && !playerManager.isCarryingPart)
        {
            Shoot();
            nextFireTime = fireRate;
        }
        
        overheatSlider.fillRect.GetComponent<Image>().color = Color.Lerp(Color.green, Color.red, overheatSlider.value / 0.70f);
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

