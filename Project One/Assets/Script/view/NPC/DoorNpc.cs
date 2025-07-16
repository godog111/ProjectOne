using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class DoorNpc : NPCBase
{
    [Header("Animation Settings")]
    [SerializeField] private Animator doorAnimator;
    //[SerializeField] private string openAnimationName = "Open";
    //[SerializeField] private string closeAnimationName = "Close";

    [Header("Interaction Settings")]
    [SerializeField] private string promptMessage = "按F开门";
    [SerializeField] private float interactionDistance = 2f;

    private bool isOpen = false;
    private bool isAnimating = false;

    protected override void Update()
    {
        //base.Update();
        if (playerTransform != null)
        {

            CheckPlayerDistance();
        }
        float distance = Vector2.Distance(transform.position, playerTransform.position);
        isPlayerInRange = distance <= interactionRange;
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.F))
        {
            //   
            if (!isPlayerInRange)
            {
                InteractingBehavior();
            }
            else
            {
                // Debug.Log(isPlayerInRange);
            }
        }
    }
    public override void InteractingBehavior()
    {

        if (isAnimating) return;

        if (!isOpen)
        {
            OpenDoor();
        }
    }
    private void OpenDoor()
    {
        Debug.Log("开门交互");
        isAnimating = true;
        doorAnimator.SetTrigger("Close");

        // 动画播放完成后锁定状态
        Invoke("FinishOpening", doorAnimator.GetCurrentAnimatorStateInfo(0).length);
    }

    private void FinishOpening()
    {
        isOpen = true;
        isAnimating = false;

        // 禁用碰撞器使门保持开启状态
        // GetComponent<Collider>().enabled = false;
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

