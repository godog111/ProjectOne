using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class View : MonoBehaviour
{
    public abstract string Name{get;}

    public List<string> AttationEvents = new List<string>();

    //注册关心事件
    public virtual void RegisterEvents()
    {

    }

    public abstract void HandleEvent(string eventName, object data);

    protected T GetModel<T>() where T:Model
    {
        return MVC.GetModel<T>() as T;
    }

    protected void SendEvent(string eventName,object data = null)
    {
        MVC.SendEvent(eventName,data);
    }

}
