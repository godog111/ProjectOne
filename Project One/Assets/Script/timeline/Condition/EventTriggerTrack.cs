using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

/// <summary>
/// 事件触发器轨道
/// </summary>
[TrackColor(0.4f, 0.8f, 0.6f)] // 轨道颜色
[TrackClipType(typeof(EventTriggerClip))] // 关联的剪辑类型
[TrackBindingType(typeof(GameObject))] // 绑定的目标类型
public class EventTriggerTrack : TrackAsset {}