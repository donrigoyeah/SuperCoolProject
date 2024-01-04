using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DevKitHandler : MonoBehaviour
{
    [Header("Input Fields")]
    [SerializeField] private InputField playerSpeed;
    [SerializeField] private InputField alienHealth;
    [SerializeField] private InputField alienAmount;
    [SerializeField] private InputField alienAmountOfBabies;

    [Header("Canvas")]
    [SerializeField] private GameObject devKit;
    [SerializeField] private bool devKitOpen = true;

    private void Start()
    {
        Debug.Log("DevKit Ready");
    }

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

    public void PlayerSpeedInput()
    {
        int.TryParse(playerSpeed.text, out int input);
        foreach (var item in GameManager.SharedInstance.players
    )
        {
            PlayerLocomotion playerLocomotion = item.GetComponent<PlayerLocomotion>();
            playerLocomotion.playerSpeed = input;
        }
    }

    public void AlienHealthInput()
    {
        float.TryParse(alienHealth.text, out float input);
        for (int i = 0; i <= PoolManager.SharedInstance.AlienPool.Count; i++)
        {
            GameObject alien = PoolManager.SharedInstance.AlienPool[i];
            AlienHandler alienHandler = alien.GetComponent<AlienHandler>();
            alienHandler.alienHealth = input;
        }
    }

    public void AlienAmountInput()
    {
        int.TryParse(alienAmount.text, out int input);
        PoolManager.SharedInstance.alienAmount = input;
    }

    public void AlienAmountOfBabiesInput()
    {
        int.TryParse(alienAmountOfBabies.text, out int input);

        for (int i = 0; i <= PoolManager.SharedInstance.AlienPool.Count; i++)
        {
            GameObject alien = PoolManager.SharedInstance.AlienPool[i];
            AlienHandler alienHandler = alien.GetComponent<AlienHandler>();
            alienHandler.maxAmountOfBabies = input;
        }
    }
}
