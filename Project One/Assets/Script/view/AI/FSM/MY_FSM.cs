using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public enum StateType
{
    Idle,
    End,
    Attack,
    Die,
    Success,
    Move,
    Patrol,
    Interace,
    Jump,
    Climb,
    Fall

}
public interface IState
{
        void OnEnter(object data = null);
        void OnExit();
        void OnClick();
        void OnFixUpdate();

        void OnCheck();
        void OnUpdate();
         
}
[Serializable]
public class BlackBoard
{
    //此处存储共享数据，或者向外部展示
    public int moveSpeed;
    public int idleTime;
    public int PatrolSpeed;

    public Transform[] patrolPoints;
    public Transform[] chasePoints;  //追击范围  

    public Animator animator;//动画组件
}
public class FSM
{

    public IState currentState { get; private set; }
    public Dictionary<StateType, IState> states = new Dictionary<StateType, IState>();
    public BlackBoard blackBoard;

    public FSM(BlackBoard blackBoard)
    {


        this.blackBoard = blackBoard;
    }

    public void AddState(StateType stateType, IState state)
    {
        if (states.ContainsKey(stateType))
        {
            Debug.Log("[AddState]>>>>>>>>>map has contain key:" + stateType);
            return;
        }
        states.Add(stateType, state);
    }

    public void SwitchState(StateType stateType, object data = null)
    {
        if (!states.ContainsKey(stateType))
        {
            Debug.Log("[SwitchState]>>>>>>>>>map has contain key:" + stateType);
            return;
        }
        if (currentState != null)
        {
            currentState.OnExit();
        }
        currentState = states[stateType];
        currentState.OnEnter(data);
    }

    public void OnUpdate()
    {
        currentState.OnUpdate();
    }

    public void OnFixUpdate()
    {
        currentState.OnFixUpdate();
    }

    public void OnCheck()
    {
        currentState.OnCheck();

    }
}


public class a
{
    int add;
}

public class b : a
{
    int dfdf;
    int add;
}