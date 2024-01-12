using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollHandler : MonoBehaviour
{
 
    protected Rigidbody rigidbody;
    protected BoxCollider boxCollider;

    public Collider[] childrenCollider;
    public Rigidbody[] childrenRigidbody;
    
    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();

        childrenCollider = GetComponentsInChildren<Collider>();
        childrenRigidbody = GetComponentsInChildren<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void RagdollActivate(bool activate)
    {
        foreach (var collider in childrenCollider)
        {
            collider.enabled = activate;
        }

        foreach (var rigidbody in childrenRigidbody)
        {
            rigidbody.isKinematic = !activate;
        }
        
        rigidbody.isKinematic = !activate;
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (collision.other.CompareTag("Bullet"))
        {
            Debug.Log("gfs");
            RagdollActivate(true);
        }
    }
}
