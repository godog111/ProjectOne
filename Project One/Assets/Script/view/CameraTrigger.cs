using Cinemachine;
using UnityEngine;

[RequireComponent(typeof(Collider2D))] // 确保触发器有碰撞体
public class SmoothCameraTrigger : MonoBehaviour
{
    [Header("Camera Reference")]
    [Tooltip("拖入需要控制的Cinemachine Virtual Camera")]
    public CinemachineVirtualCamera virtualCamera;

    [Header("Zoom Settings")]
    [Tooltip("目标正交大小")]
    public float targetOrthoSize = 5f;
    [Tooltip("过渡时间（秒）")]
    public float transitionDuration = 1f;
    [Tooltip("是否在退出触发器时恢复原始大小")]
    public bool revertOnExit = true;

    // 私有变量
    private float originalOrthoSize;    // 存储原始大小
    private float currentTransitionTime; // 当前过渡时间
    private bool isTransitioning;       // 是否正在过渡
    private bool isZoomIn;              // 是放大还是缩小

    private void Start()
    {
        // 验证必要的组件
        if (virtualCamera == null)
        {
            Debug.LogError("未分配Virtual Camera!", this);
            enabled = false;
            return;
        }

        // 存储初始的正交大小
        originalOrthoSize = virtualCamera.m_Lens.OrthographicSize;
    }

    private void Update()
    {
        // 如果正在过渡中
        if (isTransitioning)
        {
            // 更新过渡时间（限制不超过总时长）
            currentTransitionTime = Mathf.Clamp(currentTransitionTime + Time.deltaTime, 0f, transitionDuration);

            // 计算插值比例（0到1之间）
            float t = currentTransitionTime / transitionDuration;
            // 使用平滑的插值函数（可以使用不同的缓动函数）
            t = Mathf.SmoothStep(0f, 1f, t);

            // 根据方向计算当前大小
            float currentSize;
            if (isZoomIn)
            {
                currentSize = Mathf.Lerp(originalOrthoSize, targetOrthoSize, t);
            }
            else
            {
                currentSize = Mathf.Lerp(targetOrthoSize, originalOrthoSize, t);
            }

            // 应用新的正交大小
            virtualCamera.m_Lens.OrthographicSize = currentSize;

            // 检查过渡是否完成
            if (currentTransitionTime >= transitionDuration)
            {
                isTransitioning = false;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 检查是否是玩家进入
        if (other.CompareTag("Player"))
        {
            // 开始放大过渡
            StartTransition(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // 检查是否是玩家离开且需要恢复
        if (revertOnExit && other.CompareTag("Player"))
        {
            // 开始缩小过渡
            StartTransition(false);
        }
    }

    /// <summary>
    /// 开始相机过渡
    /// </summary>
    /// <param name="zoomIn">true=放大到目标大小，false=恢复到原始大小</param>
    private void StartTransition(bool zoomIn)
    {
        isTransitioning = true;
        isZoomIn = zoomIn;
        currentTransitionTime = 0f;

        // 如果是放大，确保知道当前的原始大小（可能在之前的过渡中被修改）
        if (zoomIn)
        {
            originalOrthoSize = virtualCamera.m_Lens.OrthographicSize;
        }
    }

    // 可选：在编辑器中可视化触发器区域
    private void OnDrawGizmos()
    {
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            Gizmos.color = new Color(0, 1, 0, 0.3f);
            Gizmos.DrawCube(transform.position + new Vector3(collider.offset.x, collider.offset.y, 0), 
                           new Vector3(collider.bounds.size.x, collider.bounds.size.y, 1));
        }
    }
}