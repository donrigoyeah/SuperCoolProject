using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HuntingState : BaseState
{
    public HuntingState(AlienStateMachine StateMachine) : base(StateMachine)
    {
    }

    public override void Enter()
    {
        base.Enter();
        model.agent.speed = model.alienClass.huntingSpeed;
    }

    public override void Update()
    {
        base.Update();
        model.agent.SetDestination(model.otherAlien.transform.position);

        if (!model.otherAlien.gameObject.activeInHierarchy)
        {
            model.otherAlien = null;
            model.ChangeState(model.roamingState);
        }
        
    }

    public override void Exit()
    {
        base.Exit();
        // model.agent.speed = 3.5f;
    }
}
