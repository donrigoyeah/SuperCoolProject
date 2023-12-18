using System;
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
            SpaceShipPartHandler spaceShipPartHandler = PM.currentPart.GetComponent<SpaceShipPartHandler>();
            if (PM != null && PM.isCarryingPart)
            {
                GameManager.SharedInstance.currentSpaceShipParts++;
                PM.currentPart.SetActive(false);
                PM.currentPart = null;
                PM.isCarryingPart = false;

                //This is to check which spaceship part player is holding
                if (spaceShipPartHandler.spaceShipData.partName == "AmmoBox") { GameManager.SharedInstance.hasAmmoBox = true; }

                if (spaceShipPartHandler.spaceShipData.partName == "Antenna") { GameManager.SharedInstance.hasAntenna = true; }
                
                if (spaceShipPartHandler.spaceShipData.partName == "FuelCanister") { GameManager.SharedInstance.hasFuelCanister = true; }
                
                if (spaceShipPartHandler.spaceShipData.partName == "ShieldGenerator") { GameManager.SharedInstance.hasShieldGenerator = true; }
                
                // if (spaceShipPartHandler.spaceShipData.partName == "ShieldGenerator")  this is for the fifth space part
                // {
                //     GameManager.SharedInstance.hasAntenna = true;
                // }
                
                GameManager.SharedInstance.SpaceShipPartUpdate();
            }
        }
    }
}
