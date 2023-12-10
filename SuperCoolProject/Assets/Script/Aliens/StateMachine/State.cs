using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class State : MonoBehaviour
{
    [Header("Refs")]
    public AlienHandler alienHandler;
    public GameObject targetAlien;
    public Vector3 targetPosition;
    //public Rigidbody enemyRigidbody;
    //public NavMeshAgent navMeshAgent;

    //[Header("stuff")]
    //public float distanceFromTarget;
    //public float stoppingDistance = 2f;
    //public float rangeAttackDistance = 15f;
    //public float rotationSpeed = 50;
    //public float distanceToAttack = 2f;

    private void Start()
    {
        alienHandler = GetComponentInParent<AlienHandler>();
        //enemyRigidbody = GetComponentInParent<Rigidbody>();
        //enemyManager = GetComponentInParent<EnemyManager>();
        //enemyInventory = GetComponentInParent<EnemyInventory>();
        //enemyStats = GetComponentInParent<EnemyStats>();

        //navMeshAgent.enabled = false;
        //enemyRigidbody.isKinematic = false;
    }

    public abstract State Tick(AlienHandler alienHandler);

    //public void RotateNavMeshAgent()
    //{
    //    #region Rotation
    //    Vector3 direction = enemyManager.currentTarget.transform.position - transform.position;
    //    direction.y = 0;
    //    direction.Normalize();

    //    if (direction == Vector3.zero)
    //    {
    //        direction = transform.forward;
    //    }
    //    Quaternion targetRotation = Quaternion.LookRotation(direction);
    //    enemyRigidbody.transform.rotation = Quaternion.Slerp(enemyManager.transform.rotation, targetRotation, rotationSpeed / Time.deltaTime);

    //    navMeshAgent.transform.localPosition = Vector3.zero; // resets navmeshagent but not working correctly
    //    navMeshAgent.transform.localRotation = Quaternion.identity;
    //    #endregion
    //}
}

