using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceShipHandler : MonoBehaviour
{
    public ParticleSystem ParticleSystem;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            PlayerManager PM = other.gameObject.GetComponent<PlayerManager>();
            if (PM.currentPart == null) return; // If come empty handed to spacehsip return

            SpaceShipPartHandler spaceShipPartHandler = PM.currentPart.GetComponent<SpaceShipPartHandler>();
            if (PM != null && PM.isCarryingPart)
            {
                // While I have you here, I was thinking maybe we can spawn 2-3 of the Particle system. That might look better - Delete the comment once you read it please
                Debug.Log("Added Particle System here upon object completion");
                ParticleSystem objectCompleted = Instantiate(ParticleSystem, new Vector3(0, 0, 5), Quaternion.Euler(-90, 0, 0));
                Destroy(objectCompleted, 1.2f);

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
