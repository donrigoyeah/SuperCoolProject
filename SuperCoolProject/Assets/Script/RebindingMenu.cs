using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class RebindingMenu : MonoBehaviour
{
    public InputActionReference moveRef, jumpRef, fire1Ref, fireRef2, dashRef, interactRef;

    private void OnEnable()
    {
        moveRef.action.Disable();
        fire1Ref.action.Disable();
        fireRef2.action.Disable();
        dashRef.action.Disable();
        interactRef.action.Disable();

    }

    private void OnDisable()
    {
        moveRef.action.Enable();
        fire1Ref.action.Enable();
        fireRef2.action.Enable();
        dashRef.action.Enable();
        interactRef.action.Enable();
    }

}
