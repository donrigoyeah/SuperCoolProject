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
        if (model.alienClass.isPlayerBullet)
        {
            HandleDeathByBullet(model.alienClass.isPlayerBullet, model.alienClass.CurrentBH.GetComponent<Rigidbody>().velocity);
        }
        else
        {
            HandleDeath();
        }
    }

    public override void Update()
    {
        base.Update();
    }

    public override void Exit()
    {
        model.alienClass.CurrentBH = null;
        base.Exit();
    }
    
    public void HandleDeathByBullet(bool isPlayerBullet, Vector3 bulletForce)
    {
        if (isPlayerBullet)
        {
            AlienManager.Instance.KillAlien(model.alienClass.currentSpecies);
        }

        if (model.alienClass.isDead == false)
        {
            DeadAliensRagdollSpawner(bulletForce);
        }
        HandleDeath();
    }
    
    public void DeadAliensRagdollSpawner(Vector3 forciForce)
    {
        if (model.alienClass.currentSpecies == 0)
        {
            model.alienClass.deadAlienGO = PoolManager.Instance.GetPooledDeadSphereAlien();
        }
        else if (model.alienClass.currentSpecies == 1)
        {
            model.alienClass.deadAlienGO = PoolManager.Instance.GetPooledDeadSquareAlien();
        }
        else if (model.alienClass.currentSpecies == 2)
        {
            model.alienClass.deadAlienGO = PoolManager.Instance.GetPooledDeadTriangleAlien();
        }

        model.alienClass.deadAlienGO.GetComponent<DeadAlienHandler>().bulletForce = forciForce;
        model.alienClass.deadAlienGO.transform.SetLocalPositionAndRotation(model.alienClass.MyTransform.position, model.alienClass.MyTransform.rotation);
        model.alienClass.deadAlienGO.SetActive(true);
        
        model.alienClass.CurrentBH = null;
        model.alienClass.CurrentBH.gameObject.SetActive(false);
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
