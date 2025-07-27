using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFallState : IState
{
    private FSM fsm;
    private PlayerBoard board;

    public PlayerFallState(FSM fsm)
    {
        this.fsm = fsm;
        this.board = (PlayerBoard)fsm.blackBoard;
    }

    public void OnEnter(object data = null)
    {
       // board.playeranimator.Play("Fall");
       // board.playeranimator.SetBool("IsFalling", true);
    }

    public void OnExit()
    {
       // board.playeranimator.SetBool("isGround", false);
    }

    public void OnClick() { }

    public void OnFixUpdate()
    {
        // 水平移动
        board.rb.velocity = new Vector2(
            board.moveInput * board.moveSpeed,
            board.rb.velocity.y
        );
       // Debug.Log(board.rb.velocity+"fsdff");

        // 地面检测
        board.isGrounded = Physics2D.OverlapCircle(
            board.groundCheck.position,
            board.groundCheckRadius,
            board.groundLayer


        );
       
    }

    public void OnCheck()
    {
       // Debug.Log(board.isGrounded);
        // 检查是否落地
        if (board.isGrounded)
        {
            if (Mathf.Abs(board.moveInput) > 0.1f)
            {
                fsm.SwitchState(StateType.Move);
            }
            else
            {
                fsm.SwitchState(StateType.Idle);
            }
        }
        // 检查是否在楼梯上并尝试攀爬
        else if (board.isOnStairs && Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.1f)
        {
            fsm.SwitchState(StateType.Climb);
        }
    }

    public void OnUpdate()
    {
        board.moveInput = Input.GetAxisRaw("Horizontal");
    }
}
