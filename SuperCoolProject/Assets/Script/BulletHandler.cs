using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.VFX;

public class BulletHandler : MonoBehaviour
{
    private void OnEnable()
    {
        StartCoroutine(DisableAfterSeconds(2, this.gameObject));
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
