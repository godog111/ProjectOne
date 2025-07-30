using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(SpriteRenderer))]
public class SecurityCamera : MonoBehaviour
{
    [Header("Detection Settings")]
    [Tooltip("监控范围的半径")]
    [SerializeField] private float _viewRadius = 5f;
    [Range(0, 360), Tooltip("监控的角度范围")]
    [SerializeField] private float _viewAngle = 90f;
    [Tooltip("玩家所在的层级")]
    public LayerMask targetMask;
    [Tooltip("阻挡视线的障碍物层级")]
    public LayerMask obstacleMask;

    [Header("Visualization Settings")]
    [Tooltip("视野锥形的材质（可选）")]
    public Material visionConeMaterial;
    [Range(0.1f, 5f), Tooltip("网格分辨率，值越大锥形越平滑")]
    [SerializeField] private float _meshResolution = 1f;
    [Tooltip("边缘检测迭代次数，影响边缘精度")]
    [SerializeField] private int _edgeResolveIterations = 4;
    [Tooltip("边缘距离阈值，影响边缘检测灵敏度")]
    [SerializeField] private float _edgeDstThreshold = 0.5f;
    [Tooltip("视野锥形的高度（Z轴深度）")]
    [SerializeField] private float _visionConeHeight = 0.1f;
    [Tooltip("视野锥形的偏移位置")]
    [SerializeField] private Vector3 _visionConeOffset = Vector3.zero;

    [Header("Alert Settings")]
    [Tooltip("警报持续时间（秒）")]
    public float alertDuration = 3f;
    [Tooltip("正常状态颜色")]
    public Color normalColor = Color.green;
    [Tooltip("警报状态颜色")]
    public Color alertColor = Color.red;
    [Tooltip("警报灯光对象（可选）")]
    public GameObject alertLight;
    //[Tooltip("警报音效（可选）")]
    //public AudioClip alertSound;
    
    [Header("Rendering Settings")]
    [Tooltip("视野锥形的渲染排序层级")]
    [SerializeField] private string  _sortingLayerID = "Ground";
    [Tooltip("视野锥形的渲染排序顺序")]
    [SerializeField] private int _sortingOrder = 0;
    [Tooltip("是否使用自定义渲染层级")]
    [SerializeField] private bool _useCustomSorting = false;

    // 属性封装以便在代码中访问
    public float viewRadius
    {
        get => _viewRadius;
        set { _viewRadius = Mathf.Max(0, value); UpdateVisionCone(); }
    }
    
    public float viewAngle {
        get => _viewAngle;
        set { _viewAngle = Mathf.Clamp(value, 0, 360); UpdateVisionCone(); }
    }
    
    public float meshResolution {
        get => _meshResolution;
        set { _meshResolution = Mathf.Clamp(value, 0.1f, 5f); UpdateVisionCone(); }
    }
    
    public int edgeResolveIterations {
        get => _edgeResolveIterations;
        set { _edgeResolveIterations = Mathf.Max(1, value); UpdateVisionCone(); }
    }
    
    public float edgeDstThreshold {
        get => _edgeDstThreshold;
        set { _edgeDstThreshold = Mathf.Max(0.01f, value); UpdateVisionCone(); }
    }

    // 私有变量
    private bool isAlerted = false;
    private float alertTimer = 0f;
    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;
    private MeshFilter visionConeMeshFilter;
    private MeshRenderer visionConeMeshRenderer;
    private Mesh visionConeMesh;

    void Start()
    {
        InitializeComponents();
        CreateVisionCone();
    }

    void Update()
    {
        if (!isAlerted)
        {
            DetectPlayer();
        }
        else
        {
            HandleAlert();
        }
    }

