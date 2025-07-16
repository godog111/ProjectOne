using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PackageLocalData
{
    private static PackageLocalData _instance;

    public static PackageLocalData Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new PackageLocalData();
            }
            return _instance;
        }
    }
    public List<PackageLocalItem> items;

    public void SavePackage()
    {
        string savejson = JsonUtility.ToJson(this);
        PlayerPrefs.SetString("PackageLocalData", savejson);
        PlayerPrefs.Save();
    }
    public List<PackageLocalItem> LoadPackage()
    {
        if (items != null)
        {
            return items;
        }
        if (PlayerPrefs.HasKey("PackageLocalData"))
        {
            string loadjson = PlayerPrefs.GetString("PackageLocalData");
            PackageLocalData data = JsonUtility.FromJson<PackageLocalData>(loadjson);
            items = data.items;
            return items;
        }
        else
        {
            items = new List<PackageLocalItem>();
            return items;
        }

    }

    public PackageLocalItem GetPackageLoacalItemByUid(string uid)
    {
        List<PackageLocalItem> packageLocalItems = LoadPackage();
        foreach (PackageLocalItem item in packageLocalItems)
        {
            if (item.uid == uid)
            {
                return item;
            }
        }
        return null;
    }
}

[System.Serializable]
public class PackageLocalItem
{
    public string uid;
    public int id;
    public int num;
    public int level;//等级

    public override string ToString()
    {
        return string.Format("uid:{0},id:{1},num:{2},level:{3}", uid, id, num, level);
    }

}
