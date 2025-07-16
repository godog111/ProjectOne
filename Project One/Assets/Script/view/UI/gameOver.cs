using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class gameOver : BasePanel
{

    public Button reStart;//重新开始

    public Button backMainMenu;//回到主界面
    public override string Name 
    {
        get {return Consts.V_gameOver;}
    } 

    public void ReStart(){
        UIModel.Instance.ClosePanel("gameOver");
        Game.Instance.LoadScene(3);
        //后续需要重置数据
    }

    public void BackMainMenu(){
        UIModel.Instance.ClosePanel("gameOver");
        Game.Instance.LoadScene(1);
    }



    public override void HandleEvent(string eventName, object data)
    {
       
    }
}
