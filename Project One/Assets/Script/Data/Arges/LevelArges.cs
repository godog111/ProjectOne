using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelArges : MonoBehaviour
{
    [System.Serializable]

    
    public class LevelArgesData {  
    public int levelID;//关卡ID        
    public string musicPlayer;//音源

    public string prefab;//预设预留

    public int health;//关卡生命

    public int points;//起始积分

    public text BPM;//
    }
}
