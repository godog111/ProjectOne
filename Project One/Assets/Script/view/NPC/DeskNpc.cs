using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class DeskNpc : NPCBase
{
    [Header("Animation Settings")]
    [SerializeField] private Animator doorAnimator;
    //[SerializeField] private string openAnimationName = "Open";
    //[SerializeField] private string closeAnimationName = "Close";

    [Header("Interaction Settings")]
    [SerializeField] private string promptMessage = "按F开门";
    [SerializeField] private float interactionDistance = 2f;


    protected void Start()
    {
        currentState = NPCState.Idle;
    }

    protected override void Update()
    {
        //base.Update();
        if (playerTransform != null)
        {

            CheckPlayerDistance();
        }
        float distance = Vector2.Distance(transform.position, playerTransform.position);
        isPlayerInRange = distance <= interactionRange;
        //Debug.Log(isPlayerInRange);
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.F))
        {

            InteractingBehavior();
            
          
        }
    }
    public override void InteractingBehavior()
    {
        
       // base.Interact();
        
       
       OpenUI();
        
    }
    private void OpenUI()
    {
        if (ConditionManager.Instance.CheckCondition(ConditionType.CustomFlag, "dialogue_1"))
        {
            Debug.Log("dakaiUI");
            UIModel.Instance.OpenPanel("DeskUI");
        }
        else
        {
            Debug.Log("剧情未推动，暂时无法完成");
        }
        
    }

    private void FinishOpening()
    {

    }
    public string GetPromptMessage()
    {
        return promptMessage;
    }

    public float GetInteractionDistance()
    {
        return interactionDistance;
    }

    

}

