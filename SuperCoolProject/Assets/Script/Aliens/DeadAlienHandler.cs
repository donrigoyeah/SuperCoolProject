using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DeadAlienHandler : MonoBehaviour
{
    public GameObject[] deadAlienSpecies; // 0:Sphere > 1:Square > 2:Triangle
    public Rigidbody[] Rigidbodies;
    public int currentAlienSpecies;
    public Vector3 bulletForce;
    
    [Header("Dissolve")]
    public Material dissolve;
    public float dissolveRate = 0.0125f;
    public float refreshRate = 0.025f;

    private void OnEnable()
    {
        EnableCertainRagdoll();
        StartCoroutine(DisableAfterSeconds(2f));
    }

    private void EnableCertainRagdoll()
    {
        foreach (var item in deadAlienSpecies)
        {
            item.SetActive(false);
        }

        Rigidbodies[currentAlienSpecies].velocity = Vector3.zero;
        deadAlienSpecies[currentAlienSpecies].SetActive(true);
        Rigidbodies[currentAlienSpecies].AddForce(bulletForce + Vector3.up * 5f);
        StartCoroutine(Dissolve());
    }

    private IEnumerator DisableAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        this.gameObject.SetActive(false);
    }
    
    IEnumerator Dissolve()
    {
        float counter = 0;
        while (dissolve.GetFloat("_DissolveAmount") < 1)
        {
            counter += dissolveRate;
            for (int i = 0; i <= 10; i++)
            {
                dissolve.SetFloat("_DissolveAmount", counter);
                yield return new WaitForSeconds(refreshRate);
            }
        }
        dissolve.SetFloat("_DissolveAmount", 0);
        this.gameObject.SetActive(false);
    }
}
