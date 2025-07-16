using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ryGameSpwan : View
{
    
    public TextAsset chart;
    public AudioSource musicPlayer;
    public GameObject tap;

    public int promptNum = 0;
    private GameObject[] prompts;

    float[] timeStamps, notePosition;
    float myTime = 0;
    int index = 0;

    float point =0;//游戏积分
    int health =5;//游戏生命
    float offset =1f;//偏移量
    [SerializeField] private SongData SongData;
    public Text BPM;

    public Text pointText;
    public Text healthPoint;

    public Text tishi;//打字效果
  
    public float delayBetweenChars = 0.111f;//打字效果延迟时间
     private bool isShaking = false;//画面是否抖动
    public float shakeDuration = 0.5f;//画面抖动持续时间
    private Vector3 originalPos;//画面初始位置
    
    private Camera mainCamera;//抖动的相机
    
    public delegate void TaskCompletedHandler(bool result);
    public event TaskCompletedHandler OnTaskCompleted;
    public override string Name
    {
        get { return Consts.V_ryGameSpwan; }
    }

    // Start is called before the first frame update
    void Start()
    {
        this.enabled = false;
        musicPlayer.Stop();
    }
    void Awake()
    {
        mainCamera =Camera.main;
        LoadChart(SongData);
        InitializePrompts();
        //musicPlayer.Pause();
        originalPos = mainCamera.transform.localPosition;
        StartCoroutine("GameStart");
       // this.enabled=false;

    }
     // 公开的抖动调用方法
    public void Shake()
    {
        if (!isShaking && mainCamera != null)
        {
            StartCoroutine(DoShake());
        }
    }
    
    void Update()
    {
        myTime +=Time.deltaTime;
        if(myTime >= timeStamps[index])
        {
             int s = (int)notePosition[index];
            prompts[s].GetComponent<promptScript>().SetNoteState(true);
            if(index+1<timeStamps.Length)
            {
                index++;
                Shake();
            }
            

        }
        if(Input.GetKeyDown(KeyCode.Space))
            {
            Debug.Log("点击时间："+myTime);
            Debug.Log("铺面判断时间"+timeStamps[index-1]);
            ryClick(myTime-2,timeStamps[index-1]);
            }
       
        pointText.text ="积分："+ point.ToString();
        healthPoint.text ="生命"+health.ToString();
       
       if(health == 0||!musicPlayer.isPlaying)
       {
         //游戏结束
         UIModel.Instance.OpenPanel("gameOver");
         this.enabled =false;
         musicPlayer.Pause();
       }
        
    }

    IEnumerator GameStart()
    {
        yield return new WaitForSeconds(2);
        //musicPlayer.Play();
    }

     IEnumerator ShowText(string text)
    {
        for (int i = 0; i <= text.Length; i++)
        {
            string currentText;
            currentText = text.Substring(0, i);
            tishi.text = currentText;
            
            // 播放打字音效（可选）
            // AudioManager.Instance.PlayTypeSound();
            
            yield return new WaitForSeconds(delayBetweenChars);
        }
        
        tishi.gameObject.SetActive(false);
    }

    IEnumerator DoShake()
    {
       
        //画面抖动
        isShaking = true;
        float elapsed = 0f;
        
        while (elapsed < shakeDuration)
        {
            float yShake =UnityEngine.Random.Range(-1f, 1f) * 5f;
            mainCamera.transform.localPosition = originalPos + new Vector3(0, yShake, 0);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // 恢复原位
        transform.localPosition = originalPos;
        isShaking = false;
    }
    
    private void ryClick(float clickTime,float promptsTime)
    {
        if (clickTime > promptsTime - offset && clickTime < promptsTime + offset)
        {

            point++;
            tishi.gameObject.SetActive(true);
            tishi.color = Color.green;
            StartCoroutine(ShowText("按 对 了"));
            SendEvent(Consts.E_headRight);
            OnTaskCompleted?.Invoke(true);
        }
        else
        {
            tishi.gameObject.SetActive(true);
            health--;
            tishi.color = Color.red;
            StartCoroutine(ShowText("按 错 了"));
            SendEvent(Consts.E_headError);
            OnTaskCompleted?.Invoke(false);
        }
    }


    void LoadChart(SongData songData)
    {
        
        timeStamps = new float[songData.noteTimes.Count];
        notePosition = new float[songData.noteTimes.Count]; 
        for(int i =  0;i<songData.noteTimes.Count;i++)
        {
            timeStamps[i]=songData.noteTimes[i];
            notePosition[i]=0;
           // notePosition[i]= i % 7;
           // Debug.Log(notePosition[i]);
           
        }
    }

    void InitializePrompts()
    {
        prompts = new GameObject[promptNum];
        for (int i = 0; i < promptNum; i++)
        {
            Debug.Log("加载成功");
            prompts[i] = Instantiate(tap);
            prompts[i].name = "Prompt_" + i;
            prompts[i].SetActive(true);
            SetPromptPosition(prompts[i], i);
        
        }
    }

    void SetPromptPosition(GameObject prompt, int index)
    {
        float xSpacing = 500.0f; // prompt之间的水平间距
        float startX = -(promptNum - 1) * xSpacing / 2; // 计算起始x坐标，使prompts居中
        
        Vector3 position = new Vector3(
            1550 + (index * xSpacing), // X坐标
            200f,                          // Y坐标
            0f                           // Z坐标
        );
        
        prompt.transform.position = position;
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
                musicPlayer.Play();
                this.enabled = true;
                Debug.Log("接受事件"+eventName);
                
                break;
        }
    }

    private void ShowBPM(int level)
    {
        //BPM = StaticDataManager.GetDataList
    }
}
