using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LovingState : BaseState
{
    public LovingState(AlienStateMachine StateMachine) : base(StateMachine)
    {
    }

    public override void Enter()
    {
        base.Enter();
    }

    public override void Update()
    {
        base.Update();
        
        Debug.Log("lOVING");
    }

    public override void Exit()
    {
        base.Exit();
    }
}
