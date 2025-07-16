using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public static class MVC 
{

    public static Dictionary<string,Model>Models = new Dictionary<string, Model>();//名字——模型
    public static Dictionary<string,View>Views = new Dictionary<string, View>();//名字——模型
    public static Dictionary<string,Type>Comand = new Dictionary<string, Type>();//名字——模型
    // Start is called before the first frame update
    //注册模型
    public static void RegisterModel(Model model)
    {
        Models[model.Name]=model;
    }

    //防止重复注册

    public static void RegisterView(View view)
    {
        Debug.Log("注册Vive成功"+view.name);
        if(Views.ContainsKey(view.Name))
            Views.Remove(view.Name);

        //注册关心事件
        view.RegisterEvents();

        Views[view.Name]=view;
    }

    public static void RegisterController(string eventName,Type ControllerType)
    {
        Comand[eventName]=ControllerType;
        Debug.Log("注册控制器成功"+eventName);
    }

    public static T GetModel<T>()where T:Model
    {
        foreach(Model m in Models.Values)
        {
            if(m is T)
            return (T)m;
        }
        return null;
    }

    public static T GetView<T>()where T:View
    {
        foreach(View m in Views.Values)
        {
            if(m is T)
            return (T)m;
        }
        return null;
    }
    //发送事件
    public static void SendEvent(string eventName,object data = null)
    {
        //Debug.Log("发送事件"+eventName);
        if(Comand.ContainsKey(eventName))
        {
            Debug.Log("包含事件"+eventName);
            Type t =Comand[eventName];
            Controller C =Activator.CreateInstance(t) as Controller;

            C.Execute(data);
        }
        else{Debug.Log("不包含事件"+eventName);}

        //视图响应事件
        foreach(View v in Views.Values)
        {   
            if(v.AttationEvents.Contains(eventName))
            {
                //视图响应
                v.HandleEvent(eventName, data);
            }
        }
    }
}
