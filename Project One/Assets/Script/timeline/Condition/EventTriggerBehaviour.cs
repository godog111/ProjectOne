using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

/// <summary>
/// 事件触发行为 - 增强版，支持管理类方法调用
/// </summary>
[System.Serializable]
public class EventTriggerBehaviour : PlayableBehaviour
{
    /// <summary>
    /// 触发时机类型枚举
    /// </summary>
    public enum TriggerType
    {
        OnStart,    // 当剪辑开始播放时触发
        OnEnd,      // 当剪辑结束播放时触发
        Continuous  // 在剪辑持续期间每帧触发
    }

    [Header("基本设置")]
    [Tooltip("选择触发事件的时间点类型")]
    public TriggerType triggerType = TriggerType.OnStart;

    [Header("触发目标")]
    [Tooltip("指定要触发事件的管理类对象（为空时使用轨道绑定的默认对象）")]
    public GameObject targetObject;

    [NonSerialized] public TrackAsset parentTrack; // 运行时不可序列化

    [Header("方法调用配置")]
    [Tooltip("要调用的方法名称（需要目标对象上有对应方法）")]
    public string methodName;

    [Tooltip("传递给方法的参数（支持基础类型：int/float/string/bool，或预设路径）")]
    public string methodParameter;

    [Tooltip("是否使用多参数模式")]
    public bool useMultiParameters = false;

    [Tooltip("多参数列表（当useMultiParameters为true时使用）")]
    public List<string> methodParameters = new List<string>();

    [Header("预设相关")]
    [Tooltip("是否为预设生成/销毁方法")]
    public bool isPrefabMethod = false;

    [Tooltip("预设路径（当isPrefabMethod为true时使用）")]
    public string prefabPath;

    [Header("条件系统")]
    [Tooltip("是否要更新条件系统的状态")]
    public bool useConditionSystem;

    [Tooltip("要设置的条件类型")]
    public ConditionType conditionType;

    [Tooltip("条件的唯一标识符")]
    public string conditionID;

    [Tooltip("要设置的条件状态值")]
    public bool conditionValue;

    // 内部状态跟踪
    private bool hasTriggered; // 防止重复触发

    /// <summary>
    /// 当播放图启动时重置触发状态
    /// </summary>
    public override void OnGraphStart(Playable playable)
    {
        hasTriggered = false;
    }

    /// <summary>
    /// 每帧更新处理逻辑
    /// </summary>
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        // 获取默认绑定对象（通过轨道绑定的对象）
        GameObject defaultTarget = playerData as GameObject;
        // 确定实际使用的目标对象（优先使用剪辑指定的目标）
        GameObject actualTarget = targetObject != null ? targetObject : defaultTarget;

