using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class chairNPC : NPCBase
{
    [Header("Animation Settings")]
    [SerializeField] private Animator doorAnimator;
    //[SerializeField] private string openAnimationName = "Open";
    //[SerializeField] private string closeAnimationName = "Close";

    [Header("Interaction Settings")]
    [SerializeField] private string promptMessage = "按F开门";
    [SerializeField] private float interactionDistance = 2f;


    private Canvas canvas;
    private Transform canvasTransform; // 存储 Canvas 的 Transform

    protected void Start()
    {
        currentState = NPCState.Idle;
        canvas = GameObject.FindGameObjectWithTag("MainCanvas").GetComponent<Canvas>();
        canvasTransform = canvas.transform;
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

        Debug.Log("ryLevel");
        UIModel.Instance.OpenPanel("ryLevel");

        LevelDetailPanel targetNode = (LevelDetailPanel)UIModel.Instance.GetBasePanel("ryLevel");

        targetNode.ShowLevelDetails(1);
        

        
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

