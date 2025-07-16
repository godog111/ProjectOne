using System;
using System.Collections.Generic;
using UnityEngine;




public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }
    
    private PlayerData playerData;
    private const string SAVE_KEY = "PlayerSaveData";
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadPlayerData();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    #region 存档/读档方法
    
    // 加载玩家数据
    private void LoadPlayerData()
    {
        string jsonData = PlayerPrefs.GetString(SAVE_KEY, "");
        
        if (!string.IsNullOrEmpty(jsonData))
        {
            try
            {
                playerData = JsonUtility.FromJson<PlayerData>(jsonData);
            }
            catch (Exception e)
            {
                Debug.LogError("加载存档失败: " + e.Message);
                CreateNewPlayerData();
            }
        }
        else
        {
            CreateNewPlayerData();
        }
    }
    
    // 创建新玩家数据
    private void CreateNewPlayerData()
    {
        playerData = new PlayerData
        {
            playerName = "NewPlayer",
            lastSaveTime = DateTime.Now,
            totalScore = 0,
            coins = 0
        };
        
        SavePlayerData();
    }
    
    // 保存玩家数据
    public void SavePlayerData()
    {
        playerData.lastSaveTime = DateTime.Now;
        
        try
        {
            string jsonData = JsonUtility.ToJson(playerData);
            PlayerPrefs.SetString(SAVE_KEY, jsonData);
            PlayerPrefs.Save();
        }
        catch (Exception e)
        {
            Debug.LogError("保存存档失败: " + e.Message);
        }
    }
    
    // 删除存档
    public void DeleteSaveData()
    {
        PlayerPrefs.DeleteKey(SAVE_KEY);
        CreateNewPlayerData();
    }
    
    #endregion
    
    #region 玩家属性访问方法
    
    public string GetPlayerName()
    {
        return playerData.playerName;
    }
    
    public void SetPlayerName(string newName)
    {
        playerData.playerName = newName;
        SavePlayerData();
    }
    
    public int GetTotalScore()
    {
        return playerData.totalScore;
    }
    
    public void AddToTotalScore(int score)
    {
        playerData.totalScore += score;
        SavePlayerData();
    }
    
    public int GetCoins()
    {
        return playerData.coins;
    }
    
    public void AddCoins(int amount)
    {
        playerData.coins += amount;
        SavePlayerData();
    }
    
    #endregion
    
    #region 关卡记录管理方法
    
    // 获取指定关卡记录，如果不存在则创建
    public LevelRecord GetOrCreateLevelRecord(string levelId)
    {
        if (!playerData.levelRecords.ContainsKey(levelId))
        {
            playerData.levelRecords[levelId] = new LevelRecord();
        }
        
        return playerData.levelRecords[levelId];
    }
    
    // 更新关卡记录
    public void UpdateLevelRecord(string levelId, int score, float completionTime, int stars)
    {
        LevelRecord record = GetOrCreateLevelRecord(levelId);
        
        // 更新最高分
        if (score > record.highestScore)
        {
            record.highestScore = score;
        }
        
        // 更新最快完成时间
        if (completionTime < record.bestCompletionTime || !record.isCompleted)
        {
            record.bestCompletionTime = completionTime;
        }
        
        // 更新星星数
        if (stars > record.starsEarned)
        {
            record.starsEarned = stars;
        }
        
        // 标记为已完成
        record.isCompleted = true;
        
        // 增加尝试次数
        record.attempts++;
        
        SavePlayerData();
    }
    
    // 获取关卡是否已完成
    public bool IsLevelCompleted(string levelId)
    {
        if (playerData.levelRecords.TryGetValue(levelId, out LevelRecord record))
        {
            return record.isCompleted;
        }
        return false;
    }
    
    // 获取关卡最高分
    public int GetLevelHighestScore(string levelId)
    {
        if (playerData.levelRecords.TryGetValue(levelId, out LevelRecord record))
        {
            return record.highestScore;
        }
        return 0;
    }
    
    // 获取关卡最快完成时间
    public float GetLevelBestTime(string levelId)
    {
        if (playerData.levelRecords.TryGetValue(levelId, out LevelRecord record))
        {
            return record.bestCompletionTime;
        }
        return float.MaxValue;
    }
    
    // 获取关卡获得的星星数
    public int GetLevelStars(string levelId)
    {
        if (playerData.levelRecords.TryGetValue(levelId, out LevelRecord record))
        {
            return record.starsEarned;
        }
        return 0;
    }
    
    // 获取关卡尝试次数
    public int GetLevelAttempts(string levelId)
    {
        if (playerData.levelRecords.TryGetValue(levelId, out LevelRecord record))
        {
            return record.attempts;
        }
        return 0;
    }
    
    // 获取所有关卡ID列表
    public List<string> GetAllLevelIds()
    {
        return new List<string>(playerData.levelRecords.Keys);
    }
    
    #endregion
}