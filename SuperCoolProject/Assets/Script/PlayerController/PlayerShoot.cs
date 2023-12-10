using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerShoot : MonoBehaviour
{
    public GameObject bullet;
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
        Debug.Log("Hey");
        overheatSlider.value += gunOverheatingSpeed;
        GameObject shoot = Instantiate(bullet, new Vector3(transform.position.x, transform.position.y + 1, transform.position.z), Quaternion.identity);
        Rigidbody rb = shoot.GetComponent<Rigidbody>();
        rb.AddForce(transform.forward * bulletSpeed, ForceMode.Impulse);

        Destroy(shoot, 3f);
    }
    
}

