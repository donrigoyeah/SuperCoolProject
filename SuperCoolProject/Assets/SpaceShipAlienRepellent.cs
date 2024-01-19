using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceShipAlienRepellent : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Alien"))
        {
            AlienHandler enteringAlien = other.gameObject.GetComponent<AlienHandler>();
            enteringAlien.HandleFleeing(this.gameObject, true); // this time its not an alienGO but the spaceship; true for isEvadingPlayer
        }
    }
}
