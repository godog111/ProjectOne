using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NPCType
{
    Merchant,
    QuestGiver,
    Blacksmith,
    Healer,
    // NPC类型，写死定义枚举了，可以添加更多类型...
}

public enum NPCState
{
    Idle,       // 闲置状态
    Talking,    // 对话中
    Moving,     // 移动中
    Interacting // 正在交互(如交易、任务等)
}
[CreateAssetMenu(fileName = "ConNPCConfiguration", menuName = "Game/NPC Database")]
[System.Serializable]
public class ConNPCConfiguration: ScriptableObject{
    public NPCConfiguration[] CCCEnter;

   [System.Serializable]
    public struct NPCPrefab
    {
        public NPCType type;
        public GameObject prefab;
    }

    public List<NPCPrefab> npcPrefabs;
    public GameObject GetPrefab(NPCType type)
    {
        var found = npcPrefabs.Find(x1 => x1.type == type);
        return found.prefab;
    }
}
