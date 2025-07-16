using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatNewGameComand : Controller
{
    public GameSaveData CurrentSaveData { get; private set; }
    public override void Execute(object data)
    {
      SaveGameModel sgm =  GetModel<SaveGameModel>();
      Debug.Log("创建新游戏数据");
      CurrentSaveData= SaveGameModel.CreateNewSave(0);
      sgm.SaveCurrentGame(CurrentSaveData,0);
    }


}
