using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathState : BaseState
{
    public DeathState(AlienStateMachine StateMachine) : base(StateMachine)
    {
    }

    public override void Enter()
    {
        base.Enter();
        HandleDeath();
    }

    public override void Update()
    {
        base.Update();
    }

    public override void Exit()
    {
        base.Exit();
    }
    
    public void HandleDeath()
    {
        model.gameObject.SetActive(false);
        model.alienClass.isDead = true;
        model.alienClass.brainWashed = false;
        // model.alienClass.anim[model.alienClass.currentSpecies].Stop();
        model.StopAllCoroutines();
        Debug.Log("Alien is dead");
        return;
    }
}
