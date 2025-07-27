using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


[Serializable]
public class PlayerBoard : BlackBoard
{
    [Header("移动参数")]
    [SerializeField] public float plyaermoveSpeed = 5f;          // 移动速度
    [SerializeField] public float jumpForce = 12f;        // 跳跃力度
    [SerializeField] public float groundCheckRadius = 0.2f; // 地面检测半径
    [SerializeField] public LayerMask groundLayer;         // 地面层级
    [SerializeField] public Transform groundCheck;         // 地面检测点

    [Header("攀爬参数")]
    [SerializeField] public float climbSpeed = 3f;        // 攀爬速度
    [SerializeField] public float stairExitDelay = 0.2f; // 楼梯退出延迟

    [Header("按键设置")]
    [SerializeField] public KeyCode jumpKey = KeyCode.Space; // 跳跃按键
    [SerializeField] public KeyCode climbKey = KeyCode.W;   // 攀爬按键

    // 组件引用
   // [HideInInspector] public Animator playeranimator;      // 动画控制器
    [HideInInspector] public Rigidbody2D rb;         // 刚体组件
    [HideInInspector] public float moveInput;        // 水平输入值
    [HideInInspector] public bool isGrounded;       // 是否在地面
    [HideInInspector] public bool isOnStairs = false; // 是否在楼梯上
    [HideInInspector] public Collider2D currentStairCollider; // 当前楼梯碰撞体
    [HideInInspector] public float originalGravityScale; // 原始重力比例

    public bool IsValid()
    {
        return animator != null && rb != null && groundCheck != null;
    }
}

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class PlayerController: MonoBehaviour
{

    
    [SerializeField] private PlayerBoard board; // 现在通过编辑器赋值
   
    private FSM fsm;
    private bool fsmInitialized = false;

    private void Awake()
    {
        // 确保组件存在
        if (board.animator == null) board.animator = GetComponent<Animator>();
        if (board.rb == null) board.rb = GetComponent<Rigidbody2D>();
        
       
    }


    private void Start()
    {

        InitializeFSM();
        fsm.AddState(StateType.Idle, new PlayerIdleState(fsm));
        fsm.AddState(StateType.Move, new PlayerMoveState(fsm));
        fsm.AddState(StateType.Jump, new PlayerJumpState(fsm));
        fsm.AddState(StateType.Fall, new PlayerFallState(fsm));
        fsm.AddState(StateType.Climb, new PlayerClimbState(fsm));

        fsm.SwitchState(StateType.Idle);
    }
    
    private void InitializeFSM()
    {
        if (!board.IsValid())
        {
           
            return;
        }

        fsm = new FSM(board);
        fsmInitialized = true;
        
        // 延迟一帧再切换状态（关键修复！）
        StartCoroutine(DelayedStateSwitch());
    }

    private IEnumerator DelayedStateSwitch()
    {
        yield return null; // 等待一帧
        fsm.SwitchState(StateType.Idle);
    }

    private void Update()
    {
        // 更新输入
        // board.moveInput = Input.GetAxisRaw("Horizontal");
        if (board == null || fsm == null)
        {

            return;
        }

        board.moveInput = Input.GetAxisRaw("Horizontal");
        fsm.OnUpdate();
        fsm.OnCheck();
       // Debug.Log(fsm.currentState);
    
    }

    private void FixedUpdate()
    {
        fsm.OnFixUpdate();
    }

    // 梯子检测
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


}