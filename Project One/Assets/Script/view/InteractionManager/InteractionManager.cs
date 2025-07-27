using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 交互系统核心管理器（单例模式）
/// 功能：
/// 1. 管理所有可交互对象的注册与注销
/// 2. 计算并跟踪距离玩家最近的交互对象
/// 3. 处理鼠标交互模式
/// 4. 提供交互状态查询接口
/// </summary>
public class InteractionManager : MonoBehaviour
{
    #region 单例实现
    private static InteractionManager _instance;

    /// <summary>
    /// 获取交互管理器单例（线程不安全，仅限主线程调用）
    /// </summary>
    public static InteractionManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<InteractionManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("InteractionManager");
                    _instance = go.AddComponent<InteractionManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
    }
    #endregion

    #region 核心字段
    [Header("基本设置")]
    [Tooltip("玩家对象Transform缓存，自动查找")]
    public Transform _playerTransform;

    [Tooltip("已注册的所有可交互对象列表")]
    private List<IInteractable> _registeredInteractables = new List<IInteractable>();

    [Tooltip("当前距离玩家最近的交互对象")]
    private IInteractable _currentClosestInteractable;

    [Header("交互键设置")]
    [Tooltip("全局默认交互键")]
    [SerializeField] private KeyCode _universalInteractKey = KeyCode.F;

    [Tooltip("特殊交互键覆盖字典")]
    private Dictionary<IInteractable, KeyCode> _interactKeyOverrides = new Dictionary<IInteractable, KeyCode>();

    [Header("鼠标交互模式设置")]
    [Tooltip("激活鼠标交互模式的按键")]
    [SerializeField] private KeyCode _mouseModeKey = KeyCode.E;

    [Tooltip("鼠标模式下普通可交互物体的高亮颜色")]
    [SerializeField] private Color _mouseHighlightColor = Color.cyan;

    [Tooltip("鼠标模式下距离鼠标最近物体的高亮颜色")]
    [SerializeField] private Color _mouseClosestHighlightColor = Color.magenta;

    [Tooltip("是否处于鼠标交互模式")]
    private bool _isMouseModeActive = false;

    [Tooltip("鼠标模式下当前选中的交互对象")]
    private IInteractable _mouseSelectedInteractable;

    [Tooltip("所有特殊交互物体列表")]
    private List<IInteractable> _specialInteractables = new List<IInteractable>();

    [Header("调试设置")]
    [Tooltip("是否在场景中显示交互范围")]
    [SerializeField] private bool _showDebugRadius = true;

    [Tooltip("调试用交互范围显示颜色")]
    [SerializeField] private Color _debugGizmoColor = new Color(0, 1, 0, 0.3f);
    #endregion

    #region Unity生命周期
    private void Update()
    {
        if (_playerTransform == null)
        {
            FindPlayer();
            return;
        }

        HandleMouseModeToggle();

        if (_isMouseModeActive)
        {
            HandleMouseSelection();
        }
        else
        {
            UpdateClosestInteractable();
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!_showDebugRadius || _playerTransform == null) return;

        Gizmos.color = _debugGizmoColor;
        foreach (var interactable in _registeredInteractables)
        {
            if (interactable.CanInteract())
            {
                Gizmos.DrawWireSphere(
                    interactable.GetTransform().position,
                    interactable.GetInteractionRadius()
                );
            }
        }
    }
    #endregion

    #region 公共接口
    /// <summary>
    /// 注册可交互对象
    /// </summary>
    /// <param name="interactable">要注册的交互对象</param>
    public void RegisterInteractable(IInteractable interactable)
    {
        if (!_registeredInteractables.Contains(interactable))
        {
            Debug.Log("注册普通物体");
            _registeredInteractables.Add(interactable);

            if (interactable.IsSpecialInteractable())
            {
                Debug.Log("注册特殊物体");
                _specialInteractables.Add(interactable);
            }
        }
    }

    /// <summary>
    /// 注销可交互对象
    /// </summary>
    /// <param name="interactable">要注销的交互对象</param>
    public void UnregisterInteractable(IInteractable interactable)
    {
        _registeredInteractables.Remove(interactable);
        _specialInteractables.Remove(interactable);

        if (_currentClosestInteractable == interactable)
        {
            interactable.OnExitInteractionRange();
            _currentClosestInteractable = null;
        }
    }

    /// <summary>
    /// 获取当前最近的交互对象
    /// </summary>
    /// <returns>最近的交互对象，可能为null</returns>
    public IInteractable GetCurrentClosestInteractable()
    {
       
        // 如果在鼠标模式下，返回鼠标选择的物体
        if (_isMouseModeActive)
        {
           // Debug.Log(_isMouseModeActive);
            // Debug.Log("鼠标模式");
            return _mouseSelectedInteractable;
        }


        // 重新计算最近物体（确保数据最新）
        Vector3 playerPos = _playerTransform.position;
        IInteractable closest = null;
        
        float minWeightedDistance = float.MaxValue;

        // 调试日志：显示当前注册的交互对象数量
       // Debug.Log($"正在检查 {_registeredInteractables.Count} 个已注册的交互对象");

        foreach (var interactable in _registeredInteractables)
        {
            // 跳过不可交互的对象
            if (!interactable.CanInteract())
            {
                Debug.Log($"跳过 {interactable.GetTransform().name} - 不可交互");
                continue;
            }

            // 计算实际距离
            float distance = Vector3.Distance(
                playerPos,
                interactable.GetTransform().position
            );

            // 调试日志：显示每个物体的距离信息
           // Debug.Log($"检查 {interactable.GetTransform().name} - 距离: {distance}, 交互半径: {interactable.GetInteractionRadius()}");

            // 检查是否在交互范围内
            if (distance <= interactable.GetInteractionRadius())
            {
                // 计算加权距离（考虑优先级）
                float weightedDistance = distance / (interactable.GetInteractionPriority() + 0.1f);
                
                // 调试日志：显示加权距离
                Debug.Log($"物体 {interactable.GetTransform().name} - 加权距离: {weightedDistance}");

                if (weightedDistance < minWeightedDistance)
                {
                    minWeightedDistance = weightedDistance;
                    closest = interactable;
                }
            }
            else
            {
               // Debug.Log($"物体 {interactable.GetTransform().name} 超出交互范围");
            }
        }

        // 调试日志：显示最终选择的物体
        if (closest != null)
        {
            Debug.Log($"最近交互对象确定为: {closest.GetTransform().name}");
        }
        else
        {
            //Debug.Log("未找到有效的交互对象");
        }

        return closest;
    }

    /// <summary>
    /// 获取玩家Transform（供交互对象使用）
    /// </summary>
    public Transform GetPlayerTransform()
    {
        return _playerTransform;
    }

    /// <summary>
    /// 获取指定交互对象的有效交互键
    /// 优先使用对象自定义的交互键，否则使用全局默认键
    /// </summary>
    public KeyCode GetEffectiveInteractKey(IInteractable interactable)
    {
        // 检查是否为特殊交互键
        if (_interactKeyOverrides.TryGetValue(interactable, out KeyCode customKey))
        {
            return customKey;
        }

        // 检查交互对象是否有自定义键
        if (interactable.GetInteractKey() != KeyCode.None)
        {
            return interactable.GetInteractKey();
        }

        // 返回全局默认键
        return _universalInteractKey;
    }

    /// <summary>
    /// 注册特殊交互键覆盖
    /// </summary>
    public void RegisterInteractKeyOverride(IInteractable interactable, KeyCode key)
    {
        if (!_interactKeyOverrides.ContainsKey(interactable))
        {
            _interactKeyOverrides.Add(interactable, key);
        }
    }

    /// <summary>
    /// 注销特殊交互键覆盖
    /// </summary>
    public void UnregisterInteractKeyOverride(IInteractable interactable)
    {
        _interactKeyOverrides.Remove(interactable);
    }
    #endregion

    #region 私有方法
    /// <summary>
    /// 查找玩家对象
    /// </summary>
    private void FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Debug.Log("找到玩家对象");
            _playerTransform = player.transform;
        }
        
    }

    /// <summary>
    /// 更新最近的交互对象（常规模式）
    /// </summary>
    private void UpdateClosestInteractable()
    {
        IInteractable newClosest = CalculateClosestInteractable(_playerTransform.position);

        if (newClosest != _currentClosestInteractable)
        {
            if (_currentClosestInteractable != null)
            {
                _currentClosestInteractable.OnExitInteractionRange();
            }

            _currentClosestInteractable = newClosest;

            if (_currentClosestInteractable != null)
            {
                _currentClosestInteractable.OnEnterInteractionRange();
            }
        }
    }

    /// <summary>
    /// 计算距离指定位置最近的交互对象
    /// </summary>
    /// <param name="position">参考位置</param>
    /// <returns>最近的交互对象，可能为null</returns>
    private IInteractable CalculateClosestInteractable(Vector3 position)
    {
        IInteractable closest = null;
        float minWeightedDistance = float.MaxValue;

        foreach (var interactable in _registeredInteractables)
        {
            if (!interactable.CanInteract()) continue;

            float distance = Vector3.Distance(
                position,
                interactable.GetTransform().position
            );

            if (distance <= interactable.GetInteractionRadius())
            {
                float weightedDistance = distance / interactable.GetInteractionPriority();

                if (weightedDistance < minWeightedDistance)
                {
                    minWeightedDistance = weightedDistance;
                    closest = interactable;
                }
            }
        }

        return closest;
    }

    /// <summary>
    /// 处理鼠标交互模式切换
    /// </summary>
    private void HandleMouseModeToggle()
    {
        
        if (Input.GetKeyDown(_mouseModeKey))
        {
            Debug.Log("进入鼠标模式");
            _isMouseModeActive = !_isMouseModeActive;
           
            if (_isMouseModeActive)
            {
                EnterMouseMode();
            }
            else
            {
                ExitMouseMode();
            }
        }
    }

    /// <summary>
    /// 进入鼠标交互模式
    /// </summary>
    private void EnterMouseMode()
    {
        foreach (var interactable in _specialInteractables)
        {
            interactable.SetHighlightColor(_mouseHighlightColor);
            interactable.SetHighlight(true);
        }
    }

    /// <summary>
    /// 退出鼠标交互模式
    /// </summary>
    private void ExitMouseMode()
    {
        foreach (var interactable in _specialInteractables)
        {
            interactable.SetHighlight(false);
        }
        _mouseSelectedInteractable = null;
        _isMouseModeActive = false;
    }

    /// <summary>
    /// 处理鼠标选择逻辑
    /// </summary>
    private void HandleMouseSelection()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        IInteractable closestToMouse = CalculateClosestInteractable(mousePos);

        // 更新高亮状态
        if (closestToMouse != _mouseSelectedInteractable)
        {
            if (_mouseSelectedInteractable != null)
            {
                _mouseSelectedInteractable.SetHighlightColor(_mouseHighlightColor);
            }

            _mouseSelectedInteractable = closestToMouse;

            if (_mouseSelectedInteractable != null)
            {
                _mouseSelectedInteractable.SetHighlightColor(_mouseClosestHighlightColor);
            }
        }

        // 处理鼠标点击交互
        if (Input.GetMouseButtonDown(0) && _mouseSelectedInteractable != null)
        {
            Debug.Log("已选择物体");
            _mouseSelectedInteractable.Interact();
            ExitMouseMode();
        }
       
    }
    #endregion
}

