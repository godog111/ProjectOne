using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using Unity.VisualScripting;

public class ImageGallery : BasePanel
{
    [Header("UI Elements")]
    [Tooltip("包含所有图片的父对象")]
    public GameObject galleryPanel; // 整个图片展示面板
    
    [Tooltip("所有可点击的图片数组")]
    public Image[] galleryImages; // 图片数组
    [Tooltip("对话组对应ID")]
    public int[] dialogueId;//对话组ID

    
    [Header("Animation Settings")]
    [Tooltip("缩放动画持续时间")]
    public float animationDuration = 0.3f;
    
    [Tooltip("图片放大后的尺寸")]
    public Vector2 enlargedSize = new Vector2(600, 600);

    [Tooltip("图片放大后的目标位置（屏幕中心为(0,0)）")]
    public Vector2 enlargedPosition = Vector2.zero;

    public Button close;
    
    // 当前选中的图片索引
    private int currentSelectedIndex = -1;
    
    // 图片原始尺寸存储
    private Vector2[] originalSizes;
    //图片原始位置
    private Vector2[] originalPositions;
    
    // 是否正在播放动画
    private bool isAnimating = false;

    void Start()
    {
        // 初始化原始尺寸数组
        originalSizes = new Vector2[galleryImages.Length];
        originalPositions = new Vector2[galleryImages.Length];
        close.onClick.AddListener(() => OnCloseClicked());

        // 为每张图片添加点击事件并保存原始尺寸
        for (int i = 0; i < galleryImages.Length; i++)
        {
            int index = i; // 闭包需要局部变量

            // 获取或添加Button组件
            Button btn = galleryImages[i].GetComponent<Button>();
            if (btn == null)
            {
                btn = galleryImages[i].gameObject.AddComponent<Button>();
            }

            // 移除所有现有监听器以避免重复
            btn.onClick.RemoveAllListeners();

            // 添加点击事件
            btn.onClick.AddListener(() => OnImageClicked(index));

            // 保存原始尺寸
            originalSizes[i] = galleryImages[i].rectTransform.sizeDelta;
            originalPositions[i] = galleryImages[i].rectTransform.anchoredPosition;
        }
        
        
        if (galleryPanel != null)
        {
            galleryPanel.SetActive(true);
        }
    }


    private void OnCloseClicked()
    {
        UIModel.Instance.ClosePanel("DeskUI");
    }

    // 图片点击事件处理
    void OnImageClicked(int index)
    {
        // 如果正在播放动画，忽略点击
        if (isAnimating) return;

        // 如果当前没有选中图片，或者点击的是不同图片
        if (currentSelectedIndex == -1 || currentSelectedIndex != index)
        {
            // 显示画廊面板
            if (galleryPanel != null && !galleryPanel.activeSelf)
            {
                galleryPanel.SetActive(true);
            }

            // 放大选中的图片
            StartCoroutine(ScaleAndMoveImage(index, enlargedSize, enlargedPosition));
            currentSelectedIndex = index;
            if (dialogueId[index] != 0)
            {
                Debug.Log(dialogueId[index]);
                DialogueManager.Instance.StartDialogueById(dialogueId[index]);
            }
        }
        else // 点击的是已放大的图片
        {
            // 缩小图片并关闭面板
            StartCoroutine(ScaleAndMoveImage(index, originalSizes[index], originalPositions[index], true));
            DialogueManager.Instance.EndDialogue();
        }

    }

    // 图片缩放和移动协程
    System.Collections.IEnumerator ScaleAndMoveImage(int index, Vector2 targetSize, Vector2 targetPosition, bool closeAfter = false)
    {
        isAnimating = true;
        
        RectTransform rt = galleryImages[index].rectTransform;
        Vector2 startSize = rt.sizeDelta;
        Vector2 startPosition = rt.anchoredPosition;
        float elapsedTime = 0f;
        
        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / animationDuration);
            
            // 使用平滑的插值同时缩放和移动
            rt.sizeDelta = Vector2.Lerp(startSize, targetSize, SmoothStep(t));
            rt.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, SmoothStep(t));
            
            yield return null;
        }
        
        // 确保最终尺寸和位置准确
        rt.sizeDelta = targetSize;
        rt.anchoredPosition = targetPosition;
        isAnimating = false;
        
        // 如果需要关闭面板
        if (closeAfter)
        {
            currentSelectedIndex = -1;
            if (galleryPanel != null)
            {
               // galleryPanel.SetActive(false);
            }
        }
    }
    // 平滑过渡函数
    float SmoothStep(float t)
    {
        return t * t * (3f - 2f * t);
    }

    // 可选：添加键盘ESC键关闭功能
    void Update()
    {
        // 如果按下了空格键，直接返回不处理
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("跳出监听");
            return;
        }

    }
}