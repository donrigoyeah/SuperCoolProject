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
        Debug.Log("Alien is loving");
        HandleMating();
    }

    public override void Update()
    {
        base.Update();
    }

    public override void Exit()
    {
        base.Exit();
    }
    
    private void HandleMating()
    {
        // Check if possible to spawn more aliens
        // if (brainWashed == false && PoolManager.Instance.currentAlienAmount >= PoolManager.Instance.alienAmount + PoolManager.Instance.alienAmountExtra)
        // {
        //     StartCoroutine(IdleSecsUntilNewState(AlienState.looking));
        //     return;
        // }

        // if (!audioSource.isPlaying)
        // {
        //     audioSource.PlayOneShot(RandomAudioSelectorAliens(AlienManager.Instance.lovemakingAudioList, currentSpecies), 1f);
        // }

        if (model.alienClass.hasUterus == true)
        {
            Debug.Log("Alien is pregnant");
            model.alienClass.amountOfBabies = UnityEngine.Random.Range(1, AlienManager.Instance.maxAmountOfBabies);
            if (model.alienClass.brainWashed == true) { model.alienClass.amountOfBabies = 1; }
            Debug.Log("Alien is pregnant with " + model.alienClass.amountOfBabies + " babies");
            for (var i = 0; i < model.alienClass.amountOfBabies; i++)
            {
                Debug.Log("Alien is giving birth");
                model.alienClass.newBornAlienPoolGo = PoolManager.Instance.GetPooledAliens(model.alienClass.brainWashed);
                if (model.alienClass.newBornAlienPoolGo != null)
                {
                    model.alienClass.randomOffSetBabySpawn = (UnityEngine.Random.Range(0, 5) - 2) / 2;

                    model.alienClass.newBornAlien = model.alienClass.newBornAlienPoolGo.GetComponent<AlienStateMachine>();
                    model.alienClass.newBornAlien.alienClass.currentSpecies = model.alienClass.currentSpecies;
                    model.alienClass.newBornAlien.ActivateCurrentModels(model.alienClass.currentSpecies);
                    model.alienClass.newBornAlien.transform.position = new Vector3(model.alienClass.MyTransform.position.x + model.alienClass.randomOffSetBabySpawn, 0.5f, 
                        model.alienClass.MyTransform.position.z + model.alienClass.randomOffSetBabySpawn);
                    model.alienClass.newBornAlien.gameObject.SetActive(true);
                    Debug.Log(model.alienClass.newBornAlien.gameObject.name + " is born");
                }
            }
        }
        if (model.alienClass.brainWashed)
        {
            return;
        }

        // StartCoroutine(IdleSecsUntilNewState(AlienState.looking));
    }

}
