using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerClimbState : IState
{
    private FSM fsm;
    private PlayerBoard board;
    private float verticalInput;

    public PlayerClimbState(FSM fsm)
    {
        this.fsm = fsm;
        this.board = (PlayerBoard)fsm.blackBoard;
    }

    public void OnEnter(object data = null)
    {
        board.originalGravityScale = board.rb.gravityScale;
        board.rb.gravityScale = 0; // 取消重力影响
        board.animator.Play("Climb");
        board.animator.SetBool("IsClimbing", true);
    }

    public void OnExit()
    {
        board.rb.gravityScale = board.originalGravityScale;
        board.animator.SetBool("IsClimbing", false);
    }

    public void OnClick() { }

    public void OnFixUpdate()
    {
        // 攀爬移动
        verticalInput = Input.GetAxisRaw("Vertical");
        board.rb.velocity = new Vector2(
            board.moveInput * board.moveSpeed, 
            verticalInput * board.climbSpeed
        );
        
        // 确保玩家保持在梯子上
        if (board.currentStairCollider != null)
        {
            float xPos = board.currentStairCollider.bounds.center.x;
            board.rb.position = new Vector2(
                xPos, 
                board.rb.position.y
            );
        }
    }

    public void OnCheck()
    {
        // 检查是否离开梯子
        if (!board.isOnStairs)
        {
            if (board.isGrounded)
            {
                fsm.SwitchState(StateType.Idle);
            }
            else if (board.rb.velocity.y < 0)
            {
                fsm.SwitchState(StateType.Fall);
            }
            else
            {
                fsm.SwitchState(StateType.Jump);
            }
        }
        // 检查是否跳跃离开梯子
        else if (Input.GetKeyDown(board.jumpKey))
        {
            board.rb.gravityScale = board.originalGravityScale;
            board.rb.AddForce(new Vector2(0, board.jumpForce), ForceMode2D.Impulse);
            fsm.SwitchState(StateType.Jump);
        }
    }

    public void OnUpdate()
    {
        board.moveInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
        board.animator.SetFloat("ClimbSpeed", Mathf.Abs(verticalInput));
    }
}