    void InitializeComponents()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        if (alertLight != null) alertLight.SetActive(false);
        spriteRenderer.color = normalColor;
    }

    void CreateVisionCone()
    {
        GameObject visionCone = new GameObject("VisionCone");
        visionCone.transform.SetParent(transform);
        visionCone.transform.localPosition = _visionConeOffset;
        visionCone.transform.localRotation = Quaternion.identity;

        visionConeMeshFilter = visionCone.AddComponent<MeshFilter>();
        visionConeMeshRenderer = visionCone.AddComponent<MeshRenderer>();
        visionConeMesh = new Mesh();
        visionConeMeshFilter.mesh = visionConeMesh;

        ApplyVisionConeMaterial();
        ApplySortingSettings(); // 应用渲染层级设置
        UpdateVisionCone();
    }

        /// <summary>
    /// 应用渲染层级设置
    /// </summary>
    void ApplySortingSettings()
    {
        if (!_useCustomSorting || visionConeMeshRenderer == null) return;

        // 设置排序层级ID和排序顺序
        visionConeMeshRenderer.sortingLayerName = _sortingLayerID;
        visionConeMeshRenderer.sortingOrder = _sortingOrder;
    }

    void ApplyVisionConeMaterial()
    {
        if (visionConeMaterial != null)
        {
            visionConeMeshRenderer.material = visionConeMaterial;
        }
        else
        {
            Material defaultMaterial = new Material(Shader.Find("Sprites/Default"));
            defaultMaterial.color = new Color(normalColor.r, normalColor.g, normalColor.b, 0.3f);
            visionConeMeshRenderer.material = defaultMaterial;
        }
    }

    void UpdateVisionCone()
    {
        if (visionConeMesh == null) return;

        int stepCount = Mathf.RoundToInt(_viewAngle * _meshResolution);
        float stepAngleSize = _viewAngle / stepCount;
        List<Vector3> viewPoints = new List<Vector3>();
        ViewCastInfo oldViewCast = new ViewCastInfo();

        for (int i = 0; i <= stepCount; i++)
        {
            float angle = transform.eulerAngles.z - _viewAngle / 2 + stepAngleSize * i;
            ViewCastInfo newViewCast = ViewCast(angle);

            if (i > 0)
            {
                bool edgeDstThresholdExceeded = Mathf.Abs(oldViewCast.dst - newViewCast.dst) > _edgeDstThreshold;
                if (oldViewCast.hit != newViewCast.hit || (oldViewCast.hit && newViewCast.hit && edgeDstThresholdExceeded))
                {
                    EdgeInfo edge = FindEdge(oldViewCast, newViewCast);
                    if (edge.pointA != Vector3.zero) viewPoints.Add(edge.pointA);
                    if (edge.pointB != Vector3.zero) viewPoints.Add(edge.pointB);
                }
            }

            viewPoints.Add(newViewCast.point);
            oldViewCast = newViewCast;
        }

        GenerateVisionConeMesh(viewPoints);
    }

    void GenerateVisionConeMesh(List<Vector3> viewPoints)
    {
        int vertexCount = viewPoints.Count + 1;
        Vector3[] vertices = new Vector3[vertexCount];
        Vector2[] uv = new Vector2[vertexCount];
        int[] triangles = new int[(vertexCount - 2) * 3];

        // 中心点
        vertices[0] = _visionConeOffset;
        uv[0] = new Vector2(0.5f, 0.5f);

        // 边缘点
        for (int i = 0; i < vertexCount - 1; i++)
        {
            Vector3 localPoint = transform.InverseTransformPoint(viewPoints[i]);
            localPoint.z = _visionConeHeight; // 设置Z轴高度
            vertices[i + 1] = localPoint;
            
            // 简单的UV映射
            uv[i + 1] = new Vector2(
                (localPoint.x + _viewRadius) / (2 * _viewRadius),
                (localPoint.y + _viewRadius) / (2 * _viewRadius)
            );

            if (i < vertexCount - 2)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }
        }

        visionConeMesh.Clear();
        visionConeMesh.vertices = vertices;
        visionConeMesh.uv = uv;
        visionConeMesh.triangles = triangles;
        visionConeMesh.RecalculateNormals();
        visionConeMesh.RecalculateBounds();
    }

    ViewCastInfo ViewCast(float globalAngle)
    {
        Vector3 dir = DirFromAngle(globalAngle, true);
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, _viewRadius, obstacleMask);

        if (hit.collider != null)
        {
            return new ViewCastInfo(true, hit.point, hit.distance, globalAngle);
        }
        else
        {
            return new ViewCastInfo(false, transform.position + dir * _viewRadius, _viewRadius, globalAngle);
        }
    }

    EdgeInfo FindEdge(ViewCastInfo minViewCast, ViewCastInfo maxViewCast)
    {
        float minAngle = minViewCast.angle;
        float maxAngle = maxViewCast.angle;
        Vector3 minPoint = Vector3.zero;
        Vector3 maxPoint = Vector3.zero;

        for (int i = 0; i < _edgeResolveIterations; i++)
        {
            float angle = (minAngle + maxAngle) / 2;
            ViewCastInfo newViewCast = ViewCast(angle);

            bool edgeDstThresholdExceeded = Mathf.Abs(minViewCast.dst - newViewCast.dst) > _edgeDstThreshold;
            if (newViewCast.hit == minViewCast.hit && !edgeDstThresholdExceeded)
            {
                minAngle = angle;
                minPoint = newViewCast.point;
            }
            else
            {
                maxAngle = angle;
                maxPoint = newViewCast.point;
            }
        }

        return new EdgeInfo(minPoint, maxPoint);
    }

    void DetectPlayer()
    {
        Collider2D[] targetsInViewRadius = Physics2D.OverlapCircleAll(transform.position, _viewRadius, targetMask);

        foreach (Collider2D target in targetsInViewRadius)
        {
            Transform targetTransform = target.transform;
            Vector2 dirToTarget = (targetTransform.position - transform.position).normalized;

            if (Vector2.Angle(transform.up, dirToTarget) < _viewAngle / 2)
            {
                float dstToTarget = Vector2.Distance(transform.position, targetTransform.position);

                if (!Physics2D.Raycast(transform.position, dirToTarget, dstToTarget, obstacleMask))
                {
                    TriggerAlert();
                    break;
                }
            }
        }
    }

    void TriggerAlert()
    {
        isAlerted = true;
        alertTimer = alertDuration;
        spriteRenderer.color = alertColor;
        
        if (alertLight != null) alertLight.SetActive(true);
        
       /* if (alertSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(alertSound);
        }*/
        
        if (visionConeMeshRenderer != null)
        {
            visionConeMeshRenderer.material.color = new Color(alertColor.r, alertColor.g, alertColor.b, 0.3f);
        }
    }

    void HandleAlert()
    {
        alertTimer -= Time.deltaTime;
        
        if (alertTimer <= 0)
        {
            isAlerted = false;
            spriteRenderer.color = normalColor;
            if (alertLight != null) alertLight.SetActive(false);
            
            if (visionConeMeshRenderer != null)
            {
                visionConeMeshRenderer.material.color = new Color(normalColor.r, normalColor.g, normalColor.b, 0.3f);
            }
        }
    }

    public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += transform.eulerAngles.z;
        }
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), Mathf.Cos(angleInDegrees * Mathf.Deg2Rad), 0);
    }

    void OnValidate()
    {
        if (visionConeMeshFilter != null)
        {
            UpdateVisionCone();
        }
    }

    public struct ViewCastInfo
    {
        public bool hit;
        public Vector3 point;
        public float dst;
        public float angle;

        public ViewCastInfo(bool _hit, Vector3 _point, float _dst, float _angle)
        {
            hit = _hit;
            point = _point;
            dst = _dst;
            angle = _angle;
        }
    }

    public struct EdgeInfo
    {
        public Vector3 pointA;
        public Vector3 pointB;

        public EdgeInfo(Vector3 _pointA, Vector3 _pointB)
        {
            pointA = _pointA;
            pointB = _pointB;
        }
    }
}