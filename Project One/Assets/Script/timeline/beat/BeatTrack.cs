using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[TrackColor(0.2f, 0.8f, 0.3f)] // 设置轨道颜色
[TrackClipType(typeof(BeatClip))] // 指定该轨道使用的剪辑类型
[TrackBindingType(typeof(RhythmManager))] // 指定轨道绑定的组件类型
public class BeatTrack : TrackAsset
{
    // 重写CreateTrackMixer方法，创建自定义的轨道混合器
    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    {
        // 创建ScriptPlayable<BeatMixerBehaviour>实例
        var mixer = ScriptPlayable<BeatMixerBehaviour>.Create(graph, inputCount);

        // 获取混合器行为实例
        var behaviour = mixer.GetBehaviour();

        // 如果行为实例存在，设置其轨道实例
        if (behaviour != null)
        {
            behaviour.track = this;
        }

        return mixer;
    }
    
    protected override Playable CreatePlayable(PlayableGraph graph, GameObject go, TimelineClip clip)
    {
        var playable = ScriptPlayable<BeatBehaviour>.Create(graph);
        var behaviour = playable.GetBehaviour();
        behaviour.Clip = clip; // 关键：传递 TimelineClip
        return playable;
    }
}

// 轨道混合器行为
public class BeatMixerBehaviour : PlayableBehaviour
{
    public BeatTrack track; // 关联的轨道实例
    
    // 重写ProcessFrame方法，处理每一帧的混合逻辑
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        // 获取轨道绑定的RhythmManager组件
        var rhythmManager = playerData as RhythmManager;
        
        // 如果没有绑定组件，直接返回
        if (rhythmManager == null)
            return;
        
        // 获取输入数量
        int inputCount = playable.GetInputCount();
        
        // 遍历所有输入
        for (int i = 0; i < inputCount; i++)
        {
            // 获取当前输入的权重
            float inputWeight = playable.GetInputWeight(i);
            
            // 获取当前输入的Playable
            ScriptPlayable<BeatBehaviour> inputPlayable = (ScriptPlayable<BeatBehaviour>)playable.GetInput(i);
            
            // 获取当前输入的Behaviour
            BeatBehaviour inputBehaviour = inputPlayable.GetBehaviour();
            
            // 如果权重大于0（表示剪辑处于活动状态）
            if (inputWeight > 0)
            {
                // 这里可以添加额外的混合逻辑
                // 例如根据权重调整节拍强度等
            }
        }
    }
}