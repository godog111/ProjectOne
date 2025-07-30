using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLandState : IState
{
    private FSM fsm;
    private PlayerBoard board;

    public PlayerLandState(FSM fsm)
    {
        this.fsm = fsm;
        this.board = (PlayerBoard)fsm.blackBoard;
    }

    public void OnEnter(object data = null)
    {
        board.animator.SetBool("isGround", board.isGrounded);
        board.animator.Play("code0_land");
        // fsm.PlayParticle(3, board.trans);
    }

    public void OnExit()
    {

    }

    public void OnClick() { }

    public void OnFixUpdate()
    {

       
    }

    public void OnCheck()
    {
       Debug.Log(board.isGrounded);
        // 检查是否落地
        if (board.isGrounded)
        {
            if (Mathf.Abs(board.moveInput) > 0.1f)
            {
                Debug.Log("落地状态切换移动状态");
                fsm.SwitchState(StateType.Move);
            }
            else
            {
                Debug.Log("落地状态切换待机状态");
                fsm.SwitchState(StateType.Idle);
            }
        }
    }

    public void OnUpdate()
    {

    }
}
