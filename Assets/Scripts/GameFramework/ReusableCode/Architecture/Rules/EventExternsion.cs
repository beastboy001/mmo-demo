using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
//*****************************************
//功能说明：事件系统的支持扩展
//***************************************** 
public static class EventExternsion
{
    public static void SendEvent<T>(this ICanSendEvent self, object dataObj=null) where T:new ()
    {
        StartArchitecture.Instance.GetArchitecture().SendEvent<T>(dataObj);
    }

    public static void RegistEvent<T>(this ICanRegistAndUnRegistEvent self, Action<object> onEvent) where T : new()
    {
        StartArchitecture.Instance.GetArchitecture().RegistEvent<T>(onEvent);
    }

    public static void UnRegistEvent<T>(this ICanRegistAndUnRegistEvent self, Action<object> onEvent) where T : new()
    {
        // 原魔兽课这里误写成 RegistEvent，已改为真正注销
        StartArchitecture.Instance.GetArchitecture().UnRegistEvent<T>(onEvent);
    }
}
