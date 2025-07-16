using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class textbutton : MonoBehaviour
{
    public Button bt;
    // Start is called before the first frame update
    void Start()
    {
        // 按钮点击事件处理方法
        bt = GetComponent<Button>();
        bt.onClick.AddListener(OnClick);
        
    }

    void OnClick()
    {
            Debug.Log("Button clicked!");
            Game.Instance.LoadScene(4);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
