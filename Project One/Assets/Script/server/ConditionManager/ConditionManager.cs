using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 条件类型枚举
/// </summary>
public enum ConditionType
{
    HasItem,        // 拥有物品HasItem
    VisitedLocation,// 访问过地点
    NpcInteraction, // 与NPC交互过
    QuestCompleted, // 任务完成
    CustomFlag      // 自定义标志
}

/// <summary>
/// 条件配置数据类
/// </summary>
[System.Serializable]
public class ConditionConfig
{
    public string Ctype;       // 条件类型
    public string id;               // 条件ID
    public string displayName;      // 显示名称（用于UI等）
    public string description;      // 条件描述
    public bool defaultValue;       // 默认值
    [NonSerialized] public bool currentValue; // 当前值（不序列化）
}
/// <summary>
/// 运行时条件值存储结构（用于JSON序列化）
/// </summary>
[System.Serializable]
public class RuntimeConditionValues
{
    public Dictionary<string, bool> values = new Dictionary<string, bool>();
}

/// <summary>
/// 条件配置集合（用于JSON配置）
/// </summary>
[System.Serializable]
public class ConditionConfigCollection
{
    public List<ConditionConfig> conditions = new List<ConditionConfig>();
}

/// <summary>
/// 条件变化事件参数
/// </summary>
public class ConditionChangedEventArgs : EventArgs
{
    public ConditionType Type { get; }   // 条件类型
    public string Id { get; }            // 条件ID
    public bool NewValue { get; }        // 新值
    public bool OldValue { get; }        // 旧值

    public ConditionChangedEventArgs(ConditionType type, string id, bool newValue, bool oldValue)
    {
        Type = type;
        Id = id;
        NewValue = newValue;
        OldValue = oldValue;
    }
}

// 条件变化事件委托
public delegate void ConditionChangedHandler(object sender, ConditionChangedEventArgs args);

/// <summary>
/// 条件管理器单例类（纯配置数据驱动）
/// </summary>
public class ConditionManager : MonoBehaviour
{
    private static ConditionManager _instance;
    private const string RUNTIME_VALUES_KEY = "ConditionRuntimeData";

    // 条件变化事件
    public event ConditionChangedHandler OnConditionChanged;

