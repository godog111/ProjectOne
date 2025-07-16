using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using UnityEngine;

public static class Consts
{
    //目录
    public static string LevelDir = Application.dataPath + @"/game/Res/Levels/";//关卡目录
    public static string MapDir = Application.dataPath + @"/game/soc/";//关卡目录

    //存档
    public const string GameProgress = "GameProgress";
    public const float DotClosedDistance = 0.1f;
    public const float RangeCloseDistance = 0.7f;


    //Model
    public const string M_RoundModel = "M_RoundModel";
    public const string M_GameModel = "M_GameModel";
    public const string M_SaveGameModel = "M_SaveGameModel";

    //View

    public const string V_start = "V_start";
    public const string V_Select = "V_Select";
    public const string V_CountDown = "V_CountDown";
    public const string V_Board = "V_Board";
    public const string V_Win = "V_Win";
    public const string V_Lost = "V_Lost";
    public const string V_System = "V_System";
    public const string V_Spawner = "V_Spawner";
    public const string V_Complete = "V_Complete";
    public const string V_TowerPopup = "V_TowerPopup";

    public const string V_LevelManager ="V_LevelManager";

    public const string V_SaveLoadMenu="V_SaveLoadMenu";

    public const string V_ryGameSpwan="V_ryGameSpwan";

    public const string V_gameOver="V_gameOver";
    public const string V_RhythmGuideLine="V_RhythmGuideLine";
    
    public const string V_RhythmManager="V_RhythmManager";

     public const string V_AIHead = "V_AIHead";

    //Controller
    public const string E_StartUp = "E_StartUP";//
    public const string E_EnterScene = "E_EnterScene";//SceneArgs
    public const string E_ExitScene = "E_ExitScene";//SceneArgs
    public const string E_StartLevel = "E_StartLevel";//SceneArgs
    public const string E_EndLevel = "E_EndLevel";//SceneArgs
    public const string E_CountDownComplete = "E_CountDownComplete";//SceneArgs

    public const string E_CreatNewGame = "E_CreatNewGame";//SceneArgs

  

    public const string E_StartRound = "E_StartRound";//
    public const string E_SpawnMonster = "E_SpawnMonster";//Args

    public const string E_ShowSpawnPanel = "E_ShowSpawnPanel";//
    public const string E_ShowUpgradePanel = "E_ShowUpgradePanel";//
    public const string E_HidePopups = "E_HidePopups";//

    public const string E_SpawnTower = "E_SpawnTower";//
    public const string E_UpGradeTower = "E_UpGradeTower";//
    public const string E_SellTower = "E_SellTower";//E_SellTower

     public const string E_headRight="E_headRight";
     public const string E_headError="E_headError";

    public enum GameSpeed
    {
        One,Two
    }

    public enum MonsterType
    {
        Monster0,
        Monster1,
        Monster2,
        Monster3,
        Monster4,
        Monster5

    }




}
