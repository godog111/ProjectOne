using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 游戏管理器，处理游戏逻辑
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // 玩家状态
    public int playerHealth = 100;
    //public List<string> collectedItems = new List<string>();

    private void Awake()
    {
        // 单例模式
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 收集物品
    /// </summary>
    public void CollectItem(string itemId)
    {
        /* if (!collectedItems.Contains(itemId))
         {
             collectedItems.Add(itemId);
             // 可以在这里触发UI更新或音效
         }*/
    }

    /// <summary>
    /// 玩家受伤
    /// </summary>
    public void PlayerTakeDamage(int damage)
    {
        playerHealth -= damage;

        if (playerHealth <= 0)
        {
            PlayerDie();
        }
    }

    /// <summary>
    /// 玩家死亡
    /// </summary>
    private void PlayerDie()
    {
        // 显示死亡画面或重新加载关卡
        //  EnhancedGameSceneManager.Instance.LoadLevel(
        //     EnhancedGameSceneManager.Instance.currentSaveSlot);
    }

    /// <summary>
    /// 重置游戏状态
    /// </summary>
    public void ResetGameState()
    {
        playerHealth = 100;
        //  collectedItems.Clear();
    }

    public List<PackageLocalItem> GetSortPackageLocalData()
    {
        List<PackageLocalItem> localItems = PackageLocalData.Instance.LoadPackage();
        localItems.Sort();//排序 后续可以拓展排序规则
        return localItems;
    }
}