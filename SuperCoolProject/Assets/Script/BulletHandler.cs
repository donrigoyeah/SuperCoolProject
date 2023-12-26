using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class BulletHandler : MonoBehaviour
{
    [SerializeField] private VisualEffect bulletImpactExplosion;
    
    private void OnEnable()
    {
        StartCoroutine(DisableAfterSeconds());
    }

    IEnumerator DisableAfterSeconds()
    {
        // VisualEffect explosionParticleEffect = Instantiate(bulletImpactExplosion, transform.position, Quaternion.identity);
        yield return new WaitForSeconds(2);
        // Destroy(explosionParticleEffect);
        this.gameObject.SetActive(false);
    }
}
