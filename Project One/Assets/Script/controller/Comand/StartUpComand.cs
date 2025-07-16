using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartUpComand : Controller
{
    public override void Execute(object data)
    {
        Debug.Log("开始游戏命令");
        //注册模型
        RegisterModel(new GameModel());
        RegisterModel(new SaveGameModel());
        //RegisterModel();

        //注册控制器

        RegisterController(Consts.E_EnterScene,typeof(EnterSceneComand));
        RegisterController(Consts.E_CreatNewGame,typeof(CreatNewGameComand));
        RegisterController(Consts.E_CountDownComplete, typeof(CountDownCompleteCommand));

        GameModel gModel =GetModel<GameModel>();
        gModel.Initialize();

        Game.Instance.LoadScene(1);
        
    }

    
}
