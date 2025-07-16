using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine;
using System;
using static UIModel;
/// <summary>
/// NPC对话组件
/// 挂载在NPC游戏对象上，处理对话触发逻辑
/// </summary>
public class NPCDialogue : MonoBehaviour {
    [Header("对话配置")]
    public int npcId;                      // NPC配置ID
    public float interactionRadius = 10f;   // 交互半径
    public LayerMask Player;          // 玩家层级
    
    private DialogueManager dM; //订阅管理器
    public Vector3 _fPosition;//F的坐标
    private NPCData _npcData;              // NPC配置数据
    private List<DialogueData> _dialogues; // 关联的对话列表
    private bool _isInRange = false;       // 玩家是否在交互范围内
    private bool _isDialogue = false;//是否在对话中
    public Camera mainCamera;//场景主相机
    private void Start() {
        // 初始化时加载NPC数据
        _npcData = DialogueLoader.Instance.GetNPCData(npcId);
        _dialogues = DialogueLoader.Instance.GetDialoguesForNPC(npcId);
        

        mainCamera = Camera.main;//找到主相机

        // 设置交互范围可视化(调试用)
        var collider = GetComponent<CircleCollider2D>();
        if (collider != null) {
            collider.radius = interactionRadius;
        }
    }

    private void Update() {
        // 检测玩家是否在交互范围内
        _isInRange = Physics2D.OverlapCircle(transform.position, interactionRadius,Player);
        // Debug.Log(_isInRange); 
        if (_isInRange && !_isDialogue)
        {
            _fPosition = mainCamera.WorldToScreenPoint(this.transform.position);
            _fPosition.y = _fPosition.y + 5;
        }
        // 如果在范围内且按下交互键，触发对话,判断对话开始，玩具正在对话中
        if (_isInRange && Input.GetKeyDown(KeyCode.F)) {
            Debug.Log("开始对话"); 
           if (!_isDialogue)
            {
                TriggerDialogue();
                _isDialogue = true;
                dM = FindObjectOfType<DialogueManager>();
                if (dM == null) { Debug.Log("订阅不存在"); }
                else dM.OnDialogueEnd += EndDialogue;
            }
            else
            {
                Debug.Log("正在对话中");
            }
        }

     }
        
        
    

    /// <summary>
    /// 触发NPC对话
    /// </summary>
    public void TriggerDialogue() {

        Debug.Log(_dialogues.Count);
        if (_dialogues != null && _dialogues.Count > 0) {
            // 这里简单取第一个对话，实际可以根据条件选择对话
        BasePanel basePanel = UIModel.Instance.OpenPanel(UIConst.Dialogue);
        DialogueManager.Instance.StartDialogue(_dialogues[0]);
         
        } else {
            Debug.LogWarning($"NPC {npcId} 没有配置对话!");
        }
    }
    //结束对话 重置状态
     void EndDialogue(){
            _isDialogue= false;
    }
    void OnDestroy()
    {
    if (dM != null)
    {
        dM.OnDialogueEnd -= EndDialogue;
    }
    }


    // 绘制交互范围Gizmo(编辑器可视化)
    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}