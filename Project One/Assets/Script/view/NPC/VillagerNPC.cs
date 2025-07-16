using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 村民NPC示例，实现特定功能
/// </summary>
public class VillagerNPC : NPCBase
{
    [Header("村民特有设置")]
    [SerializeField] private bool canGiveQuest = true; // 是否能给予任务
    [SerializeField] private bool isShopkeeper = false; // 是否是商店老板
    [SerializeField] private float wanderRadius = 2f; // 闲逛范围
    [SerializeField] private float wanderInterval = 2f; // 闲逛间隔
    
    private Vector2 originalPosition; // 初始位置
    private float wanderTimer = 0f;
    private bool isWandering = false;
    private Vector2 targetPosition;
    //private Animator anim;
    protected override void Awake()
    {
        base.Awake();
        //anim = GetComponent<Animator>();
        originalPosition = transform.position;
        
    }

    protected override void Update()
    {
        base.Update(); // 调用父类的Update方法
        
        // 只有在空闲状态时才计算闲逛计时器
        if (currentState == NPCState.Idle)
        {
            wanderTimer += Time.deltaTime;
        }
    }
    
      protected override void IdleBehavior()
    {
        // 当计时器达到间隔且不处于移动状态时，开始闲逛
        if (wanderTimer >= wanderInterval && !isWandering)
        {
            
            StartWandering();
             
            wanderTimer = 0f;
        }
    }

    protected override void MovingBehavior()
    {
        if (isWandering)
        {
            
            MoveToTarget();
        }
    }
    
    private void StartWandering()
    {
        
        //Debug.Log("开始闲逛");
        isWandering = true;
        
        currentState = NPCState.Moving;
        
        // 在闲逛范围内随机选择一个目标点
       // Debug.Log(originalPosition.x);
        
        targetPosition.x = originalPosition.x + (Random.Range(-wanderRadius,wanderRadius));
       // Debug.Log(targetPosition.x);
        targetPosition.y=originalPosition.y;
    }
    
    private void MoveToTarget()
    {

        //anim.SetBool("isWalking",true);
        //targetPosition = new Vector2(0,0);
        // 简单移动逻辑，实际项目中可能需要使用寻路系统
        transform.position = Vector2.MoveTowards(
            transform.position, 
            targetPosition, 
            2*Time.deltaTime);
            
        // 检查是否到达目标
        if (Vector2.Distance(transform.position, targetPosition) < 0.1f)
        {
            //Debug.Log("到达目的地");
            //anim.SetBool("isWalking",false);
            isWandering = false;
            currentState = NPCState.Idle;
        }
    }
    
    // 覆盖对话开始方法，添加村民特有逻辑
    public override void StartDialogue()
    {
        base.StartDialogue();
        
        // 村民特有的对话前行为
        if (isWandering)
        {
            isWandering = false;
            currentState = NPCState.Talking;
        }
    }
    
    // 覆盖交互方法，添加村民特有功能
    public override void Interact()
    {
        base.Interact();
        
        if (isPlayerInRange && currentState == NPCState.Idle)
        {
            if (isShopkeeper)
            {
                OpenShop();
            }
            else if (canGiveQuest)
            {
                OfferQuest();
            }
        }
    }
    
    private void OpenShop()
    {
        currentState = NPCState.Interacting;
        // 打开商店UI的逻辑
        Debug.Log($"{npcName}的商店已打开");
    }
    
    private void OfferQuest()
    {
        // 提供任务的逻辑
        Debug.Log($"{npcName}有一个任务要给你");
    }
    
    // 可以添加更多村民特有的方法...
}
