using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class RhythmGuideLine : View
{
    [Header("节奏设置")]
    public float bpm = 90;
    [Header("视觉效果")]

    public AudioSource audioSource;

    private Animator animator;

    private float beatInterval;
    private float nextBeatTime;
    private int i = 0;
    private int j = 1;
    [SerializeField] private ryGameSpwan bInstance;

    [SerializeField] GameObject right;
    [SerializeField] GameObject error;

    public AudioClip soundRgiht;
    public AudioClip soundError;
    private AudioSource audiosource;
    public List<double> recivedStart = new List<double>();//用于接收开始的clip时间
    private List<double> recivedClick = new List<double>();
    private List<double> recivedBeatTime = new List<double>();//用于接收音乐每一个小节的节拍间隔
    public List<bool[]> clipStates =new List<bool[]>();//用于记录每个clip的点击状态
    // 初始化时记录基准时间
    private double _audioStartDspTime;
    private double _audioNowTime;//当前音频时间
    private double _beatTime;//当前节拍时间
    private double pauseStartTime;//暂停时间
    
    public override string Name
    {
        get { return Consts.V_RhythmGuideLine; }
    }
    void Awake()
    {
        if (RhythmManager.Instance != null)
        {
            InitializeWithManager();
        }
        // 如果还没初始化，订阅初始化完成事件
        else
        {
            RhythmManager.OnInstanceReady += OnRhythmManagerReady;
        }
    }
    private void InitializeWithManager()
    {
        // 在这里放置需要访问RhythmManager的代码
        // 例如：订阅事件等
        RhythmManager.Instance.OnDataOver += HandleDataOver;
        RhythmManager.Instance._PauseGame += PauseScrite;
    }
    private void OnRhythmManagerReady()
    {
        // 取消订阅事件
        RhythmManager.OnInstanceReady -= OnRhythmManagerReady;
        // 执行需要RhythmManager的初始化逻辑
        InitializeWithManager();
    }
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        // 初始化LineRenderer       
        nextBeatTime = (float)AudioSettings.dspTime + beatInterval;

        _audioStartDspTime = AudioSettings.dspTime;
        //audioSource.Play();


        //Getline();
        //right.SetActive(false);
        //error.SetActive(false);

        audiosource = GetComponent<AudioSource>();
        // this.enabled = false;
    }

    private void HandleDataOver()
    {
        Debug.Log("数据完成");
        recivedClick.Clear();
        recivedClick = RhythmManager.Instance.recivedClick;
        recivedBeatTime.Clear();
        recivedBeatTime = RhythmManager.Instance.recivedBeatTime;
        recivedStart.Clear();
        recivedStart = RhythmManager.Instance.recivedStart;
        clipStates.Clear();
        clipStates = RhythmManager.Instance.clipStates;
        _beatTime = recivedStart[i] + recivedBeatTime[i];
        foreach (double item in recivedBeatTime)
        {
            Debug.Log(item);
        }
    }

    void OnDestroy()
    {
        if (RhythmManager.Instance != null)
        {
            RhythmManager.Instance.OnDataOver -= HandleDataOver;
            RhythmManager.Instance._PauseGame -= PauseScrite;
        }
    }

    void Update()
    {


        _audioNowTime = GetAudioTime();

        //Debug.Log(recivedClick[i]);
        //先判断指示线动
        if (_audioNowTime >= _beatTime)
        {
            ShowLine(i); 
            Debug.Log("指示线+"+j);
            PlayLine(j);
           

            _beatTime = _beatTime + recivedBeatTime[i];

            //指示器计算
            if (j < 8)
                { j++; }
            else
                { j = 1; }
            if (_beatTime >= recivedStart[i + 1])
             {
                if (i < recivedStart.Count)
                 { i++; }
             }
        }


    }

    void ShowLine(int i)
    {
        for (int t = 0; t < clipStates[i].Length; t++)
        {
            string childName = "Sprite-000" + t.ToString();
            Transform targetChild = transform.Find(childName);
                if (targetChild != null)
                {
                    //Debug.Log(clipStates[i][t]);
                    if (clipStates[i][t] == false)
                    {
                        animator = targetChild.GetComponent<Animator>();
                        animator.Play("miss");
                    }
                    else
                    {
                        animator = targetChild.GetComponent<Animator>();
                        animator.Play("rhIdle");
                    }
                    
                }
        }
    }

    void PlayLine(int i)
    {
        string childName = "Sprite-000" + i.ToString();
        //Debug.Log(childName);
        Transform targetChild = transform.Find(childName);
        if (targetChild != null)
        {
            animator = targetChild.GetComponent<Animator>();
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.IsName("rhIdle"))
            {
                animator.SetTrigger("play");
            }
            
        }
        else
        {
            Debug.Log("找不到子物体");
        }
    }

    public Transform Getline()
    {
        Transform targetChild = transform.Find("Sprite-1001");
        return targetChild;
    }

    // 外部调用接口
    public void UpdateBPM(float newBPM)
    {
        bpm = newBPM;
        beatInterval = 60f / bpm;
    }


    private void OnDisable()
    {
        if (RhythmManager.Instance != null)
        {
            RhythmManager.Instance.OnDataOver -= HandleDataOver;
        }

        // 取消订阅，防止内存泄漏
        if (bInstance != null)
        {
            bInstance.OnTaskCompleted -= HandleTaskCompleted;
        }
    }

    private void HandleTaskCompleted(bool result)
    {
        Debug.Log("收到订阅");
        Transform line = Getline();
        animator = line.GetComponent<Animator>();
        animator.SetTrigger("Play");
        if (result == true)
        {
            audioSource.clip = soundRgiht;
            audioSource.Play();
            right.SetActive(true);
            StartCoroutine(DelayedMethod());

        }
        else
        {
            audioSource.clip = soundError;
            audioSource.Play();
            error.SetActive(true);
            StartCoroutine(DelayedMethod());

        }
    }

    IEnumerator DelayedMethod()
    {


        yield return new WaitForSeconds(0.5f); // 延迟2秒
        right.gameObject.SetActive(false);
        error.SetActive(false);

    }
    public override void RegisterEvents()
    {
        this.AttationEvents.Add(Consts.E_CountDownComplete);
    }

    public override void HandleEvent(string eventName, object data)
    {
        switch (eventName)
        {

            case Consts.E_CountDownComplete:

                this.enabled = true;
                Debug.Log("接受事件" + eventName);

                break;
        }
    }
    // 获取当前音频播放时间（高精度）
    public double GetAudioTime()
    {
        return AudioSettings.dspTime - _audioStartDspTime;
    }
    private void PauseScrite(bool obj)
    {
        Debug.Log("暂停脚本" + this.name + obj);
        this.enabled = obj;
        if (!obj)
        {
            pauseStartTime = 0;
            pauseStartTime =AudioSettings.dspTime;
        }
        else
        {
            double pausedDuration = AudioSettings.dspTime - pauseStartTime;
            _audioStartDspTime += pausedDuration;
        }
    }
}