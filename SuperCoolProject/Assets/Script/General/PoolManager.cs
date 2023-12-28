using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class PoolManager : MonoBehaviour
{
    // SharedInstances lets us access it from everyy file
    // Only for Scripts that have exactly one instance in the scene
    public static PoolManager SharedInstance;

    // Header In Inspector
    [Header("Aliens")]
    // Amount of initial Spawned Aliens
    public int alienAmount;
    // Amount of max possible with extras
    public int alienAmountMax;
    // Actualy Pool of all GameObjects
    public List<GameObject> AlienPool;
    // The prefab
    public GameObject Alien;
    // The place to spawn the GO into
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
        Debug.Log("Code Explanation for AlienPooling");

        // Loop through initla amount of planned anliens
        for (int i = 0; i < alienAmount; i++)
        {
            // If not active in Hierarchy...
            if (!AlienPool[i].activeInHierarchy)
            {
                // Found inactive, returns "new"
                return AlienPool[i];
            }
        }
        // Buffer check / spawn only this amount of additionals
        if (alienAmount < alienAmountMax)
        {
            // Inititalize Gameobject tmp/Temporary
            GameObject tmp;
            // Add to scene
            tmp = Instantiate(Alien);
            // place in file structure so all are in a container
            tmp.transform.SetParent(AlienContainer.transform);
            // Add to pre defined List
            AlienPool.Add(tmp);
            // Add total amount for looping to find active
            alienAmount++;
            // Return newly generated GO
            return tmp;
        }
        // Returns nothing to function to "formaly end"?!
        return null;
    }

    private void AlienPooling()
    {
        // Instantiate the planned List of GO
        AlienPool = new List<GameObject>();
        // Define typ of variable
        GameObject tmp;
        // Loop though Amount set in  Inspector / Unity editor
        for (int i = 0; i < alienAmount; i++)
        {
            // Generate Opject in Scene
            tmp = Instantiate(Alien);
            // Set location for clearer structure in Scene
            tmp.transform.SetParent(AlienContainer.transform);
            // Disable the freshly instatiated object / Place to handle loading screne?!
            tmp.SetActive(false);
            // Add to List/Object Pool
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
