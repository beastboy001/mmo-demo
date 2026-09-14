using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//*****************************************
//功能说明：表现层接口
//***************************************** 
public interface IController:ICanGetSystem,ICanSendEvent,ICanRegistAndUnRegistEvent,ICanSendCommand
{
    //继承该接口的表现层 可以获取系统层可以发送事件 可以注册或注销事件 可以发送命令 
}
