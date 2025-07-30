using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJumpState : IState
{
    private FSM fsm;
    private PlayerBoard board;
    private bool hasJumped;

    public PlayerJumpState(FSM fsm)
    {
        this.fsm = fsm;
        this.board = (PlayerBoard)fsm.blackBoard;
    }

    public void OnEnter(object data = null)
    {
        Debug.Log("进入跳跃状态");
        // fsm.PlaySound(3);
        // fsm.PlayParticle(0, board.trans);
       // board.animator.Play("code0_jump");
        board.animator.SetBool("IsJumping", true);
        board.animator.SetBool("isGround", false);
        //board.animator.SetFloat("Speed",0);
        hasJumped = false;
    }

    public void OnExit()
    {
        Debug.Log("退出跳跃状态");
        // board.animator.Play("code0_land");
        // fsm.StopParticle(board.trans);
        //board.animator.SetFloat("ySpeed", board.rb.velocity.y, 0.01f, Time.deltaTime);
        board.animator.SetBool("IsJumping", false);
        Debug.Log(board.isGrounded);
    }

    public void OnClick() { }

    public void OnFixUpdate()
    {
        // 执行跳跃
        if (!hasJumped)
        {
            board.rb.velocity = new Vector2(board.rb.velocity.x, board.jumpForce);
            hasJumped = true;
        }
        
        // 水平移动
        board.rb.velocity = new Vector2(
            board.moveInput * board.moveSpeed, 
            board.rb.velocity.y
        );
    }

    public void OnCheck()
    {
        // 检查是否开始下落
        if (board.rb.velocity.y < -0.1f) // 增加阈值避免误判
        {
            fsm.SwitchState(StateType.Fall);
        }
        else if (board.isOnStairs && Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.1f)
        {
            fsm.SwitchState(StateType.Climb);
        }
        
        // 只有在下降阶段才检查边缘
        if (board.rb.velocity.y <= 0 && board.isLedgeDetected)
        {
            fsm.SwitchState(StateType.Hang);
        }
    }

    public void OnUpdate()
    {
        board.moveInput = Input.GetAxisRaw("Horizontal");
    }
}
