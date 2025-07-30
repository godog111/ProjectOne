using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


[Serializable]
public class PlayerBoard : BlackBoard
{
    [Header("移动参数")]
   // [SerializeField] public float moveSpeed = 5f;
    [SerializeField] public float jumpForce = 12f;
    [SerializeField] public float groundCheckRadius = 0.2f;
    [SerializeField] public LayerMask groundLayer;
    [SerializeField] public Transform groundCheck;

    [Header("楼梯参数")] 
    [SerializeField] public float climbSpeed = 3f;
    [SerializeField] public float stairExitDelay = 0.2f;
    
    [Header("按键设置")]
    [SerializeField] public KeyCode jumpKey = KeyCode.Space;
    [SerializeField] public KeyCode climbKey = KeyCode.W;

    [Header("边缘攀爬参数")]
    // 移除 ledgeClimbOffset
    [SerializeField] public float ledgeDetectionRange = 0.3f;
    [Tooltip("挂靠时角色的Y坐标将完全对齐检测点，此参数仅控制水平偏移")]
    [SerializeField] public float ledgeGrabHorizontalOffset = 0.2f; // 保持水平抓取偏移
    [SerializeField] public float ledgeGrabVerticalOffset = 0.1f;  // 保持垂直抓取偏移
    [SerializeField] public Transform ledgeDetectTop;
    [SerializeField] public Transform ledgeDetectMid;
    
    [Header("组件引用")]
    [HideInInspector] public Rigidbody2D rb;
   // [HideInInspector] public Animator animator;
    [HideInInspector] public Transform playerTransform;
    
    [Header("状态变量")]
    [HideInInspector] public float moveInput;
    [HideInInspector] public bool isGrounded;
    [HideInInspector] public bool isOnStairs;
    [HideInInspector] public Collider2D currentStairCollider;
    [HideInInspector] public float originalGravityScale;
    [HideInInspector] public bool isLedgeDetected;// 是否检测到可抓取边缘
    [HideInInspector] public Vector2 detectedLedgePosition;
    [HideInInspector] public bool isCurrentlyHanging; // 当前是否正在挂靠
    
    [Header("障碍物检测参数")]
    [SerializeField] public float obstacleDetectionAngle = 45f; // 检测角度
    [SerializeField] public float obstacleDetectionRange = 0.5f; // 检测范围

    [Header("攀爬参数")]
    // 统一使用一组参数
    [Tooltip("攀爬完成后的水平偏移")]
    [SerializeField] public float climbFinalXOffset = 0.3f; // 替代 climbXOffset/horizontalClimbOffset

    [Tooltip("攀爬完成后的垂直偏移")]
    [SerializeField] public float climbFinalYOffset = 0.1f; // 替代 climbYOffset/verticalClimbHeight

    [Tooltip("攀爬动作总时长（秒）")]
    [SerializeField] public float climbDuration = 0.8f;

    [Tooltip("攀爬路径的平滑度")]
    [Range(0.1f, 1f)] public float climbSmoothing = 0.5f;

    [Tooltip("角色碰撞器高度")]
    [SerializeField] public float playerColliderHeight = 0.5f;
    

