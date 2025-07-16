using System.Collections.Generic;
using Newtonsoft.Json.Converters;
using UnityEngine;

/// <summary>
/// NPC基础数据配置类
/// </summary>
[System.Serializable]
public class NPCData {
    public int npcId;              // NPC唯一ID
    public string npcName;         // NPC显示名称
    public string portraitPath;    // 头像资源路径

    public string condition; //出现条件

    public bool isDialogue;//是否可对话

    public int dialogue_ID;//对话ID

}

/// <summary>
/// 对话选项数据类
/// </summary>
[System.Serializable]
public class DialogueOption {
    public int optionId;           // 选项ID

    public int dialogue_ID;  //对话ID
    public string optionText;      // 选项显示文本

    public string res; //定义选项事件

    public int resArges;//事件对应参数

    public int nextDialogueId;//下一句对话ID
    
}

/// <summary>
/// 对话内容数据类
/// </summary>
[System.Serializable]
public class DialogueData
{
    public int dialogueId;         // 对话唯一ID
    public int npcId;              // 关联的NPC ID
    public string text;            // 对话内容文本
    public string portraitOverride;// 覆盖默认头像(可选)
    public string condition;       // 完成条件(可选)

    public int nextDialogueId;     // 选择后跳转的对话ID

    public int optionID;//选项ID

    public string isDirtor;//演出名 
}

[System.Serializable]
public class NPCGroup
{
        public string groupID;
        public List<NPCPerformanceController> members = new List<NPCPerformanceController>();
        public bool isActive = true;
}

/// <summary>
/// 对话全局配置类(从JSON加载)
/// </summary>
[System.Serializable]
public class NPCConfig {
    public NPCData[] NPC_Base;         // 所有NPC配置数组
    
}

public class DialogueConfig {
   
    public DialogueData[] Dialogue_Content; // 所有对话配置数组
}

public class OptionConfig {
    public DialogueOption[] Dialogue_Options;         // 所有NPC配置数组
    //public DialogueData[] dialogues; // 所有对话配置数组
}