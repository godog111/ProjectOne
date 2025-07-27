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
        board.animator.Play("Jump");
        board.animator.SetBool("IsJumping", true);
        hasJumped = false;
    }

    public void OnExit()
    {
        board.animator.SetBool("IsJumping", false);
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
        if (board.rb.velocity.y < 0)
        {
            fsm.SwitchState(StateType.Fall);
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
