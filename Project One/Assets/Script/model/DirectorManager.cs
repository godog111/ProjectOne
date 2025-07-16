using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using static UIModel;

public class DirectorManager : Model
{
    public override string Name => throw new System.NotImplementedException();
    
    public static DirectorManager _instance;
    public enum DirectorMode{
        GamePlay,
        DialogueMonen

    }
    public DirectorMode gameMode;
    private PlayableDirector currentPlayableDirector;

   private void Awake()
   {
    gameMode = DirectorMode.GamePlay;
    Application.targetFrameRate =-1;//OPTIONAL 不限制游戏帧率
   }

   private void Update()
   {

    if(gameMode == DirectorMode.DialogueMonen)
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
        //按下空格进入下一个片段
        ResumeTimeLine();
        }


    }
    
   }

   public void PauseTimeLine(PlayableDirector _playableDirector)
   {
        currentPlayableDirector = _playableDirector;
        gameMode =DirectorMode.DialogueMonen;
        currentPlayableDirector.playableGraph.GetRootPlayable(0).SetSpeed(0d);

        //这里插入播放对应的功能，例如对话等等，之后可以用switch来判断执行
        //UIManager.instance.ToggleSpaceBar(true);


        
   }

   public void ResumeTimeLine()
   {
     gameMode =DirectorMode.GamePlay;
     currentPlayableDirector.playableGraph.GetRootPlayable(0).SetSpeed(1d);

    
     //执行继续游戏片段
     //UIManager.instance.ToggleSpaceBar(false);  这里不显示空格按钮功能
     //UImanager.instance.ToggleDialogueBox(true);  显示对话框

   }

}
