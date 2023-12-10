using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class StateController : MonoBehaviour
{
    public State currentState;
    public AttackState attackState = new AttackState();
    public EvadeState evadeState = new EvadeState();
    public ReproductionState reproductionState = new ReproductionState();
    public RoamState roamState = new RoamState();
    //public SleepState sleepState = new SleepState();
    //public ChaseState chaseState = new ChaseState();
    //public PatrolState patrolState = new PatrolState();
    //public HurtState hurtState = new HurtState();

    //public abstract State Tick(EnemyManager enemyManager, EnemyStats enemyStats, EnemyAnimationManager enemyAnimationManager, EnemyInventory enemyInventory);



    //private void Start()
    //{
    //    ChangeState(roamState);
    //}
    //void Update()
    //{
    //    if (currentState != null)
    //    {
    //        currentState.OnUpdate();
    //    }
    //}
    //public void ChangeState(State newState)
    //{
    //    if (currentState != null)
    //    {
    //        currentState.OnExit();
    //    }
    //    currentState = newState;
    //    currentState.OnEnter(this);
    //}

}
