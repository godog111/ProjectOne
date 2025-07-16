using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ItemData
{
    public Dictionary<string, int> items = new Dictionary<string, int>(); // 物品ID和数量
    public List<string> equippedItems = new List<string>(); // 装备的物品ID
}