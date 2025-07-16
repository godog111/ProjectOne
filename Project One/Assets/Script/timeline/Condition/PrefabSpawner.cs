using UnityEngine;

public class PrefabSpawner : MonoBehaviour
{
    // 生成预设的方法
    public void SpawnPrefab(GameObject prefab)
    {
        if (prefab != null)
        {
            Instantiate(prefab, transform.position, transform.rotation);
        }
    }

    // 销毁预设的方法
    public void DestroyPrefab(GameObject prefab)
    {
        if (prefab != null)
        {
            Destroy(prefab);
        }
    }
}