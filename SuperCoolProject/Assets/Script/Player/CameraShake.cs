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
    public float shakeIntensity = 0.1f;


    private void Start()
    {
        CinemachineVirtualCamera = GetComponent<CinemachineVirtualCamera>();
        cbmcp = CinemachineVirtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        ResetCameraPosition();
    }

    //Shake Camera whenver the player shoots laser
    public void ShakeCamera()
    {
        cbmcp.m_AmplitudeGain = shakeIntensity;
    }

    public void ResetCameraPosition()
    {
        cbmcp.m_AmplitudeGain = 0f;
    }
}
