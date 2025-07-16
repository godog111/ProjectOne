using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

/// <summary>
/// 处理Timeline信号的接收器
/// 用于演出中的特定事件触发
/// </summary>
public class NPCPerformanceSignalReceiver : MonoBehaviour
{
    public NPCPerformanceController performanceController;
    
    /// <summary>
    /// Timeline信号处理
    /// </summary>
    public void OnPerformanceSignal(PerformanceSignal signal)
    {
        if(performanceController != null && signal != null)
        {
            switch(signal.signalType)
            {
                case PerformanceSignal.SignalType.ChangeExpression:
                    // 处理表情变化
                    break;
                    
                case PerformanceSignal.SignalType.TriggerEffect:
                    // 触发特效
                    break;
                    
                case PerformanceSignal.SignalType.InterruptPerformance:
                    // 中断当前演出
                    performanceController.StopCurrentPerformance();
                    break;
            }
        }
    }
}

/// <summary>
/// 自定义信号类型
/// </summary>
[System.Serializable]
public class PerformanceSignal : Marker
{
    public enum SignalType
    {
        ChangeExpression,
        TriggerEffect,
        InterruptPerformance
    }
    
    public SignalType signalType;
    public string parameter;
}