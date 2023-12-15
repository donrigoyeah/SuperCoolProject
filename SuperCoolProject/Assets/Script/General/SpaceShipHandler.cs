using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceShipHandler : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            PlayerManager PM = other.gameObject.GetComponent<PlayerManager>();
            if (PM != null && PM.isCarryingPart)
            {
                GameManager.SharedInstance.currentSpaceShipParts++;
                other.gameObject.SetActive(false);
            }
        }
    }
}
