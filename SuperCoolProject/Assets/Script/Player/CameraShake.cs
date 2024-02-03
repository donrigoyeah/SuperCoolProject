using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    [Header("Camera Shake")]
    private CinemachineVirtualCamera CinemachineVirtualCamera;
    private CinemachineBasicMultiChannelPerlin cbmcp;
    public float shakeIntensity;


    public static CameraShake Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }
    private void Start()
    {
        CinemachineVirtualCamera = GetComponent<CinemachineVirtualCamera>();
        cbmcp = CinemachineVirtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        ResetCameraPosition();
    }

    //Shake Camera whenver the player shoots laser
    public void ShakeCamera(float intensity)
    {
        if (intensity == 0)
        {
            cbmcp.m_AmplitudeGain = shakeIntensity;
        }
        else
        {
            cbmcp.m_AmplitudeGain = intensity;
        }
    }

    public void ResetCameraPosition()
    {
        cbmcp.m_AmplitudeGain = 0f;
    }
}
