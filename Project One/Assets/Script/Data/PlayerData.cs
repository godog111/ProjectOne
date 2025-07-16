using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerData
{
    // 玩家基础信息
    public string playerName;
    public DateTime lastSaveTime;
    
    // 关卡数据字典，key为关卡ID，value为关卡记录
    public Dictionary<string, LevelRecord> levelRecords = new Dictionary<string, LevelRecord>();
    
    // 其他玩家属性可以在这里添加
    public int totalScore;
    public int coins;
}
