using UnityEngine;
using UnityEngine.Playables;

/// <summary>
/// Timeline事件代理组件
/// 功能：
/// 1. 监听PlayableDirector的开始/结束事件
/// 2. 转发事件到CustomSignalReceiver
/// 使用方法：与PlayableDirector挂载到同一GameObject
/// </summary>
[RequireComponent(typeof(PlayableDirector))]
public class TimelineSignalProxy : MonoBehaviour
{
    private PlayableDirector director;
    private CustomSignalReceiver receiver;
    
    /// <summary>
    /// 初始化组件引用
    /// </summary>
    private void Awake()
    {
        // 1. 获取必备组件
        director = GetComponent<PlayableDirector>();
        
        // 2. 尝试先获取同物体的接收器
        receiver = GetComponent<CustomSignalReceiver>();
        
        // 3. 如果不存在则查找场景中的接收器
        if (receiver == null)
        {
            receiver = FindObjectOfType<CustomSignalReceiver>();
            if (receiver == null)
            {
                Debug.LogWarning("场景中未找到CustomSignalReceiver");
            }
        }
    }
    
    /// <summary>
    /// 启用时注册事件监听
    /// </summary>
    private void OnEnable()
    {
        if (director != null)
        {
            director.played += OnPlayableDirectorPlayed;
            director.stopped += OnPlayableDirectorStopped;
        }
    }
    
    /// <summary>
    /// 禁用时取消事件监听
    /// </summary>
    private void OnDisable()
    {
        if (director != null)
        {
            director.played -= OnPlayableDirectorPlayed;
            director.stopped -= OnPlayableDirectorStopped;
        }
    }
    
    /// <summary>
    /// Timeline开始播放回调
    /// </summary>
    private void OnPlayableDirectorPlayed(PlayableDirector pd)
    {
        if (receiver != null)
        {
            receiver.HandleTimelineStart();
        }
        else
        {
            Debug.Log("Timeline开始但未配置接收器");
        }
    }
    
    /// <summary>
    /// Timeline停止播放回调
    /// </summary>
    private void OnPlayableDirectorStopped(PlayableDirector pd)
    {
        if (receiver != null)
        {
            receiver.HandleTimelineStop();
        }
    }
}