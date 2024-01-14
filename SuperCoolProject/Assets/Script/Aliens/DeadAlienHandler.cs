using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DeadAlienHandler : MonoBehaviour
{
    public GameObject[] deadAlienSpecies; // 0:Sphere > 1:Square > 2:Triangle
    public int currentAlienSpecies;

    private void OnEnable()
    {
        EnableCertainRagdoll();
        StartCoroutine(DisableAfterSeconds(1));
    }

    private void EnableCertainRagdoll()
    {
        foreach (var item in deadAlienSpecies)
        {
            item.SetActive(false);
        }
        deadAlienSpecies[currentAlienSpecies].SetActive(true);
    }

    private IEnumerator DisableAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        this.gameObject.SetActive(false);
    }
}
