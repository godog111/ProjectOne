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
        Debug.Log("进入下落状态");
        board.animator.Play("code0_fall");
        board.animator.SetBool("IsFalling", true);
        board.animator.SetFloat("Speed", 0);
        board.animator.SetBool("isGround", board.isGrounded);
    }

    public void OnExit()
    {
        Debug.Log("退出下落状态");

        // board.animator.SetFloat("ySpeed", 0);

        board.animator.SetBool("IsFalling", false);
        board.animator.Play("code0_land");
        board.animator.SetBool("isGround", board.isGrounded);
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

       
    }

    public void OnCheck()
    {
        //Debug.Log(board.isGrounded);
        // 检查是否落地
        if (board.isGrounded)
        {

            Debug.Log("下落状态切换落地状态");
            fsm.SwitchState(StateType.Land);

        }
        // 检查是否在楼梯上并尝试攀爬
        else if (board.isOnStairs && Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.1f)
        {
            fsm.SwitchState(StateType.Climb);
        }

        if (board.rb.velocity.y <= 0 && board.isLedgeDetected)
        {
            Debug.Log("下坠到边缘");
            fsm.SwitchState(StateType.Hang);
        }
    }

    public void OnUpdate()
    {
        board.moveInput = Input.GetAxisRaw("Horizontal");
        board.animator.SetFloat("ySpeed", board.rb.velocity.y, 0.01f, Time.deltaTime);


    }
}
