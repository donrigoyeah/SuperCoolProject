using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollHandler : MonoBehaviour
{

    public Rigidbody rigidbody;
    private BoxCollider boxCollider;

    public Collider[] childrenCollider;
    public Rigidbody[] childrenRigidbody;

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();

        childrenCollider = GetComponentsInChildren<Collider>();
        childrenRigidbody = GetComponentsInChildren<Rigidbody>();
    }

    public void RagdollActivate(bool activate)
    {
        foreach (var collider in childrenCollider)
        {
            collider.enabled = activate;
        }

        // foreach (var rigidbody in childrenRigidbody)
        // {
        //     rigidbody.isKinematic = !activate;
        // }
        //
        // rigidbody.isKinematic = !activate;
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Bullet"))
        {
            RagdollActivate(true);
        }
    }
}
