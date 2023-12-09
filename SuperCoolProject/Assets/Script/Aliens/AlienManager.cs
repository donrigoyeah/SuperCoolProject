using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlienManager : MonoBehaviour
{
    private void Awake()
    {

    }

    private void Start()
    {
        SpawnAlien();
    }

    private void SpawnAlien()
    {
        for (int i = 0; i < PoolManager.SharedInstance.alienAmount / 2; i++)
        {
            GameObject alienPoolGo = PoolManager.SharedInstance.GetPooledAliens();
            if (alienPoolGo != null)
            {
                int randomSpecies = Random.Range(0, 3);
                //int randPosX = Random.Range(GameManager.SharedInstance.worldBoundaryMinusX, GameManager.SharedInstance.worldBoundaryX);
                //int randPosZ = Random.Range(GameManager.SharedInstance.worldBoundaryMinusY, GameManager.SharedInstance.worldBoundaryY);
                int randPosX = Random.Range(2, 198) - 100;
                int randPosZ = Random.Range(2, 198) - 100;
                alienPoolGo.SetActive(true);
                AlienHandler alienPoolGoHandler = alienPoolGo.GetComponent<AlienHandler>();
                alienPoolGoHandler.alienSpecies[randomSpecies].SetActive(true);
                alienPoolGoHandler.currentSpecies = randomSpecies;

                alienPoolGo.transform.position = new Vector3(randPosX, 0.5f, randPosZ);
            }
        }
    }

}
