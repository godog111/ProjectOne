using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class NPCVisionController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 2f;
    public float patrolDistance = 5f;
    public float idleTime = 2f;
    
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
    public GameObject exclamationMark;
    public Color normalColor = new Color(1, 1, 0, 0.3f); // 半透明黄色
    public Color suspiciousColor = Color.yellow;
    public Color alertColor = Color.red;
    [Header("Vision Cone Settings")]
    [Tooltip("视觉锥形相对于检测范围的缩放比例")]
    public float visualScaleFactor = 1.0f; // 1.0表示与检测范围相同
    
    // 私有变量
    private Rigidbody2D rb;
    private Transform player;
    private Vector2 startPosition;
    private Vector2 patrolTarget;
    private float idleTimer;
    private bool movingRight = true;
    private bool playerDetected;
    private MeshRenderer visionConeRenderer;
    private float currentConeSize;
    
    public enum DetectionState { None, Suspicious, Alert }
    [Tooltip("当前检测状态")]
    public DetectionState currentState = DetectionState.None;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // 查找玩家对象
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if(playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogError("未找到玩家对象！请确保场景中有且只有一个带有'Player'标签的对象");
            enabled = false;
            return;
        }
        
        // 初始化巡逻路径
        startPosition = transform.position;
        patrolTarget = startPosition + Vector2.right * patrolDistance;
        
        // 初始化感叹号提示
        if(exclamationMark != null)
        {
            exclamationMark.SetActive(false);
        }
        
        // 创建视野锥形
        CreateVisionConeVisual();
        currentConeSize = detectionRange;
    }

    void Update()
    {
        // 巡逻移动逻辑
        if(!playerDetected)
        {
            PatrolMovement();
        }
        
        // 检测逻辑
        DetectionLogic();
        
        // 更新锥形方向
        UpdateVisionConeDirection();
        
        // 如果范围发生变化，更新锥形大小
        if(!Mathf.Approximately(currentConeSize, detectionRange))
        {
            UpdateVisionConeSize();
            currentConeSize = detectionRange;
        }
    }

    void PatrolMovement()
    {
        if(movingRight)
        {
            // 向右移动
            rb.velocity = new Vector2(moveSpeed, rb.velocity.y);
            
            // 检查是否到达右侧巡逻点
            if(Vector2.Distance(transform.position, patrolTarget) < 0.1f)
            {
                idleTimer += Time.deltaTime;
                rb.velocity = Vector2.zero;
                
                // 停留时间结束后转向
                if(idleTimer >= idleTime)
                {
                    movingRight = false;
                    idleTimer = 0f;
                }
            }
        }
        else
        {
            // 向左移动
            rb.velocity = new Vector2(-moveSpeed, rb.velocity.y);
            
            // 检查是否返回起始位置
            if(Vector2.Distance(transform.position, startPosition) < 0.1f)
            {
                idleTimer += Time.deltaTime;
                rb.velocity = Vector2.zero;
                
                // 停留时间结束后转向
                if(idleTimer >= idleTime)
                {
                    movingRight = true;
                    idleTimer = 0f;
                }
            }
        }
    }

    void DetectionLogic()
    {
        // 如果玩家不存在，重置状态
        if(player == null)
        {
            currentState = DetectionState.None;
            UpdateVisionConeColor();
            return;
        }

        // 检查是否能看见玩家
        bool canSeePlayer = CheckPlayerVisibility();
        
        if(canSeePlayer)
        {
            // 如果看到玩家且当前不是警报状态
            if(currentState != DetectionState.Alert)
            {
                currentState = DetectionState.Alert;
                playerDetected = true;
                rb.velocity = Vector2.zero;
                
                // 显示感叹号
                if(exclamationMark != null)
                {
                    exclamationMark.SetActive(true);
                }
                
                Debug.Log("警报！发现玩家！");
            }
        }
        else
        {
            // 如果看不到玩家但之前已发现玩家
            if(playerDetected)
            {
                currentState = DetectionState.Suspicious;
                if(exclamationMark != null)
                {
                    exclamationMark.SetActive(false);
                }
                
                // 3秒后重置检测状态
                Invoke("ResetDetection", 3f);
            }
            else
            {
                currentState = DetectionState.None;
            }
        }
        
        // 更新锥形颜色
        UpdateVisionConeColor();
    }

    void ResetDetection()
    {
        playerDetected = false;
        currentState = DetectionState.None;
    }

    bool CheckPlayerVisibility()
    {
        // 如果玩家不存在，直接返回false
        if(player == null || !player.gameObject.activeInHierarchy)
        {
            return false;
        }

        // 计算到玩家的方向和距离
        Vector2 directionToPlayer = (player.position - transform.position).normalized;
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        // 1. 检查是否在检测范围内
        if(distanceToPlayer > detectionRange)
        {
            return false;
        }
        
        // 2. 检查是否在视野角度内
        float angleToPlayer = Vector2.Angle(GetFacingDirection(), directionToPlayer);
        if(angleToPlayer > detectionAngle / 2f)
        {
            return false;
        }
        
        // 3. 检查是否有障碍物阻挡视线
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position, 
            directionToPlayer, 
            distanceToPlayer, 
            obstacleLayer);
            
        if(hit.collider != null)
        {
            return false;
        }
        
        // 4. 最终检查玩家是否在目标位置
        RaycastHit2D playerHit = Physics2D.Raycast(
            transform.position, 
            directionToPlayer, 
            distanceToPlayer, 
            playerLayer);
            
        return playerHit.collider != null && playerHit.collider.CompareTag("Player");
    }

    Vector2 GetFacingDirection()
    {
        return movingRight ? Vector2.right : Vector2.left;
    }

    #region Vision Cone Implementation
    void CreateVisionConeVisual()
    {
        // 销毁旧的视野锥形（如果存在）
        Transform oldCone = transform.Find("VisionCone");
        if(oldCone != null)
        {
            DestroyImmediate(oldCone.gameObject);
        }
        
        // 创建新的锥形对象
        GameObject cone = new GameObject("VisionCone");
        cone.transform.SetParent(transform);
        cone.transform.localPosition = new Vector3(0, 0, coneZOffset);
        cone.transform.localScale = Vector3.one;
        
        // 添加网格组件
        MeshFilter meshFilter = cone.AddComponent<MeshFilter>();
        meshFilter.mesh = CreateVisionConeMesh(detectionRange);
        
        // 添加渲染器组件
        visionConeRenderer = cone.AddComponent<MeshRenderer>();
        
        // 使用指定的材质或创建默认材质
        if(visionConeMaterial != null)
        {
            visionConeRenderer.material = visionConeMaterial;
        }
        else
        {
            Material defaultMaterial = new Material(Shader.Find("Sprites/Default"));
            defaultMaterial.color = normalColor;
            visionConeRenderer.material = defaultMaterial;
        }
        
        // 设置渲染层级
        if(!string.IsNullOrEmpty(sortingLayerName))
        {
            visionConeRenderer.sortingLayerName = sortingLayerName;
        }
        visionConeRenderer.sortingOrder = sortingOrder;
        
        // 初始颜色
        UpdateVisionConeColor();
    }

    Mesh CreateVisionConeMesh(float range)
    {
        Mesh mesh = new Mesh();
        mesh.name = "VisionConeMesh";
        
        float angleStep = detectionAngle / coneSegments;
        float radius = range* visualScaleFactor;
        
        // 顶点数组：中心点 + 边缘点 + 闭合点
        Vector3[] vertices = new Vector3[coneSegments + 2];
        Vector2[] uv = new Vector2[vertices.Length];
        int[] triangles = new int[coneSegments * 3];
        
        // 中心点 (0)
        vertices[0] = Vector3.zero;
        uv[0] = new Vector2(0.5f, 0.5f);
        
        // 边缘点 (1到coneSegments+1)
        for(int i = 0; i <= coneSegments; i++)
        {
            float angle = -detectionAngle / 2 + angleStep * i;
            Vector3 dir = Quaternion.Euler(0, 0, angle) * Vector3.right;
            vertices[i + 1] = dir * radius;
            uv[i + 1] = new Vector2(
                vertices[i + 1].x / radius / 2 + 0.5f,
                vertices[i + 1].y / radius / 2 + 0.5f);
                
            // 构建三角形
            if(i < coneSegments)
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
        if(visionConeRenderer == null)
        {
            Debug.LogWarning("视野锥形渲染器丢失");
            return;
        }
        
        // 获取MeshFilter组件
        MeshFilter meshFilter = visionConeRenderer.GetComponent<MeshFilter>();
        if(meshFilter == null)
        {
            Debug.LogError("找不到MeshFilter组件");
            return;
        }
        
        // 重新创建网格
        meshFilter.mesh = CreateVisionConeMesh(detectionRange);
        
        // 重置缩放
        visionConeRenderer.transform.localScale = Vector3.one;
    }

    void UpdateVisionConeDirection()
    {
        if(visionConeRenderer == null)
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

    void UpdateVisionConeColor()
    {
        if(visionConeRenderer == null || visionConeRenderer.material == null)
        {
            Debug.LogWarning("视野锥形渲染器或材质丢失");
            return;
        }

        // 根据状态选择颜色
        Color targetColor = currentState switch
        {
            DetectionState.Suspicious => suspiciousColor,
            DetectionState.Alert => alertColor,
            _ => normalColor // 默认状态
        };

        // 保持透明度不变
        targetColor.a = visionConeRenderer.material.color.a;
        
        // 应用颜色
        visionConeRenderer.material.color = targetColor;
    }
    #endregion

    #if UNITY_EDITOR
    void OnDrawGizmos()
    {
        // 绘制检测范围
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // 获取当前朝向（编辑模式下默认向右）
        Vector2 facingDir = Application.isPlaying ? GetFacingDirection() : Vector2.right;
        
        // 绘制视野锥形边界线
        Vector3 leftDir = Quaternion.AngleAxis(-detectionAngle/2, Vector3.forward) * facingDir;
        Vector3 rightDir = Quaternion.AngleAxis(detectionAngle/2, Vector3.forward) * facingDir;
        
        Gizmos.DrawRay(transform.position, leftDir * detectionRange);
        Gizmos.DrawRay(transform.position, rightDir * detectionRange);
        
        // 编辑模式下绘制巡逻路径
        if(!Application.isPlaying)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, transform.position + Vector3.right * patrolDistance);
            Gizmos.DrawWireSphere(transform.position + Vector3.right * patrolDistance, 0.2f);
        }
    }
    #endif
}