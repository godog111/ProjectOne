using System.Collections.Generic;
using UnityEngine;

public class UIModel : Model
{
    public override string Name => throw new System.NotImplementedException();

    private static UIModel _instance;

    private Transform _uiRoot;

    private Dictionary<string,string> pathDict;

    private Dictionary<string, GameObject> prefabDict;//预制件缓存字典

    public Dictionary<string,BasePanel> panelDict;//已打开界面缓存字典
    

    public static UIModel Instance{
        get{
            if(_instance ==null)
            {
                _instance = new UIModel();
            }
            return _instance;
        }
    }

    private UIModel()
    {
        InitDicts();
    }

    private void InitDicts()
    {
        prefabDict = new Dictionary<string, GameObject>();
        panelDict = new Dictionary<string, BasePanel>();
        pathDict = new Dictionary<string, string>()
        {
            {UIConst.continueButton,"savePanel"},
            {UIConst.Dialogue,"Dialogue"},
            {UIConst.gameOver,"gameOver"},
            {UIConst.DeskUI,"DeskUI"},
            {UIConst.ryLevel,"ryLevel"},
            {UIConst.pauseUI,"pauseUI"},
            {UIConst.PackageUi,"PackageUi"},
        
        };
    }

    public Transform UIRoot
    {
        get{
            if(_uiRoot == null)
            {

                if(GameObject.Find("Canvas"))

                {
                    _uiRoot = GameObject.Find("Canvas").transform;
                }
                else{
                    _uiRoot = new GameObject("Canvas").transform;
                }
                return _uiRoot;
                
            }
                return _uiRoot;
            }       
        
    }
    public BasePanel OpenPanel(string name)
    {
        Debug.Log("请求打开界面"+name);
        //打开界面
        BasePanel panel = null;
        //检查是否打开
        if(panelDict.TryGetValue(name,out panel))
        {
            Debug.LogError("界面已打开"+name);
            return null;
        }
        //检查路径是否有配置
        string path = "";
        if(!pathDict.TryGetValue(name,out path))
        {
            
            Debug.LogError("界面路径配置错误"+name);
            return null;
        }
        //使用缓存的预制件
        GameObject panelPrefab = null;
        if(!prefabDict.TryGetValue(name,out panelPrefab))
        {
            string realPath = "prefab/UI/"+path;
            Debug.Log(realPath);
            panelPrefab = Resources.Load<GameObject>(realPath) as GameObject;
            if(panelPrefab==null){Debug.Log("加载失败");}
        }
        Canvas canvas = GameObject.FindObjectOfType<Canvas>();
        //打开界面
        GameObject panelObject = GameObject.Instantiate(panelPrefab,canvas.transform,false);
        
        panel = panelObject.GetComponent<BasePanel>();
        panelDict.Add(name,panel);
        Debug.Log("界面打开完成"+name);
        return panel;
        
    }

    public bool ClosePanel(string name)
    {
        BasePanel panel  = null;
        if(!panelDict.TryGetValue(name,out panel))
        {
            Debug.LogError("界面未打开"+name);
            return false;
        }
        
        panel.ClosePanel(name);    
        return true;
    }
    /// <summary>
    /// 公共方法，获取对应的面板
    /// </summary>
    /// <param name="basePanelName"></param>
    /// <returns></returns>
    public BasePanel GetBasePanel(string basePanelName)
    {
        if (panelDict.TryGetValue(basePanelName, out BasePanel basePanel))
        {
            return basePanel;
        }
        else
        {
            Debug.LogError("没有对应的预设"+basePanelName);
            return null;
        }
    }




    public class UIConst
    {
        public const string MainMenuPanel = "";

        public const string UserPanel = "";

        public const string NewMenuPanel = "";

        public const string continueButton = "continueButton";

        public const string Dialogue = "Dialogue";

        public const string gameOver = "gameOver";

        public const string DeskUI = "DeskUI";

        public const string ryLevel = "ryLevel";
        public const string pauseUI = "pauseUI";
        public const string PackageUi = "PackageUi";
    }
}
