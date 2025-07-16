using UnityEngine;
using UnityEngine.UI;

public class Metronome : MonoBehaviour
{
    public AudioClip tickSound; // 节拍音效
    public Text bpmText; // 显示BPM的UI文本
    
    [Range(40, 240)] // 限制BPM范围
    public int bpm = 120; // 默认120拍/分钟
    
    private AudioSource audioSource;
    private float nextTickTime;
    private bool isRunning = false;
    
    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        UpdateBPMText();
    }
    
    void Update()
    {
        if (isRunning && Time.time >= nextTickTime)
        {
            PlayTick();
            nextTickTime = Time.time + 60f / bpm; // 计算下一个节拍时间
        }
    }
    
    void PlayTick()
    {
        audioSource.PlayOneShot(tickSound);
    }
    
    void UpdateBPMText()
    {
        if (bpmText != null)
        {
            bpmText.text = "BPM: " + bpm.ToString();
        }
    }
    
    // 公共方法供UI按钮调用
    public void StartMetronome()
    {
        isRunning = true;
        nextTickTime = Time.time + 60f / bpm;
    }
    
    public void StopMetronome()
    {
        isRunning = false;
    }
    
    public void IncreaseBPM()
    {
        bpm = Mathf.Min(bpm + 5, 240); // 每次增加5BPM，最大240
        UpdateBPMText();
    }
    
    public void DecreaseBPM()
    {
        bpm = Mathf.Max(bpm - 5, 40); // 每次减少5BPM，最小40
        UpdateBPMText();
    }
}