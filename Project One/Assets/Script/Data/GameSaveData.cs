using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameSaveData
{
    // 游戏元数据
    public string version = "1.0";
    public string saveTime;
    public int saveSlot;
    
    // 剧情进度
    public string currentStoryNodeID; // 当前剧情节点ID
    public List<string> completedStoryNodes = new List<string>(); // 已完成的剧情节点
    
    // 关卡进度
    public int lastUnlockedLevel = 1; // 最后解锁的关卡
    public Dictionary<int, LevelCompletionData> levelData = new Dictionary<int, LevelCompletionData>();
    
    // 角色信息
    public PlayerData playerData;
    
    // 物品库存
    public ItemData inventory = new ItemData();
}
