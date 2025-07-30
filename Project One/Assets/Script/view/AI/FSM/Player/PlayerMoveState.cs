using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMoveState : IState
{
    private FSM fsm;
    private PlayerBoard board;

    public PlayerMoveState(FSM fsm)
    {
        Debug.Log("移动状态开始构建");
        this.fsm = fsm;
        this.board = (PlayerBoard)fsm.blackBoard;
        if (fsm.blackBoard.animator == null) Debug.Log("父类动画器未赋值！");
        if (board.animator == null) Debug.Log("动画器未赋值！");
    }

    public void OnEnter(object data = null)
    {
        
      //  board.animator.Play("code0_move");
    }

    public void OnExit()
    {
        board.animator.SetFloat("Speed",0);
        Debug.Log("移动状态退出");
     }
    
    public void OnClick() { }
    
    public void OnFixUpdate()
    {
        // 地面检测
        board.isGrounded = Physics2D.OverlapCircle(
            board.groundCheck.position, 
            board.groundCheckRadius, 
            board.groundLayer
        );
        
        // 水平移动
        board.rb.velocity = new Vector2(
            board.moveInput * board.moveSpeed, 
            board.rb.velocity.y
        );
        Debug.Log(board.rb.velocity.y);
        
        // 转向
        if (board.moveInput != 0)
        {
            board.animator.transform.localScale = new Vector3(
                Mathf.Sign(board.moveInput) * Mathf.Abs(board.animator.transform.localScale.x),
                board.animator.transform.localScale.y,
                board.animator.transform.localScale.z
            );
        }
    }
    
    public void OnCheck()
    {
       // 
        //Debug.Log(board.isGrounded);
        // 更新输入
        board.moveInput = Input.GetAxisRaw("Horizontal");
       // Debug.Log(board.moveInput);
        // 检查状态转换
        if (Mathf.Abs(board.moveInput) < 0.01f)
        {
            Debug.Log("切换待机状态");
            board.moveInput = 0;
            board.animator.SetFloat("Speed", Mathf.Abs( board.moveInput), 0.01f, Time.deltaTime);
            fsm.SwitchState(StateType.Idle);
        }
        else if (Input.GetKeyDown(board.jumpKey) && board.isGrounded)
        {
           // Debug.Log(board.rb.velocity.y);
            Debug.Log("切换跳跃状态");
            board.animator.SetFloat("Speed", 0);
            fsm.SwitchState(StateType.Jump);
        }
        else if (!board.isGrounded && board.rb.velocity.y < 0)
        {
            Debug.Log(board.isGrounded);
            Debug.Log(board.rb.velocity.y);
            // 切换下落状态
            Debug.Log("切换下落状态");
            fsm.SwitchState(StateType.Fall);
        }
        else if (board.isOnStairs && Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.1f)
        {
            // 切换爬梯状态
            Debug.Log("切换爬梯状态");
            fsm.SwitchState(StateType.Climb);
        }
    }
    
    public void OnUpdate()
    {
       // Debug.Log(Input.GetKeyDown(board.jumpKey));
        
        
        float currentSpeed = Mathf.Abs( board.moveInput);
        // 根据速度设置动画参数
        //Debug.Log(currentSpeed);
        board.animator.SetFloat("Speed", currentSpeed, 0.01f, Time.deltaTime);
    }
}
