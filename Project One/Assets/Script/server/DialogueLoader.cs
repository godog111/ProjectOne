using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 对话配置加载管理器(单例模式)
/// 负责加载和提供对话配置数据
/// </summary>
public class DialogueLoader : MonoBehaviour
{
    private static DialogueLoader _instance;
    public static DialogueLoader Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<DialogueLoader>();

                if (_instance == null)
                {
                    GameObject obj = new GameObject("DialogueLoader");
                    _instance = obj.AddComponent<DialogueLoader>();
                    DontDestroyOnLoad(obj);
                }
            }
            return _instance;
        }
    }

    // 数据存储字典
    private Dictionary<int, NPCData> _npcDataDict = new Dictionary<int, NPCData>();
    private Dictionary<int, DialogueData> _dialogueDataDict = new Dictionary<int, DialogueData>();
    private Dictionary<int, List<DialogueData>> _npcDialoguesDict = new Dictionary<int, List<DialogueData>>();

    private Dictionary<int, DialogueOption> _optionDataDict = new Dictionary<int, DialogueOption>();//选项字典

    private Dictionary<int, List<DialogueOption>> _optionDiaDataDict = new Dictionary<int, List<DialogueOption>>();//选项字典

   // private Dictionary<int, DialogueOption> _performanceDataDict = new Dictionary<int, DialogueOption>();//演出字典

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);

        LoadDialogueConfig();
    }

    /// <summary>
    /// 从JSON文件加载对话配置
    /// </summary>
    private void LoadDialogueConfig()
    {
        // 从Resources加载JSON文件
        TextAsset npc_configFile = Resources.Load<TextAsset>("GameData/NPC_Base");
        TextAsset dialogues_configFile = Resources.Load<TextAsset>("GameData/Dialogue_Content");
        TextAsset options_configFile = Resources.Load<TextAsset>("GameData/Dialogue_Options");
        if (options_configFile == null || dialogues_configFile == null || options_configFile == null)
        {
            Debug.LogError("对话配置文件未找到!");
            return;
        }
        Debug.Log(npc_configFile.text.Length);

        // 反序列化JSON数据
        NPCConfig npc_config = JsonUtility.FromJson<NPCConfig>(npc_configFile.text);
        Debug.Log(npc_config.NPC_Base[1].npcName);
        DialogueConfig dialogues_config = JsonUtility.FromJson<DialogueConfig>(dialogues_configFile.text);
        OptionConfig options_config = JsonUtility.FromJson<OptionConfig>(options_configFile.text);

        // 构建NPC数据字典
        foreach (var npc in npc_config.NPC_Base)
        {
            _npcDataDict[npc.npcId] = npc;
        }


        // 构建对话数据字典和NPC对话关系
        foreach (var dialogue in dialogues_config.Dialogue_Content)
        {
            _dialogueDataDict[dialogue.dialogueId] = dialogue;

            // 按NPC ID分组存储对话
            if (!_npcDialoguesDict.ContainsKey(dialogue.npcId))
            {
                _npcDialoguesDict[dialogue.npcId] = new List<DialogueData>();
            }
            _npcDialoguesDict[dialogue.npcId].Add(dialogue);
        }

        foreach (var option in options_config.Dialogue_Options)
        {
            _optionDataDict[option.optionId] = option;

            //按照对话ID存储选项
            if (!_optionDiaDataDict.ContainsKey(option.dialogue_ID))
            {
                //Debug.Log(option.dialogue_ID);
                _optionDiaDataDict[option.dialogue_ID] = new List<DialogueOption>();
            }
            _optionDiaDataDict[option.dialogue_ID].Add(option);

        }
    }

    /// <summary>
    /// 获取指定NPC的数据
    /// </summary>
    public NPCData GetNPCData(int npcId)
    {
        Debug.Log("获取NPC数据" + npcId);
        return _npcDataDict.TryGetValue(npcId, out var data) ? data : null;
    }

    /// <summary>
    /// 获取指定对话ID的对话数据
    /// </summary>
    public DialogueData GetDialogueData(int dialogueId)
    {
        Debug.Log("获取对话数据" + dialogueId);
        return _dialogueDataDict.TryGetValue(dialogueId, out var data) ? data : null;
    }

    /// <summary>
    /// 获取NPC关联的所有对话
    /// </summary>
    public List<DialogueData> GetDialoguesForNPC(int npcId)
    {
        Debug.Log("获取关联NPC对话数据" + npcId);
        return _npcDialoguesDict.TryGetValue(npcId, out var dialogues) ? dialogues : null;
    }

    //获取指定选项
    public DialogueOption GetOption(int optionId)
    {
        return _optionDataDict.TryGetValue(optionId, out var data) ? data : null;
    }

    public List<DialogueOption> GetDialoguesForOption(int dialoguaId)
    {
        //Debug.Log(dialoguaId);
        return _optionDiaDataDict.TryGetValue(dialoguaId, out var options) ? options : null;
        //Debug.Log(options);
    }
    
}