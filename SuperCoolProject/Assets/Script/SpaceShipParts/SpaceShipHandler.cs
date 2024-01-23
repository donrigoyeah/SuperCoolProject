using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class SpaceShipHandler : MonoBehaviour
{
    public GameObject ParticleSystem;
    ParticleSystem.MainModule ParticleSystem1Main;
    ParticleSystem.MainModule ParticleSystem2Main;
    public TextMeshProUGUI abilityText;
    public GameObject UpgradeInformationScreen;
    public Button okButton;

    private void Start()
    {
        ParticleSystem[] particleSystems = ParticleSystem.GetComponentsInChildren<ParticleSystem>();

        // TODO: Risky call?
        ParticleSystem1Main = particleSystems[0].main;
        ParticleSystem2Main = particleSystems[1].main;
    }

    private void OnTriggerEnter(Collider other)
    {
        //Check for player dragging the deadbody
        if (other.gameObject.CompareTag("DeadBody"))
        {
            Destroy(other.gameObject);
            GameManager.Instance.playerDeadBody = true;
            GameManager.Instance.currentCloneJuice += 10f;
            StartCoroutine(PlayRetrieveParticle(false));
            return;
        }

        if (other.gameObject.CompareTag("Player"))
        {
            PlayerManager PM = other.gameObject.GetComponent<PlayerManager>();

            // Showing the current values of the resources
            StartCoroutine(PM.UnfoldResource(PM.ResourceUISphere, 50));
            StartCoroutine(PM.UnfoldResource(PM.ResourceUISquare, 25));
            StartCoroutine(PM.UnfoldResource(PM.ResourceUITriangle, 0));

            // PlayerManager PM = other.gameObject.GetComponent<PlayerManager>();
            if (PM.currentPart == null) return; // If come empty handed to spacehsip return

            SpaceShipPartHandler spaceShipPartHandler = PM.currentPart.GetComponent<SpaceShipPartHandler>();
            if (PM != null && PM.isCarryingPart)
            {
                StartCoroutine(PlayRetrieveParticle(true));
                PlayerLocomotion PL = PM.GetComponent<PlayerLocomotion>();
                PL.playerSpeed = spaceShipPartHandler.previousPlayerSpeed;
                ParticleSystem.SetActive(true);

                GameManager.Instance.currentSpaceShipParts++;
                PM.currentPart.SetActive(false);
                PM.currentPart = null;
                PM.isCarryingPart = false;

                //This is to check which spaceship part player is holding
                if (spaceShipPartHandler.spaceShipData.partName == "AmmoBox") // Unlocks Granades
                {
                    GameManager.Instance.hasAmmoBox = true;
                    abilityText.text = spaceShipPartHandler.spaceShipData.abilityUnlock;
                }

                if (spaceShipPartHandler.spaceShipData.partName == "Antenna") // Unlocks 
                {
                    GameManager.Instance.hasAntenna = true;
                    abilityText.text = spaceShipPartHandler.spaceShipData.abilityUnlock;
                }

                if (spaceShipPartHandler.spaceShipData.partName == "FuelCanister")
                {
                    GameManager.Instance.hasFuelCanister = true;
                    abilityText.text = spaceShipPartHandler.spaceShipData.abilityUnlock;
                }

                if (spaceShipPartHandler.spaceShipData.partName == "ShieldGenerator")
                {
                    GameManager.Instance.hasShieldGenerator = true;
                    abilityText.text = spaceShipPartHandler.spaceShipData.abilityUnlock;
                }

                if (spaceShipPartHandler.spaceShipData.partName == "Unknown")
                {
                    GameManager.Instance.hasDashPart = true;
                    abilityText.text = spaceShipPartHandler.spaceShipData.abilityUnlock;
                }

                UpgradeInformationScreen.SetActive(true);
                okButton.Select();

                GameManager.Instance.SpaceShipPartUpdate();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerManager PM = other.gameObject.GetComponent<PlayerManager>();

        StartCoroutine(PM.FoldResource(PM.ResourceUISphere));
        StartCoroutine(PM.FoldResource(PM.ResourceUISquare));
        StartCoroutine(PM.FoldResource(PM.ResourceUITriangle));
    }

    IEnumerator PlayRetrieveParticle(bool isPart)
    {
        if (isPart)
        {
            ParticleSystem1Main.startColor = new ParticleSystem.MinMaxGradient(Color.green, Color.yellow);
            ParticleSystem2Main.startColor = new ParticleSystem.MinMaxGradient(Color.green, Color.yellow);
        }
        else
        {
            ParticleSystem1Main.startColor = new ParticleSystem.MinMaxGradient(Color.green, Color.magenta);
            ParticleSystem2Main.startColor = new ParticleSystem.MinMaxGradient(Color.green, Color.magenta);
        }

        ParticleSystem.SetActive(true);
        yield return new WaitForSeconds(1.5f);
        ParticleSystem.SetActive(false);

    }
}
