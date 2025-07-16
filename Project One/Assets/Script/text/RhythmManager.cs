using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class RhythmManager : View, INotificationReceiver
{
    // 单例模式
    public static RhythmManager Instance { get; private set; }

    public override string Name
    {
        get { return Consts.V_RhythmManager; }
    }


    // 节拍事件委托
    public delegate void BeatEventHandler(BeatBehaviour beat);
    public event BeatEventHandler OnBeat;

    public PlayableDirector director;//获取到timeline播放器
    public TimelineAsset timeline;

    public List<double> recivedStart = new List<double>();//用于接收开始的clip时间
    private List<double> recivedEnd = new List<double>();//用于接收结束的clip时间
    public List<double> recivedClick = new List<double>();//用于接收结束的敲击节拍时间点
    public List<double> recivedBeatTime = new List<double>();//用于接收音乐每一个小节的节拍间隔

    public List<bool[]> clipStates =new List<bool[]>();//用于记录每个clip的点击状态
    public List<bool> clipState = new List<bool>();//用于记录当前clip的点击状态
    // 是否正在排序的标志
    //private bool isSorting = false;
    float point = 0;//游戏积分
    int health = 5;//游戏生命
    float offset = 1f;//点击误差偏移量
    float myTime = 0;//当前时间
    int index = 0;//当前节拍计时数
    public event Action OnDataOver;//订阅数据完成事件
    public event Action<bool> _PauseGame;// bool参数：true=暂停, false=恢复游戏

    // 添加初始化完成事件
    public static event Action OnInstanceReady;
    private static bool isInitialized = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        // 标记初始化完成
        isInitialized = true;
        // 触发初始化完成事件
        OnInstanceReady?.Invoke();
    }

    /// <summary>
    /// 安全获取实例的方法
    /// </summary>
    public static RhythmManager GetInstance()
    {
        if (!isInitialized)
        {
            Debug.LogWarning("RhythmManager is not yet initialized. Consider subscribing to OnInstanceReady event.");
            return null;
        }
        return Instance;
    }

    void Start()
    {
        if (director == null) director = GetComponent<PlayableDirector>();
        if (timeline == null) timeline = (TimelineAsset)director.playableAsset;

        //遍历所有轨道
        foreach (var track in timeline.GetOutputTracks())
        {
            if (track is BeatTrack)
            {
                foreach (var clip in track.GetClips())
                {
                    BeatClip beatClip = clip.asset as BeatClip;
                    if (beatClip != null)
                    {
                        double beatTime = (clip.end - clip.start) / beatClip.beatCountInMeasure;
                        double clikeTime = beatTime * beatClip.clickTriggerd;
                        double startTime = clip.start;
                        clikeTime += clip.start;
                        bool[] bools = beatClip.clipState;

                        //Debug.Log(beatTime + "节拍间隔");
                        recivedBeatTime.Add(beatTime);
                        recivedClick.Add(clikeTime);
                        recivedStart.Add(startTime);
                        clipStates.Add(bools);
                        // Debug.Log(clikeTime);

                    }
                    else
                    {
                        Debug.Log("Clip为空");
                    }
                }

            }
        }
        recivedClick.Sort();
        recivedStart.Sort();
        foreach (var item in recivedStart)
        {
            //Debug.Log(item);
        }
        OnDataOver?.Invoke();


    }
    void Update()
    {

        myTime = myTime + Time.deltaTime;
        if (myTime >= recivedClick[index])
        {
            if (index < recivedClick.Count - 1)
            {

                index++;
            }

        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ryClcik(myTime, recivedClick[index]);
        }
    }

    private void ryClcik(float clickTime, double promptsTime)
    {
        Debug.Log("点击时间" + clickTime + "提示时间" + promptsTime);
        if (clickTime > promptsTime - offset && clickTime < promptsTime + offset)
        {
            point++;//积分增加，后续换成积分增加方法
            SendEvent(Consts.E_headRight);
            Debug.Log(point);
        }
        else
        {
            health++;
            SendEvent(Consts.E_headError);
            Debug.Log(health);
        }
    }

    // 接收Timeline通知
    public void OnNotify(Playable origin, INotification notification, object context)
    {
        // 可以处理其他通知类型
    }

    // 节拍触发时的处理
    public void OnBeatTrigger(BeatBehaviour beat)
    {
        // 触发事件
        //Debug.Log("触发节拍");
        OnBeat?.Invoke(beat);

        // 根据节拍类型处理不同逻辑
        switch (beat.beatType)
        {
            case BeatType.Normal:
                // HandleNormalBeat(beat);
                break;
            case BeatType.Kick:
                // HandleKickBeat(beat);
                break;
            case BeatType.Snare:
                // HandleSnareBeat(beat);
                break;
            case BeatType.HiHat:
                //  HandleHiHatBeat(beat);
                break;
            default:
                //   HandleCustomBeat(beat);
                break;
        }
    }

    private void HandleNormalBeat(BeatBehaviour beat)
    {
        Debug.Log($"Normal beat at position {beat.beatPosition}, intensity: {beat.intensity}");
        // 在这里添加处理普通节拍的逻辑
    }

    private void HandleKickBeat(BeatBehaviour beat)
    {
        Debug.Log($"Kick beat at position {beat.beatPosition}");
        // 在这里添加处理鼓点节拍的逻辑
    }

    private void HandleSnareBeat(BeatBehaviour beat)
    {
        Debug.Log($"Snare beat at position {beat.beatPosition}");
        // 在这里添加处理军鼓节拍的逻辑
    }

    private void HandleHiHatBeat(BeatBehaviour beat)
    {
        Debug.Log($"HiHat beat at position {beat.beatPosition}");
        // 在这里添加处理踩镲节拍的逻辑
    }

    private void HandleCustomBeat(BeatBehaviour beat)
    {
        Debug.Log($"Custom beat ({beat.beatType}) at position {beat.beatPosition}");
        // 在这里添加处理自定义节拍的逻辑
    }

    /// <summary>
    /// 下面是监听clip事件的方法
    /// </summary>
    void OnEnable()
    {
        BeatBehaviour.OnClipInitialized += HandleClipInitialized;
        BeatBehaviour.OnClick += HandClick;
        this._PauseGame += PauseScrite;
    }



    void OnDisable()
    {
        BeatBehaviour.OnClipInitialized -= HandleClipInitialized;
        BeatBehaviour.OnClick -= HandClick;
    }

    void HandleClipInitialized(TimelineClip clip)
    {
        // Debug.Log($"收到Clip: Start={clip.start}, End={clip.end}");
        // recivedStart.Add(clip.start);


    }
    void HandClick(double clickTime)
    {
        recivedClick.Add(clickTime);
        Debug.Log(clickTime);
    }

    // 获取排序后的数据（供其他脚本调用）
    public List<double> GetSortedData()
    {
        return new List<double>(recivedClick);
    }

    public override void HandleEvent(string eventName, object data)
    {
        throw new System.NotImplementedException();
    }

    public void PauseGame(bool isPaused)
    {
        Debug.Log("发出订阅是否暂停游戏通知" + isPaused);
        Time.timeScale = isPaused ? 0f : 1f; // 根据isPaused参数设置时间缩放
        _PauseGame?.Invoke(!isPaused); // 注意：这里传递!isPaused，因为我们要控制脚本是否启用
    }

    private void PauseScrite(bool obj)
    {
        this.enabled = obj;
    }
}