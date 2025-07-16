using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 通用条件触发器
/// </summary>
public class ConditionTrigger : MonoBehaviour
{
    [System.Serializable]
    public class ConditionSet
    {
        public ConditionType type;
        public string conditionID;
    }

    [Header("触发设置")]
    [Tooltip("需要满足的条件集合")] 
    public ConditionSet[] requiredConditions;

    [Tooltip("条件检查逻辑")] 
    public ConditionCheckType checkType = ConditionCheckType.All;

    [Header("触发方式")]
    [Tooltip("自动检测条件变化")] 
    public bool autoCheck = true;
    
    [Tooltip("检测间隔（秒）")] 
    public float checkInterval = 0.5f;
    
    [Tooltip("需要玩家交互触发")] 
    public bool requireInteraction;

    [Header("触发后行为")]
    [Tooltip("触发事件")] 
    public UnityEvent onConditionsMet;

    [Tooltip("触发后禁用组件")] 
    public bool disableAfterTrigger = true;

    [Header("视觉反馈")]
    [Tooltip("条件未满足时显示提示")] 
    public GameObject hintIndicator;
    
    [Tooltip("条件满足时改变材质")] 
    public Material activatedMaterial;

    [Header("交互范围设置")]
    [Tooltip("启用交互范围检测")]
    public bool useInteractionRange = true;
    
    [Tooltip("交互范围半径（单位：米）")]
    public float interactionRange = 10f;
    
    [Tooltip("检测目标标签（默认为Player）")]
    public string targetTag = "Player";
     private Transform playerTransform; 
    private Material originalMaterial;
    private Renderer targetRenderer;
    private float checkTimer;
    private bool isTriggered;

    public enum ConditionCheckType
    {
        All,    // 需要满足所有条件
        Any     // 满足任一条件即可
    }

    private void Start()
    {
        
        InitializeVisualFeedback();
        UpdateVisualFeedback();
        // 查找玩家对象（优化性能，避免每帧GameObject.Find）
        GameObject player = GameObject.FindGameObjectWithTag(targetTag);
        if (player != null) playerTransform = player.transform;
        foreach (var condition in requiredConditions)
        {
            ConditionManager.Instance.SubscribeToCondition(
                condition.type, 
                condition.conditionID, 
                OnConditionChanged);
        }
    }

    private void OnDestroy()
    {
        // 取消订阅条件变化
        foreach (var condition in requiredConditions)
        {
            ConditionManager.Instance.UnsubscribeFromCondition(
                condition.type, 
                condition.conditionID, 
                OnConditionChanged);
        }
    }

    private void InitializeVisualFeedback()
    {
        if (activatedMaterial != null)
        {
            targetRenderer = GetComponent<Renderer>();
            if (targetRenderer != null)
            {
                originalMaterial = targetRenderer.material;
            }
        }
    }

    private void Update()
    {

       
        if (!isTriggered && autoCheck && !requireInteraction)
        {
            checkTimer += Time.deltaTime;
            if (checkTimer >= checkInterval)
            {
                CheckConditions();
                checkTimer = 0;
            }
        }
         // 范围实时检测（可选）
        if (requireInteraction && useInteractionRange && playerTransform != null)
        {
            float distance = Vector3.Distance(transform.position, playerTransform.position);
            //
            if (distance <= interactionRange && Input.GetKeyDown(KeyCode.F))
            {

                
                CheckConditions();
            }
        }
    }
    // 条件变化处理函数
    private void OnConditionChanged(object sender, ConditionChangedEventArgs args)
    {
        if (!isTriggered && autoCheck)
        {
            CheckConditions();
        }
    }

    public void CheckConditions()
    {
        
        //Debug.Log(isTriggered);
        if (isTriggered) return;

        bool conditionsMet = checkType == ConditionCheckType.All ?
            CheckAllConditions() :
            CheckAnyCondition();

        if (conditionsMet)
        {

            TriggerActions();
        }

        UpdateVisualFeedback();
    }

    private bool CheckAllConditions()
    {
        foreach (var condition in requiredConditions)
        {
            if (!ConditionManager.Instance.CheckCondition(condition.type, condition.conditionID))
            {
                return false;
            }
        }
        return true;
    }

    private bool CheckAnyCondition()
    {
        foreach (var condition in requiredConditions)
        {
             return true;
           /* if (ConditionManager.Instance.CheckCondition(condition.type, condition.conditionID))
             {
                 return true;
             }*/
        }
        return false;
    }

    private void TriggerActions()
    {
        isTriggered = true;
        onConditionsMet.Invoke();

        if (disableAfterTrigger)
        {
            enabled = false;
        }
    }

    private void UpdateVisualFeedback()
    {
        bool conditionsMet = checkType == ConditionCheckType.All ? 
            CheckAllConditions() : 
            CheckAnyCondition();

        // 更新提示显示
        if (hintIndicator != null)
        {
            hintIndicator.SetActive(!conditionsMet);
        }

        // 更新材质
        if (targetRenderer != null && activatedMaterial != null)
        {
            targetRenderer.material = conditionsMet ? activatedMaterial : originalMaterial;
        }
    }

    // 玩家交互触发
    private void OnTriggerStay(Collider other)
    {
        if (requireInteraction && other.CompareTag("Player"))
        {
            if (Input.GetButtonDown("F"))
            {
                Debug.Log("碰撞触发交互"); 
                CheckConditions();
            }
        }
    }

    // 手动重置触发器
    public void ResetTrigger()
    {
        isTriggered = false;
        enabled = true;
        UpdateVisualFeedback();
    }
}