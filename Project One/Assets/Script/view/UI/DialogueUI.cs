using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 对话系统UI控制器
/// 负责所有对话UI的显示和交互
/// 需要挂载在对话UI预设体上
/// </summary>
public class DialogueUI : MonoBehaviour 
{
    [Header("主面板")]
    public GameObject dialoguePanel;
    
    [Header("文本组件")]
    public Text nameText;          // 说话者名字
    public Text dialogueText;      // 对话内容
    
    [Header("图像组件")]
    public Image portraitImage;    // 头像
    
    [Header("选项相关")]
    public GameObject optionsPanel;        // 选项容器面板
    public GameObject optionButtonPrefab;  // 选项按钮预制体
    
    // 选项布局偏移量（用于动态调整按钮位置）
    private int _optionYOffset = 0;

    /// <summary>
    /// 显示对话内容
    /// </summary>
    /// <param name="dialogue">对话数据</param>
    public void ShowDialogue(DialogueData dialogue)
    {
        transform.SetAsLastSibling();
        Debug.Log("加载对话" + dialogue.dialogueId);
        // 激活主面板
        gameObject.SetActive(true);
        
        // 设置说话者名称
        nameText.text = dialogue.npcId > 0 ? 
            DialogueLoader.Instance.GetNPCData(dialogue.npcId).npcName : "玩家";
        
        // 停止之前的打字效果（防止冲突）
        StopAllCoroutines();
        // 开始新的打字效果
        StartCoroutine(TypeText(dialogue.text));

        // 加载头像
        string portraitPath = string.IsNullOrEmpty(dialogue.portraitOverride) ? 
            DialogueLoader.Instance.GetNPCData(dialogue.npcId).portraitPath : 
            dialogue.portraitOverride;
        portraitImage.sprite = Resources.Load<Sprite>(portraitPath);

        // 如果有选项则创建
        if (dialogue.optionID > 0) 
        {
            CreateOptions(dialogue.dialogueId);
        }
    }

    /// <summary>
    /// 隐藏整个对话UI
    /// </summary>
    public void HideDialogue()
    {
        Debug.Log("对话结束");
        // 禁用主面板（保留实例）
        gameObject.SetActive(false);
        // 确保选项面板关闭
        optionsPanel.SetActive(false);
    }

    /// <summary>
    /// 仅隐藏选项面板
    /// </summary>
    public void HideOptions()
    {
        optionsPanel.SetActive(false);
    }

    /// <summary>
    /// 创建选项按钮
    /// </summary>
    /// <param name="optionId">选项组ID</param>
    private void CreateOptions(int optionId)
    {
        // 清除现有选项（防止重复）
        foreach (Transform child in optionsPanel.transform)
        {
            Destroy(child.gameObject);
        }

        // 从数据加载器获取选项列表
        var options = DialogueLoader.Instance.GetDialoguesForOption(optionId);
        if(options == null) 
        {
            Debug.LogWarning($"未找到ID为{optionId}的选项组");
            return;
        }

        // 动态生成每个选项按钮
        foreach (var option in options)
        {
            _optionYOffset++; // 增加布局偏移
            
            // 实例化按钮
            var button = Instantiate(optionButtonPrefab, optionsPanel.transform);
            
            // 设置按钮文本
            button.GetComponentInChildren<Text>().text = option.optionText;
            
            // 添加点击事件
            button.GetComponent<Button>().onClick.AddListener(() => 
            {
                Debug.Log($"选择了选项: {option.optionText}");
                DialogueManager.Instance.SelectOption(option);
            });
        }
        
        // 显示选项面板
        optionsPanel.SetActive(true);
    }

    /// <summary>
    /// 打字机效果协程
    /// </summary>
    /// <param name="text">要显示的完整文本</param>
    private IEnumerator TypeText(string text)
    {
        // 清空初始文本
        dialogueText.text = "";
        
        // 逐个字符追加
        foreach (char c in text)
        {
            dialogueText.text += c;
            // 每个字符间隔0.05秒（可调整）
            yield return new WaitForSeconds(0.05f);
        }
    }
}