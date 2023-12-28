using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.VFX;

public class BulletHandler : MonoBehaviour
{
    // These values get updated after spawning from object pool
    public float bulletDamage = 1;
    public float bulletSpeed = 1;
    public float lifeTime = 2;

    // To make in available in other script. Probably not very secure for competitive online play but here should be fine :)
    public Rigidbody rb;

    private void Awake()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        StartCoroutine(DisableAfterSeconds(lifeTime, this.gameObject));
    }

    private void OnTriggerEnter(Collider other)
    {
        GameObject BulletExp = PoolManager.SharedInstance.GetPooledBulletExplosion();

        if (BulletExp != null)
        {
            BulletExp.transform.position = other.transform.position;
            BulletExp.transform.rotation = other.transform.rotation;
            BulletExp.SetActive(true);
        }
        this.gameObject.SetActive(false);
    }



    IEnumerator DisableAfterSeconds(float sec, GameObject objectToDeactivate)
    {
        // VisualEffect explosionParticleEffect = Instantiate(bulletImpactExplosion, transform.position, Quaternion.identity);
        yield return new WaitForSeconds(sec);
        // Destroy(explosionParticleEffect);
        objectToDeactivate.SetActive(false);
    }
}
