using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;

public class CopHandler : MonoBehaviour
{
    public bool isAggro;
    public bool canShoot;
    public float copHealthCurrent;
    public float copHealthMax = 100;
    public float copSpeed = 5;
    public float attackRange = 10;

    PlayerManager closestPlayer;

    public Transform leftGun;
    public Transform rightGun;

    public float fireRate = .5f;
    public float fireTimer = 0;
    public float copBulletSpeed = 30;
    public float copBulletDamage = 10;
    private bool leftRightSwitch;

    public Transform CopCar;
    public Animation anim;
    public GameObject copCorpse;

    [Header("Audio")]
    public AudioClip copMumbling;
    public AudioClip copShooting;
    private AudioSource audioSource;

    private void OnEnable()
    {
        copHealthCurrent = copHealthMax;
        isAggro = false;
        canShoot = false;
        fireTimer = 0;
    }

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void FixedUpdate()
    {
        fireTimer += Time.deltaTime;
        if (closestPlayer == null) { FindClosestPlayer(); }
        if (copHealthCurrent < 0) { HandleDeath(); }

        HandleMovement();

        if (!isAggro)
        {
            anim.Play("Armature|T-Pose");
        }
        else
        {
            if (!closestPlayer.isAlive)
            {
                isAggro = false;
                return;
            }
            anim.Play("Armature|Attack");
            if (canShoot)
            {
                HandleAttacking();
                return;
            }
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
        if (!audioSource.isPlaying && !canShoot)
        {
            audioSource.PlayOneShot(copMumbling, 1f);
        }

        if (closestPlayer == null) { return; }

        float step = copSpeed * Time.deltaTime;

        if (CopManager.SharedInstance.hasBeenServed == true && isAggro == false)
        {
            transform.LookAt(CopCar.transform.position, Vector3.up);
            transform.position = Vector3.MoveTowards(transform.position, CopCar.transform.position, step);
            return;
        }

        transform.LookAt(closestPlayer.transform.position, Vector3.up);

        if (Vector3.Distance(this.transform.position, closestPlayer.transform.position) > attackRange)
        {
            transform.position = Vector3.MoveTowards(transform.position, closestPlayer.transform.position, step);
        }
        else
        {
            if (CopManager.SharedInstance.hasBeenServed == false)
            {
                CopManager.SharedInstance.CopScreenGO.SetActive(true);
                CopManager.SharedInstance.payButton.Select();
                Time.timeScale = 0;
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
                audioSource.PlayOneShot(copShooting, 1f);
                return;
            }
            else
            {
                // Instantiate Bullet right
                HandleSpawnCopLazer(rightGun);
                leftRightSwitch = true;
                audioSource.PlayOneShot(copShooting, 1f);
                return;
            }
        }
    }

    private void HandleDeath()
    {
        // TODO: Make nicer
        GameObject deadCop = Instantiate(copCorpse, transform.position, quaternion.identity);
        Destroy(deadCop, 2f);
        CopManager.SharedInstance.currentCops.Remove(this);
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
            BH.rb.velocity = Vector3.zero;
            BH.rb.velocity = lazerSpawnLocation.forward * copBulletSpeed;

            copBulletPoolGo.SetActive(true);
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