using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DevKitHandler : MonoBehaviour
{
    [Header("Input Fields")]
    public InputField playerSpeed;
    public InputField alienHealth;
    public InputField alienAmount;
    public InputField alienAmountOfBabies;

    [Header("Canvas")]
    public GameObject devKit;
    public bool devKitOpen = true;


    /*
    private void Update()
    {
     if (inputHandler.inputDevKit)
        {
        if (devKitOpen)
        {
            devKit.SetActive(true);
            StartCoroutine(DevKitOpener());    
        }

        if (devKitOpen == false)
        {
            devKit.SetActive(false);
            StartCoroutine(DevKitCloser());
        }
    }
    }
    */

    //IEnumerator is used so that it does not double trigger
    // private IEnumerator DevKitCloser()
    // {
    //     yield return new WaitForSeconds(0.5f);
    //     devKitOpen = true;
    // }
    //
    // private IEnumerator DevKitOpener()
    // {
    //     yield return new WaitForSeconds(0.5f);
    //     devKitOpen = false;
    // }
    //if (inputHandler.inputDevKit)
    //{
    //    if (devKitOpen)
    //    {
    //        devKit.SetActive(true);
    //        StartCoroutine(DevKitOpener());
    //    }

    //    if (devKitOpen == false)
    //    {
    //        devKit.SetActive(false);
    //        StartCoroutine(DevKitCloser());
    //    }
    //}
    //}

    //IEnumerator is used so that it does not double trigger
    private IEnumerator DevKitCloser()
    {
        yield return new WaitForSeconds(0.5f);
        devKitOpen = true;
    }

    private IEnumerator DevKitOpener()
    {
        yield return new WaitForSeconds(0.5f);
        devKitOpen = false;
    }

    public void ToggleDevMode()
    {
        GameManager.Instance.devMode = !GameManager.Instance.devMode;
    }

    public void PlayerSpeedInput()
    {
        int.TryParse(playerSpeed.text, out int input);
        foreach (var item in GameManager.Instance.players
    )
        {
            PlayerLocomotion playerLocomotion = item.GetComponent<PlayerLocomotion>();
            playerLocomotion.playerSpeed = input;
        }
    }

    public void AlienHealthInput()
    {
        float.TryParse(alienHealth.text, out float input);
        for (int i = 0; i <= PoolManager.Instance.AlienPool.Count; i++)
        {
            GameObject alien = PoolManager.Instance.AlienPool[i];
            AlienHandler alienHandler = alien.GetComponent<AlienHandler>();
            alienHandler.alienHealth = input;
        }
    }

    public void AlienAmountInput()
    {
        int.TryParse(alienAmount.text, out int input);
        PoolManager.Instance.alienAmount = input;
    }

    public void AlienAmountOfBabiesInput()
    {
        int.TryParse(alienAmountOfBabies.text, out int input);

        for (int i = 0; i <= PoolManager.Instance.AlienPool.Count; i++)
        {
            GameObject alien = PoolManager.Instance.AlienPool[i];
            AlienHandler alienHandler = alien.GetComponent<AlienHandler>();
            alienHandler.maxAmountOfBabies = input;
        }
    }
}
