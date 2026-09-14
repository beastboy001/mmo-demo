using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//*****************************************
//功能说明：命令接口
//***************************************** 
public interface ICommand:ICanGetModel,ICanGetSystem,ICanGetUtility,ICanSendEvent,ICanSendCommand
{//命令可以跨模块，和其他板块不耦合，而且不需要存储
    //Execute执行的意思
    public void Execute(object dataObj);
}

//继承该接口的 具体命令 可以获取模型层 获取系统层获取工具层 可以发送事件 可以发送命令 
