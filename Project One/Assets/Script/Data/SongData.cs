using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewSongData", menuName = "Rhythm Game/Song Data")]
public class SongData : ScriptableObject
{
    public AudioClip audioClip;
    public List<float> noteTimes = new List<float>(); // 存储标记时间点
}