        switch (triggerType)
        {
            case TriggerType.OnStart when !hasTriggered:
                TriggerActions(actualTarget);
                hasTriggered = true;
                break;

            case TriggerType.Continuous:
                TriggerActions(actualTarget);
                break;
        }
    }

    /// <summary>
    /// 当行为暂停时触发结束事件
    /// </summary>
    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        if (triggerType == TriggerType.OnEnd && info.effectiveWeight > 0f)
        {
            var director = playable.GetGraph().GetResolver() as PlayableDirector;
            GameObject defaultTarget = GetTrackBinding(director);
            GameObject actualTarget = targetObject != null ? targetObject : defaultTarget;

            if (actualTarget != null)
            {
                TriggerActions(actualTarget);
            }
            else
            {
                Debug.LogWarning($"触发失败：{(parentTrack != null ? parentTrack.name : "未知轨道")}未绑定对象");
            }
        }
    }

    /// <summary>
    /// 获取轨道绑定对象的安全方法
    /// </summary>
    private GameObject GetTrackBinding(PlayableDirector director)
    {
        if (director == null || parentTrack == null) return null;

        // 通过导演获取轨道绑定对象
        return director.GetGenericBinding(parentTrack) as GameObject;
    }

    /// <summary>
    /// 执行实际的触发操作
    /// </summary>
    /// <param name="target">目标对象</param>
    private void TriggerActions(GameObject target)
    {
        // 更新条件系统状态
        if (useConditionSystem)
        {
            //ConditionManager.Instance?.SetCondition(conditionType, conditionID, conditionValue);
        }
            
        if (target != null && !string.IsNullOrEmpty(methodName))
        {
            var components = target.GetComponents<MonoBehaviour>();
            foreach (var component in components)
            {
                var componentType = component.GetType();
                var method = componentType.GetMethod(methodName,
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.Instance);

                if (method != null)
                {
                    // 找到方法，执行调用
                    object[] parameters = PrepareParameters(method);
                    method.Invoke(component, parameters);
                    return; // 调用成功后退出
                    
                }
                    else
                {
                    Debug.LogWarning($"未找到方法：{methodName} 在对象 {target.name} 上");
                }
                }       


           
        }
    }

    /// <summary>
    /// 准备方法参数
    /// </summary>
    private object[] PrepareParameters(System.Reflection.MethodInfo method)
    {
        if (useMultiParameters)
        {
            return PrepareMultiParameters(method);
        }
        else if (isPrefabMethod)
        {
            return PreparePrefabParameters(method);
        }
        else
        {
            return PrepareSingleParameter(method);
        }
    }

    /// <summary>
    /// 准备多参数
    /// </summary>
    private object[] PrepareMultiParameters(System.Reflection.MethodInfo method)
    {
        var paramInfos = method.GetParameters();
        if (paramInfos.Length != methodParameters.Count)
        {
            Debug.LogError($"参数数量不匹配！方法需要 {paramInfos.Length} 个参数，但提供了 {methodParameters.Count} 个");
            return null;
        }

        object[] parameters = new object[paramInfos.Length];
        for (int i = 0; i < paramInfos.Length; i++)
        {
            try
            {
                parameters[i] = System.Convert.ChangeType(methodParameters[i], paramInfos[i].ParameterType);
            }
            catch
            {
                Debug.LogError($"参数 {i} 转换失败: {methodParameters[i]} -> {paramInfos[i].ParameterType.Name}");
                parameters[i] = null;
            }
        }
        return parameters;
    }

    /// <summary>
    /// 准备预设相关参数
    /// </summary>
    private object[] PreparePrefabParameters(System.Reflection.MethodInfo method)
    {
        // 检查参数是否为空
        if (string.IsNullOrEmpty(prefabPath))
        {
            Debug.LogError($"预设路径不能为空！方法: {methodName}");
            return null;
        }

        // 尝试从 Resources 加载预设
        GameObject prefab = Resources.Load<GameObject>(prefabPath);
        if (prefab == null)
        {
            Debug.LogError($"无法加载预设: {prefabPath}，请检查路径是否正确！");
            return null;
        }

        // 检查方法参数类型
        var paramInfos = method.GetParameters();
        if (paramInfos.Length == 0 || paramInfos[0].ParameterType != typeof(GameObject))
        {
            Debug.LogError($"预设方法 {methodName} 的第一个参数必须是 GameObject 类型");
            return null;
        }

        return new object[] { prefab };
    }

    /// <summary>
    /// 准备单参数
    /// </summary>
    private object[] PrepareSingleParameter(System.Reflection.MethodInfo method)
    {
        if (string.IsNullOrEmpty(methodParameter))
        {
            return null;
        }

        var paramInfos = method.GetParameters();
        if (paramInfos.Length == 0)
        {
            return null;
        }

        try
        {
            object param = System.Convert.ChangeType(methodParameter, paramInfos[0].ParameterType);
            return new object[] { param };
        }
        catch
        {
            Debug.LogError($"参数转换失败: {methodParameter} -> {paramInfos[0].ParameterType.Name}");
            return null;
        }
    }

    // 新增预设应用方法
    public void ApplyPreset(EventTriggerPreset preset)
    {
        if (preset == null) return;

        triggerType = preset.triggerType;
        methodName = preset.methodName;
        methodParameter = preset.methodParameter;
        useMultiParameters = preset.useMultiParameters;
        methodParameters = new List<string>(preset.methodParameters);
        isPrefabMethod = preset.isPrefabMethod;
        prefabPath = preset.prefabPath;
        useConditionSystem = preset.useConditionSystem;
        conditionType = preset.conditionType;
        conditionID = preset.conditionID;
        conditionValue = preset.conditionValue;

        if (preset.overrideTarget)
        {
            targetObject = preset.targetOverride;
        }
        else
        {
            targetObject = null;
        }
    }

    /// <summary>
    /// 从当前配置生成新的预设
    /// </summary>
    public EventTriggerPreset GeneratePreset(string presetName, bool includeTarget = false)
    {
        // 创建新的预设资产
        var preset = ScriptableObject.CreateInstance<EventTriggerPreset>();
        
        // 复制基础配置
        preset.triggerType = triggerType;
        preset.methodName = methodName;
        preset.methodParameter = methodParameter;
        preset.useMultiParameters = useMultiParameters;
        preset.methodParameters = new List<string>(methodParameters);
        preset.isPrefabMethod = isPrefabMethod;
        preset.prefabPath = prefabPath;
        
        // 复制条件系统配置
        preset.useConditionSystem = useConditionSystem;
        preset.conditionType = conditionType;
        preset.conditionID = conditionID;
        preset.conditionValue = conditionValue;
        
        // 处理目标覆盖
        preset.overrideTarget = includeTarget;
        if (includeTarget)
        {
            preset.targetOverride = targetObject;
        }
        
        // 设置预设名称
        preset.name = presetName;
        
        return preset;
    }
}