using UnityEngine;
using System.IO; // 如果使用非Resources加载方式需要这个命名空间



public class JSONReader : MonoBehaviour
{
    [Header("Resources加载方式")]
    public string jsonFilePath = "Data/example"; // 在Resources文件夹下的路径
    
    [Header("直接引用方式")]
    public TextAsset jsonFile; // 可以直接拖拽赋值
    public NPCConfiguration[] NPC;

    
    void Start()
    {
        ConNPCConfiguration container = JsonUtility.FromJson<ConNPCConfiguration>(jsonFile.text);
        
        // 方法1：通过Resources加载
        TextAsset textAsset = Resources.Load<TextAsset>(jsonFilePath);
        
        /*if(textAsset == null)
        {
            //Debug.LogError("无法加载JSON文件，请检查路径: " + jsonFilePath);
            return;
        }*/

        // 方法2：使用直接引用的TextAsset
        // TextAsset textAsset = jsonFile;

        // 解析JSON数据
        Debug.Log("开始");
    
        
        
        foreach(NPCConfiguration entry in container.CCCEnter)
        {
            Debug.Log($"名称: {entry.ID}\n" +
                      $"生命值: {entry.isUnique}\n" +
                      $"存活状态: {entry.spawnPosition}\n" +
                      $"位置: {string.Join(",", entry.ID)}");
        }

        // 如果JSON是单个对象而不是数组，可以直接解析：
        // DataEntry singleEntry = JsonUtility.FromJson<DataEntry>(textAsset.text);
    }
}


