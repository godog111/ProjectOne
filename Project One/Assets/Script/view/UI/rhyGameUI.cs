using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class rhyGameUI : BasePanel
{
    public Button pause;
    // Start is called before the first frame update
    void Start()
    {
        pause = GameObject.Find("pause").GetComponent<Button>();
        pause.onClick.AddListener(pauseGame);
    }

    private void pauseGame()
    {
        RhythmManager.Instance.PauseGame(true);
        UIModel.Instance.OpenPanel("pauseUI");
    }


}
