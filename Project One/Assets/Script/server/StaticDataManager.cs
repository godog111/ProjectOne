using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class StaticDataManager
{
    // 存储所有加载的数据，以文件名作为键
    private static readonly Dictionary<string, object> _dataCache = new Dictionary<string, object>();
    
    // 配置：JSON文件所在的文件夹路径（相对于Resources文件夹）
    private const string JSON_FOLDER_PATH = "GameData/";

    // 初始化标志
    private static bool _isInitialized = false;

    /// <summary>
    /// 初始化数据管理器，加载所有JSON文件
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        if (_isInitialized) return;
        
        LoadAllJsonData();
        _isInitialized = true;
        
        Debug.Log("StaticDataManager initialized successfully.");
    }

    /// <summary>
    /// 加载指定文件夹中的所有JSON文件
    /// </summary>
    private static void LoadAllJsonData()
    {
        // 从Resources文件夹加载所有文本文件
        TextAsset[] jsonFiles = Resources.LoadAll<TextAsset>(JSON_FOLDER_PATH);

        if (jsonFiles == null || jsonFiles.Length == 0)
        {
            Debug.LogWarning($"No JSON files found in Resources/{JSON_FOLDER_PATH}");
            return;
        }

        foreach (var file in jsonFiles)
        {
            string fileName = Path.GetFileNameWithoutExtension(file.name);
           // Debug.Log($"Loading JSON file: {fileName}");
            
            try
            {
                // 处理包含数组/列表的JSON数据
                if (file.text.Contains("\"Dialogue_Content\":"))
                {
                    var wrapper = JsonUtility.FromJson<DialogueWrapper>(file.text);
                    // Debug.Log($"Loaded JSON data: {wrapper.Dialogue_Content[1].dialogueId}");
                    _dataCache[fileName] = wrapper.Dialogue_Content;
                }
                else if (file.text.Contains("\"item\":"))
                {
                    var wrapper = JsonUtility.FromJson<ItemList>(file.text);
                    //Debug.Log($"Loaded JSON data: {wrapper.item[1].description}");
                    _dataCache[fileName] = wrapper.item;
                }
                else if (file.text.Contains("\"Dialogue_Options\":"))
                {
                    var wrapper = JsonUtility.FromJson<DialogueOptionsWrapper>(file.text);
                    _dataCache[fileName] = wrapper.Dialogue_Options;
                }
                else
                {
                    // 默认处理为单个对象
                    object data = JsonUtility.FromJson<object>(file.text);
                    _dataCache[fileName] = data;
                }
                
                Debug.Log($"Loaded JSON data: {fileName}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to parse JSON file: {fileName}. Error: {e.Message}");
            }
        }
    }

    /// <summary>
    /// 获取指定名称的JSON数据（泛型方法）
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    /// <param name="fileName">JSON文件名（不带扩展名）</param>
    /// <returns>解析后的数据对象</returns>
    public static T GetData<T>(string fileName)
    {
        if (!_isInitialized)
        {
            Debug.LogWarning("StaticDataManager not initialized. Initializing now...");
            Initialize();
        }

        if (_dataCache.TryGetValue(fileName, out object data))
        {
            if (data is T typedData)
            {
                return typedData;
            }
            
            try
            {
                // 如果存储的是Json字符串，尝试重新解析
                if (data is string jsonString)
                {
                    T parsedData = JsonUtility.FromJson<T>(jsonString);
                    _dataCache[fileName] = parsedData; // 更新缓存
                    return parsedData;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to convert data to type {typeof(T)}. Error: {e.Message}");
            }
        }

        Debug.LogError($"Data not found or type mismatch for file: {fileName}");
        return default;
    }

    /// <summary>
    /// 获取指定名称的JSON列表数据
    /// </summary>
    public static List<T> GetDataList<T>(string fileName)
    {
        if (!_isInitialized)
        {
            Debug.LogWarning("StaticDataManager not initialized. Initializing now...");
            Initialize();
        }

        if (_dataCache.TryGetValue(fileName, out object data))
        {
            //Debug.Log("已找到"+data);
            if (data is List<T> listData)
            {
                
                return listData;
            }
            
            // 处理数组情况
            if (data is T[] arrayData)
            {Debug.Log("已找3到"+data);
                return new List<T>(arrayData);
            }
        }
        else{Debug.Log("不存在");}

        Debug.LogError($"List data not found or type mismatch for file: {fileName}");
        return new List<T>();
    }

    /// <summary>
    /// 检查指定文件的数据是否已加载
    /// </summary>
    public static bool HasData(string fileName)
    {
        return _dataCache.ContainsKey(fileName);
    }

    /// <summary>
    /// 清空所有缓存数据（主要用于测试和热重载）
    /// </summary>
    public static void ClearCache()
    {
        _dataCache.Clear();
        _isInitialized = false;
        Debug.Log("StaticDataManager cache cleared.");
    }

    public static ItemArgs GetItemArgs(int itemID)
    {
        List<ItemArgs> timeList = null;
        timeList =StaticDataManager.GetDataList<ItemArgs>("item");
        ItemArgs item = null;
        foreach(ItemArgs itemArgs in timeList)
        {
            if (itemArgs.id == itemID)
            {
                item = itemArgs;
                break;
            }
        }
        return item;
    }











    // 包装类用于解析包含Dialogue_Content数组的JSON
    [System.Serializable]
    private class DialogueWrapper
    {
        public List<DialogueData> Dialogue_Content;
    }
    [System.Serializable]
    private class DialogueOptionsWrapper
    {
        public List<DialogueOption> Dialogue_Options;
    }

    // 通用列表包装类
    [System.Serializable]
    private class GenericListWrapper<T>
    {
        public T[] Items;
    }
    
}