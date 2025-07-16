using UnityEngine;

/// <summary>
/// NPC对话系统与演出系统的集成示例
/// </summary>
public class NPCDialogueSystem : MonoBehaviour
{
    public NPCPerformanceController performanceController;
    
    /// <summary>
    /// 根据对话行触发对应演出
    /// </summary>
    public void OnDialogueLineStart(DialogueLine line)
    {
        if(performanceController == null) return;
        
        switch(line.emotion)
        {
            case DialogueEmotion.Neutral:
                performanceController.RequestPerformance("Neutral");
                break;
                
            case DialogueEmotion.Happy:
                performanceController.RequestPerformance("Happy");
                break;
                
            case DialogueEmotion.Angry:
                // 愤怒演出有高优先级
                performanceController.RequestPerformanceWithPriority("Angry");
                break;
                
            case DialogueEmotion.Surprise:
                // 惊讶演出会中断当前演出
                performanceController.RequestPerformanceWithPriority("Surprise");
                break;
        }
        
        // 特殊动作触发
        if(line.specialAction != "")
        {
            performanceController.RequestPerformance(line.specialAction);
        }
    }
}

/// <summary>
/// 对话行数据结构
/// </summary>
[System.Serializable]
public class DialogueLine
{
    public string text;
    public DialogueEmotion emotion;
    public string specialAction;
}

public enum DialogueEmotion
{
    Neutral,
    Happy,
    Angry,
    Surprise
}