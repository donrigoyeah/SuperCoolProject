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
            enteringAlien.currentState = AlienHandler.AlienState.evading;
            enteringAlien.HandleFleeing(this.gameObject); // this time its not an alienGO but the player
        }
    }
}
