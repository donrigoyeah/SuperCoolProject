using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Serialization;

public class GameManager : MonoBehaviour
{
    public static GameManager SharedInstance;

    [Header("World")]
    public int worldRadius = 98;

    [Header("Spaceship")]
    public GameObject SpaceShip;

    [Header("SpaceshipParts")]
    public GameObject SpaceShipPart;
    public Transform SpaceShipPartContainer;
    public int totalSpaceShipParts = 5;
    public int currentSpaceShipParts;
    public TextMeshProUGUI spaceShipPartsDisplay;
    [SerializeField] private SpaceShipScriptable[] spaceShipScriptable;
    
    [Header("SpaceShipPartsBoolValues")]
    public bool hasFuelCanister = false;
    public bool hasAmmoBox = false;
    public bool hasShieldGenerator = false;
    public bool hasAntenna = false;

    [Header("KillCounter")] 
    public int sphereKilled = 0;
    public int squareKilled = 0;
    public int triangleKilled = 0;
    
    [SerializeField] private PlayerLocomotion playerLocomotion;
    [SerializeField] private GameObject map;
    private void Awake()
    {
        SharedInstance = this;
        currentSpaceShipParts = 0;
        HandleSpawnShipParts();
    }

    private void FixedUpdate()
    {
        spaceShipPartsDisplay.text = currentSpaceShipParts.ToString() + "/" + totalSpaceShipParts.ToString();
        HandleWin();
    }


    private void HandleSpawnShipParts()
    {
        float radius = 0;
        float angle = 0;
        for (int i = 0; i < totalSpaceShipParts; i++)
        {
            // TODO: Increase Radius with each part so some are close, while others are far
            radius = Random.Range(40, 65);
            //angle = Random.Range(0, 360);

            float randPosX = radius * Mathf.Cos(angle);
            float randPosZ = radius * Mathf.Sin(angle);

            angle += 360 / totalSpaceShipParts;
            GameObject Go = Instantiate(SpaceShipPart, SpaceShipPartContainer);
            Go.transform.position = new Vector3(randPosX, 0, randPosZ);

            SpaceShipPartHandler DataAssign = Go.GetComponent<SpaceShipPartHandler>();

            if (spaceShipScriptable.Length > i)
            {
                 DataAssign.spaceShipData = spaceShipScriptable[i];
            }
        }
    }

    //Space Ships parts are collected and abilities are unlocked here
    public void SpaceShipPartUpdate()
    {
        if (hasFuelCanister) { Debug.Log("worked"); playerLocomotion.playerSpeed = 13f; }

        if (hasAntenna) { Debug.Log("Found Antenna"); map.SetActive(true);}
    }

    private void HandleWin()
    {
        if (currentSpaceShipParts == totalSpaceShipParts)
        {
            Debug.Log("Player won");
        }
    }

    // TODO: Handle stuff like day/night cycle here
    // Handle spaceship parts collected
    // Handle gametime
}