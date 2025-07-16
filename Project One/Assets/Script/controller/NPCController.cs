using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCController : MonoBehaviour
{
    public string NPCID { get; private set; }
    public NPCType NPCType { get; private set; }
    
   // private DialogueData currentDialogue;
    
    public void Initialize(NPCConfiguration config)
    {
        NPCID = config.ID;
        NPCType = config.type;
      //  currentDialogue = config.startingDialogue;
        
        // 其他初始化逻辑...
    }
    
    // 与玩家交互
   /* public void Interact()
    {
        if (currentDialogue != null)
        {
            DialogueSystem.Instance.StartDialogue(currentDialogue);
        }
    }
    
    // 更新NPC状态(如任务完成后)
    public void UpdateNPCState(DialogueData newDialogue)
    {
        currentDialogue = newDialogue;
    }*/
}