    public bool IsValid()
    {
        return animator != null && rb != null &&
               groundCheck != null && playerTransform != null;
    }
}

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private PlayerBoard board;
    private FSM fsm;
    private bool fsmInitialized = false;

    private void Awake()
    {
        if (board.animator == null) board.animator = GetComponent<Animator>();
        if (board.rb == null) board.rb = GetComponent<Rigidbody2D>();
        board.playerTransform = transform;
        board.originalGravityScale = board.rb.gravityScale;
    }

    private void Start()
    {
        InitializeFSM();
        fsm.AddState(StateType.Idle, new PlayerIdleState(fsm));
        fsm.AddState(StateType.Move, new PlayerMoveState(fsm));
        fsm.AddState(StateType.Jump, new PlayerJumpState(fsm));
        fsm.AddState(StateType.Fall, new PlayerFallState(fsm));
        fsm.AddState(StateType.Stairs, new PlayerStairsState(fsm));
        fsm.AddState(StateType.Climb, new PlayerClimbState(fsm));
        fsm.AddState(StateType.Land, new PlayerLandState(fsm));
        fsm.AddState(StateType.Hang, new PlayerHangState(fsm));
        fsm.SwitchState(StateType.Idle);
    }

    private void InitializeFSM()
    {
        if (!board.IsValid())
        {
            Debug.LogError("PlayerBoard 组件未正确初始化!");
            return;
        }
        fsm = new FSM(board);
        fsmInitialized = true;
        StartCoroutine(DelayedStateSwitch());
    }

    private IEnumerator DelayedStateSwitch()
    {
        yield return null;
        fsm.SwitchState(StateType.Idle);
    }

    private void Update()
    {
        if (!fsmInitialized || board == null || fsm == null) return;
        
        // 基础状态检测
        board.isGrounded = Physics2D.OverlapCircle(
            board.groundCheck.position,
            board.groundCheckRadius,
            board.groundLayer
        );
        
        board.moveInput = Input.GetAxisRaw("Horizontal");
        
        // 边缘和障碍物检测
        board.isLedgeDetected = CheckLedgeOrObstacle();
        Debug.Log(board.isLedgeDetected);
        fsm.OnUpdate();
        fsm.OnCheck();

        // 只有不在挂靠状态时才更新边缘检测
        if (!board.isCurrentlyHanging)
        {
            board.isLedgeDetected = CheckLedgeOrObstacle();
        }
        
        // 状态转换逻辑
        if (board.isLedgeDetected &&
           (fsm.currentState is PlayerJumpState || fsm.currentState is PlayerFallState))
        {
            fsm.SwitchState(StateType.Hang);
        }
    }

    private bool CheckLedgeOrObstacle()
    {
        if (board.isCurrentlyHanging) return true;
        float direction = Mathf.Sign(board.playerTransform.localScale.x);
        
        // 1. 优先检测平台边缘（原有逻辑）
        RaycastHit2D topHit = Physics2D.Raycast(
            board.ledgeDetectTop.position,
            Vector2.right * direction,
            board.ledgeDetectionRange,
            board.groundLayer
        );
        
        bool midCheck = !Physics2D.Raycast(
            board.ledgeDetectMid.position,
            Vector2.right * direction,
            board.ledgeDetectionRange,
            board.groundLayer
        );
        
        if (topHit.collider != null && midCheck)
        {
            board.detectedLedgePosition = new Vector2(
                topHit.point.x - (direction * board.ledgeGrabHorizontalOffset),
                topHit.point.y - board.ledgeGrabVerticalOffset
            );
            return true;
        }
        
        // 2. 改进的障碍物检测逻辑
        // 从玩家头顶向斜上方检测障碍物
        Vector2 obstacleCheckStart = board.ledgeDetectTop.position;
        RaycastHit2D obstacleHit = Physics2D.Raycast(
            obstacleCheckStart,
            new Vector2(direction, 1).normalized, // 45度角斜向检测
            board.ledgeDetectionRange * 1.5f,    // 稍长的检测距离
            board.groundLayer
        );
        
        if (obstacleHit.collider != null)
        {
            // 从障碍物碰撞点向下检测，确认是否有可抓取的边缘
            RaycastHit2D downHit = Physics2D.Raycast(
                new Vector2(obstacleHit.point.x, obstacleHit.point.y - 0.1f),
                Vector2.down,
                board.ledgeGrabVerticalOffset * 2f,
                board.groundLayer
            );
            
            // 如果没有检测到下方碰撞，说明这是可抓取的边缘
            if (downHit.collider == null)
            {
                board.detectedLedgePosition = new Vector2(
                    obstacleHit.point.x - (direction * board.ledgeGrabHorizontalOffset),
                    obstacleHit.point.y - board.ledgeGrabVerticalOffset
                );
                return true;
            }
        }
        
        return false;
    }

    private void FixedUpdate()
    {
        if (fsmInitialized) fsm.OnFixUpdate();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Stairs"))
        {
            board.isOnStairs = true;
            board.currentStairCollider = other;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Stairs"))
        {
            board.isOnStairs = false;
            board.currentStairCollider = null;
        }
    }

    private void OnDrawGizmos()
    {
        if (board == null) return;
        
        // 地面检测
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(board.groundCheck.position, board.groundCheckRadius);
        
        // 边缘检测
        if (board.ledgeDetectTop != null && board.ledgeDetectMid != null)
        {
            float direction = Mathf.Sign(transform.localScale.x);
            
            Gizmos.color = Color.red;
            Gizmos.DrawLine(
                board.ledgeDetectTop.position,
                board.ledgeDetectTop.position + Vector3.right * direction * board.ledgeDetectionRange
            );
            
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(
                board.ledgeDetectMid.position,
                board.ledgeDetectMid.position + Vector3.right * direction * board.ledgeDetectionRange
            );
            
            // 障碍物检测
            //direction = Mathf.Sign(transform.localScale.x);
            Vector2 obstacleDir = new Vector2(direction, 1).normalized;
            
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(
                board.ledgeDetectTop.position,
                board.ledgeDetectTop.position + (Vector3)(obstacleDir * board.ledgeDetectionRange * 1.5f)
            );
        }
        
        // 检测到的边缘位置
        if (board.isLedgeDetected)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(board.detectedLedgePosition, 0.15f);
        }
    }
}