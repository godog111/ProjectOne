using UnityEngine;
using UnityEngine.Playables;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Timeline;

/// <summary>
/// NPC演出系统核心控制器
/// 管理多套Timeline演出的播放、过渡和优先级
/// </summary>
[RequireComponent(typeof(PlayableDirector))]
public class NPCPerformanceController : MonoBehaviour
{
    #region 数据结构
    /// <summary>
    /// 单个演出配置
    /// </summary>
    [System.Serializable]
    public class PerformanceSet
    {
        [Tooltip("演出名称(唯一标识)")]
        public string performanceName;

        [Tooltip("关联的Timeline资源")]
        public TimelineAsset timeline;

        [Tooltip("过渡时间(秒)")]
        [Range(0.1f, 2f)]
        public float transitionDuration = 0.5f;

        [Tooltip("是否可被更高优先级中断")]
        public bool canBeInterrupted = true;
    }

    /// <summary>
    /// 演出优先级配置
    /// </summary>
    [System.Serializable]
    public class PerformancePriority
    {
        [Tooltip("演出名称(需与PerformanceSet匹配)")]
        public string performanceName;

        [Tooltip("优先级数值(越大优先级越高)")]
        public int priority = 0;
    }

    /// <summary>
    /// 演出状态枚举
    /// </summary>
    public enum PerformanceState
    {
        Idle,           // 闲置状态
        Preparing,      // 准备中(过渡进入)
        Performing,     // 演出中
        Transitioning   // 过渡中(退出)
    }
    #endregion

    #region 公开变量
    [Header("基础配置")]
    [Tooltip("所有可用演出配置")]
    public List<PerformanceSet> performanceSets = new List<PerformanceSet>();

    [Tooltip("演出优先级配置")]
    public List<PerformancePriority> performancePriorities = new List<PerformancePriority>();

    [Tooltip("调试模式")]
    public bool debugMode = false;
    #endregion

    #region 私有变量
    private PlayableDirector director;          // Timeline播放器
    private Animator animator;                  // 动画控制器
    private PerformanceState currentState;     // 当前状态
    private Queue<PerformanceSet> performanceQueue = new Queue<PerformanceSet>(); // 演出队列
    private PerformanceSet currentPerformance;  // 当前演出
    private Dictionary<string, int> priorityDict = new Dictionary<string, int>(); // 优先级字典
    #endregion

    #region 初始化
    private void Awake()
    {
        director = GetComponent<PlayableDirector>();
        animator = GetComponentInChildren<Animator>();
        currentState = PerformanceState.Idle;

        // 初始化优先级字典
        foreach (var priority in performancePriorities)
        {
            priorityDict[priority.performanceName] = priority.priority;
        }
    }

    private void Start()
    {
        // 初始状态设置
        if (animator != null)
        {
            animator.SetLayerWeight(1, 0); // 重置动画混合层
        }
       // RequestPerformance("per1");

    }
    #endregion

    #region 公开方法
    /// <summary>
    /// 请求播放指定演出
    /// </summary>
    /// <param name="performanceName">演出名称</param>
    public void RequestPerformance(string performanceName)
    {
        Debug.Log("演出开始");
        var performance = performanceSets.FirstOrDefault(p => p.performanceName == performanceName);
        if (performance != null)
        {
            EnqueuePerformance(performance);
        }
        else if (debugMode)
        {
            Debug.LogWarning($"未找到演出配置: {performanceName}");
        }
    }

    /// <summary>
    /// 带优先级检查的演出请求
    /// </summary>
    public void RequestPerformanceWithPriority(string performanceName)
    {
        var performance = performanceSets.FirstOrDefault(p => p.performanceName == performanceName);
        if (performance == null) return;

        // 检查优先级
        if (currentPerformance != null &&
           GetPriority(currentPerformance.performanceName) > GetPriority(performanceName) &&
           currentPerformance.canBeInterrupted)
        {
            if (debugMode) Debug.Log($"当前演出 {currentPerformance.performanceName} 优先级更高，拒绝中断");
            return;
        }

        // 中断当前演出
        if (currentState == PerformanceState.Performing ||
           currentState == PerformanceState.Preparing)
        {
            StopCurrentPerformance();
        }

        // 清空队列并添加新演出
        performanceQueue.Clear();
        EnqueuePerformance(performance);
    }

