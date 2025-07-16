using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class PackageUi : BasePanel
{
    private Transform UIMenu;

    private Transform UIScrollView;

    private Transform UIDetail;

    public GameObject packeageCell;



    private string _chooseUid;

    public string ChooseUid
    {
        get { return _chooseUid; }
        set
        {
            _chooseUid = value;
            RefreshDetail();
        }
    }



    GameModel gModel =MVC.GetModel<GameModel>();

    private void Start()
    {
        RefreshUI();
    }

    override protected void Awake()
    {
        base.Awake();
        InitUI();
    }

    private void InitUI()
    {
        InitUIName();
        InitClick();
    }

    private void RefreshUI()
    {
        RefreshScroll();
    }

    private void RefreshDetail()
    {

        PackageLocalItem localItem = PackageLocalData.Instance.GetPackageLoacalItemByUid(_chooseUid);
        UIDetail.GetComponent<PackageDetail>().Refresh(localItem,this);
    }

    private void RefreshScroll()
    {

        //清空滚动条列表
        RectTransform scrollContent = UIScrollView.GetComponent<ScrollRect>().content;
        for (int i = 0; i < scrollContent.childCount; i++)
        {
            Destroy(scrollContent.GetChild(i).gameObject);
        }

        foreach (var item in gModel.GetSortPackageLocalData())
        {
            Debug.Log(gModel.GetSortPackageLocalData().Count);
            Transform cell = Instantiate(packeageCell.transform, scrollContent) as Transform;
            PackageCell packageCell = cell.GetComponent<PackageCell>();
            if (packageCell == null)
            {
                Debug.LogError("PackageCell为空");
            }
            packageCell.Refresh(item, this);
        }


    }

    private void InitUIName()
    {
        //初始化UI
        UIMenu = transform.Find("UIMenu");
        UIScrollView = transform.Find("Center/Scroll View");
        UIDetail = transform.Find("Center/DetailPanel");
    }

    private void InitClick()
    {
        //初始化点击事件
    }
}
