using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class pauseUI : BasePanel
{
    [SerializeField] private GameObject pausePanel; // 暂停面板对象

    private Button backMain;//返回主界面按钮
    private Button quick;//返回主界面按钮
    private Button restart;//返回主界面按钮
    private Button backGame;//返回主界面按钮
    private bool isPaused = false;

    // Start is called before the first frame update
    void Start()
    {
        // 确保游戏开始时暂停面板是隐藏的
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }
        // 初始化按钮并绑定事件
        backMain = GameObject.Find("backMain").GetComponent<Button>();
        quick = GameObject.Find("quick").GetComponent<Button>();
        restart = GameObject.Find("restart").GetComponent<Button>();
        backGame = GameObject.Find("backGame").GetComponent<Button>();
        
        backMain.onClick.AddListener(ReturnToMainMenu);
        quick.onClick.AddListener(QuitGame);
        restart.onClick.AddListener(RestartGame);
        backGame.onClick.AddListener(ResumeGame);
        
    }

    // Update is called once per frame
    void Update()
    {
        // 检测是否按下ESC键来触发暂停
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    /// <summary>
    /// 切换暂停状态
    /// </summary>
    public void TogglePause()
    {
        isPaused = !isPaused;
        
        if (isPaused)
        {
            PauseGame();
        }
        else
        {
            ResumeGame();
        }
    }

    /// <summary>
    /// 暂停游戏
    /// </summary>
    public void PauseGame()
    {
        Time.timeScale = 0f; // 设置时间缩放为0，暂停游戏
        if (pausePanel != null)
        {
            pausePanel.SetActive(true); // 显示暂停面板
        }
    }

    /// <summary>
    /// 返回游戏（继续游戏）
    /// </summary>
    public void ResumeGame()
    {
        RhythmManager.Instance.PauseGame(false);
        UIModel.Instance.ClosePanel("pauseUI"); // 关闭暂停面板
    }

    /// <summary>
    /// 重新开始当前场景
    /// </summary>
    public void RestartGame()
    {
        Time.timeScale = 1f; // 确保时间缩放恢复正常
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); // 重新加载当前场景
    }

    /// <summary>
    /// 返回主界面
    /// </summary>
    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f; // 确保时间缩放恢复正常
        Game.Instance.LoadScene(1); // 加载主菜单场景，请确保场景名称正确
    }

    /// <summary>
    /// 关闭游戏
    /// </summary>
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // 在编辑器中停止播放模式
#else
        Application.Quit(); // 在构建的应用程序中退出
#endif
    }
}