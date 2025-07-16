using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;

public class LevelDetailPanel : BasePanel
{
    [Header("UI References")]
    [SerializeField] private Image levelImage;
    [SerializeField] private Text levelNameText;
    [SerializeField] private Text levelDescriptionText;
    [SerializeField] private Text levelScoreText;
    [SerializeField] private Button backButton;
    [SerializeField] private Button startButton;

    private LevelData currentLevelData;
    // private static LevelDetailPanel instance;

    private void Awake()
    {
        // 设置按钮点击事件
        backButton.onClick.AddListener(ClosePanel);
        startButton.onClick.AddListener(StartLevel);
    }

    // 公开方法：打开面板并显示指定关卡信息
    public void ShowLevelDetails(int levelId)
    {


        // 加载关卡数据
        LevelData levelData = LoadLevelData(levelId);
        currentLevelData = levelData;
        if (levelData == null)
        {
            Debug.LogError($"Level data for ID {levelId} not found");
            return;
        }
        else
        {
            this.UpdateUI();
        }
            
        
        
    }

    // 加载关卡数据
    private static LevelData LoadLevelData(int levelId)
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("GameData/levels");
        if (jsonFile == null)
        {
            Debug.LogError("level_data.json not found in Resources/Levels/");
            return null;
        }
        
        LevelDataWrapper wrapper = JsonUtility.FromJson<LevelDataWrapper>(jsonFile.text);
        if (wrapper == null || wrapper.levels == null)
        {
            Debug.LogError("Failed to parse level data");
            return null;
        }
        
        foreach (LevelData level in wrapper.levels)
        {
            if (level.id == levelId)
            {
                return level;
            }
        }
        
        return null;
    }

    // 更新UI显示
    private void UpdateUI()
    {
         Debug.Log(currentLevelData.name);
        if (currentLevelData == null)
        {
            Debug.Log("没有关卡数据");
            return;
        }    

        // 设置文本
        levelNameText.text = currentLevelData.name;
        levelDescriptionText.text = currentLevelData.description;

        // 加载并设置图片
        Sprite levelSprite = Resources.Load<Sprite>(currentLevelData.imagePath);
        if (levelSprite != null)
        {
            levelImage.sprite = levelSprite;
        }
        else
        {
            Debug.LogWarning($"Level image not found at path: {currentLevelData.imagePath}");
        }

        // 加载并显示分数（这里假设分数存储在PlayerPrefs中）
        int score = PlayerPrefs.GetInt($"Level_{currentLevelData.id}_Score", 0);
        levelScoreText.text = $"最高分: {score}";
    }

    // 关闭面板
    private void ClosePanel()
    {
        gameObject.SetActive(false);
        // 如果不需要保留面板，可以销毁
        // Destroy(gameObject);
        // instance = null;
    }

    // 开始关卡
    private void StartLevel()
    {
        if (currentLevelData == null) return;
        
        Debug.Log($"Loading level scene: {currentLevelData.sceneName}");
        SceneManager.LoadScene("ryGame");
    }

    // 清理
    private void OnDestroy()
    {
        backButton.onClick.RemoveListener(ClosePanel);
        startButton.onClick.RemoveListener(StartLevel);
    }
}