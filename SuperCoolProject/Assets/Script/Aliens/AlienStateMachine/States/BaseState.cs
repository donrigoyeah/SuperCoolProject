using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseState
{
    protected AlienStateMachine model;
    
    public BaseState(AlienStateMachine StateMachine)
    {
        model = StateMachine;
    }
    
    public virtual void Enter() { }
    public virtual void Update() { }
    public virtual void Exit() { }
}
