using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PatrolBlackBoard : BlackBoard
{
    [Header("Movement Settings")]
   // public float moveSpeed = 2f;
    public float patrolDistance = 5f;
   // public float idleTime = 2f;

    [Header("Vision Settings")]
    [Tooltip("控制检测和视觉范围的大小")]
    public float detectionRange = 5f;
    [Range(0, 360)]
    public float detectionAngle = 90f;
    public LayerMask obstacleLayer;
    public LayerMask playerLayer;

    [Header("Vision Cone Settings")]
    public float coneZOffset = 0.1f;
    public float coneForwardOffset = 0.5f;
    [Range(10, 72)]
    public int coneSegments = 36;
    public Material visionConeMaterial;

    [Header("Rendering Settings")]
    [Tooltip("渲染层级名称")]
    public string sortingLayerName = "Foreground";
    [Tooltip("渲染顺序（值越大显示越靠前）")]
    public int sortingOrder = 0;

    [Header("Detection Visuals")]
    public GameObject exclamationMark;//感叹号资源
    public Color normalColor = new Color(1, 1, 0, 0.3f); // 半透明黄色
    public Color suspiciousColor = Color.yellow;
    public Color alertColor = Color.red;
    [Tooltip("视觉锥形相对于检测范围的缩放比例")]
    public float visualScaleFactor = 1.0f; // 1.0表示与检测范围相同

    // 运行时变量
    [HideInInspector] public Vector2 startPosition;
    [HideInInspector] public Vector2 patrolTarget;
    [HideInInspector] public bool movingRight = true;
    [HideInInspector] public bool playerDetected=false;
    [HideInInspector] public float idleTimer;
    [HideInInspector] public MeshRenderer visionConeRenderer;
    [HideInInspector] public float currentConeSize;
    [HideInInspector] public Transform player;
    [HideInInspector] public Transform npcTransform;
    [HideInInspector] public Rigidbody2D npcRigidbody;

    /// <summary>
    /// 检查玩家是否在视野范围内
    /// </summary>
    /// <returns>如果玩家可见返回true，否则返回false</returns>
    public bool CheckPlayerVisibility()
    {
        // 如果玩家不存在，直接返回false
        if (player == null || !player.gameObject.activeInHierarchy)
        {
            return false;
        }

        // 计算到玩家的方向和距离
        Vector2 directionToPlayer = (player.position - npcTransform.position).normalized;
        float distanceToPlayer = Vector2.Distance(npcTransform.position, player.position);
        
        // 1. 检查是否在检测范围内
        if (distanceToPlayer > detectionRange)
        {
            return false;
        }
        
        // 2. 检查是否在视野角度内
        float angleToPlayer = Vector2.Angle(GetFacingDirection(), directionToPlayer);
        if (angleToPlayer > detectionAngle / 2f)
        {
            return false;
        }
        
        // 3. 检查是否有障碍物阻挡视线
        RaycastHit2D hit = Physics2D.Raycast(
            npcTransform.position, 
            directionToPlayer, 
            distanceToPlayer, 
            obstacleLayer);
            
        if (hit.collider != null)
        {
            return false;
        }
        
        // 4. 最终检查玩家是否在目标位置
        RaycastHit2D playerHit = Physics2D.Raycast(
            npcTransform.position, 
            directionToPlayer, 
            distanceToPlayer, 
            playerLayer);
            
        return playerHit.collider != null && playerHit.collider.CompareTag("Player");
    }

    /// <summary>
    /// 获取当前面向方向
    /// </summary>
    /// <returns>返回当前面向方向的向量</returns>
    public Vector2 GetFacingDirection()
    {
        return movingRight ? Vector2.right : Vector2.left;
    }

    /// <summary>
    /// 更新视野锥形的颜色
    /// </summary>
    public void UpdateVisionConeColor()
    {
        if (visionConeRenderer == null || visionConeRenderer.material == null)
        {
            Debug.LogWarning("视野锥形渲染器或材质丢失");
            return;
        }

        // 根据状态选择颜色
        Color targetColor = visionConeRenderer.material.color;
        
        if (playerDetected)
        {
            targetColor = alertColor;
        }
        else
        {
            targetColor = normalColor;
        }

        // 保持透明度不变
        targetColor.a = visionConeRenderer.material.color.a;
        
        // 应用颜色
        visionConeRenderer.material.color = targetColor;
    }

    /// <summary>
    /// 更新视野锥形的方向
    /// </summary>
    public void UpdateVisionConeDirection()
    {
        if (visionConeRenderer == null)
        {
            return;
        }
        
        Transform coneTransform = visionConeRenderer.transform;
        
        // 位置偏移（根据移动方向）
        coneTransform.localPosition = new Vector3(
            movingRight ? coneForwardOffset : -coneForwardOffset, 
            0, 
            coneZOffset);
        
        // 方向控制（根据移动方向旋转）
        coneTransform.localRotation = Quaternion.Euler(0, 0, movingRight ? 0 : 180);
    }
}

