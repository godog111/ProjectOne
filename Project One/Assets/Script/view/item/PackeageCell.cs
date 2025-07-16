using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PackageCell : MonoBehaviour,IPointerClickHandler,IPointerEnterHandler,IPointerExitHandler
{
    private Transform UIIcon;

    private Transform UITopBg;
    private Transform UISelect;

    private Transform UILevel;
    private Transform UIDeleteSelect;
    private Transform UIName;

    private PackageLocalItem packageLocaData;//当前物品的动态数据

    private ItemArgs packageTable;//当前物品的静态数据

    private PackageUi packageUi;

    private List<ItemArgs> timeList;

    private void Awake()
    {
        InitUIName();
    }

    private void InitUIName()
    {
        UIIcon = transform.Find("Top/Icon");
        UITopBg = transform.Find("Top/bg");
        UISelect = transform.Find("Select");
        UIName = transform.Find("Bottom/itemName");
        UIDeleteSelect = transform.Find("UIDeleteSelect");
    }

    public void Refresh(PackageLocalItem packageLocaData, PackageUi packageUi)
    {
        //数据初始化
        this.packageLocaData = packageLocaData;
        this.packageTable = StaticDataManager.GetItemArgs(packageLocaData.id);
        this.packageUi = packageUi;
        //信息初始化
        Debug.Log(packageTable.name);
        UIName.GetComponent<Text>().text = packageTable.name;
        //物品图片
    }



    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("点击了" + eventData.ToString());
        if (this.packageUi.ChooseUid == this.packageLocaData.uid)
        {
            return;
        }
        this.packageUi.ChooseUid = this.packageLocaData.uid;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
         Debug.Log("进入"+eventData.ToString());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("退出"+eventData.ToString());
    }
}
