using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.VFX;

public class BulletHandler : MonoBehaviour
{
    // These values get updated after spawning from object pool
    public float bulletDamage = 1;
    public float bulletSpeed = 1;
    public float lifeTime = 2;
    public bool isPlayerBullet;

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
        if (isPlayerBullet)
        {
            GameObject BulletExp = PoolManager.SharedInstance.GetPooledBulletExplosion();

            if (BulletExp != null)
            {
                BulletExp.transform.position = other.transform.position;
                BulletExp.transform.rotation = other.transform.rotation;
                BulletExp.SetActive(true);
            }

            if (other.CompareTag("Cop"))
            {
                CopHandler CH = other.gameObject.GetComponent<CopHandler>();
                if (CH == null) { return; }

                CH.copHealthCurrent -= bulletDamage;


                GameObject damageUIGo = PoolManager.SharedInstance.GetPooledDamageUI();
                if (damageUIGo != null)
                {
                    damageUIGo.transform.position = other.transform.position;

                    DamageUIHandler DUIH = damageUIGo.GetComponentInChildren<DamageUIHandler>();
                    DUIH.damageValue = bulletDamage;

                    damageUIGo.SetActive(true);
                }

                // Aggro all cops
                foreach (var item in CopManager.Instance.currentCops)
                {
                    item.isAggro = true;
                }
            }

            this.gameObject.SetActive(false);
        }
        else if (isPlayerBullet == false) // Cop Bullet
        {
            if (other.CompareTag("Cop")) { return; }
            if (other.CompareTag("Player"))
            {
                Debug.Log("Got hit by cop");
                other.gameObject.GetComponent<PlayerManager>().HandleHit();

            }
            this.gameObject.SetActive(false);
        }
        // TODO: Make cop Bullet Explosion

    }



    IEnumerator DisableAfterSeconds(float sec, GameObject objectToDeactivate)
    {
        // VisualEffect explosionParticleEffect = Instantiate(bulletImpactExplosion, transform.position, Quaternion.identity);
        yield return new WaitForSeconds(sec);
        // Destroy(explosionParticleEffect);
        objectToDeactivate.SetActive(false);
    }
}
