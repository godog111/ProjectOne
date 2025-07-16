using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

/// <summary>
/// 事件触发器剪辑数据
/// </summary>
[System.Serializable]
public class EventTriggerClip : PlayableAsset, ITimelineClipAsset
{
    public EventTriggerBehaviour template = new EventTriggerBehaviour();

    public ClipCaps clipCaps => ClipCaps.None; // 不需要特殊能力

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        return  ScriptPlayable<EventTriggerBehaviour>.Create(graph, template);
    }
}

/// <summary>
/// 事件触发器预设数据
/// </summary>
[CreateAssetMenu(fileName = "NewEventPreset", menuName = "Timeline/Event Trigger Preset")]
public class EventTriggerPreset : ScriptableObject
{
    [Header("基础配置")]
    public EventTriggerBehaviour.TriggerType triggerType;
    public string methodName;
    public string methodParameter;

    [Header("多参数配置")]
    public bool useMultiParameters;
    public List<string> methodParameters = new List<string>();

    [Header("预设相关")]
    public bool isPrefabMethod;
    public string prefabPath;

    [Header("条件系统")]
    public bool useConditionSystem;
    public ConditionType conditionType;
    public string conditionID;
    public bool conditionValue;

    [Header("目标覆盖")]
    public bool overrideTarget;
    public GameObject targetOverride;
}