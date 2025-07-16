using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System;
using UnityEngine.Scripting;

/// <summary>
/// 增强版游戏场景管理器，支持多存档和更多功能
/// </summary>

public class SaveGameModel : Model
{
    private const string SAVE_FILE_PREFIX = "save_";
    private const string SAVE_FILE_EXTENSION = ".json";
    public static SaveGameModel Instance { get; private set; }

    public override string Name
    {
        get { return Consts.M_SaveGameModel; }    
    }

   
    // 当前选择的存档槽
    public int currentSaveSlot = 0;
    
    // 存档数据路径
    private static string saveDataPath;
    
    // 关卡总数
    public  int TotalLevels = 5;

    private const string SAVE_FOLDER = "Saves";
    private const string SAVE_EXTENSION = ".json";   
    

 
    
    /// <summary>
    /// 保存当前游戏状态
    /// </summary>
    public void SaveCurrentGame(GameSaveData saveData, int slot)
    {
        saveData.saveSlot = slot;
        saveData.saveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        
        string jsonData = JsonUtility.ToJson(saveData, true);
        string savePath = GetSaveFilePath(slot);
        
        try
        {
            File.WriteAllText(savePath, jsonData);
            Debug.Log($"游戏已保存到槽位 {slot}, 路径: {savePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"保存游戏失败: {e.Message}");
        }
    }


    // 删除指定槽位的存档
    public static bool DeleteSave(int slot)
    {
        string savePath = GetSaveFilePath(slot);
        
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
            Debug.Log($"已删除槽位 {slot} 的存档");
            return true;
        }
        
        return false;
    }
    // 检查指定槽位是否有存档
    public static bool HasSave(int slot)
    {
        return File.Exists(GetSaveFilePath(slot));
    }
    
    // 捕获屏幕截图协程,model里面不能用，后续转到截图脚本去
    private System.Collections.IEnumerator CaptureScreenshot(GameSaveData saveData)
    {
        yield return new WaitForEndOfFrame();
        
        Texture2D screenshot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        screenshot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        screenshot.Apply();
        
       // saveData.screenshot = screenshot;
    }
    //创建新存档,开始新游戏时创建
    public static GameSaveData CreateNewSave(int slot)
    {
        return new GameSaveData()
        {
            saveSlot = 1,
            saveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            currentStoryNodeID = "start",
            playerData = new PlayerData()
            {
                playerName = "Player",
                lastSaveTime = DateTime.Now,
                
            },
            inventory = new ItemData()
        };
    }

    // 获取存档文件路径
    private static string GetSaveFilePath(int slot)
    {
        return Path.Combine(Application.persistentDataPath, $"{SAVE_FILE_PREFIX}{slot}{SAVE_FILE_EXTENSION}");
    }


     // 获取所有存档槽位信息
    public static List<SaveSlotInfo> GetAllSaveSlotsInfo()
    {
        List<SaveSlotInfo> slotsInfo = new List<SaveSlotInfo>();
        
        for (int i = 1; i <= 3; i++) // 假设有3个存档槽位
        {
            string path = GetSaveFilePath(i);
            if (File.Exists(path))
            {
                try
                {
                    string jsonData = File.ReadAllText(path);
                    GameSaveData data = JsonUtility.FromJson<GameSaveData>(jsonData);
                    slotsInfo.Add(new SaveSlotInfo()
                    {
                        slotNumber = i,
                        hasData = true,
                        saveTime = data.saveTime,
                        previewInfo = $"关卡: {data.lastUnlockedLevel}, 角色: {data.playerData.playerName}"
                    });
                }
                catch
                {
                    slotsInfo.Add(new SaveSlotInfo()
                    {
                        slotNumber = i,
                        hasData = false,
                        saveTime = "损坏的存档",
                        previewInfo = ""
                    });
                }
            }
            else
            {
                slotsInfo.Add(new SaveSlotInfo()
                {
                    slotNumber = i,
                    hasData = false,
                    saveTime = "空槽位",
                    previewInfo = ""
                });
            }
        }
        
        return slotsInfo;
    }


    
    /// <summary>
    /// 加载指定关卡
    /// </summary>
    public void LoadLevel(int level)
    {
        SceneManager.LoadScene(level);
    }

    
    /// <summary>
    /// 通关当前关卡
    /// </summary>
    public void CompleteCurrentLevel()
    {

    }
    
    /// <summary>
    /// 获取所有存档数据
    /// </summary>

    
    // 保存游戏数据到文件
    private void SaveGameData(GameSaveData data, int slot)
    {
        string filePath = saveDataPath + "save_" + slot + ".dat";
        
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(filePath);
        
        bf.Serialize(file, data);
        file.Close();
    }
    
    // 从文件加载游戏数据
    public static GameSaveData LoadGameData(int slot)
    {
        string savePath = GetSaveFilePath(slot);
        
        if (!File.Exists(savePath))
        {
            Debug.LogWarning($"槽位 {slot} 的存档文件不存在");
            return null;
        }
        
        try
        {
            string jsonData = File.ReadAllText(savePath);
            GameSaveData loadedData = JsonUtility.FromJson<GameSaveData>(jsonData);
            
            // 验证加载的数据
            if (loadedData == null || string.IsNullOrEmpty(loadedData.version))
            {
                Debug.LogError("存档数据损坏或格式不正确");
                return null;
            }
            
            Debug.Log($"从槽位 {slot} 成功加载游戏");
            return loadedData;
        }
        catch (Exception e)
        {
            Debug.LogError($"加载游戏失败: {e.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// 保存游戏设置
    /// </summary>
    public void SaveGameSettings()
    {

    }


     private static string GetSaveDirectory()
    {
        string path = Path.Combine(Application.persistentDataPath);
        
        // 确保目录存在
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        
        return path;
    }
    // 获取特定槽位的完整存档路径
    public static string GetSavePath(int slot)
    {
        return Path.Combine(GetSaveDirectory(), $"save_{slot}{SAVE_EXTENSION}");
    } 

    


}
public class SaveSlotInfo
{
    public int slotNumber;
    public bool hasData;
    public string saveTime;
    public string previewInfo;
}