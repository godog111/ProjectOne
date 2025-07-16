using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class textdianji : MonoBehaviour
{
    void Start()
    {
        // 获取按钮组件并添加点击事件监听
        GetComponent<Button>().onClick.AddListener(OnButtonClick);
    }
    
    void OnButtonClick()
    {
        Debug.Log("按钮被点击了！");
    }
}
