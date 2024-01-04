using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUDHandler : MonoBehaviour
{

    public static HUDHandler SharedInstance;

    public int currentHUD;
    public GameObject[] HUDS;
    public RectTransform HUDScaler;

    private float lastInputTimer;
    private float minWaitDuration = .5f;
    private float timeThreshold = 1;
    private bool isResizing = false;
    private float transitionDuration = .5f;


    private void Awake()
    {
        SharedInstance = this;
        DisbaleAllHUDS();
    }

    private void FixedUpdate()
    {
        lastInputTimer += Time.deltaTime;

        if (lastInputTimer > timeThreshold)  // && HUDScaler.localScale != Vector3.one * .5f && !isResizing
        {
            //HUDScaler.localScale = Vector3.one * .5f;
            //TODO: Make it smooth
            if (HUDScaler.localScale == Vector3.one)
            {
                StartCoroutine(ScaleDown());
            }
        }
    }

    public void ChangeHUD()
    {
        // Prevent double clicking
        if (lastInputTimer < minWaitDuration) return;
        lastInputTimer = 0;

        if (HUDScaler.localScale == Vector3.one * .5f) // && !isResizing
        {
            HUDScaler.localScale = Vector3.one;
            //TODO: Make it smooth
            StartCoroutine(ScaleUp());
        }
        else
        {
            currentHUD++;
            if (currentHUD == HUDS.Length) { currentHUD = 0; }
            EnableCurrentHUD(currentHUD);
        }
    }

    private void DisbaleAllHUDS()
    {
        foreach (var item in HUDS)
        {
            item.SetActive(false);
        }
    }

    private void EnableCurrentHUD(int index)
    {
        DisbaleAllHUDS();
        HUDS[index].SetActive(true);
    }

    IEnumerator ScaleUp()
    {
        isResizing = true;

        for (int i = 0; i < 10; i++)
        {
            yield return new WaitForSeconds(transitionDuration / 10);
            HUDScaler.localScale = Vector3.one * .5f + Vector3.one * i * transitionDuration / 10;
        }

        isResizing = false;
    }

    IEnumerator ScaleDown()
    {
        isResizing = true;

        for (int i = 0; i < 10; i++)
        {
            yield return new WaitForSeconds(transitionDuration / 10);
            HUDScaler.localScale = Vector3.one - Vector3.one * i * transitionDuration / 10;
        }

        isResizing = false;
    }
}
