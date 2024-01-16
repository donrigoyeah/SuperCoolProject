using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SpaceShipGameAnimation : MonoBehaviour
{
    public int animationSteps = 100;
    public float animationDuration = 3f;

    Vector3 endPosition = new Vector3(0, 0, 15);
    Vector3 startPosition = new Vector3(-260, 130, 15);

    public GameObject SpaceShipCanvas;
    public GameObject DamageParticlesGO;
    ParticleSystem DamageParticles;
    private ParticleSystem.MainModule DamageParticlesMain;

    public GameObject ExhaustParticlesGO;
    ParticleSystem ExhaustParticles;
    private ParticleSystem.MainModule ExhaustParticlesMain;

    public GameObject LandingParticlesGO;

    public PlayerInputManager playerInputManager;
    public GameObject TutorialGameObject;



    private void Awake()
    {
        this.transform.position = startPosition;

        DamageParticles = DamageParticlesGO.GetComponent<ParticleSystem>();
        DamageParticlesMain = DamageParticles.main;

        ExhaustParticles = ExhaustParticlesGO.GetComponent<ParticleSystem>();
        ExhaustParticlesMain = ExhaustParticles.main;

        SpaceShipCanvas.SetActive(false);
        playerInputManager.DisableJoining();
        StartCoroutine(CrashAnimation(animationDuration));
    }


    IEnumerator CrashAnimation(float seconds)
    {
        float elapsedTime = 0;
        Vector3 startingPos = this.transform.position;
        while (elapsedTime < seconds)
        {
            this.transform.position = Vector3.Lerp(startingPos, endPosition, (elapsedTime / seconds));
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        // TODO: Add cameraShake
        this.transform.position = endPosition;
        LandingParticlesGO.SetActive(true);
        Destroy(LandingParticlesGO, 2);

        SpaceShipCanvas.SetActive(true);
        playerInputManager.EnableJoining();

        DamageParticlesGO.transform.rotation = Quaternion.Euler(-90, 90, 90);
        DamageParticlesMain.startSpeed = 0.5f;
        DamageParticlesMain.startLifetime = 6f;

        ExhaustParticlesGO.transform.rotation = Quaternion.Euler(-90, 90, 90);
        ExhaustParticlesMain.startSpeed = 0.5f;
        ExhaustParticlesMain.startLifetime = 6f;

        Debug.Log("Hide Tutorial here as well");
        //TutorialGameObject.SetActive(true);
    }
}
