using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class LevelRecord
{
    public bool isCompleted;          // 是否完成
    public int highestScore;          // 最高分
    public float bestCompletionTime;  // 最快完成时间
    public int starsEarned;           // 获得的星星数
    public int attempts;              // 尝试次数
}
public class LevelManager : View
{
    [SerializeField] private LevelConfiguration[] levelNPCConfigs;

    private int currentLevelIndex = -1;

    public override string Name { get { return Consts.V_LevelManager; } }

    private GameObject Ts;

    // private Vector3 playerPositon;

    public void Start()
    {
        Ts = GameObject.Find("TStarter");
        if (Ts == null)
        {
            LoadLevel(1);
        }
    }
    public void LoadLevel(int levelIndex)
    {
        // 卸载当前关卡
        if (currentLevelIndex >= 0)
        {
            UnloadLevel(currentLevelIndex);
        }

        currentLevelIndex = levelIndex;
        levelIndex = levelIndex - 1;
        // 加载新关卡的NPC
        //Debug.Log(levelIndex-1);
        NPCManager.Instance.SpawnNpc(levelNPCConfigs[0]);
        //this.loadBackGround(levelNPCConfigs[0].background);
        // this.loadPlayer(levelNPCConfigs[0].player,levelNPCConfigs[0].playerPositon);
        // 加载其他关卡内容...
    }

    private void UnloadLevel(int levelIndex)
    {
        // Debug.Log(levelIndex);
        // NPCManager.Instance.UnloadLevelNPCs(levelNPCConfigs[levelIndex]);

        // 卸载其他关卡内容...
    }

    private void loadBackGround(GameObject background)
    {
        Vector3 v3 = new Vector3(0, 0, 0);
        var bg = Instantiate(background, v3, Quaternion.identity);
    }

    private void loadPlayer(GameObject player, Vector3 v3)
    {

        var bg = Instantiate(player, v3, Quaternion.identity);
    }

    public override void HandleEvent(string eventName, object data)
    {
        // throw new System.NotImplementedException();
    }
}
