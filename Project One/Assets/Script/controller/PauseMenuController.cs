using UnityEngine;

/// <summary>
/// 暂停菜单控制器
/// </summary>
public class PauseMenuController : MonoBehaviour
{
    [Header("暂停菜单")]
    [SerializeField] private GameObject pauseMenu;
    
    private bool isPaused = false;
    
    private void Update()
    {
        // 按ESC键暂停/继续
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }
    
    /// <summary>
    /// 暂停游戏
    /// </summary>
    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f; // 暂停游戏时间
        pauseMenu.SetActive(true);
        Cursor.visible = true; // 显示鼠标
    }
    
    /// <summary>
    /// 继续游戏
    /// </summary>
    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f; // 恢复游戏时间
        pauseMenu.SetActive(false);
        Cursor.visible = false; // 隐藏鼠标
    }
    
    /// <summary>
    /// 保存游戏
    /// </summary>
    public void SaveGame()
    {
       // EnhancedGameSceneManager.Instance.SaveCurrentGame();
    }
    
    /// <summary>
    /// 返回主菜单
    /// </summary>
    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f; // 确保时间恢复
       // EnhancedGameSceneManager.Instance.LoadMainMenu();
    }
    
    /// <summary>
    /// 退出游戏
    /// </summary>
    public void QuitGame()
    {
       // EnhancedGameSceneManager.Instance.QuitGame();
    }
}