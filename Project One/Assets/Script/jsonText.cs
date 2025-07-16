using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class jsonText : MonoBehaviour
{
    [SerializeField] private InputField input;
    [SerializeField] private Button Abutton;
    [SerializeField] private Button Bbutton;
    [SerializeField] private Button Cbutton;
    [SerializeField] private Button Dbutton;
    [SerializeField] private GameObject settingsPanel;

    private List<ItemArgs> textJson; // 关联的对话列表

    private void Start()
    {
        //input.onEndEdit.AddListener(OnInputEnd);

        Abutton.onClick.AddListener(tiqu);
        Bbutton.onClick.AddListener(CreateLocalPackageData);
        Cbutton.onClick.AddListener(ReadLocalPackageData);
        Dbutton.onClick.AddListener(OpenPackagePanel);


    }
    void Update()
    {

    }

    private void tiqu()
    {

    }
    private void OpenPackagePanel()
    {
        UIModel.Instance.OpenPanel("PackageUi");
        //settingsPanel.SetActive(true);
    }

    void OnInputEnd(string text)
    {
        Debug.Log("用户输入: " + text);
        textJson = StaticDataManager.GetDataList<ItemArgs>(text);
        if (textJson == null) { Debug.Log("数据为空"); } else { Debug.Log(textJson.Count); }

        foreach (var tj in textJson)
        {
            Debug.Log($"对话ID: {tj.id}, 内容: {tj.description}");
        }
    }

    private static void CreateLocalPackageData()
    {
        //保存数据测试
        PackageLocalData.Instance.items = new List<PackageLocalItem>();
        for (int i = 1; i < 9; i++)
        {
            PackageLocalItem packageLocalItem = new()
            {
                uid = Guid.NewGuid().ToString(),
                id = i,
                num = i,
                level = i
            };
            PackageLocalData.Instance.items.Add(packageLocalItem);
        }
        PackageLocalData.Instance.SavePackage();
        Debug.Log("保存数据成功");
    }

    private static void ReadLocalPackageData()
    {
        List<PackageLocalItem> items = PackageLocalData.Instance.LoadPackage();
        foreach (var item in items)
        {
            Debug.Log($"uid:{item.uid},id:{item.id},num:{item.num},level:{item.level}");
        }
    }

}
