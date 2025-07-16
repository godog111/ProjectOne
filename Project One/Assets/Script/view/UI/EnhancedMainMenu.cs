using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using NUnit.Framework.Constraints;
using static UIModel;

/// <summary>
/// 增强版主菜单控制器，支持存档选择和设置
/// </summary>
// EnhancedMainMenuController.cs 完整实现
public class EnhancedMainMenu : BasePanel
{
    // 主菜单按钮
    private Button newGameButton;
    private Button continueButton;
    private Button loadButton;
    private Button settingsButton;
    private Button quitButton;

    // 面板引用
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject saveSlotPanel;
    [SerializeField] private GameObject settingsPanel;

    public override string Name
    {
        get { return Consts.V_Board; }    
    }

    private void Start()
    {
        // 初始化按钮引用
        InitializeButtons();

        // 默认显示主菜单
        mainPanel.SetActive(true);
       // saveSlotPanel.SetActive(false);
        settingsPanel.SetActive(false);
    }

    private void InitializeButtons()
    {
        // 主菜单按钮
        newGameButton = mainPanel.transform.Find("NewGameButton").GetComponent<Button>();
        continueButton = mainPanel.transform.Find("ContinueButton").GetComponent<Button>();
        settingsButton = mainPanel.transform.Find("SettingsButton").GetComponent<Button>();
        quitButton = mainPanel.transform.Find("QuitButton").GetComponent<Button>();

        // 存档选择面板按钮
       // var backButton = saveSlotPanel.transform.Find("BackButton").GetComponent<Button>();

        // 设置面板按钮
        var settingsBackButton = settingsPanel.transform.Find("BackButton").GetComponent<Button>();

        // 绑定事件
        newGameButton.onClick.AddListener(OnNewGameClicked);//新游戏按钮事件
        continueButton.onClick.AddListener(OnContinueClicked);//继续游戏
        settingsButton.onClick.AddListener(OnSettingsClicked);//设置游戏
        quitButton.onClick.AddListener(OnQuitClicked);//退出游戏
       // backButton.onClick.AddListener(OnBackToMainMenu);//返回按钮
        settingsBackButton.onClick.AddListener(OnBackToMainMenu);//设置返回游戏
    }

   
    private void OnNewGameClicked()
    {
        //mainPanel.SetActive(false);
        //saveSlotPanel.SetActive(true);
        
        // 刷新存档槽显示
        RefreshSaveSlots();//待定不一定有用
        MVC.SendEvent(Consts.E_CreatNewGame,1);
        Game.Instance.LoadScene(2);
        //EnhancedGameSceneManager.Instance.StartNewGame(1);
    }

    private void OnContinueClicked()
    {
       //继续游戏事件 EnhancedGameSceneManager.Instance.ContinueGame();

       UIModel.Instance.OpenPanel(UIConst.continueButton);
       // mainPanel.SetActive(false);
       // saveSlotPanel.SetActive(true);
    }

    private void OnSettingsClicked()
    {
        mainPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    private void OnQuitClicked()
    {
        Game.Instance.QuitGame();
    }

    private void OnBackToMainMenu()
    {
        mainPanel.SetActive(true);
        saveSlotPanel.SetActive(false);
        settingsPanel.SetActive(false);
    }

    private void RefreshSaveSlots()
    {
        // 存档槽刷新逻辑（见之前实现）
    }

    public override void HandleEvent(string eventName, object data)
    {
        throw new System.NotImplementedException();
    }
}