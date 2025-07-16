using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class AudioMarkerEditor : EditorWindow
{
    // ---------------------------- 配置参数 ----------------------------
    private const int VISIBLE_CHUNK_DURATION = 30;      // 可视窗口时长（秒）
    private const int WAVEFORM_RESOLUTION = 1024;        // 波形显示分辨率（点数）
    private const float UPDATE_INTERVAL = 0.1f;         // 波形更新频率（秒）

     // ---------------------------- 新增高度控制参数 ----------------------------
    private float waveformHeight = 200f; // 默认高度
    private const string WAVEFORM_HEIGHT_KEY = "AudioMarker_WaveformHeight";

    // ---------------------------- 运行时变量 ----------------------------
    private SongData songData;
    private AudioSource previewSource;
    private float[] currentWaveform = new float[WAVEFORM_RESOLUTION];
    private int currentChunkStartSample;
    private float lastUpdateTime;
    private Vector2 scrollPos;
    private bool isPlaying;

    // ---------------------------- 初始化 ----------------------------
    [MenuItem("Window/Rhythm Game/Audio Marker")]
    public static void ShowWindow()
    {
        GetWindow<AudioMarkerEditor>("Audio Marker").Show();
    }

    private void OnEnable()
    {
        // 创建临时AudioSource用于预览
        GameObject tempObj = new GameObject("AudioPreview");
        previewSource = tempObj.AddComponent<AudioSource>();
        EditorApplication.update += OnEditorUpdate; // 注册更新回调
    }

    private void OnDisable()
    {
        EditorApplication.update -= OnEditorUpdate;
        if (previewSource != null)
            DestroyImmediate(previewSource.gameObject);
    }

    // ---------------------------- 主界面渲染 ----------------------------
    private void OnGUI()
    {
        DrawHeader();
        DrawAudioControls();
        DrawWaveformDisplay();
        DrawNoteList();
        DrawWaveformHeightControl(); 
    }

    // ---------------------------- 新增高度控制UI ----------------------------
    private void DrawWaveformHeightControl()
    {
        EditorGUILayout.BeginHorizontal();
        {
            EditorGUILayout.LabelField("Waveform Height", GUILayout.Width(100));
            float newHeight = EditorGUILayout.Slider(waveformHeight, 100f, 400f);
            if (newHeight != waveformHeight)
            {
                waveformHeight = newHeight;
                Repaint(); // 高度变化时立即重绘
            }
        }
        EditorGUILayout.EndHorizontal();
    }

    // 顶部标题和SongData选择
    private void DrawHeader()
    {
        GUILayout.Label("Rhythm Game Audio Marker", EditorStyles.boldLabel);
        songData = EditorGUILayout.ObjectField("Song Data", songData, typeof(SongData), false) as SongData;
    }

    // ---------------------------- 音频控制区 ----------------------------
    private void DrawAudioControls()
    {
        GUILayout.BeginHorizontal();
        
        // 播放/停止按钮
        if (GUILayout.Button(isPlaying ? "Pause" : "Play"))
        {
            TogglePlayback();
        }
        if (GUILayout.Button("Stop"))
        {
            StopPlayback();
        }

        // 添加标记按钮
        if (GUILayout.Button("Add Note") && isPlaying)
        {
            AddNoteAtCurrentTime();
        }

        GUILayout.EndHorizontal();

        // 音频进度条（可拖拽）
        if (songData?.audioClip != null)
        {
            float newTime = EditorGUILayout.Slider(previewSource.time, 0, songData.audioClip.length);
            if (Mathf.Abs(newTime - previewSource.time) > 0.1f)
            {
                previewSource.time = newTime;
                UpdateWaveformChunk();
            }
        }
    }

    // ---------------------------- 波形显示区 ----------------------------
    private void DrawWaveformDisplay()
    {
        if (songData?.audioClip == null) return;

        // 波形背景区域
        Rect waveformRect = GUILayoutUtility.GetRect(position.width, waveformHeight);
        EditorGUI.DrawRect(waveformRect, new Color(0.1f, 0.1f, 0.1f));

        // 绘制波形（绿色）
        float sampleWidth = waveformRect.width / (WAVEFORM_RESOLUTION);
        float chunkStartTime = (float)currentChunkStartSample / songData.audioClip.frequency ;
   

    // 修复2：绘制波形时，严格限制在可视区域内
        
        for (int i = 0; i < WAVEFORM_RESOLUTION; i++)
        {
            float height = currentWaveform[i] * waveformRect.height*0.38f;
            float x = waveformRect.x + (i) * sampleWidth;
            float y = waveformRect.y + (waveformRect.height - height)/2;
            EditorGUI.DrawRect(new Rect(x, y, sampleWidth, height), Color.green);
        }

    // 修复3：播放头位置限制在波形区域内
    if (isPlaying)
    {
        float playHeadNormalized = ((previewSource.time - chunkStartTime) / VISIBLE_CHUNK_DURATION/2);
         float playHeadPos = waveformRect.x +( playHeadNormalized *4)* waveformRect.width;
        EditorGUI.DrawRect(new Rect(playHeadPos, waveformRect.y, 2, waveformRect.height), Color.yellow);
    }
    }

    // ---------------------------- 标记点列表 ----------------------------
    private void DrawNoteList()
    {
        if (songData?.noteTimes == null) return;

        GUILayout.Label($"Notes ({songData.noteTimes.Count}):");
        scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.Height(150));
        
        for (int i = 0; i < songData.noteTimes.Count; i++)
        {
            GUILayout.BeginHorizontal();
            
            // 显示时间点和跳转按钮
            EditorGUILayout.LabelField($"{i}: {songData.noteTimes[i]:F2}s");
            if (GUILayout.Button("Jump", GUILayout.Width(60)))
            {
                previewSource.time = songData.noteTimes[i];
                UpdateWaveformChunk();
            }
            
            // 删除按钮
            if (GUILayout.Button("Delete", GUILayout.Width(60)))
            {
                songData.noteTimes.RemoveAt(i);
                EditorUtility.SetDirty(songData);
            }
            
            GUILayout.EndHorizontal();
        }
        
        GUILayout.EndScrollView();
    }

    // ---------------------------- 核心逻辑 ----------------------------
    private void OnEditorUpdate()
    {
        // 定时更新波形显示（避免每帧刷新）
        if (isPlaying && Time.realtimeSinceStartup - lastUpdateTime > UPDATE_INTERVAL)
        {
            UpdateWaveformChunk();
            Repaint();
            lastUpdateTime = Time.realtimeSinceStartup;
        }
    }

 private void UpdateWaveformChunk()
{
    if (songData?.audioClip == null) return;

    // 计算当前播放时间对应的样本位置（直接映射，不加额外缩放）
    int currentSample = Mathf.FloorToInt(previewSource.time * songData.audioClip.frequency);

    // 计算当前可视窗口的起始样本（居中显示）
    currentChunkStartSample = Mathf.Clamp(
        currentSample - (VISIBLE_CHUNK_DURATION * songData.audioClip.frequency )/4,
        0,
        Mathf.Max(0, songData.audioClip.samples - VISIBLE_CHUNK_DURATION * songData.audioClip.frequency)
    );

    // 加载波形数据（确保范围正确）
    AudioUtility.GetWaveFormChunk(
        songData.audioClip,
        currentWaveform,
        currentChunkStartSample,
        currentChunkStartSample + VISIBLE_CHUNK_DURATION * songData.audioClip.frequency
    );
}

    private void TogglePlayback()
    {
        if (songData?.audioClip == null) return;

        if (isPlaying)
        {
            previewSource.Pause();
        }
        else
        {
            previewSource.clip = songData.audioClip;
            previewSource.Play();
            UpdateWaveformChunk();
        }
        isPlaying = !isPlaying;
    }

    private void StopPlayback()
    {
        previewSource.Stop();
        isPlaying = false;
    }

    private void AddNoteAtCurrentTime()
    {
        if (songData == null) return;

        songData.noteTimes.Add(previewSource.time);
        EditorUtility.SetDirty(songData); // 标记数据需要保存
    }
}