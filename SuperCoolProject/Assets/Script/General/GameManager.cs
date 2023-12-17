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

    public bool hasFuelCanister = false;

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

            SpaceShipPartHandler dataAssign = Go.GetComponent<SpaceShipPartHandler>();

            if (spaceShipScriptable.Length > i)
            {
                 dataAssign.spaceShipData = spaceShipScriptable[i];

            }
        }
    }

    private void SpaceShipPartsCollection()
    {
        if (hasFuelCanister)
        {
            Debug.Log("worked");
            //twinStickMovement.playerSpeed = 13f;
        }
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
