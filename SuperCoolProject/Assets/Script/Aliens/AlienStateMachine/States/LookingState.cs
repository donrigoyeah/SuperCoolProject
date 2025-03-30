using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LookingState : BaseState
{
    private AlienStateMachine model;
    
    public LookingState(AlienStateMachine StateMachine) : base(StateMachine)
    {
        model = StateMachine;
    }

    public override void Enter()
    {
        base.Enter();
        model.agent.speed = 3.5f; //Default speed
        Looking();
    }

    public override void Update()
    {

    }

    public override void Exit()
    {
        base.Exit();
    }

    private void Looking()
    {
        model.alienClass.aliensInRangeCount = model.aliensInRange.Count;
        
        if (model.alienClass.aliensInRangeCount == 0 || model.aliensInRange == null)
        {
            model.aliensInRangeCollider = Physics.OverlapSphere(model.transform.position, AlienManager.Instance.lookRadius, model.alienClass.layerMaskAlien, QueryTriggerInteraction.Ignore);
            model.aliensInRangeColliderOrdered = model.aliensInRangeCollider.OrderBy(c => (model.transform.position - c.transform.position).sqrMagnitude).ToArray();
            foreach (var item in model.aliensInRangeColliderOrdered)
            {
                model.aliensInRange.Add(item);
            }
            model.alienClass.aliensInRangeCount = model.aliensInRange.Count;
        }
        
        EvaulateAlien();
    }

    private void EvaulateAlien()
    {
        
        foreach (var alienCollider in model.aliensInRangeColliderOrdered)
        {
            if (alienCollider == model.alienClass.MyCollisionCollider || 
                alienCollider.gameObject == model.alienClass.lastTargetAlien) 
                continue;

            model.otherAlien = alienCollider.GetComponent<AlienStateMachine>();

            if (model.otherAlien.currentAge == AlienStateMachine.AlienAge.resource) continue;

            // Check for mating conditions (same species)
            if (model.alienClass.currentSpecies == model.otherAlien.alienClass.currentSpecies)
            {
                if (model.alienClass.hasUterus != model.otherAlien.alienClass.hasUterus && 
                    model.currentAge == AlienStateMachine.AlienAge.sexualActive && 
                    model.otherAlien.currentAge == AlienStateMachine.AlienAge.sexualActive &&
                    model.alienClass.lustTimer > AlienManager.Instance.lustTimerThreshold &&
                    model.otherAlien.alienClass.lustTimer > AlienManager.Instance.lustTimerThreshold) 
                {
                    model.agent.SetDestination(model.otherAlien.transform.position);
                    model.otherAlien.ChangeState(model.otherAlien.lovingState);
                    model.otherAlien.agent.SetDestination(model.transform.position);
                    model.ChangeState(model.lovingState);
                    break;
                }
            }
            else // Check predator-prey relationships
            {
                bool isPredator = (model.alienClass.currentSpecies == (model.otherAlien.alienClass.currentSpecies + 1) % 3); // 0 > 1 > 2 > 0 cycle

                if (model.alienClass.hungerTimer > AlienManager.Instance.hungerTimerThreshold && isPredator)
                {
                    model.ChangeState(model.huntingState);
                    Debug.Log("Hunting");
                    // model.alienClass.agent.SetDestination(model.transform.position);
                    // model.ChangeState(model.otherAlien.evadingState);
                    break;
                }
                else if (!isPredator)
                {
                    model.agent.SetDestination(model.otherAlien.transform.position);
                    model.ChangeState(model.evadingState);
                    Debug.Log("Evading");
                    break;
                }
            }
        }
        
        //TODO: The game crashes whenever this code is uncommented out
        
        // if (model.alienClass.targetAlien == null && model.otherAlien == null)
        // {
        //     Debug.Log("Roaming --- 1");
        //     model.ChangeState(model.roamingState);
        // }
    }
    
}

