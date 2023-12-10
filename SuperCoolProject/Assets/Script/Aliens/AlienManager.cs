using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlienManager : MonoBehaviour
{
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
                //int randPosX = Random.Range(0, GameManager.SharedInstance.worldBoundaryX * 2) - GameManager.SharedInstance.worldBoundaryX;
                //int randPosZ = Random.Range(0, GameManager.SharedInstance.worldBoundaryZ * 2) - GameManager.SharedInstance.worldBoundaryZ;
                int randPosX = Random.Range(0, 200) - 100;
                int randPosZ = Random.Range(0, 200) - 100;

                alienPoolGo.SetActive(true);
                AlienHandler alienPoolGoHandler = alienPoolGo.GetComponent<AlienHandler>();
                alienPoolGoHandler.alienSpecies[randomSpecies].SetActive(true);
                alienPoolGoHandler.currentSpecies = randomSpecies;
                alienPoolGoHandler.ActivateCurrentModels(randomSpecies);

                // TODO: Maybe have them spawn in groups of the same kind to make sure they have good start oppertunity
                alienPoolGo.transform.position = new Vector3(randPosX, 0.5f, randPosZ);
            }
        }
    }

    // TODO: Handle population control stuff here
}
