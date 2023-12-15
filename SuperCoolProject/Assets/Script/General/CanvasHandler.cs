using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasHandler : MonoBehaviour
{
    private void LateUpdate()
    {
        // Rotate the canvas at the end of player rotation etc.
        transform.forward = Camera.main.transform.forward;
    }
}
