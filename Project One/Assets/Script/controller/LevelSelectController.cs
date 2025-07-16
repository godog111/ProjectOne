using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// 关卡选择控制器
/// </summary>
public class LevelSelectController : MonoBehaviour
{
    [Header("关卡按钮容器")]
    [SerializeField] private Transform levelButtonContainer;
    
    [Header("关卡按钮预制体")]
    [SerializeField] private GameObject levelButtonPrefab;
    
    private void Start()
    {
 
    }
    
    /// <summary>
    /// 关卡选择
    /// </summary>
    private void OnLevelSelected(int level)
    {
        //SaveGameModel.Instance.LoadLevel(level);
    }
    
    /// <summary>
    /// 返回主菜单
    /// </summary>
    public void BackToMainMenu()
    {
       // SaveGameModel.Instance.LoadMainMenu();
    }
}