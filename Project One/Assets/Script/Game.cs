using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(StaticData))]
public class Game : ApplicationBase<Game>
{
   
   
   public StaticData StaticData =null;//静态数据

    //加载 游戏场景
   public void LoadScene (int level)
   {    
        SceneArgs e = new SceneArgs();
        e.id =SceneManager.GetActiveScene().buildIndex;
        SendEvent(Consts.E_ExitScene, e);//发布事件
        SceneManager.LoadScene(level,LoadSceneMode.Single);
   }

    void OnEnable()
    {
        // 注册场景加载完成的事件
        SceneManager.sceneLoaded += OnSceneLoaded;
        
        SceneArgs enterArgs = new SceneArgs();
        enterArgs.id = SceneManager.GetActiveScene().buildIndex;
        string sceneName = SceneManager.GetActiveScene().name;
        Debug.Log("进入新场景"+sceneName);
        
    }

    void OnDisable()
    {
        // 取消注册事件（重要！避免内存泄漏）
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneArgs exitArgs = new SceneArgs();
        exitArgs.id = SceneManager.GetActiveScene().buildIndex;
        //exitArgs.sceneName = SceneManager.GetActiveScene().name;
        SendEvent(Consts.E_ExitScene, exitArgs);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneArgs enterArgs = new SceneArgs();
        enterArgs.id = scene.buildIndex;
        Debug.Log("场景加载完成: " + scene.name);
        Debug.Log("加载模式: " + mode);
        SendEvent(Consts.E_EnterScene, enterArgs);
        // 在这里执行场景加载后需要进行的操作
        InitializeSceneObjects();
    }

    void InitializeSceneObjects()
    {
        // 初始化场景中的对象
    }

    
    void Start()
    {
        Object.DontDestroyOnLoad(this.gameObject);
        StaticData = StaticData.Instance;

        RegisterController(Consts.E_StartUp,typeof(StartUpComand));

        SendEvent(Consts.E_StartUp);
    }

     public void QuitGame()
    {
        Application.Quit();
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

}
