using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[System.Serializable]
public class BeatBehaviour : PlayableBehaviour
{
    public int beatPosition;          // 节拍在小节中的位置
    public float intensity;           // 节拍强度
    public bool isStrongBeat;         // 是否是大节拍
    public BeatType beatType;         // 节拍类型
    public int beatCountInMeasure;    // 当前小节的总节拍数

    public int clickTriggerd; //发送信号的拍子数

    private bool wasTriggered;

    public TimelineClip Clip { get; set; } // 存储 TimelineClip

    public double clickTime;
    public static event Action<TimelineClip> OnClipInitialized;

    public static event Action<double> OnClick;
    //在游戏开始时执行方法
    public override void OnGraphStart(Playable playable)
    {
        if (Clip != null)
        {

            OnClipInitialized?.Invoke(Clip);
        }
    }

    // 重写PlayableBehaviour的ProcessFrame方法，该方法会在每一帧被调用
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {

        // 检查是否满足触发条件：
        // 1. 当前未处于已触发状态（!wasTriggered）
        // 2. 当前权重满足触发条件（shouldTrigger）
        if (!wasTriggered)
        {
            // Debug.Log(Clip.start);
            // 标记为已触发，防止同一节拍重复触发
            wasTriggered = true;

            // 尝试将playerData转换为BeatEventReceiver类型
            // 问号(?)表示安全转换，如果转换失败则为null
            var receiver = playerData as RhythmManager;

            // 如果receiver不为null，则调用其OnBeatTrigger方法
            // 并将当前BeatBehaviour实例(this)作为参数传递
            receiver?.OnBeatTrigger(this);
        }
        // 检查是否应该重置触发状态：
        // 1. 当前处于已触发状态（wasTriggered）
        // 2. 当前权重不满足触发条件（!shouldTrigger）
        else if (wasTriggered)
        {
            // 重置触发状态，允许下次权重超过阈值时再次触发
            wasTriggered = false;
        }

        // 注意：这里没有显式的else情况处理
        // 当wasTriggered和shouldTrigger状态相同时（都为true或都为false）
        // 不会执行任何操作，保持当前状态不变
    }

}

// 节拍类型枚举
public enum BeatType
{
    Normal,         // 普通节拍
    Kick,           // 鼓点
    Snare,          // 军鼓
    HiHat,          // 踩镲
    Custom1,        // 自定义类型1
    Custom2         // 自定义类型2
}