public class Patrolsoldier : MonoBehaviour
{
    private FSM fsm;
    public PatrolBlackBoard blackBoard;
    protected int currentPatrolIndex = 0;
    
    void Start()
    {
        blackBoard.npcTransform = transform;
        blackBoard.npcRigidbody = GetComponent<Rigidbody2D>();
        
        // 初始化状态机
        fsm = new FSM(blackBoard);
        
        // 查找玩家对象
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            blackBoard.player = playerObj.transform;
        }
        else
        {
            Debug.LogError("未找到玩家对象！请确保场景中有且只有一个带有'Player'标签的对象");
            enabled = false;
            return;
        }
        
        // 初始化巡逻路径
        blackBoard.startPosition = transform.position;
        blackBoard.patrolTarget = blackBoard.startPosition + Vector2.right * blackBoard.patrolDistance;
        
        // 初始化感叹号提示
        if (blackBoard.exclamationMark != null)
        {
            blackBoard.exclamationMark.SetActive(false);
        }
        
        // 创建视野锥形
        CreateVisionConeVisual();
        blackBoard.currentConeSize = blackBoard.detectionRange;
        
        // 添加状态
        fsm.AddState(StateType.Idle, new SoldierIdleState(fsm));
        fsm.AddState(StateType.Patrol, new SoldierPatrolState(fsm));
        fsm.AddState(StateType.Move, new SoldierMoveState(fsm));
        
        // 初始状态
        fsm.SwitchState(StateType.Patrol);
    }

    void Update()
    {


        // 如果范围发生变化，更新锥形大小
        if (!Mathf.Approximately(blackBoard.currentConeSize, blackBoard.detectionRange))
        {
            UpdateVisionConeSize();
            blackBoard.currentConeSize = blackBoard.detectionRange;
        }
        fsm.OnUpdate(); 
        fsm.OnCheck();
    }

    private void FixedUpdate()
    {
        fsm.OnFixUpdate();
    }

    #region Vision Cone Implementation
    void CreateVisionConeVisual()
    {
        // 销毁旧的视野锥形（如果存在）
        Transform oldCone = transform.Find("VisionCone");
        if (oldCone != null)
        {
            DestroyImmediate(oldCone.gameObject);
        }
        
        // 创建新的锥形对象
        GameObject cone = new GameObject("VisionCone");
        cone.transform.SetParent(transform);
        cone.transform.localPosition = new Vector3(0, 0, blackBoard.coneZOffset);
        cone.transform.localScale = Vector3.one;
        
        // 添加网格组件
        MeshFilter meshFilter = cone.AddComponent<MeshFilter>();
        meshFilter.mesh = CreateVisionConeMesh(blackBoard.detectionRange);
        
        // 添加渲染器组件
        blackBoard.visionConeRenderer = cone.AddComponent<MeshRenderer>();
        
        // 使用指定的材质或创建默认材质
        if (blackBoard.visionConeMaterial != null)
        {
            blackBoard.visionConeRenderer.material = blackBoard.visionConeMaterial;
        }
        else
        {
            Material defaultMaterial = new Material(Shader.Find("Sprites/Default"));
            defaultMaterial.color = blackBoard.normalColor;
            blackBoard.visionConeRenderer.material = defaultMaterial;
        }
        
        // 设置渲染层级
        if (!string.IsNullOrEmpty(blackBoard.sortingLayerName))
        {
            blackBoard.visionConeRenderer.sortingLayerName = blackBoard.sortingLayerName;
        }
        blackBoard.visionConeRenderer.sortingOrder = blackBoard.sortingOrder;
        
        // 初始颜色
        blackBoard.UpdateVisionConeColor();
    }

    Mesh CreateVisionConeMesh(float range)
    {
        Mesh mesh = new Mesh();
        mesh.name = "VisionConeMesh";
        
        float angleStep = blackBoard.detectionAngle / blackBoard.coneSegments;
        float radius = range * blackBoard.visualScaleFactor;
        
        // 顶点数组：中心点 + 边缘点 + 闭合点
        Vector3[] vertices = new Vector3[blackBoard.coneSegments + 2];
        Vector2[] uv = new Vector2[vertices.Length];
        int[] triangles = new int[blackBoard.coneSegments * 3];
        
        // 中心点 (0)
        vertices[0] = Vector3.zero;
        uv[0] = new Vector2(0.5f, 0.5f);
        
        // 边缘点 (1到coneSegments+1)
        for (int i = 0; i <= blackBoard.coneSegments; i++)
        {
            float angle = -blackBoard.detectionAngle / 2 + angleStep * i;
            Vector3 dir = Quaternion.Euler(0, 0, angle) * Vector3.right;
            vertices[i + 1] = dir * radius;
            uv[i + 1] = new Vector2(
                vertices[i + 1].x / radius / 2 + 0.5f,
                vertices[i + 1].y / radius / 2 + 0.5f);
                
            // 构建三角形
            if (i < blackBoard.coneSegments)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }
        }
        
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        
        return mesh;
    }

    void UpdateVisionConeSize()
    {
        if (blackBoard.visionConeRenderer == null)
        {
            Debug.LogWarning("视野锥形渲染器丢失");
            return;
        }
        
        // 获取MeshFilter组件
        MeshFilter meshFilter = blackBoard.visionConeRenderer.GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            Debug.LogError("找不到MeshFilter组件");
            return;
        }
        
        // 重新创建网格
        meshFilter.mesh = CreateVisionConeMesh(blackBoard.detectionRange);
        
        // 重置缩放
        blackBoard.visionConeRenderer.transform.localScale = Vector3.one;
    }
    #endregion

    #if UNITY_EDITOR
    void OnDrawGizmos()
    {
        // 绘制检测范围
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, blackBoard.detectionRange);
        
        // 获取当前朝向（编辑模式下默认向右）
        Vector2 facingDir = Application.isPlaying ? blackBoard.GetFacingDirection() : Vector2.right;
        
        // 绘制视野锥形边界线
        Vector3 leftDir = Quaternion.AngleAxis(-blackBoard.detectionAngle/2, Vector3.forward) * facingDir;
        Vector3 rightDir = Quaternion.AngleAxis(blackBoard.detectionAngle/2, Vector3.forward) * facingDir;
        
        Gizmos.DrawRay(transform.position, leftDir * blackBoard.detectionRange);
        Gizmos.DrawRay(transform.position, rightDir * blackBoard.detectionRange);
        
        // 编辑模式下绘制巡逻路径
        if (!Application.isPlaying)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, transform.position + Vector3.right * blackBoard.patrolDistance);
            Gizmos.DrawWireSphere(transform.position + Vector3.right * blackBoard.patrolDistance, 0.2f);
        }
    }
    #endif
}

