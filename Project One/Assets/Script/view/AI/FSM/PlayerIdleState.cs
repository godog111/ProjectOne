using System;
using UnityEngine;


// 玩家空闲状态
public class PlayerIdleState : IState
{
    private FSM fsm;
    private PlayerBoard board;
    private float idleTimer;

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
        Debug.Log("进入待机状态");
        board.animator.SetFloat("Speed", 0);
        board.animator.Play("Idle");
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
        //Debug.Log("待机状态");
        // 获取水平输入
        board.moveInput = Input.GetAxisRaw("Horizontal");
        
        // 检查是否需要转换状态
        if (Mathf.Abs(board.moveInput) > 0.01f)
        {
            Debug.Log("切换移动状态");
            fsm.SwitchState(StateType.Move);
        }
        else if (Input.GetKeyDown(board.jumpKey) && board.isGrounded)
        {
            Debug.Log("切换跳跃状态");
            fsm.SwitchState(StateType.Jump);
        }
        else if (!board.isGrounded && board.rb.velocity.y < 0)
        {
            fsm.SwitchState(StateType.Fall);
        }
        else if (board.isOnStairs && Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.1f)
        {
            fsm.SwitchState(StateType.Climb);
        }
    }

    public void OnUpdate()
    {
        
        this.OnCheck(); 
        idleTimer += Time.deltaTime;
    }
}
