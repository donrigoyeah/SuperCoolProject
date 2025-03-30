using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RoamingState : BaseState
{
    private AlienStateMachine model;

    [Header("Roaming Settings")]
    private float roamRadius = 25f;
    private Vector3 finalPosition;
    
    public RoamingState(AlienStateMachine StateMachine) : base(StateMachine)
    {
        model = StateMachine;
    }

    public override void Enter()
    {
        base.Enter();
        Roaming();
    }

    public override void Update()
    {
        base.Update();
    }

    public override void Exit()
    { 
        base.Exit();
    }
    
    private void Roaming()
    {
        if (model.alienClass.isAttackingPlayer == true || model.alienClass.isEvadingPlayer == true) return;
        Vector3 randomDirection = Random.insideUnitSphere * roamRadius;
        randomDirection += model.transform.position;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, roamRadius, 1))
        {
            finalPosition = hit.position;
            model.agent.SetDestination(finalPosition);
        }
        
        model.StartCoroutine(CheckIfReachedDestination());
    }
    
    private IEnumerator CheckIfReachedDestination()
    {
        //TODO: Find a better way to switch states
        yield return new WaitForSeconds(4f);
        
        model.ChangeState(model.lookingState);
    }
}
