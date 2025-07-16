using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class text : MonoBehaviour
{
// 在任意脚本的Start方法中添加：
void Start()
{
    // 使用完整路径（不含Resources/前缀和.prefab后缀）
    string path = "prefab/UI/Dialogue"; 
    GameObject prefab = Resources.Load<GameObject>(path);
    GameObject canvasObj = GameObject.Find("Canvas");
    GameObject panelObject = Instantiate(prefab, canvasObj.transform, false);
  
   
}


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
           // DialogueManager.Instance.nameText.text ="fff";
        }
    }
    // 辅助方法：列出Resources下所有内容
    void CheckResourcesContents()
{
    Debug.Log("正在扫描Resources文件夹...");
    var allPrefabs = Resources.LoadAll<GameObject>("");
    foreach(var p in allPrefabs)
    {
       // string assetPath = AssetDatabase.GetAssetPath(p);
      //  Debug.Log($"找到预制体: {p.name} | 完整路径: {assetPath}");
    }
}
}
