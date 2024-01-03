using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasHandler : MonoBehaviour
{
    public Transform[] transforms;
    private void LateUpdate()
    {
        // Rotate the canvas at the end of player rotation etc.
        foreach (var item in transforms)
        {
            item.forward = Camera.main.transform.forward;
        }
    }
}
