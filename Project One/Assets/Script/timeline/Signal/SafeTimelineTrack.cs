// SafeTimelineTrack.cs
using UnityEngine;
using UnityEngine.Timeline;

[ExecuteInEditMode]
public class SafeTimelineTrack : MonoBehaviour
{
    public TimelineAsset timeline;
    
    private void OnEnable()
    {
        if (timeline == null) return;
        
        foreach (var track in timeline.GetOutputTracks())
        {
            // 确保所有轨道都正确初始化
            if (track == null) continue;
            
            // 修复可能存在的空引用问题
            track.name = string.IsNullOrEmpty(track.name) ? "Unnamed Track" : track.name;
        }
    }
}