public class SoldierPatrolState : IState
{
    private FSM fsm;
    private PatrolBlackBoard blackBoard;
    private Rigidbody2D rb;

    public SoldierPatrolState(FSM fsm)
    {
        this.fsm = fsm;
        this.blackBoard = fsm.blackBoard as PatrolBlackBoard;
        this.rb = blackBoard.npcRigidbody;
    }

    public void OnCheck()
    {
        // 检查是否能看见玩家
        bool canSeePlayer = blackBoard.CheckPlayerVisibility();
        
        if (canSeePlayer)
        {
            // 如果看到玩家
            if (!blackBoard.playerDetected)
            {
                blackBoard.playerDetected = true;
                rb.velocity = Vector2.zero;
                
                // 显示感叹号
                if (blackBoard.exclamationMark != null)
                {
                    blackBoard.exclamationMark.SetActive(true);
                }
                
                Debug.Log("警报！发现玩家！");
            }
        }
        else
        {
            // 如果看不到玩家但之前已发现玩家
            if (blackBoard.playerDetected)
            {
                if (blackBoard.exclamationMark != null)
                {
                    blackBoard.exclamationMark.SetActive(false);
                }
            }
        }
        
        // 更新锥形颜色
        blackBoard.UpdateVisionConeColor();
    }

    public void OnClick()
    {
        // 点击事件处理
    }

    public void OnEnter(object data = null)
    {
        // 进入巡逻状态
        //Debug.Log("进入巡逻状态");
    }

    public void OnExit()
    {
        // 退出巡逻状态
      //  Debug.Log("退出巡逻状态");
    }

    public void OnFixUpdate()
    {
        // 固定更新逻辑
    }

    public void OnUpdate()
    {
        // 巡逻移动逻辑
        if (!blackBoard.playerDetected)
        {
           // Debug.Log("正在巡逻中");
            PatrolMovement();
        }
        
        // 更新锥形方向
        blackBoard.UpdateVisionConeDirection();
    }

