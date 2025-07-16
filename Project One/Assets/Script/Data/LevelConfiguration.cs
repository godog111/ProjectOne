using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelConfiguration", menuName = "Game/Level Database")]
public class LevelConfiguration : ScriptableObject {
    public int levelID;

    public string l_name;//关卡名称

    public AudioSource audio;//关卡音频
    public GameObject player;//主角预设
    public GameObject background;//关卡背景
    public Vector3 playerPositon;//角色出生点
    public List<NPCConfiguration> npcsInLevel;
   // public List<NPCConfiguration> npcs;
}
