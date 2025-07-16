using UnityEngine;
using System;

/// <summary>
/// 对话系统核心管理器（场景单例）
/// 负责对话流程控制、选项处理和事件触发
/// 需要挂载在场景中的空物体上
/// </summary>
public class DialogueManager : MonoBehaviour 
{
    // 单例实例（通过属性访问器实现线程安全）
    public static DialogueManager Instance { get; private set; }
    
    [Header("UI预设引用")]
    [Tooltip("拖入包含DialogueUI组件的UI预设体")]
    [SerializeField] private GameObject dialogueUIPrefab;

    // 当前对话数据引用
    private DialogueData _currentDialogue;
    private Canvas canvas;
    private Transform canvasTransform; // 存储 Canvas 的 Transform
    
    // 对话结束回调事件
    public event Action OnDialogueEnd;
    
    [Header("NPC表演控制器")]
    public NPCPerformanceController performanceController;

    // 私有字段
    private DialogueUI _dialogueUI; // 当前UI实例的引用
    
    /// <summary>
    /// 初始化单例实例
    /// </summary>
    private void Awake() 
    {
        canvas = GameObject.FindGameObjectWithTag("MainCanvas").GetComponent<Canvas>();
        canvasTransform = canvas.transform;
        
        //canvasTransform = GetComponentInParent<Canvas>().transform;
        // 单例模式实现
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    private void Start()
    {
         
    }

    /// <summary>
    /// 开始新对话
    /// </summary>
    /// <param name="dialogue">对话数据</param>
    /// <param name="onEnd">对话结束回调</param>
    public void StartDialogue(DialogueData dialogue, Action onEnd = null)
    {
        // 检查并实例化UI
        if (_dialogueUI == null)
        {
            UIModel.Instance.OpenPanel("Dialogue");
            //Debug.Log(canvasTransform.name);
            Transform targetNode = canvasTransform.Find("Dialogue(Clone)");
            Debug.Log(targetNode.name);            
            _dialogueUI = targetNode.GetComponent<DialogueUI>();

            // 防御性编程：检查组件是否存在
            if (_dialogueUI == null)
            {
                Debug.LogError("UI预设缺少DialogueUI组件!");
                return;
            }

            // 确保UI初始状态
            _dialogueUI.gameObject.SetActive(false);
        }

        // 设置当前对话数据
        _currentDialogue = dialogue;
        OnDialogueEnd = onEnd;

        // 调用UI显示方法
        _dialogueUI.ShowDialogue(dialogue);
    }

    /// <summary>
    /// 处理选项选择
    /// </summary>
    /// <param name="option">被选中的选项数据</param>
    public void SelectOption(DialogueOption option) 
    {
        // 先隐藏选项UI
        _dialogueUI.HideOptions();
        
        // 根据选项类型处理不同逻辑
        switch(option.res)
        {
            case "openGame":
                // 场景跳转逻辑
                Game.Instance.LoadScene(3);
                break;
                
            case "nextDiaglue":
                // 继续下一段对话
                if(option.nextDialogueId > 0)
                {
                    NextDialogue(option.nextDialogueId);
                }
                break;
        }
    }

    /// <summary>
    /// 跳转到指定ID的对话
    /// </summary>
    /// <param name="nextDialogueId">下一条对话ID</param>
    public void NextDialogue(int nextDialogueId) 
    {
        // 检查是否为有效对话ID
        if (nextDialogueId != 0) 
        {
            // 从数据加载器获取下一条对话
            var next = DialogueLoader.Instance.GetDialogueData(nextDialogueId);
            
            // 重新开始新对话（保持原有回调）
            StartDialogue(next, OnDialogueEnd);
            
            // 处理NPC表演指令
            if (_currentDialogue.isDirtor != "per0")
            {
               // performanceController.RequestPerformanceWithPriority(_currentDialogue.isDirtor);
            }
        }
    }
    
    /// <summary>
    /// 直接通过对话ID开始对话（简化版）
    /// </summary>
    /// <param name="dialogueId">对话ID</param>
    /// <param name="onEnd">结束回调</param>
    public void StartDialogueById(int dialogueId, Action onEnd = null)
    {
        DialogueData dialogue = DialogueLoader.Instance.GetDialogueData(dialogueId);
        if (dialogue != null)
        {
            StartDialogue(dialogue, onEnd);
        }
        else
        {
            Debug.LogError($"未找到ID为{dialogueId}的对话数据！");
        }
    }

    /// <summary>
    /// 发送条件更改
    /// </summary>
    /// 
    public void SetCondition(string conditionId)
    {
        string cdn = conditionId.ToString();
        ConditionManager.Instance.UpdateCondition(ConditionType.CustomFlag,cdn,true);
    }

    /// <summary>
    /// 结束当前对话
    /// </summary>
    public void EndDialogue()
    {
        // 隐藏UI
        if (_dialogueUI != null)
        {
            _dialogueUI.HideDialogue();
        }

        // 触发结束事件
        OnDialogueEnd?.Invoke();

        // 清理引用
        _currentDialogue = null;
    }

    /// <summary>
    /// 每帧检测空格键继续对话
    /// </summary>
    private void Update()
    {
        // 仅在存在当前对话时检测
        if (_currentDialogue != null && Input.GetKeyDown(KeyCode.E))
        {
            // 如果没有后续对话则结束
            if (_currentDialogue.nextDialogueId == 0)
            {
                SetCondition(_currentDialogue.condition);
                EndDialogue();
            }
            else
            {
                // 否则继续下一条
                NextDialogue(_currentDialogue.nextDialogueId);
            }
        }
    }
}