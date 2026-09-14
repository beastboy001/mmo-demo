using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//*****************************************
//功能说明：系统层接口
//***************************************** 
public interface ISystem :INeedInit,ICanSendCommand,ICanSendEvent,ICanRegistAndUnRegistEvent
{
    //需要初始化 可以发送命令 可以发送事件 可以注册或注销事件 
}
