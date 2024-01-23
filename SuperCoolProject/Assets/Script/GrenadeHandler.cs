using System;
using UnityEngine;
using UnityEngine.Serialization;

public class GrenadeHandler : MonoBehaviour
{

    private bool _isGhost;
    [SerializeField] private float explosionForce = 700f;
    [SerializeField] private float explosionRadius = 5f;
    [SerializeField] private GameObject explosionEffect;
    private float countdown = 1f;
    private bool hasExploded = false;

    public PlayerAttacker playerAttacker;
    public float speed;

    public float time;
    public Vector3 x;
    public AlienHandler alienHandler;

    private void Start()
    {
        time = 0f;
    }

    private void Update()
    {
        time += Time.deltaTime * speed;
        transform.position = playerAttacker.Evaluate(time);

        if (time >= 1f)
        {
            Explode();
        }
    }

    void Explode()
    {
        GameObject explosion = Instantiate(explosionEffect, transform.position, Quaternion.identity);
        Destroy(explosion, 1f);
        Explosion();
    }

    void Explosion()
    {
        // TODO: CHeck if we only need the  rb.AddExplosionForce(explosionForce, transform.position, explosionRadius); without the sphere cast
        
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);

        foreach (Collider nearbyObects in colliders)
        {
            AlienHandler alien = nearbyObects.GetComponent<AlienHandler>();
            if (alien != null)
            {
                Rigidbody rb = nearbyObects.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    float distance = Vector3.Distance(transform.position, alien.transform.position);

                    float damage = CalculateDamage(distance);

                    alien.alienHealth -= (int)damage;
                }
            }
        }
        time = 0;
        hasExploded = false;
    }
    
    float CalculateDamage(float distance)
    {

        float maxDamage = 4f;
        float minDamage = 1f;
        float maxDistance = explosionRadius;

        float damage = maxDamage - (distance / maxDistance) * (maxDamage - minDamage);
        Debug.Log(damage);
        return damage;
    }
}