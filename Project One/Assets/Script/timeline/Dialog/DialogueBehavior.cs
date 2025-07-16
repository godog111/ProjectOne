using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using static UIModel;

[System.Serializable]
public class DialogueBehavior : PlayableBehaviour
{
  private PlayableDirector playableDirector;

  public string characterName;
  [TextArea(8,1)]public string dialogueLine;
  public int dialougeId;
  public int dialogueSize;

  private bool isClipPlayed;//是否这个对话片段已经结束
  public bool requirePause;//用户设置：这个对话完成之后，是否需要按建继续
  private bool pauseScheduled;
  private List<DialogueData> textJson;
    public override void OnPlayableCreate(Playable playable)
    {
        //该方法在执行的时候进行调用
        playableDirector = playable.GetGraph().GetResolver() as PlayableDirector;
    }

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        //类似update方法，在资源播放的时候没一帧进行检查调用
        if(isClipPlayed == false && info.weight>0)
        {
            UIModel.Instance.OpenPanel(UIConst.Dialogue);
            textJson= StaticDataManager.GetDataList<DialogueData>("Dialogue_Content");
            DialogueManager.Instance.StartDialogue(textJson[dialougeId]);
            //这里后面加入对话管理系统的文字显示系统,类似的可以替换到后面其他类型的轨道上
            
            //DialogueManager.instance.StartDialogue(_dialogues[0]);
            if(requirePause)
                pauseScheduled = true;
            isClipPlayed = true;
        }
    }

    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        //接口 在进入暂停时自动执行以下逻辑
        isClipPlayed = false;
        Debug.Log("暂停成功");
       if(pauseScheduled)
       {
        pauseScheduled = false;
        //暂停timeline播放
        DirectorManager._instance.PauseTimeLine(playableDirector);

       }
       else
       {
        //关闭对话框功能，待实现
        UIModel.Instance.ClosePanel(UIConst.Dialogue);
       }
    }

}
