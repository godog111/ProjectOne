using UnityEngine;
using System.Collections;



/// <summary>
/// NPC基类，所有NPC的通用功能
/// </summary>
public abstract class NPCBase : MonoBehaviour
{
    [Header("基础设置")]
    [SerializeField] protected string npcName = "NPC"; // NPC名称
    [SerializeField] protected float interactionRange = 20f; // 交互范围
    [SerializeField] protected Transform playerTransform; // 玩家参考
    
    [Header("对话设置")]
    [SerializeField] protected string[] dialogueLines; // 对话内容
    [SerializeField] protected float textSpeed = 0.05f; // 文字显示速度
    
    protected NPCState currentState = NPCState.Idle; // 当前状态
    protected bool isPlayerInRange = false; // 玩家是否在交互范围内
    
    // 属性
    public string NPCName => npcName;
    public NPCState CurrentState => currentState;
    private Animator anim;
    protected virtual void Awake()
    {
        anim = GetComponent<Animator>();
        // 如果没有指定玩家，尝试自动查找
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerTransform = player.transform;
        }
       // OnDrawGizmosSelected();
    }
    
    protected virtual void Update()
    {
        if (playerTransform != null)
        {
            
            CheckPlayerDistance();
        }
        
        StateUpdate();
    }
    
    /// <summary>
    /// 检查玩家距离
    /// </summary>
    protected virtual void CheckPlayerDistance()
    {
        float distance = Vector2.Distance(transform.position, playerTransform.position);
        isPlayerInRange = distance <= interactionRange;
        
        // 如果玩家进入范围且NPC空闲，可以显示交互提示
        if (isPlayerInRange && currentState == NPCState.Idle)
        {
            
            ShowInteractionPrompt();
        }
        else
        {
            HideInteractionPrompt();
        }
    }
    
    /// <summary>
    /// 显示交互提示
    /// </summary>
    protected virtual void ShowInteractionPrompt()
    {
        // 实现显示交互提示UI的逻辑
        // 例如显示"按E交互"的提示
    }
    
    /// <summary>
    /// 隐藏交互提示
    /// </summary>
    protected virtual void HideInteractionPrompt()
    {
        // 实现隐藏交互提示UI的逻辑
    }
    
    /// <summary>
    /// 状态更新
    /// </summary>
    protected virtual void StateUpdate()
    {
        switch (currentState)
        {
            case NPCState.Idle:
                anim.SetBool("isWalking",false);
                IdleBehavior();
                break;
            case NPCState.Talking:
                TalkingBehavior();
                break;
            case NPCState.Moving:
                anim.SetBool("isWalking",true);
                MovingBehavior();
                break;
            case NPCState.Interacting:
                InteractingBehavior();
                break;
        }
    }
    
    // 各种状态的行为方法
    protected virtual void IdleBehavior() { }
    protected virtual void TalkingBehavior() { }
    protected virtual void MovingBehavior() { }
    public virtual void InteractingBehavior() { }
    
    /// <summary>
    /// 开始对话
    /// </summary>
    public virtual void StartDialogue()
    {
        if (currentState != NPCState.Idle) return;
        
        currentState = NPCState.Talking;
        StartCoroutine(TypeDialogue());
    }
    
    /// <summary>
    /// 打字机效果显示对话
    /// </summary>
    protected virtual IEnumerator TypeDialogue()
    {
        // 实现对话显示逻辑
        foreach (string line in dialogueLines)
        {
            // 显示每行对话
            yield return new WaitForSeconds(textSpeed * line.Length);
        }
        
        // 对话结束
        EndDialogue();
    }
    
    /// <summary>
    /// 结束对话
    /// </summary>
    public virtual void EndDialogue()
    {
        currentState = NPCState.Idle;
    }
    
    /// <summary>
    /// 与NPC交互
    /// </summary>
    public virtual void Interact()
    {
        if (!isPlayerInRange) return;
        
        switch (currentState)
        {
            case NPCState.Idle:
                StartDialogue();
                break;
            case NPCState.Talking:
                // 可以在这里实现跳过当前对话或显示下一句
                break;
        }
    }
    
    // 可视化交互范围
    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}