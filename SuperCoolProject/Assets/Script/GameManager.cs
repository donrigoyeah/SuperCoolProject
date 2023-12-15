using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager SharedInstance;

    [Header("World")]
    public int worldRadius = 98;

    [Header("Spaceship")]
    public GameObject SpaceShip;

    [Header("SpaceshipParts")]
    public GameObject SpaceShipPart;
    public int totalSpaceShipParts = 5;
    public int currentSpaceShipParts;


    private void Awake()
    {
        SharedInstance = this;
        currentSpaceShipParts = 0;
    }

    private void FixedUpdate()
    {

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