    /// <summary>
    /// 立即停止当前演出
    /// </summary>
    public void StopCurrentPerformance()
    {
        if (currentState != PerformanceState.Idle)
        {
            director.Stop();
            StopAllCoroutines();
            currentState = PerformanceState.Idle;

            // 重置动画状态
            if (animator != null)
            {
                animator.SetLayerWeight(1, 0);
                animator.Rebind();
            }
        }
    }
    #endregion

    #region 私有方法
    /// <summary>
    /// 将演出加入队列并处理
    /// </summary>
    private void EnqueuePerformance(PerformanceSet performance)
    {
        performanceQueue.Enqueue(performance);
        if (currentState == PerformanceState.Idle)
        {
            StartCoroutine(ProcessPerformanceQueue());
        }
    }

    /// <summary>
    /// 处理演出队列的协程
    /// </summary>
    private IEnumerator ProcessPerformanceQueue()
    {
        while (performanceQueue.Count > 0)
        {
            currentPerformance = performanceQueue.Dequeue();

            // 过渡进入
            currentState = PerformanceState.Preparing;
            yield return TransitionToPerformanceStart();

            // 开始演出
            currentState = PerformanceState.Performing;
            director.playableAsset = currentPerformance.timeline;
            director.Play();

            // 等待演出完成
            yield return new WaitWhile(() => director.state == PlayState.Playing);

            // 过渡退出
            currentState = PerformanceState.Transitioning;
            yield return TransitionToIdle();
        }

        currentState = PerformanceState.Idle;
    }

    /// <summary>
    /// 过渡到演出开始状态
    /// </summary>
    private IEnumerator TransitionToPerformanceStart()
    {
        float timer = 0;
        while (timer < currentPerformance.transitionDuration)
        {
            timer += Time.deltaTime;
            float weight = Mathf.Clamp01(timer / currentPerformance.transitionDuration);

            // 动画混合
            if (animator != null)
            {
                animator.SetLayerWeight(1, weight);
            }

            yield return null;
        }
    }

    /// <summary>
    /// 过渡回闲置状态
    /// </summary>
    private IEnumerator TransitionToIdle()
    {
        float timer = 0;
        float startWeight = animator != null ? animator.GetLayerWeight(1) : 0;

        while (timer < currentPerformance.transitionDuration)
        {
            timer += Time.deltaTime;
            float weight = Mathf.Lerp(startWeight, 0, timer / currentPerformance.transitionDuration);

            // 动画混合
            if (animator != null)
            {
               // animator.SetLayerWeight(1, weight);
            }

            yield return null;
        }
    }

    /// <summary>
    /// 获取演出优先级
    /// </summary>
    private int GetPriority(string performanceName)
    {
        return priorityDict.TryGetValue(performanceName, out int priority) ? priority : 0;
    }
    #endregion

    #region 调试工具
    private void OnGUI()
    {
        if (!debugMode) return;

        GUILayout.BeginArea(new Rect(10, 10, 300, 400));
        GUILayout.Label($"当前状态: {currentState}");

        if (currentPerformance != null)
        {
            GUILayout.Label($"当前演出: {currentPerformance.performanceName}");
        }

        GUILayout.Space(20);
        GUILayout.Label("手动触发演出:");

        foreach (var perf in performanceSets)
        {
            if (GUILayout.Button(perf.performanceName))
            {
                RequestPerformance(perf.performanceName);
            }
        }

        GUILayout.EndArea();
    }
    #endregion

    public void SetActive(bool active)
    {
        if (active)
        {
            // 激活时恢复状态
            if (animator != null) animator.enabled = true;
            this.enabled = true;
        }
        else
        {
            // 停用时释放资源
            StopCurrentPerformance();
            if (animator != null)
            {
                animator.SetLayerWeight(1, 0);
                animator.enabled = false;
            }
            this.enabled = false;
        }
    }
    
    public void InitializePerformances(List<PerformanceSet> performances)
    {
        performanceSets = performances;
    
        // 预加载资源
         if(gameObject.activeInHierarchy)
            {
                 StartCoroutine(PreloadPerformances());
            }
    }

    private IEnumerator PreloadPerformances()
    {
    foreach(var perf in performanceSets)
    {
        if(perf.timeline != null)
        {
            director.playableAsset = perf.timeline;
            director.Evaluate();
            yield return null;
        }
    }
    }
}