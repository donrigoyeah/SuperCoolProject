using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    public GameObject bullet;
    public float fireRate = 0.5f;
    public float nextFireTime = 0f;
    public float bulletSpeed = 150;
    public bool shootTrigger;
    private bool isShooting = false;
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
        shootTrigger = playerControls.PlayerActionMap.Shoot.triggered;

        if (nextFireTime >= 0f)
        {
            nextFireTime -= Time.deltaTime;
        }
        
        if (isShooting && nextFireTime <= 0f)
        {
            Shoot();
            nextFireTime = fireRate;
            isShooting = false;
        }
    }

    private void Shoot()
    {
        Debug.Log("Hey");
        GameObject shoot = Instantiate(bullet, new Vector3(transform.position.x, transform.position.y + 1, transform.position.z), Quaternion.identity);
        Rigidbody rb = shoot.GetComponent<Rigidbody>();
        rb.AddForce(transform.forward * bulletSpeed, ForceMode.Impulse);

        Destroy(shoot, 3f);
    }
    
}

