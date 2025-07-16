using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PackageTable : MonoBehaviour
{

}
[System.Serializable]
public class ItemArgs
{
    public int id;
    public string name;
    public int type;
    public int amount;

    public string description;//物品描述

    public string icon;//物品图标路径

}
[System.Serializable]
public class ItemList
{
    public List<ItemArgs> item;
}
