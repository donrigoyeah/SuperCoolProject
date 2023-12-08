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


    private void Awake()
    {
        SharedInstance = this;
    }

    private void Start()
    {
        AlienPooling();
    }

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
}
