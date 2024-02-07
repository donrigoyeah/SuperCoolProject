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
    private PlayerManager PM;
    private SpaceShipPartHandler spaceShipPartHandler;
    private PlayerLocomotion PL;

    public Color collectPartColor1;
    public Color collectPartColor2;
    public Color collectCloneColor1;
    public Color collectCloneColor2;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            GameManager.Instance.canPlayerBeAdded = true;
            Debug.Log("Insert boolean for enabling joining, and respawning if mulitplayer and one person is dead");
            PM = other.gameObject.GetComponent<PlayerManager>();

            // Showing the current values of the resources
            StartCoroutine(PM.UnfoldResource(PM.ResourceUISphere, 50));
            StartCoroutine(PM.UnfoldResource(PM.ResourceUISquare, 25));
            StartCoroutine(PM.UnfoldResource(PM.ResourceUITriangle, 0));
            // PlayerManager PM = other.gameObject.GetComponent<PlayerManager>();
            if (PM.currentPart == null || PM.isCarryingPart == false) { return; } // If come empty handed to spacehsip return

            if (PM != null && PM.isCarryingPart)
            {
                StartCoroutine(PlayRetrieveParticle(true));

                spaceShipPartHandler = PM.currentPart.GetComponent<SpaceShipPartHandler>();
                PL = PM.GetComponent<PlayerLocomotion>();
                if (PL != null)
                {
                    PL.playerSpeed = spaceShipPartHandler.previousPlayerSpeed;
                }

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

                if (spaceShipPartHandler.spaceShipData.partName == "Lightmachine")
                {
                    GameManager.Instance.hasLightmachine = true;
                    abilityText.text = spaceShipPartHandler.spaceShipData.abilityUnlockText;
                }

                if (spaceShipPartHandler.spaceShipData.partName == "CloneJuicer")
                {
                    GameManager.Instance.hasCloneJuicer = true;
                    abilityText.text = spaceShipPartHandler.spaceShipData.abilityUnlockText;
                }

                if (spaceShipPartHandler.spaceShipData.partName == "Radar")
                {
                    GameManager.Instance.hasRadar = true;
                    abilityText.text = spaceShipPartHandler.spaceShipData.abilityUnlockText;
                    showNewUpgradeBinging = true;
                    upgradeButtonBinding.text = TutorialHandler.Instance.toggleNavButton;
                }

                if (spaceShipPartHandler.spaceShipData.partName == "AimAssist")
                {
                    GameManager.Instance.hasAimAssist = true;
                    abilityText.text = spaceShipPartHandler.spaceShipData.abilityUnlockText;
                }

                StartCoroutine(ShowInfoPanel());
                GameManager.Instance.SpaceShipPartUpdate();
            }
        }

        //Check for player dragging the deadbody
        if (other.gameObject.CompareTag("DeadBody"))
        {
            Destroy(other.gameObject);
            GameManager.Instance.playerDeadBody = true;
            GameManager.Instance.HandleGainCloneJuivce(10);
            StartCoroutine(PlayRetrieveParticle(false));
            return;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        GameManager.Instance.canPlayerBeAdded = false;

        if (other.gameObject.CompareTag("Player"))
        {
            PM = other.gameObject.GetComponent<PlayerManager>();

            StartCoroutine(PM.FoldResource(PM.ResourceUISphere));
            StartCoroutine(PM.FoldResource(PM.ResourceUISquare));
            StartCoroutine(PM.FoldResource(PM.ResourceUITriangle));
        }
    }

    IEnumerator ShowInfoPanel()
    {
        yield return new WaitForSeconds(1);
        Time.timeScale = 0;
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
                player.ParticleSystem1Main.startColor = new ParticleSystem.MinMaxGradient(collectPartColor1, collectPartColor2);
                player.ParticleSystem2Main.startColor = new ParticleSystem.MinMaxGradient(collectPartColor1, collectPartColor2);
                player.ParticleSystem3Main.startColor = new ParticleSystem.MinMaxGradient(collectPartColor1, collectPartColor2);
            }
            else
            {
                player.ParticleSystem1Main.startColor = new ParticleSystem.MinMaxGradient(collectCloneColor1, collectCloneColor2);
                player.ParticleSystem2Main.startColor = new ParticleSystem.MinMaxGradient(collectCloneColor1, collectCloneColor2);
                player.ParticleSystem3Main.startColor = new ParticleSystem.MinMaxGradient(collectCloneColor1, collectCloneColor2);
            }
            player.UpgradeParticles.SetActive(true);
        }

        yield return new WaitForSeconds(1.5f);

        foreach (var player in GameManager.Instance.players)
        {
            player.UpgradeParticles.SetActive(false);
        }
    }

    // Gets called on button in editor
    public void CloseUpgradeScreen()
    {
        showNewUpgradeBinging = false;
        UpgradeInformationScreen.SetActive(false);
        NewAbilityWithKey.SetActive(false);
        Time.timeScale = 1;
    }
}
