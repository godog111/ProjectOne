using UnityEngine;

public class PlayerClimbState : IState
{
    private FSM fsm;
    private PlayerBoard board;
    private float timer;
    private Transform playerTransform;
    private Vector3[] climbPath; // 攀爬路径关键点
    private float targetLedgeTopY; // 攀爬目标的顶部Y坐标

    public PlayerClimbState(FSM fsm)
    {
        this.fsm = fsm;
        this.board = (PlayerBoard)fsm.blackBoard;
        this.playerTransform = board.rb.transform;
    }

    public void OnEnter(object data = null)
    {
        timer = 0f;
        
        // 计算目标顶部Y坐标（攀爬目标的Y + 角色碰撞器高度）
        targetLedgeTopY = board.detectedLedgePosition.y + board.playerColliderHeight;
        
        // 初始位置：对齐角色底部与目标底部
        Vector2 startPos = new Vector2(
            board.rb.position.x,
            targetLedgeTopY - board.playerColliderHeight
        );
        board.rb.position = startPos;
        
        // 计算终点位置（应用水平偏移）
        float direction = Mathf.Sign(board.playerTransform.localScale.x);
        Vector2 endPos = new Vector2(
            board.detectedLedgePosition.x + (direction * board.climbFinalXOffset),
            targetLedgeTopY - board.playerColliderHeight + board.climbFinalYOffset
        );
        
        // 存储路径点（确保Y轴严格对齐）
        climbPath = new Vector3[] {
            startPos,
            new Vector3(
                startPos.x, 
                targetLedgeTopY + board.climbFinalYOffset, // 垂直上升到目标高度
                0
            ),
            endPos
        };
        
        board.animator.Play("code0_climb");
    }

    public void OnExit()
    {
        // 确保最终位置精确落在终点（Y轴严格对齐）
        board.rb.position = new Vector2(
            climbPath[2].x,
            targetLedgeTopY - board.playerColliderHeight + board.climbFinalYOffset
        );
    }

    public void OnClick() { }

    public void OnFixUpdate()
    {
        timer += Time.fixedDeltaTime;
        float progress = Mathf.Clamp01(timer / board.climbDuration);
        
        // 使用严格对齐的Y轴计算
        Vector3 currentPos = CalculatePositionOnPath(progress);
        
        // 应用位置（Y轴严格对齐）
        board.rb.position = new Vector2(
            currentPos.x,
            currentPos.y
        );
    }

    /// <summary>
    /// 根据进度计算攀爬路径上的位置（确保Y轴对齐）
    /// </summary>
    private Vector3 CalculatePositionOnPath(float progress)
    {
        if (progress <= 0.5f)
        {
            // 第一阶段：垂直上升 (0% -> 50%时间)
            float verticalProgress = progress * 2f;
            return new Vector3(
                climbPath[0].x, // X轴保持不变
                Mathf.Lerp(
                    climbPath[0].y,
                    climbPath[1].y,
                    Mathf.Pow(verticalProgress, board.climbSmoothing)
                ),
                0
            );
        }
        else
        {
            // 第二阶段：水平移动 (50% -> 100%时间)
            float horizontalProgress = (progress - 0.5f) * 2f;
            return new Vector3(
                Mathf.Lerp(
                    climbPath[1].x,
                    climbPath[2].x,
                    Mathf.Pow(horizontalProgress, 1f / board.climbSmoothing)
                ),
                climbPath[1].y, // Y轴保持顶点高度
                0
            );
        }
    }

    public void OnCheck()
    {
        if (timer >= board.climbDuration)
        {
            fsm.SwitchState(StateType.Idle);
        }
    }

    public void OnUpdate()
    {
        // 更新动画
       // OnDrawGizmos();
    }
    
    #if UNITY_EDITOR
    public void OnDrawGizmos()
    {
        if (climbPath == null || climbPath.Length < 3) return;

        Gizmos.color = Color.green;
        Gizmos.DrawLine(climbPath[0], climbPath[1]);
        Gizmos.DrawLine(climbPath[1], climbPath[2]);

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(climbPath[0], 0.05f);
        Gizmos.DrawSphere(climbPath[1], 0.05f);
        Gizmos.DrawSphere(climbPath[2], 0.08f);
    }
    #endif
}