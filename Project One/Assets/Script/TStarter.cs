using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TStarter : MonoBehaviour
{
   // [SerializeField] private int initialLevelID = 1;

    public  LevelManager LM;
    public TextAsset conditionsConfigFile;//条件管理配置器配置地址
    void Start()
    {
        // 延迟一帧避免Awake顺序问题
        LM = FindObjectOfType<LevelManager>();
        StartCoroutine(LoadInitialLevel());
        ConditionManager.Instance.LoadConditionConfigs(conditionsConfigFile);
    }

    IEnumerator LoadInitialLevel()
    {
        yield return null; 
        LM.LoadLevel(1);
    }
}
