using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

/// <summary>
/// Timeline信号发射器标记
/// 功能：
/// 1. 在Timeline中作为标记点触发信号
/// 2. 关联CustomSignal资产
/// </summary>
[System.Serializable]
public class CustomSignalEmitter : Marker, INotification, ISerializationCallbackReceiver
{
    [SerializeField, Tooltip("关联的信号资产")] 
    private CustomSignal _asset;
    
    /// <summary> 序列化前调用（Unity内部使用） </summary>
    public void OnBeforeSerialize() { }
    
    /// <summary>
    /// 反序列化后调用
    /// 解决Timeline资源加载时的空引用问题
    /// </summary>
    public void OnAfterDeserialize()
    {
        if (_asset == null)
        {
            Debug.LogWarning("信号资产反序列化为空，请检查Timeline资源");
        }
    }
    
    /// <summary> 公开属性：获取或设置信号资产 </summary>
    public CustomSignal asset
    {
        get => _asset;
        set => _asset = value;
    }
    
    /// <summary> 实现INotification接口需要的唯一标识符 </summary>
    public PropertyName id => new PropertyName(GetType().Name);
}