using UnityEngine;
using UnityEditor;

public class ResourceDebugger : MonoBehaviour
{
    [MenuItem("Tools/Debug/Print Resources Paths")]
    public static void PrintResourcesPaths()
    {
        // 获取Resources文件夹下所有GameObject预制体
        GameObject[] allPrefabs = Resources.LoadAll<GameObject>("");
        
        Debug.Log("=== Resources目录结构 ===");
        Debug.Log($"找到 {allPrefabs.Length} 个GameObject资源");
        
        // 按路径分组打印
        foreach(var prefab in allPrefabs)
        {
            // 获取资源的相对路径
            string path = GetResourcePath(prefab);
            Debug.Log($"- {prefab.name} (路径: {path})");
        }
        
        Debug.Log("=== 打印完成 ===");
    }

    private static string GetResourcePath(Object asset)
    {
        // 获取完整Asset路径
        string fullPath = AssetDatabase.GetAssetPath(asset);
        
        // 提取Resources后面的部分
        int resourcesIndex = fullPath.IndexOf("Resources/");
        if(resourcesIndex >= 0)
        {
            return fullPath.Substring(resourcesIndex + 10).Replace(".prefab", "");
        }
        
        return fullPath;
    }
}