using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
//*****************************************
//功能说明：
//***************************************** 
public class GameEventSystem : IEventSystem
{
    //事件注册字典
    private Dictionary<Type, IEventRegistraion> eventRegistrationsDict = new Dictionary<Type, IEventRegistraion>();

    public void Regist<T>(Action<object> onEvent)     //T你要监听的事件的类型//T 是你要监听的具体事件类，比如 PlayerHurtEvent, GameStartEvent 等
    {
        var type = typeof(T);  //获取需要注册的事件的类型
        IEventRegistraion eventRegistraion;  //创建一个父类容器
        if (eventRegistrationsDict.TryGetValue(type,out eventRegistraion))    //TryGetValue这个方法需要out参数//意思就是 查看字典里面是否有这个type这个key 有的话就把 value 扔给输出给 eventRegistraion
        {
            (eventRegistraion as EventRegistration).OnEvent += onEvent;    //eventRegistraion这个是接口类型要转为实际类型（父类转换成子类 ）
        }     //把需要监听的 地方的方法 存到委托里面                       //光是接口没有这个方法，所以要转换一下类型
        else
        {
            eventRegistraion = new EventRegistration()   //等价于：eventRegistraion = new EventRegistration(); // 1. 创建对象
            {                                                   //eventRegistraion.OnEvent = onEvent;         // 2. 设置属性
                OnEvent = onEvent
            };
            eventRegistrationsDict.Add(type,eventRegistraion);
        }
    }

    public void Send<T>(object obj) where T : new()
    {
        var type = typeof(T);
        IEventRegistraion eventRegistraion;   //创建具体的消息内容
        if (eventRegistrationsDict.TryGetValue(type, out eventRegistraion))
        {
            (eventRegistraion as EventRegistration).OnEvent.Invoke(obj);
        }
    }

    public void UnRegist<T>(Action<object> onEvent)
    {
        var type = typeof(T);
        IEventRegistraion eventRegistraion;
        if (eventRegistrationsDict.TryGetValue(type, out eventRegistraion))
        {
            (eventRegistraion as EventRegistration).OnEvent -= onEvent;
        }
    }
}