    // 单例访问器
    public static ConditionManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<ConditionManager>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject("ConditionManager");
                    _instance = obj.AddComponent<ConditionManager>();
                    DontDestroyOnLoad(obj);
                }
            }
            return _instance;
        }
    }

    // 存储所有条件配置的字典（键为"类型_ID"格式）
    private Dictionary<string, ConditionConfig> conditionConfigs = new Dictionary<string, ConditionConfig>();

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    /// <summary>
    /// 从JSON文件加载条件配置
    /// </summary>
    /// <param name="configFile">包含条件配置的JSON文件</param>
    public void LoadConditionConfigs(TextAsset configFile)
    {
        if (configFile == null)
        {
            Debug.LogError("Config file is null");
            return;
        }

        try
        {
            // 清空现有配置
            conditionConfigs.Clear();

            // 从JSON反序列化配置集合
            var collection = JsonUtility.FromJson<ConditionConfigCollection>(configFile.text);

            // 初始化所有条件配置
            foreach (var config in collection.conditions)
            {
                Debug.Log(config.Ctype);
                ConditionType condition = (ConditionType)Enum.Parse(typeof(ConditionType), config.Ctype);
                string key = GetConditionKey(condition, config.id);
                Debug.Log(key);
                
                // 设置当前值为默认值
                config.currentValue = config.defaultValue;
                
                // 添加到字典
                conditionConfigs[key] = config;
            }
            LoadRuntimeValues();

            Debug.Log($"Loaded {collection.conditions.Count} condition configs");
        }
        catch (Exception e)
        {
            Debug.LogError($"Load condition configs failed: {e.Message}");
        }
    }
    /// <summary>
    /// 加载保存的运行时条件值
    /// </summary>
    private void LoadRuntimeValues()
    {
        if (!PlayerPrefs.HasKey(RUNTIME_VALUES_KEY))
        {
            Debug.Log("No saved runtime values found, using defaults");
            return;
        }

        try
        {
            string json = PlayerPrefs.GetString(RUNTIME_VALUES_KEY);
            var runtimeData = JsonUtility.FromJson<RuntimeConditionValues>(json);

            foreach (var pair in runtimeData.values)
            {
                if (conditionConfigs.TryGetValue(pair.Key, out ConditionConfig config))
                {
                    config.currentValue = pair.Value;
                }
            }

            Debug.Log($"Loaded {runtimeData.values.Count} runtime condition values");
        }
        catch (Exception e)
        {
            Debug.LogError($"Load runtime values failed: {e.Message}");
        }
    }
    
    /// <summary>
    /// 保存当前所有运行时条件值
    /// </summary>
    public void SaveRuntimeValues()
    {
        try
        {
            var runtimeData = new RuntimeConditionValues();

            foreach (var pair in conditionConfigs)
            {
                runtimeData.values[pair.Key] = pair.Value.currentValue;
            }

            string json = JsonUtility.ToJson(runtimeData);
            PlayerPrefs.SetString(RUNTIME_VALUES_KEY, json);
            PlayerPrefs.Save();

            Debug.Log($"Saved {runtimeData.values.Count} runtime condition values");
        }
        catch (Exception e)
        {
            Debug.LogError($"Save runtime values failed: {e.Message}");
        }
    }

    /// <summary>
    /// 更新条件值并自动保存
    /// </summary>
    public void UpdateCondition(ConditionType type, string id, bool newValue, bool autoSave = true)
    {
        string key = GetConditionKey(type, id);
        
        if (!conditionConfigs.TryGetValue(key, out ConditionConfig config))
        {
            Debug.LogWarning($"Condition not found: {key}");
            return;
        }

        if (config.currentValue == newValue)
        {
            return;
        }

        bool oldValue = config.currentValue;
        config.currentValue = newValue;

        Debug.Log($"Condition updated: {key} = {newValue}");

        OnConditionChanged?.Invoke(this, new ConditionChangedEventArgs(type, id, newValue, oldValue));

        if (autoSave)
        {
            SaveRuntimeValues();
        }
    }

        /// <summary>
    /// 检查条件是否满足
    /// </summary>
    public bool CheckCondition(ConditionType type, string id)
    {
        string key = GetConditionKey(type, id);
        Debug.Log(key);
        
        if (conditionConfigs.TryGetValue(key, out ConditionConfig config))
        {
            return config.currentValue;
        }

        Debug.LogWarning($"Condition not found: {key}");
        return false;
    }

    /// <summary>
    /// 重置所有条件为默认值（不自动保存）
    /// </summary>
    public void ResetAllConditionsToDefault()
    {
        foreach (var pair in conditionConfigs)
        {
            var config = pair.Value;
            if (config.currentValue != config.defaultValue)
            {
                bool oldValue = config.currentValue;
                config.currentValue = config.defaultValue;
                ConditionType condition = (ConditionType)Enum.Parse(typeof(ConditionType), config.Ctype);
                OnConditionChanged?.Invoke(this, 
                    new ConditionChangedEventArgs(condition, config.id, config.defaultValue, oldValue));
            }
        }
        
        Debug.Log("All conditions reset to default values");
    }

    /// <summary>
    /// 清除所有保存的运行时数据
    /// </summary>
    public void ClearRuntimeData()
    {
        PlayerPrefs.DeleteKey(RUNTIME_VALUES_KEY);
        PlayerPrefs.Save();
        Debug.Log("Runtime condition data cleared");
    }

    /// <summary>
    /// 生成条件键（格式：类型_ID）
    /// </summary>
    private string GetConditionKey(ConditionType type, string id)
    {
        return $"{type}_{id}";
    }
    /// <summary>
    /// 更新条件值
    /// </summary>
    public void UpdateCondition(ConditionType type, string id, bool newValue)
    {
        string key = GetConditionKey(type, id);

        if (!conditionConfigs.TryGetValue(key, out ConditionConfig config))
        {
            Debug.LogWarning($"Condition not found: {key}");
            return;
        }

        // 如果值没有变化，则不触发事件
        if (config.currentValue == newValue)
        {
            return;
        }

        // 保存旧值
        bool oldValue = config.currentValue;

        // 更新值
        config.currentValue = newValue;

        Debug.Log($"Condition updated: {key} = {newValue}");

        // 触发条件变化事件
        OnConditionChanged?.Invoke(this, new ConditionChangedEventArgs(type, id, newValue, oldValue));
    }

    /// <summary>
    /// 检查多个条件是否全部满足（AND逻辑）
    /// </summary>
    public bool CheckConditionsAll(params (ConditionType type, string id)[] conditionsToCheck)
    {
        foreach (var condition in conditionsToCheck)
        {
            if (!CheckCondition(condition.type, condition.id))
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// 检查多个条件中是否有任意一个满足（OR逻辑）
    /// </summary>
    public bool CheckConditionsAny(params (ConditionType type, string id)[] conditionsToCheck)
    {
        foreach (var condition in conditionsToCheck)
        {
            if (CheckCondition(condition.type, condition.id))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 获取条件配置信息
    /// </summary>
    public ConditionConfig GetConditionConfig(ConditionType type, string id)
    {
        string key = GetConditionKey(type, id);
        return conditionConfigs.TryGetValue(key, out var config) ? config : null;
    }

    /// <summary>
    /// 获取所有条件配置
    /// </summary>
    public IEnumerable<ConditionConfig> GetAllConditionConfigs()
    {
        return conditionConfigs.Values;
    }

    /// <summary>
    /// 导出所有条件状态为字符串（用于调试）
    /// </summary>
    public string ExportConditions()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine("=== Current Conditions ===");
        
        foreach (var pair in conditionConfigs)
        {
            var config = pair.Value;
            sb.AppendLine($"{pair.Key} = {config.currentValue} (Default: {config.defaultValue})");
            sb.AppendLine($"Display: {config.displayName}, Desc: {config.description}");
            sb.AppendLine();
        }
        
        sb.AppendLine($"Total: {conditionConfigs.Count} conditions");
        return sb.ToString();
    }

    /// <summary>
    /// 订阅特定条件变化事件
    /// </summary>
    public void SubscribeToCondition(ConditionType type, string id, ConditionChangedHandler handler)
    {
        OnConditionChanged += (sender, args) => 
        {
            if (args.Type == type && args.Id == id)
            {
                handler(sender, args);
            }
        };
    }
    
    /// <summary>
    /// 取消订阅条件变化事件
    /// </summary>
    public void UnsubscribeFromCondition(ConditionType type, string id, ConditionChangedHandler handler)
    {
        OnConditionChanged -= (sender, args) => 
        {
            if (args.Type == type && args.Id == id)
            {
                handler(sender, args);
            }
        };
    }
}