using System;
using UnityEngine;
using UnityEngine.Serialization;

public class GrenadeHandler : MonoBehaviour
{

    private bool _isGhost;
    public float explosionForce = 700f;
    public float explosionRadius = 10f;
    public GameObject explosionEffect;

    public PlayerAttacker playerAttacker;
    public float speed;

    public float time;
    public Vector3 x;

    private float distance;
    private float damage;

    private AlienHandler alien;
    private Rigidbody rb;
    public bool hasExploded = false;

    private int layerMaskAlien = 1 << 9; // Lyer 9 is Alien


    private void Start()
    {
        time = 0f;
    }

    private void OnEnable()
    {
        hasExploded = false;
    }

    private void FixedUpdate()
    {
        time += Time.deltaTime * speed;
        transform.position = playerAttacker.Evaluate(time);
    
        if (time >= 0.98f && !hasExploded || transform.position.y < .1f)
        {
            Explode();
            hasExploded = true;
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

        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius, layerMaskAlien, QueryTriggerInteraction.Ignore);
        Debug.Log("collider length" + colliders.Length);

        foreach (Collider nearbyObects in colliders)
        {
            rb = nearbyObects.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Debug.Log("Rigidbody: " + rb.name);
                Debug.Log("BOOM");
                distance = Vector3.Distance(transform.position, nearbyObects.transform.position);
                Debug.Log("Distance: " + distance);
                damage = CalculateDamage(distance);
                rb.AddExplosionForce(explosionForce, transform.position, explosionRadius);
                AlienHandler nearAlien = nearbyObects.gameObject.GetComponentInParent<AlienHandler>();
                if (nearAlien != null)
                {
                    nearAlien.alienHealth -= damage;
                    if (nearAlien.alienHealth <= 0 && nearAlien.isDead == false)
                    {
                        Debug.Log("I am not sure if its write place to put maybe it needs to be in Alien Handler only");
                        nearAlien.gameObject.SetActive(false);
                    }
                    Debug.Log(nearAlien.alienHealth);
                }
            }
        }
        time = 0;
    }

    float CalculateDamage(float distance)
    {
        float maxDamage = 50f;
        float minDamage = 1f;
        float maxDistance = explosionRadius;
        damage = maxDamage - (distance / maxDistance) * (maxDamage - minDamage);
        return damage;
    }
}