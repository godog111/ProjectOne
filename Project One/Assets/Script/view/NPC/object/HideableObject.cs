using UnityEngine;

/// <summary>
/// 可隐藏物体脚本 - 解决地面碰撞问题版本
/// </summary>
public class HideableObject : MonoBehaviour
{
    [Header("隐藏设置")]
    public KeyCode hideKey = KeyCode.F;
    [Range(0, 1)] public float hideAlpha = 0.5f;
    
    [Header("检测设置")]
    public float detectionRadius = 1.5f;
    public Vector2 detectionOffset = Vector2.zero;
    
    // 玩家相关引用
    private bool isPlayerHidden = false;
    private GameObject player;
    private SpriteRenderer playerSpriteRenderer;
    private Collider2D playerCollider;
    
    // 地面图层相关
    private int groundLayer;
    private int hiddenLayer;
    
    private void Awake()
    {
        // 初始化图层
        groundLayer = LayerMask.NameToLayer("Ground");
        hiddenLayer = LayerMask.NameToLayer("Hidden");
        
        // 如果没有Hidden图层则创建
        if (hiddenLayer == -1)
        {
            Debug.LogWarning("请创建一个名为'Hidden'的图层用于隐藏功能");
            hiddenLayer = LayerMask.NameToLayer("Default");
        }
    }
    
    private void Update()
    {
        if (isPlayerHidden)
        {
            CheckUnhide();
        }
        else
        {
            CheckHide();
        }
    }
    
    /// <summary>
    /// 改进的隐藏方法 - 保留与地面的碰撞
    /// </summary>
    private void HidePlayer()
    {
        isPlayerHidden = true;
        
        // 设置玩家与隐藏物体的碰撞
        Physics2D.IgnoreLayerCollision(player.layer, hiddenLayer, true);
        
        // 保留与地面的碰撞
        if (groundLayer != -1)
        {
            Physics2D.IgnoreLayerCollision(player.layer, groundLayer, false);
        }
        
        // 视觉效果处理
        if (playerSpriteRenderer != null)
        {
            Color newColor = playerSpriteRenderer.color;
            newColor.a = hideAlpha;
            playerSpriteRenderer.color = newColor;
        }
        
        // 禁用玩家移动脚本（示例）
        var movement = player.GetComponent<PlayerController>();
        if (movement != null) movement.enabled = false;
        
        Debug.Log("玩家已隐藏（保留地面碰撞）");
    }
    
    /// <summary>
    /// 改进的取消隐藏方法
    /// </summary>
    private void UnhidePlayer()
    {
        isPlayerHidden = false;
        
        // 恢复图层碰撞
        Physics2D.IgnoreLayerCollision(player.layer, hiddenLayer, false);
        
        // 恢复视觉效果
        if (playerSpriteRenderer != null)
        {
            Color newColor = playerSpriteRenderer.color;
            newColor.a = 1f;
            playerSpriteRenderer.color = newColor;
        }
        
        // 恢复玩家移动
        var movement = player.GetComponent<PlayerController>();
        if (movement != null) movement.enabled = true;
        
        Debug.Log("玩家取消隐藏");
        
        player = null;
        playerSpriteRenderer = null;
        playerCollider = null;
    }
    
    private void CheckHide()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(
            (Vector2)transform.position + detectionOffset, 
            detectionRadius);
        
        foreach (Collider2D collider in colliders)
        {
            if (collider.CompareTag("Player"))
            {
                player = collider.gameObject;
                playerSpriteRenderer = player.GetComponent<SpriteRenderer>();
                playerCollider = player.GetComponent<Collider2D>();
                
                if (Input.GetKeyDown(hideKey))
                {
                    HidePlayer();
                    return;
                }
            }
        }
    }
    
    private void CheckUnhide()
    {
        if (Input.GetKeyDown(hideKey) || 
            Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.1f || 
            Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.1f)
        {
            UnhidePlayer();
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere((Vector2)transform.position + detectionOffset, detectionRadius);
    }
}