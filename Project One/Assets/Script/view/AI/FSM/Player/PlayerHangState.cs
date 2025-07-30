using UnityEngine;

public class PlayerHangState : IState
{
    private FSM fsm;
    private PlayerBoard board;

    private Vector2 correctedHangPosition;// 在OnEnter中存储修正后的挂靠位置

    public PlayerHangState(FSM fsm)
    {
        this.fsm = fsm;
        this.board = (PlayerBoard)fsm.blackBoard;
    }

    public void OnEnter(object data = null)
    {
        Debug.Log("挂靠点"+board.lastLedgeTopPosition.y+"角色高度"+board.playerColliderHeight+"修正点高度"+board.ledgeGrabVerticalOffset);
        // 使用记录的平台顶部坐标进行修正
        float targetY = board.lastLedgeTopPosition.y - board.playerColliderHeight+ board.ledgeGrabVerticalOffset;
        float direction = Mathf.Sign(board.playerTransform.localScale.x);
        
        // 水平位置使用最新检测结果，垂直位置使用修正值
            correctedHangPosition = new Vector2(
            board.detectedLedgePosition.x - (direction * board.ledgeGrabHorizontalOffset),
            targetY
             );

        // 保持原有物理和状态设置
        board.rb.position = correctedHangPosition;
        board.rb.velocity = Vector2.zero;
        board.rb.gravityScale = 0;
        board.isCurrentlyHanging = true;
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
        board.rb.position = correctedHangPosition;
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