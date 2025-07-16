using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SerializeField]
public class ZombieBlackboard:BlackBoard
{
    public float ideleTime;

    public float moveSpeed;

    public Transform transform;
}

public class AI_IdleState : IState
{

    private float idleTimer;

    private FSM fsm;

    private ZombieBlackboard blackboard;

    public AI_IdleState(FSM fsm)
    {
            this.fsm = fsm;

            this.blackboard = fsm.blackBoard as ZombieBlackboard;
    }
    public void OnCheck()
    {
        throw new System.NotImplementedException();
    }

    public void OnClick()
    {
        throw new System.NotImplementedException();
    }

    public void OnEnter()
    {
        idleTimer=0;
    }

    public void OnEnter(object data = null)
    {
        throw new System.NotImplementedException();
    }

    public void OnExit()
    {
        throw new System.NotImplementedException();
    }

    public void OnFixUpdate()
    {
        throw new System.NotImplementedException();
    }

    public void OnUpdate()
    {
        idleTimer+=Time.deltaTime;
        if(idleTimer>blackboard.ideleTime)
            {
                this.fsm.SwitchState(StateType.MOVE);
            }
    }
}

public class AI_MoveState : IState
{
    private float idleTimer;

    private Vector2 targetPos;

    private FSM fsm;

    private ZombieBlackboard blackboard;

    public AI_MoveState(FSM fsm)
    {
            this.fsm = fsm;

            this.blackboard = fsm.blackBoard as ZombieBlackboard;
    }
    public void OnCheck()
    {
        throw new System.NotImplementedException();
    }

    public void OnClick()
    {
        throw new System.NotImplementedException();
    }

    public void OnEnter()
    {
       float randomX = Random.Range(-10,10);
       float randomY = Random.Range(-10,10);
       targetPos = new Vector2(blackboard.transform.position.x+randomX,blackboard.transform.position.y);
    }

    public void OnEnter(object data = null)
    {
        throw new System.NotImplementedException();
    }

    public void OnExit()
    {
        throw new System.NotImplementedException();
    }

    public void OnFixUpdate()
    {
        throw new System.NotImplementedException();
    }

    public void OnUpdate()
    {
        if(Vector2.Distance(blackboard.transform.position,targetPos)<0.1f)
        {
            fsm.SwitchState(StateType.Idle);
        }
        else
        {
            blackboard.transform.position = Vector2.MoveTowards(blackboard.transform.position,targetPos,blackboard.moveSpeed*Time.deltaTime);
        }
    }
}
public class AINew : MonoBehaviour
{
    private FSM fsm;
    public ZombieBlackboard blackboard;
    void Start()
    {
        fsm = new FSM(blackboard);
        fsm.AddState(StateType.Idle,new AI_IdleState(fsm));
        fsm.AddState(StateType.MOVE,new AI_MoveState(fsm));
        fsm.SwitchState(StateType.Idle);
    }

    void Update()
    {
        fsm.OnCheck();
        fsm.OnUpdate();
    }

    private void FixedUpdate()
    {
        fsm.OnFixUpdate();
    }
}
