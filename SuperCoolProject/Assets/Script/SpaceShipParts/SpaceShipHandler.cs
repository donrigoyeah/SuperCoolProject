using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class SpaceShipHandler : MonoBehaviour
{
    public TextMeshProUGUI abilityText;
    public TextMeshProUGUI upgradeButtonBinding;
    public GameObject UpgradeInformationScreen;
    public GameObject NewAbilityWithKey;
    public Button okButton;
    public bool showNewUpgradeBinging = false;



    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            PlayerManager PM = other.gameObject.GetComponent<PlayerManager>();

            // Showing the current values of the resources
            StartCoroutine(PM.UnfoldResource(PM.ResourceUISphere, 50));
            StartCoroutine(PM.UnfoldResource(PM.ResourceUISquare, 25));
            StartCoroutine(PM.UnfoldResource(PM.ResourceUITriangle, 0));

            // PlayerManager PM = other.gameObject.GetComponent<PlayerManager>();
            if (PM.currentPart == null || PM.isCarryingPart == false) { return; } // If come empty handed to spacehsip return

            if (PM != null && PM.isCarryingPart)
            {
                StartCoroutine(PlayRetrieveParticle(true));

                SpaceShipPartHandler spaceShipPartHandler = PM.currentPart.GetComponent<SpaceShipPartHandler>();
                PlayerLocomotion PL = PM.GetComponent<PlayerLocomotion>();
                PL.playerSpeed = spaceShipPartHandler.previousPlayerSpeed;

                GameManager.Instance.currentSpaceShipParts++;
                PM.currentPart.SetActive(false);
                PM.currentPart = null;
                PM.isCarryingPart = false;

                //This is to check which spaceship part player is holding
                if (spaceShipPartHandler.spaceShipData.partName == "AmmoBox") // Unlocks Granades
                {
                    GameManager.Instance.hasAmmoBox = true;
                    abilityText.text = spaceShipPartHandler.spaceShipData.abilityUnlockText;
                    showNewUpgradeBinging = true;
                    upgradeButtonBinding.text = TutorialHandler.Instance.secondaryShootButton;
                }

                if (spaceShipPartHandler.spaceShipData.partName == "Antenna") // Unlocks 
                {
                    GameManager.Instance.hasAntenna = true;
                    abilityText.text = spaceShipPartHandler.spaceShipData.abilityUnlockText;
                    showNewUpgradeBinging = true;
                    upgradeButtonBinding.text = TutorialHandler.Instance.toggleNavButton;
                }

                if (spaceShipPartHandler.spaceShipData.partName == "FuelCanister")
                {
                    GameManager.Instance.hasFuelCanister = true;
                    abilityText.text = spaceShipPartHandler.spaceShipData.abilityUnlockText;
                }

                if (spaceShipPartHandler.spaceShipData.partName == "ShieldGenerator")
                {
                    GameManager.Instance.hasShieldGenerator = true;
                    abilityText.text = spaceShipPartHandler.spaceShipData.abilityUnlockText;
                }

                if (spaceShipPartHandler.spaceShipData.partName == "Unknown")
                {
                    GameManager.Instance.hasDashPart = true;
                    abilityText.text = spaceShipPartHandler.spaceShipData.abilityUnlockText;
                }

                StartCoroutine(ShowInfoPanel());

                GameManager.Instance.SpaceShipPartUpdate();
            }
        }



        //TODO:
        //- SpaceShipparts have ontriggerenter thing that displays "press interaction" to drag
        //    - Placemat antenna in front of spawn point



        //Check for player dragging the deadbody
        if (other.gameObject.CompareTag("DeadBody"))
        {
            Destroy(other.gameObject);
            GameManager.Instance.playerDeadBody = true;
            GameManager.Instance.currentCloneJuice += 10f;
            StartCoroutine(PlayRetrieveParticle(false));
            return;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerManager PM = other.gameObject.GetComponent<PlayerManager>();

        StartCoroutine(PM.FoldResource(PM.ResourceUISphere));
        StartCoroutine(PM.FoldResource(PM.ResourceUISquare));
        StartCoroutine(PM.FoldResource(PM.ResourceUITriangle));
    }

    IEnumerator ShowInfoPanel()
    {
        yield return new WaitForSeconds(1);
        UpgradeInformationScreen.SetActive(true);
        if (showNewUpgradeBinging == true)
        {
            NewAbilityWithKey.SetActive(true);
        }

        EventSystem.current.SetSelectedGameObject(okButton.gameObject);
    }

    IEnumerator PlayRetrieveParticle(bool isPart)
    {
        foreach (var player in GameManager.Instance.players)
        {

            if (isPart)
            {
                player.ParticleSystem1Main.startColor = new ParticleSystem.MinMaxGradient(Color.green, Color.yellow);
                player.ParticleSystem2Main.startColor = new ParticleSystem.MinMaxGradient(Color.green, Color.yellow);
                player.ParticleSystem3Main.startColor = new ParticleSystem.MinMaxGradient(Color.green, Color.yellow);
            }
            else
            {
                player.ParticleSystem1Main.startColor = new ParticleSystem.MinMaxGradient(Color.green, Color.magenta);
                player.ParticleSystem2Main.startColor = new ParticleSystem.MinMaxGradient(Color.green, Color.magenta);
                player.ParticleSystem3Main.startColor = new ParticleSystem.MinMaxGradient(Color.green, Color.magenta);
            }
            player.UpgradeParticles.SetActive(true);
        }

        yield return new WaitForSeconds(1.5f);

        foreach (var player in GameManager.Instance.players)
        {
            player.UpgradeParticles.SetActive(false);
        }
    }

    public void CloseUpgradeScreen()
    {
        showNewUpgradeBinging = false;
        UpgradeInformationScreen.SetActive(false);
        NewAbilityWithKey.SetActive(false);
    }
}
