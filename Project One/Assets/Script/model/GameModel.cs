using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameModel : Model
{

    int gold = 0;//游戏积分

    int m_GameProgress = -1;//游戏最大关卡数量
    int m_CurrentLevelIndex = -1;//游戏当前关卡


    //bool m_isPlaying = false;

    public override string Name { get { return Consts.M_GameModel; } }

    public int Gold { get { return gold; } set { gold = value; } }
    public int GameProgress { get { return m_GameProgress; } set { m_GameProgress = value; } }
    public int CurrentLevelIndex { get { return m_CurrentLevelIndex; } set { m_CurrentLevelIndex = value; } }

    #region 方法
    public void Initialize()
    {
        //构建关卡
    }

    //开始游戏
    public void StartGame(int level)
    {

    }
    //结束游戏
    public void EndGame(bool isSuccess)
    {


    }
    //进入剧情场景
    public void EnterScene(int level)
    {

    }
    //退出场景
    public void EndScene()
    {

    }
    //获取背包动态数据
    public List<PackageLocalItem> GetSortPackageLocalData()
    {
        List<PackageLocalItem> localItems = PackageLocalData.Instance.LoadPackage();
        //localItems.Sort();//排序 后续可以拓展排序规则
        return localItems;
    }
    #endregion
}
