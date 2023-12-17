using System;
using UnityEngine;

public class Grenade : MonoBehaviour
{

    [SerializeField] private Rigidbody _rb;
    private bool _isGhost;
    [SerializeField] private float explosionForce = 700f;
    [SerializeField] private float explosionRadius = 5f;
    [SerializeField] private GameObject explosionEffect;
    private float countdown = 1f;
    private bool hasExploded = false;
    public void Init(Vector3 velocity, bool isGhost)
    {
        _rb.AddForce(velocity, ForceMode.Impulse);
    }

    private void Update()
    {
        if (!hasExploded)
        {
            countdown -= Time.deltaTime;
            if (countdown <= 0f)
            {
                Explode();
                hasExploded = true;
            }
        }
    }

    void Explode()
    {
        GameObject explosion = Instantiate(explosionEffect, transform.position, Quaternion.identity);
        Destroy(explosion, 1f);
        Explosion();
        Destroy(gameObject);
    }

    void Explosion()
    {
        // TODO: CHeck if we only need the  rb.AddExplosionForce(explosionForce, transform.position, explosionRadius); without the sphere cast
        
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);

        foreach (Collider nearbyObects in colliders)
        {
            Rigidbody rb = nearbyObects.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddExplosionForce(explosionForce, transform.position, explosionRadius);
            }
        }
    }
}