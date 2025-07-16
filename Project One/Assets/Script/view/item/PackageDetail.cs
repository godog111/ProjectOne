using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PackageDetail : MonoBehaviour
{
    private Transform UIItemName;

    private Transform UIItemIcon;
    private Transform UIItemDesc;

    private PackageLocalItem packageLocaData;//当前物品的动态数据

    private ItemArgs packageTable;//当前物品的静态数据

    private PackageUi packageUi;

    private void Awake()
    {
        InitUIName();

        Refresh(PackageLocalData.Instance.LoadPackage()[0], null);
    }

    private void InitUIName()
    {
        UIItemName = transform.Find("top/itemName");
       // UIItemIcon = transform.Find("UIItemIcon");
        UIItemDesc = transform.Find("center/itemDesc");
    }

    public void Refresh(PackageLocalItem packageLocaData, PackageUi packageUi)
    {
        //初始化数据，父节点逻辑
        this.packageLocaData = packageLocaData;
        this.packageTable = StaticDataManager.GetItemArgs(packageLocaData.id);
        this.packageUi = packageUi;
        //初始化UI
        UIItemName.GetComponent<Text>().text = packageTable.name;
       // UIItemIcon.GetComponent<Image>().sprite = Resources.Load<Sprite>("ItemIcon/" + packageTable.icon);
        UIItemDesc.GetComponent<Text>().text = packageTable.description;
    }
}
