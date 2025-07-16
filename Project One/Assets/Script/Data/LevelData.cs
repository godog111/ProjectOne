using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LevelData
{
    public int id;
    public string name;
    public string description;
    public string imagePath;
    public string sceneName;
}

[System.Serializable]
public class LevelDataWrapper
{
    public List<LevelData> levels;
}