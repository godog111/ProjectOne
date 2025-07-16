using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
/// <summary>
/// 所有控制器的基类，负责协调Model和View
/// 节奏游戏特别添加了节拍处理相关功能
/// </summary>
public abstract class Controller 
  
{
    protected T GetModel<T>() where T:Model {
        return MVC.GetModel<T>()as T;
    }
    
    protected T GetView<T>() where T:View{
        return MVC.GetView<T>()as T;
    }

    protected void RegisterModel(Model model)
    {
        MVC.RegisterModel(model);
    }

    protected void RegisterView(View view)
    {
        MVC.RegisterView(view);
    }

    protected void RegisterController(string eventName,Type controllerType)
    {
        MVC.RegisterController(eventName,controllerType);
    }

    public abstract void Execute(object data);
}
