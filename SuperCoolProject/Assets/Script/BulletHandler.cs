using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletHandler : MonoBehaviour
{
    // These values get updated after spawning from object pool
    public float bulletDamage = 1;
    public float bulletSpeed = 1;
    public float lifeTime = 2;
    public bool isPlayerBullet;

    // To make in available in other script. Probably not very secure for competitive online play but here should be fine :)
    public Rigidbody rb;
    private GameObject BulletExplosion;
    private CopHandler CH;
    private GameObject damageUIGo;
    private DamageUIHandler DUIH;


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
            BulletExplosion = PoolManager.Instance.GetPooledBulletExplosion();

            if (BulletExplosion != null)
            {
                BulletExplosion.transform.position = other.transform.position;
                BulletExplosion.transform.rotation = other.transform.rotation;
                BulletExplosion.SetActive(true);
            }

            if (other.CompareTag("Cop"))
            {
                CH = other.gameObject.GetComponent<CopHandler>();
                if (CH == null) { return; }

                CH.copHealthCurrent -= bulletDamage;


                damageUIGo = PoolManager.Instance.GetPooledDamageUI();
                if (damageUIGo != null)
                {
                    damageUIGo.transform.position = other.transform.position;

                    DUIH = damageUIGo.GetComponentInChildren<DamageUIHandler>();
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
