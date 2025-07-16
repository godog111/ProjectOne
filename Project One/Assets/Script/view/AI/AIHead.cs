using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[SerializeField]
public class HeadBlackboard:BlackBoard
{

}


public class AIHead : View
{
        private FSM fsm;
        public HeadBlackboard blackboard;
        private Animator animator;
        

    public override string Name 
    {
        get{return Consts.V_AIHead;}
    } 

    void Start()
    {
     // fsm = new FSM(blackboard);
      animator = GetComponent<Animator>();
      animator.Play("head");
      animator.SetTrigger("error1");

    }
    public override void RegisterEvents()
    {
        AttationEvents.Add(Consts.E_headRight);
        AttationEvents.Add(Consts.E_headError);
       
    }
    public override void HandleEvent(string eventName, object data)
    { 
        switch(eventName)
        {
            case Consts.E_headRight:
                Debug.Log("跳转错误1");
               // fsm.SwitchState(StateType.End,1);
                animator.SetTrigger("error1");
            break;

            case Consts.E_headError:
                Debug.Log("跳转正确1");
                //fsm.SwitchState(StateType.End,2);
                animator.SetTrigger("error1");
            break;
        }
    }
}

