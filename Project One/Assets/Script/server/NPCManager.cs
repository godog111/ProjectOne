using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NPCManager : MonoBehaviour
{

    public static NPCManager Instance;

    [SerializeField] private ConNPCConfiguration npcDatabase;
    public LevelConfiguration currentLevelConfig;

    private Dictionary<string, GameObject> spawnedUniqueNPCs = new Dictionary<string, GameObject>();
    [Tooltip("NPC分组配置")]
    public List<NPCGroup> npcGroups = new List<NPCGroup>();

    #region 私有变量
    private Dictionary<string, NPCGroup> groupDict = new Dictionary<string, NPCGroup>();
    private float updateTimer;

    
    private List<NPCPerformanceController> activeNPCs = new List<NPCPerformanceController>();
    #endregion
    void Awake()//判断当前是否唯一
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            ConditionManager.Instance.OnConditionChanged += OnConditionChanged;

            
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void OnDestroy()
    {
        if (Instance == this)
        {
            // 取消订阅条件变化事件
            ConditionManager.Instance.OnConditionChanged -= OnConditionChanged;
        }
    }
    
    //事件订阅，条件变化时可以调用
    public void OnConditionChanged(object sender, ConditionChangedEventArgs args)
    {
        // 示例：当特定任务完成时，让所有NPC欢呼
        /*if (args.Type == ConditionType.QuestCompleted && args.Id == "main_quest" && args.NewValue)
        {
            PerformToAll(n => true, "Cheer");
        }*/

        // 可以根据需要添加更多条件处理逻辑
    }


    public void SpawnNpc(LevelConfiguration levelConfig)
    {

        foreach (var npcConfig in levelConfig.npcsInLevel)
        {

            // 检查唯一NPC是否已经存在
            if (npcConfig.isUnique && spawnedUniqueNPCs.ContainsKey(npcConfig.ID))
            {
                Debug.Log("已存在，只需移动位置或激活");
                var existingNPC = spawnedUniqueNPCs[npcConfig.ID];
                existingNPC.transform.position = npcConfig.spawnPosition;
                existingNPC.SetActive(true);
                continue;
            }
            Debug.Log(npcConfig.type);
            var prefab = npcDatabase.GetPrefab(npcConfig.type);//生成新的NPC开始

            if (prefab != null)
            {
                var npcInstance = Instantiate(prefab, npcConfig.spawnPosition, Quaternion.identity);

                // 初始化NPC组件
                var npcController = npcInstance.GetComponent<NPCController>();
                if (npcController != null)
                {
                    npcController.Initialize(npcConfig);
                }

                // 如果是唯一NPC，加入字典
                if (npcConfig.isUnique)
                {
                    spawnedUniqueNPCs.Add(npcConfig.ID, npcInstance);
                }

            }
        }
    }

    public void targetSpawnNpc(string resources, string prefabName, string position)
    {
        Debug.Log(resources);
        GameObject prefab = Resources.Load<GameObject>(resources);
        if (prefab != null)
        {
            // 实例化预设
            GameObject instance = Instantiate(prefab);

            // 可选：设置位置和旋转
            Vector2 coordinate = StringToVector2(position);
            instance.transform.position = coordinate;
            instance.transform.rotation = Quaternion.identity;
        }
        else
        {
            Debug.LogError("Prefab not found in Resources folder!");
        }
        //  var npcInstance = Instantiate(prefab, npcConfig.spawnPosition, Quaternion.identity);
    }

    //销毁NPC
    public void UnloadLevelNPCs(LevelConfiguration levelConfig)
    {
        foreach (var npcConfig in levelConfig.npcsInLevel)
        {
            if (!npcConfig.isUnique)
            {
                // 查找并销毁非唯一NPC
                var npcs = FindObjectsOfType<NPCController>();
                foreach (var npc in npcs)
                {
                    if (npc.NPCID == npcConfig.ID && !npcConfig.isUnique)
                    {
                        Destroy(npc.gameObject);
                    }
                }
            }
            else
            {
                // 唯一NPC只是禁用而非销毁
                if (spawnedUniqueNPCs.TryGetValue(npcConfig.ID, out var npcObj))
                {
                    npcObj.SetActive(false);
                }
            }
        }
    }

    /// <summary>
    /// 向指定分组NPC发送演出指令
    /// </summary>
    public void PerformToGroup(string groupID, string performanceName, bool forceInterrupt = false)
    {
        if (groupDict.TryGetValue(groupID, out var group))
        {
            foreach (var npc in group.members.Where(n => n != null))
            {
                if (forceInterrupt)
                    npc.RequestPerformanceWithPriority(performanceName);
                else
                    npc.RequestPerformance(performanceName);
            }
        }
    }

    /// <summary>
    /// 向符合条件的所有NPC发送演出指令
    /// </summary>
    public void PerformToAll(System.Predicate<NPCPerformanceController> condition, string performanceName)
    {
        foreach (var group in npcGroups)
        {
            foreach (var npc in group.members.Where(n => n != null && condition(n)))
            {
                npc.RequestPerformance(performanceName);
            }
        }
    }
    /// <summary>
    /// 注册新NPC到指定分组
    /// </summary>
    public void RegisterNPC(NPCPerformanceController npc, string groupID)
    {
        if (groupDict.TryGetValue(groupID, out var group))
        {
            if (!group.members.Contains(npc))
            {
                group.members.Add(npc);
                //预加载NPC资源，后续看情况重做
                //npc.InitializePerformances(globalPerformances.ToList());
            }
        }
        else
        {
            Debug.LogWarning($"创建新NPC分组: {groupID}");
            var newGroup = new NPCGroup() { groupID = groupID };
            newGroup.members.Add(npc);
            npcGroups.Add(newGroup);
            groupDict.Add(groupID, newGroup);
        }
    }
    
    Vector2 StringToVector2(string str)
    {
        // 分割字符串
        string[] parts = str.Split(',');
        
        if (parts.Length != 2)
        {
            Debug.LogError("字符串格式不正确，应为'x,y'格式");
            return Vector2.zero;
        }

        // 转换为float
        float x = float.Parse(parts[0]);
        float y = float.Parse(parts[1]);
        
        return new Vector2(x, y);
    }
}
