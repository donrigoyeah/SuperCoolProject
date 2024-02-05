using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    // SharedInstances lets us access it from everyy file
    // Only for Scripts that have exactly one instance in the scene
    public static PoolManager Instance;

    // Header In Inspector
    [Header("Aliens")]

    // Amount of currently Spawned Aliens
    public int currentAlienAmount;
    // Amount of initial Spawned Aliens
    public int alienAmount;
    // Amount of max possible extras
    public int alienAmountExtra;
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

    [Header("Cops")]
    public int copAmount;
    public List<GameObject> CopPool;
    public GameObject Cop;
    public GameObject CopContainer;

    [Header("Cop Bullets")]
    public int copBulletAmount;
    public List<GameObject> CopBulletPool;
    public GameObject CopBullet;
    public GameObject CopBulletContainer;

    [Header("Cop Muzzle Flash")]
    public int copMuzzleAmount;
    public GameObject copMuzzle;
    public List<GameObject> CopMuzzlePool;
    public GameObject CopMuzzleContainer;

    [Header("Foot Step Smoke")]
    public int FSSAmount;
    public List<GameObject> FSSPool;
    public GameObject FSS;
    public GameObject FSSContainer;

    [Header("Dead Alien")]
    public int deadAlienAmount;
    public List<GameObject> DeadAlienPool;
    public GameObject DeadAlien;
    public GameObject DeadAlienContainer;

    [Header("Damage UI")]
    public int damageUIAmounts;
    public List<GameObject> DamageUIPool;
    public GameObject DamageUI;
    public GameObject DamageUIContainer;

    [Header("Grenades")]
    public int grenadeAmount;
    public List<GameObject> GrenadePool;
    public GameObject grenade;
    public GameObject GrenadeContainer;

    [Header("Reference")]
    public Transform DeadPlayerContainer;
    public LoadingScreenHandler loadingScreenHandler;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }

        // Player Stuff
        BulletPooling();
        BulletExpPooling();
        MuzzlePooling();
        FSSPooling(); // FootStepSmoke

        // Cop Stuff
        CopPooling();
        CopBulletPooling();
        CopMuzzlePooling();

        //Dead ALien
        DeadAlienPooling();

        // UI Elements
        DamageUIPooling();

        // Alien Stuff
        AlienPooling();

        //Grenade Pooling
        GrenadePooling();
    }

    #region Alien Pooling
    public GameObject GetPooledAliens(bool forceAdd)
    {
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
        if (currentAlienAmount < alienAmount + alienAmountExtra)
        {
            // Add only when no more are available
            // Debug.Log("Add additionl alien to the pool");
            // Inititalize Gameobject tmp/Temporary
            GameObject tmp;
            // Add to scene
            tmp = Instantiate(Alien);
            // place in file structure so all are in a container
            tmp.transform.SetParent(AlienContainer.transform);
            // Add to pre defined List
            AlienPool.Add(tmp);
            // Add total amount for looping to find active
            currentAlienAmount++;
            // Return newly generated GO
            return tmp;
        }

        // Add only when no more are available
        if (forceAdd == true)
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
            currentAlienAmount++;
            // If Force added, include in allAlienHandlerList
            AlienManager.Instance.allAlienHandlers.Add(tmp.GetComponent<AlienHandler>());
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
            // Set currently Spawned Alien Amount
            currentAlienAmount++;
        }
        // Send info to loading screen that this pool is ready
        loadingScreenHandler.currentLoadedPools++;
    }
    #endregion

    #region Bullet Pooling
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
        loadingScreenHandler.currentLoadedPools++;
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
        loadingScreenHandler.currentLoadedPools++;
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
        loadingScreenHandler.currentLoadedPools++;
    }
    #endregion

    #region Cop Pooling
    public GameObject GetPooledCop()
    {
        for (int i = 0; i < copAmount; i++)
        {
            if (!CopPool[i].activeInHierarchy)
            {
                return CopPool[i];
            }
        }

        return null;
    }

    private void CopPooling()
    {
        CopPool = new List<GameObject>();
        GameObject tmp;
        for (int i = 0; i < copAmount; i++)
        {
            tmp = Instantiate(Cop);
            tmp.transform.SetParent(CopContainer.transform);
            tmp.SetActive(false);
            CopPool.Add(tmp);
        }
        loadingScreenHandler.currentLoadedPools++;
    }
    #endregion

    #region Cop Bullet Pooling
    public GameObject GetPooledCopBullets()
    {
        for (int i = 0; i < copBulletAmount; i++)
        {
            if (!CopBulletPool[i].activeInHierarchy)
            {
                return CopBulletPool[i];
            }
        }

        // Add only when no more are available
        Debug.Log("Add additionl alien to the pool");
        GameObject tmp;
        tmp = Instantiate(CopBullet);
        tmp.transform.SetParent(CopBulletContainer.transform);
        CopBulletPool.Add(tmp);
        copBulletAmount++;
        return tmp;
    }
    private void CopBulletPooling()
    {
        CopBulletPool = new List<GameObject>();
        GameObject tmp;
        for (int i = 0; i < copBulletAmount; i++)
        {
            tmp = Instantiate(CopBullet);
            tmp.transform.SetParent(CopBulletContainer.transform);
            tmp.SetActive(false);
            CopBulletPool.Add(tmp);
        }
        loadingScreenHandler.currentLoadedPools++;
    }
    #endregion

    #region Cop Muzzle Pooling
    public GameObject GetPooledCopMuzzle()
    {
        for (int i = 0; i < copMuzzleAmount; i++)
        {
            if (!CopMuzzlePool[i].activeInHierarchy)
            {
                return CopMuzzlePool[i];
            }
        }

        return null;
    }

    private void CopMuzzlePooling()
    {
        CopMuzzlePool = new List<GameObject>();
        GameObject tmp;
        for (int i = 0; i < copMuzzleAmount; i++)
        {
            tmp = Instantiate(copMuzzle);
            tmp.transform.SetParent(CopMuzzleContainer.transform);
            tmp.SetActive(false);
            CopMuzzlePool.Add(tmp);
        }
        loadingScreenHandler.currentLoadedPools++;
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
        loadingScreenHandler.currentLoadedPools++;
    }
    #endregion

    #region Dead Alien Pooling
    public GameObject GetPooledDeadAlien()
    {
        for (int i = 0; i < deadAlienAmount; i++)
        {
            if (!DeadAlienPool[i].activeInHierarchy)
            {
                return DeadAlienPool[i];
            }
        }

        return null;
    }

    private void DeadAlienPooling()
    {
        DeadAlienPool = new List<GameObject>();
        GameObject tmp;
        for (int i = 0; i < deadAlienAmount; i++)
        {
            tmp = Instantiate(DeadAlien);
            tmp.transform.SetParent(DeadAlienContainer.transform);
            tmp.SetActive(false);
            DeadAlienPool.Add(tmp);
        }
        loadingScreenHandler.currentLoadedPools++;
    }
    #endregion

    #region DamageUI Pooling
    public GameObject GetPooledDamageUI()
    {
        for (int i = 0; i < damageUIAmounts; i++)
        {
            if (!DamageUIPool[i].activeInHierarchy)
            {
                return DamageUIPool[i];
            }
        }

        return null;
    }

    private void DamageUIPooling()
    {
        DamageUIPool = new List<GameObject>();
        GameObject tmp;
        for (int i = 0; i < damageUIAmounts; i++)
        {
            tmp = Instantiate(DamageUI);
            tmp.transform.SetParent(DamageUIContainer.transform);
            tmp.SetActive(false);
            DamageUIPool.Add(tmp);
        }
        loadingScreenHandler.currentLoadedPools++;
    }
    #endregion

    #region Grenade Pooling

    public GameObject GetPooledGrenade()
    {
        for (int i = 0; i < grenadeAmount; i++)
        {
            if (!GrenadePool[i].activeInHierarchy)
            {
                return GrenadePool[i];
            }
        }

        return null;
    }

    private void GrenadePooling()
    {
        CopPool = new List<GameObject>();
        GameObject tmp;
        for (int i = 0; i < grenadeAmount; i++)
        {
            tmp = Instantiate(grenade);
            tmp.transform.SetParent(GrenadeContainer.transform);
            tmp.SetActive(false);
            GrenadePool.Add(tmp);
        }
        loadingScreenHandler.currentLoadedPools++;
    }
    #endregion
}
