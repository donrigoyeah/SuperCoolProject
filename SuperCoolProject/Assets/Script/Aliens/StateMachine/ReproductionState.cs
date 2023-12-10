using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class ReproductionState : State
{
    public AttackState attackState;
    public EvadeState evadeState;
    public RoamState roamState;
    public override State Tick(AlienHandler alienHandler)
    {
        if (Vector3.Distance(alienHandler.closestAlien.transform.position, transform.position) < 10f)
        {
            float step = alienHandler.alienSpeed * Time.deltaTime; // calculate distance to move
            transform.position = Vector3.MoveTowards(transform.position, alienHandler.closestAlien.transform.position, step);
            return this;
        }
        else if (Vector3.Distance(alienHandler.closestAlien.transform.position, transform.position) < .1f)
        {
            if (alienHandler.isFemale)
            {
                GameObject alienPoolGo = PoolManager.SharedInstance.GetPooledAliens();
                if (alienPoolGo != null)
                {
                    AlienHandler newBornAlien;
                    alienPoolGo.SetActive(true);
                    newBornAlien = alienPoolGo.GetComponent<AlienHandler>();
                    newBornAlien.alienSpecies[alienHandler.currentSpecies].SetActive(true);
                    newBornAlien.currentSpecies = alienHandler.currentSpecies;
                    alienHandler.isFemale = UnityEngine.Random.Range(0, 2) == 1;

                    alienPoolGo.transform.position = new Vector3(transform.position.x, 0.5f, transform.position.z) + Vector3.forward;
                }
            }
            alienHandler.lastClosestAlien = alienHandler.closestAlien;
            alienHandler.closestAlien = null;
            alienHandler.closestAlienHandler = null;
            alienHandler.mateTimer = 0;
            return roamState;
        }
        else
        {

            return roamState;
        }
    }
}
