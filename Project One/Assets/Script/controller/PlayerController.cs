using UnityEngine;

public class PlayerController2 : MonoBehaviour
{
    [Header("移动参数")]
    [SerializeField] private float moveSpeed = 5f;          // 移动速度
    [SerializeField] private float jumpForce = 12f;        // 跳跃力度
    [SerializeField] private float groundCheckRadius = 0.2f; // 地面检测半径
    [SerializeField] private LayerMask groundLayer;         // 地面层级
    [SerializeField] private Transform groundCheck;         // 地面检测点

    [Header("楼梯参数")]
   // [SerializeField] private float stairClimbSpeed = 3f;    // 爬梯速度
   // [SerializeField] private LayerMask stairLayer;          // 楼梯层级
    [SerializeField] private float stairEnterThreshold = 0.5f; // 进入楼梯的输入阈值
    [SerializeField] private float stairExitDelay = 0.5f;    // 退出楼梯的延迟时间

    [Header("按键设置")]
    [SerializeField] private KeyCode jumpKey = KeyCode.Space; // 跳跃按键

    // 组件引用
    private Animator animator;      // 动画控制器
    private Rigidbody2D rb;         // 刚体组件
    private float _lastInteractionTime;
    // 移动相关变量
    private float moveInput;        // 水平输入值
    private bool isGrounded;       // 是否在地面
    private Vector3 originalScale;  // 原始缩放比例

    // 楼梯相关变量
    private bool isOnStairs = false;           // 是否在楼梯上
    private bool isActivelyClimbing = false;   // 是否正在主动爬梯
    private Collider2D currentStairCollider;   // 当前楼梯碰撞体
    private float originalGravityScale;        // 原始重力比例
    private float stairExitTimer;             // 楼梯退出计时器

    private bool isJump;

    void Start()
    {
        // 初始化组件引用
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        originalScale = transform.localScale;  // 保存初始缩放
        originalGravityScale = rb.gravityScale; // 保存初始重力
    }

    void Update()
    {
        // 获取输入
        moveInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");

        // 处理角色翻转
        HandleFlip();

        // 只在有垂直输入时更新主动爬梯状态
        if (Mathf.Abs(verticalInput) > stairEnterThreshold)
        {
            isActivelyClimbing = isOnStairs;
        }
        // if (Input.GetKeyDown(jumpKey)) Debug.Log("跳");
        // 跳跃检测
        if (Input.GetKeyDown(jumpKey) && isGrounded && !isOnStairs)
        {
            Debug.Log("跳");
            Jump();
            isJump = true;
        }

        if (isJump)
        {

            if (rb.velocity.y < 0)
            {
                // rb.gravityScale = 3;
                isJump = false;
            }
            animator.SetFloat("VerticalSpeed", rb.velocity.y);
        }
        HandleInteraction();//获取交互按键
        // 更新动画参数
        UpdateAnimationParameters();
        Debug.Log(rb.velocity);
    }

    void FixedUpdate()
    {
        // 不在楼梯上时检测地面
        if (!isOnStairs)
        {
            CheckGrounded();
        }

        // 根据是否在楼梯上选择移动方式
        if (isOnStairs)
        {
            
        }
        else
        {
            HandleNormalMovement();
        }
    }

    /// <summary>
    /// 处理角色朝向翻转
    /// </summary>
    private void HandleFlip()
    {
        // 修改：调整翻转逻辑，使角色朝向与移动方向一致
        if (moveInput > 0.1f)
        {
            transform.localScale = originalScale;  // 向右移动时保持原始朝向
        }
        else if (moveInput < -0.1f)
        {
            transform.localScale = new Vector3(-originalScale.x, originalScale.y, originalScale.z); // 向左移动时翻转
        }
    }

    /// <summary>
    /// 检测角色是否在地面
    /// </summary>
    private void CheckGrounded()
    {
        bool newGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // 地面状态变化时更新
        if (newGrounded != isGrounded)
        {
            isGrounded = newGrounded;
            animator.SetBool("isGrounded", isGrounded);

            if (isGrounded)
            {
                animator.SetTrigger("Land");  // 落地动画
            }
        }
    }

    /// <summary>
    /// 处理普通地面移动
    /// </summary>
    private void HandleNormalMovement()
    {
        if(isGrounded)
            rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);
    }





    /// <summary>
    /// 执行跳跃
    /// </summary>
    private void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        
        //animator.SetFloat("VerticalSpeed", rb.velocity.y);
        isGrounded = false;
    }

    /// <summary>
    /// 更新动画参数
    /// </summary>
    private void UpdateAnimationParameters()
    {
        float currentSpeed = Mathf.Abs(moveInput);


        // 设置动画参数
        animator.SetFloat("Speed", currentSpeed, 0.001f, Time.deltaTime);
        animator.SetBool("isGrounded", isGrounded);
        if (isJump)
        //    { animator.SetFloat("VerticalSpeed", rb.velocity.y); }// 触发跳跃动画
        animator.SetBool("OnStairs", isOnStairs);
    }

    /// <summary>
    /// 绘制调试Gizmos
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
    
    /// <summary>
    /// 处理玩家交互输入的核心方法
    /// 每帧检测玩家是否尝试与最近的交互对象进行交互
    /// </summary>    /// <summary>
    /// 处理玩家交互输入

    private void HandleInteraction()
    {
        if(InteractionManager.Instance._playerTransform ==null) return;
        IInteractable interactable = InteractionManager.Instance.GetCurrentClosestInteractable();
        if (interactable == null) return;

        // 通过管理器获取实际有效的交互键
        KeyCode interactKey = InteractionManager.Instance.GetEffectiveInteractKey(interactable);
        
        if (Input.GetKeyDown(interactKey))
        {
            if (Time.time > _lastInteractionTime + interactable.GetInteractionCooldown())
            {
                if (interactable.CanInteract())
                {
                    interactable.Interact();
                    _lastInteractionTime = Time.time;
                    
                    // 播放交互反馈

                }
            }
        }
    }
}