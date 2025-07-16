using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelCompletionData
{
    public bool isCompleted;
    public int highScore;
    public float completionRate; // 0-1表示完成百分比
    public int starRating; // 星级评价
    public bool isPerfect; // 是否完美通关
}