    void PatrolMovement()
    {
        if (blackBoard.movingRight)
        {
            // 向右移动
            rb.velocity = new Vector2(blackBoard.moveSpeed, rb.velocity.y);
            
            // 检查是否到达右侧巡逻点
            if (Vector2.Distance(rb.transform.position, blackBoard.patrolTarget) < 0.1f)
            {
                blackBoard.idleTimer += Time.deltaTime;
                rb.velocity = Vector2.zero;
                
                // 停留时间结束后转向
                if (blackBoard.idleTimer >= blackBoard.idleTime)
                {
                    blackBoard.movingRight = false;
                    blackBoard.idleTimer = 0f;
                    fsm.SwitchState(StateType.Idle);
                }
            }
        }
        else
        {
            // 向左移动
            rb.velocity = new Vector2(-blackBoard.moveSpeed, rb.velocity.y);
            
            // 检查是否返回起始位置
            if (Vector2.Distance(rb.transform.position, blackBoard.startPosition) < 0.1f)
            {
                blackBoard.idleTimer += Time.deltaTime;
                rb.velocity = Vector2.zero;
                
                // 停留时间结束后转向
                if (blackBoard.idleTimer >= blackBoard.idleTime)
                {
                    blackBoard.movingRight = true;
                    blackBoard.idleTimer = 0f;
                    fsm.SwitchState(StateType.Idle);
                }
            }
        }
    }
}

public class SoldierIdleState : IState
{
    private FSM fsm;
    private PatrolBlackBoard blackBoard;
    private float idleTimer;

    public SoldierIdleState(FSM fsm)
    {
        this.fsm = fsm;
        this.blackBoard = fsm.blackBoard as PatrolBlackBoard;
    }

    public void OnCheck()
    {
        // 检查是否能看见玩家
        bool canSeePlayer = blackBoard.CheckPlayerVisibility();
        
        if (canSeePlayer)
        {
            // 如果看到玩家
            blackBoard.playerDetected = true;
            
            // 显示感叹号
            if (blackBoard.exclamationMark != null)
            {
                blackBoard.exclamationMark.SetActive(true);
            }
            
            Debug.Log("警报！发现玩家！");
        }
        
        // 更新锥形颜色
        blackBoard.UpdateVisionConeColor();
    }

    public void OnClick()
    {
        // 点击事件处理
    }

    public void OnEnter(object data = null)
    {
        // 进入待机状态
       // Debug.Log("进入待机状态");
        idleTimer = 0f;
    }

    public void OnExit()
    {
        // 退出待机状态
      //  Debug.Log("退出待机状态");
    }

    public void OnFixUpdate()
    {
        // 固定更新逻辑
    }

    public void OnUpdate()
    {
        // 待机计时
        idleTimer += Time.deltaTime;
        
        // 待机时间结束后返回巡逻状态
        if (idleTimer >= blackBoard.idleTime)
        {
            fsm.SwitchState(StateType.Patrol);
        }
        
        // 更新锥形方向
        blackBoard.UpdateVisionConeDirection();
    }
}

public class SoldierMoveState : IState
{
    private FSM fsm;
    private PatrolBlackBoard blackBoard;
    private Rigidbody2D rb;

    public SoldierMoveState(FSM fsm)
    {
        this.fsm = fsm;
        this.blackBoard = fsm.blackBoard as PatrolBlackBoard;
        this.rb = blackBoard.npcRigidbody;
    }

    public void OnCheck()
    {
        // 检查是否能看见玩家
        bool canSeePlayer = blackBoard.CheckPlayerVisibility();
        
        if (canSeePlayer)
        {
            // 如果看到玩家
            blackBoard.playerDetected = true;
            
            // 显示感叹号
            if (blackBoard.exclamationMark != null)
            {
                blackBoard.exclamationMark.SetActive(true);
            }
            
            Debug.Log("警报！发现玩家！");
        }
        
        // 更新锥形颜色
        blackBoard.UpdateVisionConeColor();
    }

    public void OnClick()
    {
        // 点击事件处理
    }

    public void OnEnter(object data = null)
    {
        // 进入移动状态
        Debug.Log("进入移动状态");
    }

    public void OnExit()
    {
        // 退出移动状态
        Debug.Log("退出移动状态");
    }

    public void OnFixUpdate()
    {
        // 固定更新逻辑
    }

    public void OnUpdate()
    {
        // 移动逻辑
        // 这里可以根据需要实现追击玩家或其他移动逻辑
        
        // 更新锥形方向
        blackBoard.UpdateVisionConeDirection();
    }
}