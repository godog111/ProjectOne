using UnityEngine;

public class PlayerHangState : IState
{
    private FSM fsm;
    private PlayerBoard board;

    public PlayerHangState(FSM fsm)
    {
        this.fsm = fsm;
        this.board = (PlayerBoard)fsm.blackBoard;
    }

    public void OnEnter(object data = null)
    {
        // 计算角色顶部Y坐标
        float playerTopY = board.rb.position.y + board.playerColliderHeight;
        if (playerTopY >= board.detectedLedgePosition.y)
        {
            // 直接吸附到平台顶部
            float directionEnter = Mathf.Sign(board.playerTransform.localScale.x);
            float targetX = board.detectedLedgePosition.x + (directionEnter * board.climbFinalXOffset);
            float targetY = board.detectedLedgePosition.y - board.playerColliderHeight;
            
            board.rb.position = new Vector2(targetX, targetY);
            fsm.SwitchState(StateType.Idle);
            return;
        }
        // 确保Y坐标与挂靠点完全一致
        float direction = Mathf.Sign(board.playerTransform.localScale.x);
        
        // 直接使用检测到的边缘位置Y值（不再添加任何偏移）
        board.rb.position = new Vector2(
            board.detectedLedgePosition.x - (direction * board.ledgeGrabHorizontalOffset),
            board.detectedLedgePosition.y // 直接使用原始Y值
        );
        
        // 停止物理运动
        board.rb.velocity = Vector2.zero;
        board.rb.gravityScale = 0;
        
        // 更新状态
        board.isCurrentlyHanging = true;
        board.isLedgeDetected = true;
        
        // 播放动画
        board.animator.Play("code0_hung");
    }

    public void OnExit()
    {
        board.isCurrentlyHanging = false;
        // 恢复物理
        board.rb.gravityScale = board.originalGravityScale;
    }

    public void OnClick() { }

    public void OnFixUpdate()
    {
        // 保持位置（确保Y轴始终对齐）
        float direction = Mathf.Sign(board.playerTransform.localScale.x);
        board.rb.position = new Vector2(
            board.detectedLedgePosition.x - (direction * board.ledgeGrabHorizontalOffset),
            board.detectedLedgePosition.y // 每帧强制对齐Y轴
        );
    }

    public void OnCheck()
    {
        // 攀爬输入
        if (Input.GetKey(KeyCode.W) || Input.GetAxisRaw("Vertical") > 0.5f)
        {
            fsm.SwitchState(StateType.Climb);
        }
        // 松开或掉落
        else if (Input.GetKey(KeyCode.S) || !board.isLedgeDetected)
        {
            fsm.SwitchState(StateType.Fall);
        }
    }

    public void OnUpdate()
    {
        // 可以添加输入处理
    }
}