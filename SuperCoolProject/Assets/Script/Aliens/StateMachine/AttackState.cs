using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class AttackState : State
{
    public ReproductionState repoState;
    public EvadeState evadeState;
    public RoamState roamState;
    public override State Tick(AlienHandler alienHandler)
    {
        if (Vector3.Distance(alienHandler.closestAlien.transform.position, transform.position) > .01f)
        {
            float step = alienHandler.alienSpeed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, alienHandler.closestAlien.transform.position, step);
            return this;
        }
        else
        {
            return roamState;
        }

    }
}
