using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

/// <summary>
/// 可配置按钮控制器，支持动态修改文本、颜色和点击事件
/// </summary>
[AddComponentMenu("UI/Custom Button Configurator")]
public class ButtonConfigurator : MonoBehaviour
{
    // ================== 组件引用 ================== //
    [Tooltip("按钮的文本组件（自动查找子物体）")]
    [SerializeField] private Text _textComponent;
    
    [Tooltip("按钮的Button组件（自动挂载在自身）")]
    [SerializeField] private Button _buttonComponent;

    // ================== 默认配置 ================== //
    [Tooltip("按钮默认显示文本")]
    [SerializeField] private string _defaultText = "Button";
    
    [Tooltip("按钮默认颜色")]
    [SerializeField] private Color _defaultColor = Color.white;

    /// <summary>
    /// 初始化时自动获取组件
    /// </summary>
    private void Awake()
    {
        // 自动查找组件（如果未手动赋值）
        if (_textComponent == null)
            _textComponent = GetComponentInChildren<Text>();
        
        if (_buttonComponent == null)
            _buttonComponent = GetComponent<Button>();
    }

    // ================== 核心方法 ================== //
    
    /// <summary>
    /// 基础配置方法
    /// </summary>
    /// <param name="buttonText">按钮显示文本</param>
    /// <param name="onClickAction">点击时触发的事件</param>
    public void Configure(string buttonText, UnityAction onClickAction)
    {
        SetText(buttonText);
        AddListener(onClickAction);
    }

    /// <summary>
    /// 高级配置方法（带颜色设置）
    /// </summary>
    /// <param name="buttonText">按钮显示文本</param>
    /// <param name="onClickAction">点击时触发的事件</param>
    /// <param name="buttonColor">按钮背景颜色</param>
    public void Configure(string buttonText, UnityAction onClickAction, Color buttonColor)
    {
        Configure(buttonText, onClickAction);
        SetColor(buttonColor);
    }

    /// <summary>
    /// 设置按钮文本
    /// </summary>
    /// <param name="newText">要显示的文本</param>
    public void SetText(string newText)
    {
        if (_textComponent != null)
            _textComponent.text = newText;
        else
            Debug.LogWarning("未找到Text组件", this);
    }

    /// <summary>
    /// 设置按钮颜色
    /// </summary>
    /// <param name="newColor">新的背景颜色</param>
    public void SetColor(Color newColor)
    {
        if (_buttonComponent.TryGetComponent<Image>(out var image))
            image.color = newColor;
        else
            Debug.LogWarning("未找到Image组件", this);
    }

    /// <summary>
    /// 添加点击事件监听（自动清除旧事件）
    /// </summary>
    /// <param name="action">无参void方法</param>
    public void AddListener(UnityAction action)
    {
        if (_buttonComponent == null) return;
        
        _buttonComponent.onClick.RemoveAllListeners();
        _buttonComponent.onClick.AddListener(action);
        
        // 安全校验：确保事件能正常触发
        #if UNITY_EDITOR
        if (action == null)
            Debug.LogWarning("添加了空事件监听", this);
        #endif
    }

    /// <summary>
    /// 重置按钮到默认状态
    /// </summary>
    public void ResetToDefault()
    {
        SetText(_defaultText);
        SetColor(_defaultColor);
        
        if (_buttonComponent != null)
            _buttonComponent.onClick.RemoveAllListeners();
    }

    // ================== 编辑器辅助 ================== //
    #if UNITY_EDITOR
    private void OnValidate()
    {
        // 编辑器模式下自动补全组件引用
        if (_textComponent == null)
            _textComponent = GetComponentInChildren<Text>(true);
        
        if (_buttonComponent == null)
            _buttonComponent = GetComponent<Button>();
    }
    #endif
}