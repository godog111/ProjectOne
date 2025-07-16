using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

/// <summary>
/// 信号接收器组件
/// 功能：
/// 1. 接收Timeline触发的信号
/// 2. 处理Timeline开始/结束事件
/// 使用方法：挂载到需要响应信号的GameObject上
/// </summary>
[AddComponentMenu("Signals/Custom Signal Receiver")]
public class CustomSignalReceiver : MonoBehaviour, INotificationReceiver
{
    
    /// <summary>
    /// 处理Timeline开始播放事件
    /// </summary>
    public void HandleTimelineStart()
    {
        Debug.Log($"{gameObject.name}接收到Timeline开始信号");
        // 在此添加自定义开始逻辑
    }

    /// <summary>
    /// 处理Timeline结束播放事件
    /// </summary>
    public void HandleTimelineStop()
    {
        Debug.Log($"{gameObject.name}接收到Timeline结束信号");
        // 在此添加自定义结束逻辑
    }

    /// <summary>
    /// Timeline信号触发回调（核心方法）
    /// </summary>
    /// <param name="origin">触发信号的Playable</param>
    /// <param name="notification">信号通知对象</param>
    /// <param name="context">附加上下文</param>
    public void OnNotify(Playable origin, INotification notification, object context)
    {
        // 1. 空值安全检查
        if (notification == null) return;
        
        // 2. 类型检查
        if (notification is CustomSignalEmitter signalEmitter)
        {
            // 3. 二次空值检查
            if (signalEmitter == null || signalEmitter.asset == null)
            {
                Debug.LogWarning($"空信号发射器: {gameObject.name}");
                return;
            }
            
            // 4. 获取信号数据
            var signalAsset = signalEmitter.asset;
            Debug.Log($"接收到信号: {signalAsset.Message} 数值: {signalAsset.Value}");
            ShowShader(signalAsset.Value);
            
            // 5. 示例：根据信号值改变物体颜色
            if (TryGetComponent<Renderer>(out var renderer))
            {
                if (renderer.material == null)
                {
                    Debug.LogWarning($"物体 {gameObject.name} 缺少材质");
                    return;
                }

                // 根据信号值切换颜色
                renderer.material.color = signalAsset.Value switch
                {
                    0 => Color.red,
                    1 => Color.green,
                    2 => Color.blue,
                    _ => Color.white
                };
            }
        }
    }

    private void ShowShader(float value)
    {
        var glitchEffect = GetComponent<GlitchEffect>();
        if (value == 0)
        {
            glitchEffect.SetDigitalGlitchEnabled(false);
            glitchEffect.SetAnalogGlitchEnabled(false);
            PostProcessingController.ApplyPresetByName_Static(PresetType.Horror);
        }
        else
        {
            glitchEffect.SetDigitalGlitchEnabled(true);
            glitchEffect.SetAnalogGlitchEnabled(true);
        }
    }
    
    /// <summary>
    /// 编辑器模式下绘制辅助Gizmo
    /// </summary>
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, Vector3.one * 0.5f);
    }
}