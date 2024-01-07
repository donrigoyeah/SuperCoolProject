using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CopHandler : MonoBehaviour
{
    public bool isAggro;
    public bool canShoot;
    public float copHealthCurrent;
    public float copHealthMax = 100;
    public float copSpeed = 10;
    public float attackRange = 10;

    PlayerManager closestPlayer;

    public Transform leftGun;
    public Transform rightGun;

    public float fireRate = .5f;
    public float fireTimer = 0;
    public float copBulletSpeed = 10;
    public float copBulletDamage = 10;
    private bool leftRightSwitch;

    public Transform CopCar;
    public Animation anim;

    private void OnEnable()
    {
        copHealthCurrent = copHealthMax;
        isAggro = false;
        canShoot = false;
        fireTimer = 0;
    }

    private void FixedUpdate()
    {
        fireTimer += Time.deltaTime;
        if (closestPlayer == null) { FindClosestPlayer(); }

        HandleMovement();
        if (isAggro)
        {
            anim.Play("Armature|Attack");
            if (canShoot)
            {
                HandleAttacking();
                return;
            }
        }
        else
        {
            anim.Play("Armature|T-Pose");
        }
        if (copHealthCurrent < 0)
        {
            HandleDeath();
        }
    }

    private void FindClosestPlayer()
    {
        float dist = 1000;

        foreach (var item in GameManager.SharedInstance.players)
        {
            float tempDist = Vector3.Distance(this.transform.position, item.transform.position);
            if (tempDist < dist)
            {
                dist = tempDist;
                closestPlayer = item;
            }
        }
    }

    private void HandleMovement()
    {
        if (closestPlayer == null) { return; }

        float step = (copSpeed) * Time.deltaTime;
        transform.LookAt(closestPlayer.transform.position, Vector3.up);

        if (Vector3.Distance(this.transform.position, closestPlayer.transform.position) > attackRange)
        {
            transform.position = Vector3.MoveTowards(transform.position, closestPlayer.transform.position, step);
        }
        else
        {
            if (GameManager.SharedInstance.hasBeenServed == false)
            {
                GameManager.SharedInstance.CopScreenGO.SetActive(true);
            }
            canShoot = true;
        }
    }

    private void HandleAttacking()
    {
        if (fireTimer > fireRate)
        {
            fireTimer = 0;
            if (leftRightSwitch == true)
            {
                // Instantiate Bullet left
                HandleSpawnCopLazer(leftGun);
                leftRightSwitch = false;
                return;
            }
            else
            {
                // Instantiate Bullet right
                HandleSpawnCopLazer(rightGun);
                leftRightSwitch = true;
                return;
            }
        }
    }

    private void HandleDeath()
    {
        // TODO: Make nicer
        this.gameObject.SetActive(false);
    }

    private void HandleSpawnCopLazer(Transform lazerSpawnLocation)
    {
        GameObject copBulletPoolGo = PoolManager.SharedInstance.GetPooledCopBullets();
        if (copBulletPoolGo != null)
        {
            copBulletPoolGo.transform.position = lazerSpawnLocation.position;
            copBulletPoolGo.transform.rotation = lazerSpawnLocation.rotation;
            BulletHandler BH = copBulletPoolGo.GetComponent<BulletHandler>();
            BH.bulletDamage = copBulletDamage;
            BH.isPlayerBullet = false;
            copBulletPoolGo.SetActive(true);
            BH.rb.velocity = Vector3.zero;
            BH.rb.velocity = lazerSpawnLocation.forward * copBulletSpeed;
        }
        GameObject copMuzzlePoolGo = PoolManager.SharedInstance.GetPooledCopMuzzle();
        if (copMuzzlePoolGo != null)
        {
            copMuzzlePoolGo.transform.position = lazerSpawnLocation.position;
            copMuzzlePoolGo.transform.rotation = lazerSpawnLocation.rotation;
            copMuzzlePoolGo.SetActive(true);
            StartCoroutine(DisableAfterSeconds(1, copMuzzlePoolGo));
        }
    }

    IEnumerator DisableAfterSeconds(int sec, GameObject objectToDeactivate)
    {
        yield return new WaitForSeconds(sec);
        objectToDeactivate.SetActive(false);
    }

}
