using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class EnterSceneComand : Controller
{
    public override void Execute(object data)
    {
      
      SceneArgs e = data as SceneArgs;


    switch (e.id)
    {
      case 0://
        Debug.Log("进入场景" + e.id);
        break;

      case 1://strat
        Debug.Log("进入场景" + e.id);
        // RegisterView(GameObject.Find("UIStart").GetComponent<EnhancedMainMenu>());
        break;
      case 2://levelScene
        Debug.Log("进入场景" + e.id);
        //  RegisterView(GameObject.Find("Canvas").GetComponent<DialogueManager>());
        //  RegisterView(GameObject.Find("Start").GetComponent<LevelManager>());
        // RegisterView(GameObject.Find("UIStart").GetComponent<EnhancedMainMenu>());
        break;
      case 3://
        Debug.Log("进入场景" + e.id);
        //RegisterView(GameObject.Find("ryGameSpwan").GetComponent<ryGameSpwan>());
        RegisterView(GameObject.Find("Canvas").transform.Find("UICountDown").GetComponent<UICountDown>());
        RegisterView(GameObject.Find("head").GetComponent<AIHead>());
        RegisterView(GameObject.Find("RhythmGuide").GetComponent<RhythmGuideLine>());
        RegisterController(Consts.E_headError, typeof(HeadErrorComand));
        RegisterController(Consts.E_headRight, typeof(HeadRightComand));
        break;
      case 4://
        RegisterView(GameObject.Find("texttimeline").GetComponent<RhythmManager>());
        Debug.Log("进入场景" + e.id);
        break;
      case 5:
       
        Debug.Log("进入场景" + e.id);
        break;
      }
    }


}
