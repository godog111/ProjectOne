using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


    public enum StateType
    {
        Idle,
        End,
        Attack,
        Die,
        Success,
        MOVE,

    }
    public interface IState
    {
        void OnEnter(object data = null);
        void OnExit();
        void OnClick();
        void OnFixUpdate();

        void OnCheck();
        void OnUpdate();
        
    }
        [Serializable]
        public class BlackBoard
        {
                //此处存储共享数据，或者向外部展示
        }
    public class FSM{
        
        public IState curState;
        public Dictionary<StateType,IState> states;
        public BlackBoard blackBoard;
        
        public  FSM(BlackBoard blackBoard)
        {

            this.states = new Dictionary<StateType, IState>();
            this.blackBoard = blackBoard;
        }

        public void AddState(StateType stateType,IState state)
        {
            if(states.ContainsKey(stateType))
            {
                Debug.Log("[AddState]>>>>>>>>>map has contain key:"+stateType);
                return;
            }
            states.Add(stateType,state);
        }

        public void SwitchState(StateType stateType,object data = null)
        {
            if(!states.ContainsKey(stateType))
            {
                Debug.Log("[SwitchState]>>>>>>>>>map has contain key:"+stateType);
                return ;
            }   
            if(curState !=null)
            {
                curState.OnExit();
            }
            curState = states[stateType];
            curState.OnEnter(data);
        }

        public void OnUpdate()
        {
            curState.OnUpdate();
        }

        public void OnFixUpdate()
        {
            //curState.OnFixUpdate();
        }

        public void OnCheck()
        {
            curState.OnCheck();
        }

    }