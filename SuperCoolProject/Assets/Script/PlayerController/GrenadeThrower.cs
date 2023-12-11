using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class GrenadeThrower : MonoBehaviour
{
    [SerializeField] private GameObject grenadePrefab;
    [SerializeField] private Slider grenadeRechargeSlider;

    public bool grenadeCharged = false;
    private bool isCharging = false;
    private float chargeTime = 0f;
    private Camera mainCamera;

    [SerializeField] private float throwForce = 10f;
    [SerializeField] private float maxForce = 20f;

    [SerializeField] private Transform throwPosition;
    [SerializeField] private Vector3 throwDirection = new Vector3(0, 1, 0);

    [SerializeField] private LineRenderer trajectoryLine;
    
    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        grenadeRechargeSlider.value += 0.0010f;
        
        if (grenadeRechargeSlider.value >= 0.98f)
        {
            grenadeCharged = true;
            
            if (Input.GetKeyDown(KeyCode.Q) && grenadeCharged)
            {
                StartThrowing();
            }
            
            if (Input.GetKeyUp(KeyCode.Q))
            {
                ReleaseThrow();
            }
        }
        


        if (isCharging)
        {
            ChargeThrow();
        }


    }

    void StartThrowing()
    {
        isCharging = true;
        chargeTime = 0f;

        trajectoryLine.enabled = true;
    }

    void ChargeThrow()
    {
        chargeTime += Time.deltaTime;

        Vector3 grenadeVelocity = (mainCamera.transform.forward + throwDirection).normalized * Mathf.Min(chargeTime * throwForce, maxForce);
        ShowTrajectory(throwPosition.position + throwPosition.forward, grenadeVelocity);
    }

    void ReleaseThrow()
    {
        ThrowGrenade(Mathf.Min(chargeTime * throwForce, maxForce));
        isCharging = false;

        trajectoryLine.enabled = false;
        
        grenadeCharged = false;
        grenadeRechargeSlider.value = 0f;
    }

    void ThrowGrenade(float force)
    {
        Vector3 spawnPosition = throwPosition.position + mainCamera.transform.forward;

        GameObject grenade = Instantiate(grenadePrefab, spawnPosition, mainCamera.transform.rotation);

        Rigidbody rb = grenade.GetComponent<Rigidbody>();

        Vector3[] points = new Vector3[100];
        Vector3 playerForward = transform.forward;

        for (int i = 0; i < points.Length; i++)
        {
            float time = i * 0.1f;
            points[i] = spawnPosition + playerForward * force * time + 0.5f * Physics.gravity * time * time;
        }

        Vector3 throwDirection = (points[1] - points[0]).normalized;

        rb.AddForce(throwDirection * force, ForceMode.VelocityChange);
    }

    void ShowTrajectory(Vector3 origin, Vector3 speed)
    {
        Vector3[] points = new Vector3[100];
        trajectoryLine.positionCount = points.Length;

        Vector3 playerForward = transform.forward; 

        for (int i = 0; i < points.Length; i++)
        {
            float time = i * 0.1f;

            points[i] = origin + playerForward * speed.magnitude * time + 0.5f * Physics.gravity * time * time;
        }

        trajectoryLine.SetPositions(points);
    }
    
}
