using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

/// <summary>
/// 自定义信号资产，用于在Timeline中传递数据
/// 创建方法：在Project视图右键 -> Create -> Signals -> Custom Signal
/// </summary>
[CreateAssetMenu(menuName = "Signals/Custom Signal")]
public class CustomSignal : SignalAsset
{
    [SerializeField, Tooltip("信号携带的文本消息")]
    private string message = "Hello from Signal!";
    
    [SerializeField, Tooltip("信号携带的整数值")]
    private int value = 0;
    
    /// <summary> 公开属性：获取信号消息 </summary>
    public string Message => message;
    
    /// <summary> 公开属性：获取信号值 </summary>
    public int Value => value;
    
    /// <summary> 重写ToString用于调试显示 </summary>
    public override string ToString()
    {
        return $"CustomSignal: {message} (Value: {value})";
    }
}