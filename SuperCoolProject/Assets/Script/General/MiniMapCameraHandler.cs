using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniMapCameraHandler : MonoBehaviour
{

    public Transform PlayerTransform;
    public RectTransform MiniMapCrossHair;
    public static MiniMapCameraHandler Instance;

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(Instance);
        }
        else
        {
            Instance = this;
        }
    }


    private void FixedUpdate()
    {
        if(PlayerTransform == null) { return; }

        this.transform.position = new Vector3(PlayerTransform.position.x, this.transform.position.y, PlayerTransform.position.z);
        MiniMapCrossHair.position = new Vector3(PlayerTransform.position.x, MiniMapCrossHair.transform.position.y, PlayerTransform.position.z);
    }
}
