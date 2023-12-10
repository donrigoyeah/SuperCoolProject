using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EvadeState : State
{
    public AttackState attackState;
    public ReproductionState repoState;
    public RoamState roamState;
    public override State Tick(AlienHandler alienHandler)
    {

        if (Vector3.Distance(alienHandler.closestAlien.transform.position, transform.position) < 10f)
        {
            float step = alienHandler.alienSpeed * Time.deltaTime; // calculate distance to move
            Vector3 fleeDir = alienHandler.closestAlien.transform.position - transform.position;
            transform.position = Vector3.MoveTowards(transform.position, fleeDir, step);
            return this;
        }
        else
        {

            return roamState;
        }



    }
}
