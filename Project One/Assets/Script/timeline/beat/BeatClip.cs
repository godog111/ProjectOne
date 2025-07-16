using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[System.Serializable]
public class BeatClip : PlayableAsset, ITimelineClipAsset
{
    [Tooltip("节拍在小节中的位置(0=第一拍)")]
    public int beatPosition;
    
    [Tooltip("节拍强度(用于视觉效果)")]
    [Range(0, 1)] public float intensity = 1f;
    
    [Tooltip("是否是大节拍(用于强调)")]
    public bool isStrongBeat;
    
    [Tooltip("节拍类型")]
    public BeatType beatType = BeatType.Normal;
    
    [Tooltip("当前小节的总节拍数")]
    public int beatCountInMeasure = 4;

    [Tooltip("发送信号的节拍数")]
    public int clickTriggerd = 4;

    public bool[] clipState ;
        
    public ClipCaps clipCaps => ClipCaps.Blending;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<BeatBehaviour>.Create(graph);
        var behaviour = playable.GetBehaviour();
        clipState = new bool[clickTriggerd];
        
        behaviour.beatPosition = beatPosition;
        behaviour.intensity = intensity;
        behaviour.isStrongBeat = isStrongBeat;
        behaviour.beatType = beatType;
        behaviour.beatCountInMeasure = beatCountInMeasure;
        behaviour.clickTriggerd = clickTriggerd;
        
        
        return playable;
    }
}