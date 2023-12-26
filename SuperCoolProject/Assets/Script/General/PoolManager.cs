using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class PoolManager : MonoBehaviour
{
    public static PoolManager SharedInstance;

    [Header("Aliens")]
    public int alienAmount;
    public int alienAmountMax;
    public List<GameObject> AlienPool;
    public GameObject Alien;
    public GameObject AlienContainer;

    [Header("Bullets")]
    public int bulletAmount;
    public List<GameObject> BulletPool;
    public GameObject Bullet;
    public GameObject BulletContainer;

    [Header("Bullet Explosions")]
    public int bulletExpAmount;
    public GameObject bulletExp;
    public List<GameObject> BulletExpPool;
    public GameObject BulletExpContainer;

    [Header("Muzzle Flash")]
    public int muzzleAmount;
    public GameObject muzzle;
    public List<GameObject> MuzzlePool;
    public GameObject MuzzleContainer;

    [Header("Foot Step Smoke")]
    public int FSSAmount;
    public List<GameObject> FSSPool;
    public GameObject FSS;
    public GameObject FSSContainer;

    private void Awake()
    {
        SharedInstance = this;
        AlienPooling();
        BulletPooling();
        BulletExpPooling();
        MuzzlePooling();
        FSSPooling();
    }

    #region AlienPooling
    public GameObject GetPooledAliens()
    {
        for (int i = 0; i < alienAmount; i++)
        {
            if (!AlienPool[i].activeInHierarchy)
            {
                return AlienPool[i];
            }
        }
        if (alienAmount < alienAmountMax)
        {
            // Add only when no more are available
            Debug.Log("Add additionl alien to the pool");
            GameObject tmp;
            tmp = Instantiate(Alien);
            tmp.transform.SetParent(AlienContainer.transform);
            AlienPool.Add(tmp);
            alienAmount++;
            return tmp;
        }
        return null;
    }

    private void AlienPooling()
    {
        AlienPool = new List<GameObject>();
        GameObject tmp;
        for (int i = 0; i < alienAmount; i++)
        {
            tmp = Instantiate(Alien);
            tmp.transform.SetParent(AlienContainer.transform);
            tmp.SetActive(false);
            AlienPool.Add(tmp);
        }
    }
    #endregion

    #region BulletPooling
    public GameObject GetPooledBullets()
    {
        for (int i = 0; i < bulletAmount; i++)
        {
            if (!BulletPool[i].activeInHierarchy)
            {
                return BulletPool[i];
            }
        }

        // Add only when no more are available
        Debug.Log("Add additionl alien to the pool");
        GameObject tmp;
        tmp = Instantiate(Alien);
        tmp.transform.SetParent(BulletContainer.transform);
        BulletPool.Add(tmp);
        bulletAmount++;
        return tmp;
    }
    private void BulletPooling()
    {
        BulletPool = new List<GameObject>();
        GameObject tmp;
        for (int i = 0; i < bulletAmount; i++)
        {
            tmp = Instantiate(Bullet);
            tmp.transform.SetParent(BulletContainer.transform);
            tmp.SetActive(false);
            BulletPool.Add(tmp);
        }
    }
    #endregion

    #region Bullet Explosion Pooling
    public GameObject GetPooledBulletExplosion()
    {
        for (int i = 0; i < bulletExpAmount; i++)
        {
            if (!BulletExpPool[i].activeInHierarchy)
            {
                return BulletExpPool[i];
            }
        }

        return null;
    }

    private void BulletExpPooling()
    {
        BulletExpPool = new List<GameObject>();
        GameObject tmp;
        for (int i = 0; i < bulletExpAmount; i++)
        {
            tmp = Instantiate(bulletExp);
            tmp.transform.SetParent(BulletExpContainer.transform);
            tmp.SetActive(false);
            BulletExpPool.Add(tmp);
        }
    }
    #endregion

    #region Muzzle Pooling
    public GameObject GetPooledMuzzle()
    {
        for (int i = 0; i < muzzleAmount; i++)
        {
            if (!MuzzlePool[i].activeInHierarchy)
            {
                return MuzzlePool[i];
            }
        }

        return null;
    }

    private void MuzzlePooling()
    {
        MuzzlePool = new List<GameObject>();
        GameObject tmp;
        for (int i = 0; i < muzzleAmount; i++)
        {
            tmp = Instantiate(muzzle);
            tmp.transform.SetParent(MuzzleContainer.transform);
            tmp.SetActive(false);
            MuzzlePool.Add(tmp);
        }
    }
    #endregion

    #region Footstep Smoke Pooling
    public GameObject GetPooledFSS()
    {
        for (int i = 0; i < FSSAmount; i++)
        {
            if (!FSSPool[i].activeInHierarchy)
            {
                return FSSPool[i];
            }
        }

        return null;
    }

    private void FSSPooling()
    {
        FSSPool = new List<GameObject>();
        GameObject tmp;
        for (int i = 0; i < FSSAmount; i++)
        {
            tmp = Instantiate(FSS);
            tmp.transform.SetParent(FSSContainer.transform);
            tmp.SetActive(false);
            FSSPool.Add(tmp);
        }
    }
    #endregion
}
