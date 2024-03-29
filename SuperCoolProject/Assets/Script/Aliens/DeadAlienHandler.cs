using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DeadAlienHandler : MonoBehaviour
{
    public Rigidbody[] Rigidbodies;
    public int currentAlienSpecies;
    public Vector3 bulletForce;

    [Header("Dissolve")]
    public Material dissolve;
    public float dissolveRate = 0.0125f;
    public float refreshRate = 0.025f;
    private float steps;
    private float durtaion;
    private float dissolveAMount;
    Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        rb.AddForce((bulletForce * 20) + Vector3.up * 10);
        StartCoroutine(Dissolve());
    }

    private void OnDisable()
    {
        rb.velocity = Vector3.zero;
        this.transform.position = Vector3.zero;
    }


    IEnumerator Dissolve()
    {
        dissolve.SetFloat("_DissolveAmount", 0);
        durtaion = 1f;
        steps = 30;

        for (int i = 0; i < steps; i++)
        {
            yield return new WaitForSeconds(durtaion / steps);
            dissolveAMount = (i / steps);
            dissolve.SetFloat("_DissolveAmount", (dissolveAMount));
        }
        this.gameObject.SetActive(false);
    }
}
