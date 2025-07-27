using UnityEngine;

/// <summary>
/// 可交互对象基类（抽象类）
/// 提供默认实现，具体交互对象应继承此类
/// </summary>
public abstract class InteractableBase : MonoBehaviour, IInteractable
{
    #region 配置字段
    [Header("基本交互设置")]
    [Tooltip("是否可以交互")]
    [SerializeField] protected bool _canInteract = true;
    
    [Tooltip("交互半径（单位：米）")]
    [SerializeField] protected float _interactionRadius = 2f;
    
    [Tooltip("交互按键")]
    [SerializeField] protected KeyCode _interactKey = KeyCode.F;
    
    [Tooltip("交互优先级（数值越大优先级越高）")]
    [SerializeField] protected int _interactionPriority = 1;
    
    [Tooltip("交互冷却时间（单位：秒）")]
    [SerializeField] protected float _interactionCooldown = 0.5f;

    [Header("鼠标交互设置")]
    [Tooltip("是否为特殊交互物体")]
    [SerializeField] private bool _isSpecial = false;
    
    [Tooltip("自定义高亮颜色")]
    [SerializeField] private Color _customHighlightColor = Color.yellow;
    #endregion

    #region 运行时字段
    [Tooltip("精灵渲染器缓存")]
    private SpriteRenderer _spriteRenderer;
    
    [Tooltip("原始材质")]
    private Material _originalMaterial;
    
    [Tooltip("高亮材质")]
    private Material _highlightMaterial;
    
    [Tooltip("当前高亮颜色")]
    private Color _currentHighlightColor;
    public Shader outlineShader;
    #endregion

    #region Unity生命周期
    protected virtual void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (_spriteRenderer != null)
        {
            _originalMaterial = _spriteRenderer.material;
            _highlightMaterial = new Material(Shader.Find("Sprites/Outline"));
            _highlightMaterial.CopyPropertiesFromMaterial(_originalMaterial);
            _currentHighlightColor = _customHighlightColor;
        }

        // 自动添加碰撞器
        if (GetComponent<Collider2D>() == null)
        {
            var collider = gameObject.AddComponent<CircleCollider2D>();
            collider.radius = _interactionRadius;
            collider.isTrigger = true;
        }
    }

    private void OnEnable()
    {
        InteractionManager.Instance.RegisterInteractable(this);
        
        // 如果使用非默认交互键，注册键位覆盖
        if (_interactKey != KeyCode.F)
        {
            InteractionManager.Instance.RegisterInteractKeyOverride(this, _interactKey);
        }
    }

    private void OnDisable()
    {
        InteractionManager.Instance.UnregisterInteractable(this);
        
        // 注销键位覆盖
        if (_interactKey != KeyCode.F)
        {
            InteractionManager.Instance.UnregisterInteractKeyOverride(this);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 1, 0, 0.3f);
        Gizmos.DrawWireSphere(transform.position, _interactionRadius);
    }
    #endregion

    #region IInteractable实现
    public Transform GetTransform() => transform;
    public float GetInteractionRadius() => _interactionRadius;
    public KeyCode GetInteractKey() => _interactKey;
    public int GetInteractionPriority() => _interactionPriority;
    public float GetInteractionCooldown() => _interactionCooldown;
    public virtual bool CanInteract() => _canInteract;
    public bool IsSpecialInteractable() => _isSpecial;

    public abstract void Interact();

    public virtual void OnEnterInteractionRange()
    {
        SetHighlight(true);
    }

    public virtual void OnExitInteractionRange()
    {
        SetHighlight(false);
    }

    public void SetHighlight(bool isOn)
    {
        if (_spriteRenderer != null)
        {
            _spriteRenderer.material = isOn ? _highlightMaterial : _originalMaterial;
            if (isOn)
            {
                _highlightMaterial.SetColor("_OutlineColor", _currentHighlightColor);
                _highlightMaterial.SetFloat("_OutlineThickness", 1.2f);
            }
        }
    }

    public void SetHighlightColor(Color color)
    {
        _currentHighlightColor = color;
    }
    #endregion

    #region 辅助方法
    /// <summary>
    /// 设置是否可以交互
    /// </summary>
    /// <param name="value">是否可交互</param>
    public void SetCanInteract(bool value)
    {
        _canInteract = value;
        if (!value)
        {
            OnExitInteractionRange();
        }
    }
    #endregion
}