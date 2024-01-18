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

    private void Awake()
    {
        for (int i = 0; i < Rigidbodies.Length; i++)
        {
            Rigidbodies[i].velocity = Vector3.zero;
            Rigidbodies[i].position = Vector3.zero;
            Rigidbodies[i].rotation = Quaternion.identity;
            deadAlienSpecies[i].SetActive(false);
        }
    }

    private void OnEnable()
    {
        deadAlienSpecies[currentAlienSpecies].SetActive(true);
        Rigidbodies[currentAlienSpecies].AddForce((bulletForce * 4) + Vector3.up * 4);
        StartCoroutine(Dissolve());
    }

    private void OnDisable()
    {
        for (int i = 0; i < Rigidbodies.Length; i++)
        {
            Rigidbodies[i].velocity = Vector3.zero;
            Rigidbodies[i].position = Vector3.zero;
            Rigidbodies[i].rotation = Quaternion.identity;
            deadAlienSpecies[i].SetActive(false);
        }
        bulletForce = Vector3.zero;
    }


    IEnumerator Dissolve()
    {
        dissolve.SetFloat("_DissolveAmount", 0);
        float durtaion = 1f;
        float steps = 30;

        for (int i = 0; i < steps; i++)
        {
            yield return new WaitForSeconds(durtaion / steps);
            float dissolveAMount = (i / steps);
            dissolve.SetFloat("_DissolveAmount", (dissolveAMount));
        }
        Rigidbodies[currentAlienSpecies].velocity = Vector3.zero;
        this.gameObject.SetActive(false);
    }
}
