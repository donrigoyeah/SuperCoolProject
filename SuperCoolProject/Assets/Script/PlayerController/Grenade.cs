using System;
using UnityEngine;

public class Grenade : MonoBehaviour {
    [SerializeField] private Rigidbody _rb;
    private bool _isGhost;

    public void Init(Vector3 velocity, bool isGhost) {
        _rb.AddForce(velocity, ForceMode.Impulse);
    }

    private void Update()
    {
        Destroy(gameObject, 1f);
    }
    
}