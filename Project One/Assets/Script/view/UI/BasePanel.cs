using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasePanel : View
{

    protected bool isRemove =false;//是否被关闭

    protected new string name;
    
    public override string Name => throw new System.NotImplementedException();

    public override void HandleEvent(string eventName, object data)
    {
        throw new System.NotImplementedException();
    }

    protected virtual void Awake()
    {

    }

    public virtual void OpenPanel(string name)
    {
        this.name = name;
        gameObject.SetActive(true);
    }

    public virtual void ClosePanel(string name)
    {
        isRemove=true;
        gameObject.SetActive(false);
        Destroy(gameObject);

        if(UIModel.Instance.panelDict.ContainsKey(name))
        {
            UIModel.Instance.panelDict.Remove(name);
        }
    }
}
