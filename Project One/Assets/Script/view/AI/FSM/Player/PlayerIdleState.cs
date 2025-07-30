using System;
using UnityEngine;


// 玩家空闲状态
public class PlayerIdleState : IState
{
    private FSM fsm;
    private PlayerBoard board;
    private float idleTimer;

    private float anmimatorTime = 0.18f;

    public PlayerIdleState(FSM fsm)
    {
        this.fsm = fsm;
        this.board = (PlayerBoard)fsm.blackBoard;
        if (board.animator == null)
        {
            Debug.LogError("动画器未赋值！");
            return;
        }
    }

    public void OnEnter(object data = null)
    {
        if (board.animator == null)
        {
            Debug.LogError("动画器未赋值！");
            return;
        }
        
        idleTimer = 0;
        Debug.Log("进入落地状态");
        

        board.animator.SetBool("isGround", board.isGrounded);
        
    }

    public void OnExit()
    {
        Debug.Log("退出待机状态");
    }

    public void OnClick() { }

    public void OnFixUpdate()
    {
        // 持续检测地面
        board.isGrounded = Physics2D.OverlapCircle(
            board.groundCheck.position, 
            board.groundCheckRadius, 
            board.groundLayer
        );
    }

    public void OnCheck()
    {
      
        // 检查是否需要转换状态
        if (Mathf.Abs(board.moveInput) > 0.01f)
        {
            Debug.Log("切换移动状态");
            fsm.SwitchState(StateType.Move);
        }
        else if (Input.GetKeyDown(board.jumpKey) && board.isGrounded)
        {
            Debug.Log("切换跳跃状态");
          //  board.animator.SetBool("IsJumping", true);
            fsm.SwitchState(StateType.Jump);
        }

    }

    public void OnUpdate()
    {
        idleTimer += Time.deltaTime;
        board.animator.SetBool("isGround", board.isGrounded);
    }
}
