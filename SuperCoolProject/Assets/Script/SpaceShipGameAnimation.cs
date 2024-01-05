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
    [SerializeField] private PlayerInputManager playerInputManager;



    private void Awake()
    {
        this.transform.position = startPosition;
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
        this.transform.position = endPosition;
        SpaceShipCanvas.SetActive(true);
        playerInputManager.EnableJoining();
    }
}
