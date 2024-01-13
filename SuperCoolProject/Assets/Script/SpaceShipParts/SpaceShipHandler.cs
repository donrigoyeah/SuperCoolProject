using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class SpaceShipHandler : MonoBehaviour
{
    public ParticleSystem ParticleSystem;
    public TextMeshProUGUI displayAbility;

    private void OnTriggerEnter(Collider other)
    {
        
        //Check for player dragging the deadbody
        if (other.gameObject.CompareTag("DeadBody"))
        {
            Destroy(other.gameObject);
            GameManager.SharedInstance.playerDeadBody = true;
            GameManager.SharedInstance.currentCloneJuice += 10f;
            ParticleSystem objectCompleted = Instantiate(ParticleSystem, new Vector3(0, 0, 5), Quaternion.Euler(-90, 0, 0));
            Destroy(objectCompleted, 1.2f);
            Debug.Log("incrase juice");
            return;
        }
        
        if (other.gameObject.CompareTag("Player"))
        {

            PlayerManager PM = other.gameObject.GetComponent<PlayerManager>();
            if (PM.currentPart == null) return; // If come empty handed to spacehsip return

            SpaceShipPartHandler spaceShipPartHandler = PM.currentPart.GetComponent<SpaceShipPartHandler>();
            if (PM != null && PM.isCarryingPart)
            {
                Debug.Log("Added Particle System here upon object completion");
                PlayerLocomotion PL = PM.GetComponent<PlayerLocomotion>();
                PL.playerSpeed = spaceShipPartHandler.previousPlayerSpeed;
                ParticleSystem objectCompleted = Instantiate(ParticleSystem, new Vector3(0, 0, 5), Quaternion.Euler(-90, 0, 0));
                Destroy(objectCompleted, 1.2f);

                GameManager.SharedInstance.currentSpaceShipParts++;
                PM.currentPart.SetActive(false);
                PM.currentPart = null;
                PM.isCarryingPart = false;


                
                //This is to check which spaceship part player is holding
                if (spaceShipPartHandler.spaceShipData.partName == "AmmoBox") { GameManager.SharedInstance.hasAmmoBox = true; displayAbility.text = spaceShipPartHandler.spaceShipData.abilityUnlock; }

                if (spaceShipPartHandler.spaceShipData.partName == "Antenna") { GameManager.SharedInstance.hasAntenna = true; displayAbility.text = spaceShipPartHandler.spaceShipData.abilityUnlock; }

                if (spaceShipPartHandler.spaceShipData.partName == "FuelCanister") { GameManager.SharedInstance.hasFuelCanister = true; displayAbility.text = spaceShipPartHandler.spaceShipData.abilityUnlock; }

                if (spaceShipPartHandler.spaceShipData.partName == "ShieldGenerator") { GameManager.SharedInstance.hasShieldGenerator = true; displayAbility.text = spaceShipPartHandler.spaceShipData.abilityUnlock; }

                // if (spaceShipPartHandler.spaceShipData.partName == "ShieldGenerator")  this is for the fifth space part
                // {
                //     GameManager.SharedInstance.hasAntenna = true;
                // }
                StartCoroutine(EnableAndDisableDisplayAbility());

                GameManager.SharedInstance.SpaceShipPartUpdate();
            }
        }
    }
    
    IEnumerator EnableAndDisableDisplayAbility()
    {
        displayAbility.enabled = true;
        yield return new WaitForSeconds(10f);
        displayAbility.enabled = false;
    }
}
