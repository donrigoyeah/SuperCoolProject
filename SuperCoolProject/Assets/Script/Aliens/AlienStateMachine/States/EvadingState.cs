using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EvadingState : BaseState
{
    public EvadingState(AlienStateMachine StateMachine) : base(StateMachine)
    {
    }

    public override void Enter()
    {
        base.Enter();

    }

    public override void Update()
    {
        base.Update();
        Evading();
        CheckDistance();
    }

    public override void Exit()
    {
        base.Exit();
        model.otherAlien = null;
    }

    public void Evading()
    {
        if (model.otherAlien != null)
        {
            Vector3 directionAwayFromOtherAlien = (model.transform.position - model.otherAlien.transform.position).normalized;
    
            Vector3 randomOffset = new Vector3(Random.Range(-0.5f, 0.5f), 0, Random.Range(-0.5f, 0.5f));
            directionAwayFromOtherAlien += randomOffset;
            directionAwayFromOtherAlien.Normalize();
    
            Vector3 runToPosition = model.transform.position + directionAwayFromOtherAlien * 20f;
            model.agent.SetDestination(runToPosition);
        }
    }
    
    private void CheckDistance()
    {
        if (model.otherAlien != null)
        {
            float distance = Vector3.Distance(model.transform.position, model.otherAlien.transform.position);
            if (distance > AlienManager.Instance.lookRadius + 1)
            {
                model.ChangeState(model.roamingState);
            }
        }
    }
}
