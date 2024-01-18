using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class SpaceShipHandler : MonoBehaviour
{
    public ParticleSystem ParticleSystem;
    public TextMeshProUGUI abilityText;
    public GameObject UpgradeInformationScreen;
    public Button okButton;

    private void OnTriggerEnter(Collider other)
    {

        //Check for player dragging the deadbody
        if (other.gameObject.CompareTag("DeadBody"))
        {
            Destroy(other.gameObject);
            GameManager.Instance.playerDeadBody = true;
            GameManager.Instance.currentCloneJuice += 10f;
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

                GameManager.Instance.currentSpaceShipParts++;
                PM.currentPart.SetActive(false);
                PM.currentPart = null;
                PM.isCarryingPart = false;



                //This is to check which spaceship part player is holding
                if (spaceShipPartHandler.spaceShipData.partName == "AmmoBox") { GameManager.Instance.hasAmmoBox = true; abilityText.text = spaceShipPartHandler.spaceShipData.abilityUnlock; }

                if (spaceShipPartHandler.spaceShipData.partName == "Antenna") { GameManager.Instance.hasAntenna = true; abilityText.text = spaceShipPartHandler.spaceShipData.abilityUnlock; }

                if (spaceShipPartHandler.spaceShipData.partName == "FuelCanister") { GameManager.Instance.hasFuelCanister = true; abilityText.text = spaceShipPartHandler.spaceShipData.abilityUnlock; }

                if (spaceShipPartHandler.spaceShipData.partName == "ShieldGenerator") { GameManager.Instance.hasShieldGenerator = true; abilityText.text = spaceShipPartHandler.spaceShipData.abilityUnlock; }

                if (spaceShipPartHandler.spaceShipData.partName == "Unknown") { GameManager.Instance.hasDashPart = true; abilityText.text = spaceShipPartHandler.spaceShipData.abilityUnlock; }
                
                UpgradeInformationScreen.SetActive(true);
                okButton.Select();

                GameManager.Instance.SpaceShipPartUpdate();
            }
        }
    }
}
