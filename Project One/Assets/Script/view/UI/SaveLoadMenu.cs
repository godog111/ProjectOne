using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using System.Linq;

public class SaveLoadMenu : BasePanel
{
    [Header("UI References")]
    [SerializeField] private Transform saveSlotsContainer;
    [SerializeField] private GameObject saveSlotPrefab;
    [SerializeField] private Button backButton;
    [SerializeField] private Text noSavesText;
    
    [Header("Preview Settings")]
    [SerializeField] private int maxSlots = 3;
    [SerializeField] private Sprite defaultThumbnail;

    public override string Name
    {
        get { return Consts.V_SaveLoadMenu; }    
    }


    private void OnEnable()
    {
        RefreshSaveSlots();
    }
    
    // 刷新所有存档槽位显示
    public void RefreshSaveSlots()
    {
        // 清除现有槽位
        foreach (Transform child in saveSlotsContainer)
        {
            Destroy(child.gameObject);
        }
        
        // 获取所有存档信息
        List<SaveSlotInfo> slotsInfo = SaveGameModel.GetAllSaveSlotsInfo();
        
        bool anySaveExists = false;
        
        // 为每个槽位创建UI
        foreach (var slot in Enumerable.Range(1, maxSlots))
        {
            int currentSlot = slot;
            GameObject slotObj = Instantiate(saveSlotPrefab, saveSlotsContainer);
            SaveSlotUI slotUI = slotObj.GetComponent<SaveSlotUI>();
            
            // 查找这个槽位的信息
            SaveSlotInfo info = slotsInfo.Find(s => s.slotNumber == currentSlot);
            
            if (info != null && info.hasData)
            {
                anySaveExists = true;
                // 加载详细存档数据用于显示更多信息
                GameSaveData saveData = SaveGameModel.LoadGameData(currentSlot);
                Debug.Log(saveData.version);
                // 设置UI显示
                slotUI.SetupForExistingSave(
                    slotNumber: currentSlot,
                    saveTime: info.saveTime,
                    previewText: GetPreviewText(saveData),
                    thumbnail: GetThumbnail(saveData),
                    onLoad: () => LoadGame(currentSlot),
                    onDelete: () => DeleteSave(currentSlot)
                );
            }
            else
            {
                // 设置空槽位UI
                slotUI.SetupForNewSave(
                    slotNumber: currentSlot,
                    onCreate: () => CreateNewGame(currentSlot)
                );
            }
        }
        
        noSavesText.gameObject.SetActive(!anySaveExists);
    }
    
    // 获取存档预览文本
    private string GetPreviewText(GameSaveData saveData)
    {
        return $"<b>进度:</b> 第{saveData.lastUnlockedLevel}关\n" +
               $"<b>角色:</b> Lv.{saveData.playerData.lastSaveTime} {saveData.playerData.playerName}\n" +
               $"<b>物品:</b> {saveData.inventory.items.Count}种\n" +
               $"<b>剧情:</b> {saveData.completedStoryNodes.Count}节点完成";
    }
    
    // 获取缩略图（可以根据游戏实际情况实现）
    private Sprite GetThumbnail(GameSaveData saveData)
    {
        // 这里可以:
        // 1. 从存档数据中保存的截图路径加载
        // 2. 根据进度使用不同的预设图片
        // 3. 动态生成缩略图
        return defaultThumbnail;
    }
    
    // 加载游戏
    private void LoadGame(int slot)
    {
        GameSaveData loadedData=SaveGameModel.LoadGameData(slot);
        SendEvent(Consts.E_ExitScene);
        SendEvent(Consts.E_EnterScene,loadedData);
        CloseMenu();
        Debug.Log(loadedData.saveTime);
    }
    
    // 创建新游戏
    private void CreateNewGame(int slot)
    {
        // 如果有存档，显示确认覆盖对话框
        if (SaveGameModel.HasSave(slot))
        {
            ConfirmationDialog.Show(
                "覆盖存档",
                "此槽位已有存档，确定要覆盖吗？",
                () => {
                   // GameManager.Instance.StartNewGame(slot);
                    CloseMenu();
                },
                () => { /* 取消操作 */ }
            );
        }
        else
        {
           // GameManager.Instance.StartNewGame(slot);
            CloseMenu();
        }
    }
    
    // 删除存档
    private void DeleteSave(int slot)
    {
        ConfirmationDialog.Show(
            "删除存档",
            "确定要删除此存档吗？此操作不可恢复！",
            () => {
                SaveGameModel.DeleteSave(slot);
                RefreshSaveSlots();
            },
            () => { /* 取消操作 */ }
        );
    }
    
    private void CloseMenu()
    {
        gameObject.SetActive(false);
        // 可能还需要调用其他关闭逻辑
    }

    public override void HandleEvent(string eventName, object data)
    {
        throw new NotImplementedException();
    }

    
}