using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    public static PoolManager SharedInstance;

    [Header("Aliens")]
    public int alienAmount;
    public List<GameObject> AlienPool;
    public GameObject Alien;
    public GameObject AlienContainer;

    [Header("Bullets")]
    public int bulletAmount;
    public List<GameObject> BulletPool;
    public GameObject Bullet;
    public GameObject BulletContainer;

    private void Awake()
    {
        SharedInstance = this;
        AlienPooling();
        BulletPooling();
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
        return null;
    }

    private void AlienPooling()
    {
        Debug.Log("until here?");
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
        return null;
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
}
