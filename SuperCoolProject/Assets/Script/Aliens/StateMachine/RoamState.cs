using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoamState : State
{
    public AttackState attackState;
    public ReproductionState repoState;
    public EvadeState evadeState;

    public override State Tick(AlienHandler alienHandler)
    {
        // walk around
        WalkAround();

        targetAlien = alienHandler.gameObject;
        // Look around
        int layerMask = 1 << 9; // Lyer 9 is Enemy
        Collider[] aliensInRange;
        aliensInRange = Physics.OverlapSphere(this.transform.position, alienHandler.lookRadius, layerMask);

        for (int i = 0; i < aliensInRange.Length; i++)  //list of gameObjects to search through
        {
            // Ignore last mating partner
            if (aliensInRange[i] == alienHandler.lastClosestAlien) continue;

            // Check few Aliens. Dont cycle through all of them (Save computing power on 100 enemies?!)
            // Maybe check for gender and distance if same speciesIndex?!
            float dist = Vector3.Distance(aliensInRange[i].transform.position, transform.position);
            if (dist > .1f && dist < alienHandler.lookRadius)
            {
                // TODO: Better handling of this situation
                targetAlien = aliensInRange[i].gameObject;
                alienHandler.closestAlien = aliensInRange[i].gameObject;
                alienHandler.closestAlienIndex = alienHandler.closestAlien.GetComponent<AlienHandler>().currentSpecies;
                break;
            }
        }
        if (targetAlien != null || targetAlien == alienHandler.gameObject)
        {
            targetPosition = targetAlien.transform.position;
            if (alienHandler.closestAlienIndex == alienHandler.currentSpecies &&
                alienHandler.lifeTime > 20 &&
                alienHandler.mateTimer > 10 &&
                alienHandler.isFemale != alienHandler.closestAlienHandler.isFemale)
            {
                return repoState;
            }
            else if (alienHandler.closestAlienIndex > alienHandler.currentSpecies || (alienHandler.currentSpecies == 3 && alienHandler.closestAlienIndex == 0)) // 0:Sphere, 1:Square, 2:Triangle
            {
                return evadeState;
            }
            else if (alienHandler.closestAlienIndex < alienHandler.currentSpecies || (alienHandler.currentSpecies == 0 && alienHandler.closestAlienIndex == 3)) // 0:Sphere, 1:Square, 2:Triangle
            {
                return attackState;
            }
            else
            {
                return this;
            }
        }
        return this;

        #region Loop over List approach
        //for (int i = 0; i < PoolManager.SharedInstance.AlienPool.Count; i++)  //list of gameObjects to search through
        //{
        //    if (PoolManager.SharedInstance.AlienPool[i] == this.gameObject || PoolManager.SharedInstance.AlienPool[i] == lastClosestAlien) continue;

        //    float dist = Vector3.Distance(PoolManager.SharedInstance.AlienPool[i].transform.position, transform.position);
        //    if (dist < lookRadius)
        //    {
        //        closestAlien = PoolManager.SharedInstance.AlienPool[i];
        //        closestAlienIndex = closestAlien.GetComponent<AlienHandler>().currentSpecies;
        //        break;
        //    }
        //}
        //return closestAlien;
        #endregion
    }


    private void WalkAround()
    {
        if (targetPosition == Vector3.one * 1000 || transform.position == targetPosition)
        {
            float randDirX = UnityEngine.Random.Range(0, 2) - .5f;
            float randDirY = UnityEngine.Random.Range(0, 2) - .5f;
            targetPosition = transform.position + new Vector3(randDirX, 0, randDirY) * 5;

            if (targetPosition.x > GameManager.SharedInstance.worldBoundaryX ||
                targetPosition.x < GameManager.SharedInstance.worldBoundaryMinusX ||
                targetPosition.z < GameManager.SharedInstance.worldBoundaryMinusZ ||
                targetPosition.z > GameManager.SharedInstance.worldBoundaryZ)
            {
                targetPosition = Vector3.one * 1000;
            }
        }
        else
        {
            float step = alienHandler.alienSpeed * Time.deltaTime; // calculate distance to move
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, step);
        }
    }
}