/// <summary>
/// 可交互对象接口
/// 所有可交互对象必须实现此接口
/// </summary>
public interface IInteractable
{
    #region 基础属性
    /// <summary>
    /// 获取对象的Transform组件
    /// </summary>
    Transform GetTransform();
    
    /// <summary>
    /// 获取交互半径（单位：米）
    /// </summary>
    float GetInteractionRadius();
    
    /// <summary>
    /// 获取交互按键
    /// </summary>
    KeyCode GetInteractKey();
    
    /// <summary>
    /// 获取交互优先级（数值越大优先级越高）
    /// </summary>
    int GetInteractionPriority();
    
    /// <summary>
    /// 获取交互冷却时间（单位：秒）
    /// </summary>
    float GetInteractionCooldown();
    
    /// <summary>
    /// 检查当前是否可以交互
    /// </summary>
    bool CanInteract();
    #endregion

    #region 交互行为
    /// <summary>
    /// 执行交互行为
    /// </summary>
    void Interact();
    
    /// <summary>
    /// 当进入交互范围时调用
    /// </summary>
    void OnEnterInteractionRange();
    
    /// <summary>
    /// 当退出交互范围时调用
    /// </summary>
    void OnExitInteractionRange();
    #endregion

    #region 视觉反馈
    /// <summary>
    /// 设置高亮状态
    /// </summary>
    /// <param name="isOn">是否高亮</param>
    void SetHighlight(bool isOn);
    
    /// <summary>
    /// 设置高亮颜色
    /// </summary>
    /// <param name="color">高亮颜色</param>
    void SetHighlightColor(Color color);
    #endregion

    #region 特殊交互
    /// <summary>
    /// 检查是否为特殊交互物体
    /// </summary>
    bool IsSpecialInteractable();
    #endregion
}