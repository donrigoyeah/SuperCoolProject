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
    public Transform myTransform;
    
    [Header("Dissolve")]
    public Material dissolve;
    public float dissolveRate = 0.0125f;
    public float refreshRate = 0.025f;

    private void Awake()
    {
        myTransform = this.transform;
    }

    private void OnEnable()
    {
        Rigidbodies[currentAlienSpecies].velocity = Vector3.zero;
        Rigidbodies[currentAlienSpecies].position = myTransform.position;
        dissolve.SetFloat("_DissolveAmount", 0);
        EnableCertainRagdoll();
    }

    private void OnDisable()
    {
        foreach (var item in Rigidbodies)
        {
            item.velocity = Vector3.zero;
            item.position = myTransform.position;
        }
        bulletForce = Vector3.zero;
    }

    private void EnableCertainRagdoll()
    {
        foreach (var item in deadAlienSpecies)
        {
            item.SetActive(false);
        }
        
        deadAlienSpecies[currentAlienSpecies].SetActive(true);
        Rigidbodies[currentAlienSpecies].AddForce((bulletForce * 5f) + Vector3.up * 5f);
        StartCoroutine(Dissolve());
    }
    
    IEnumerator Dissolve()
    {
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
