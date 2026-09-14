using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//*****************************************
//功能说明：可以发送命令的拓展方法
//***************************************** 
public static class CanSendCommandExternsion//这个名字无所谓就是用来存储下面方法的
{
    public static void SendCommand<T>(this ICanSendCommand self,object dataObj=null) where T:ICommand,new ()//new() 是 C# 中的泛型约束，表示泛型参数 T 必须有一个无参的公共构造函数。
    {//这里的self只是一个参数名字 
        StartArchitecture.Instance.GetArchitecture().SendCommand<T>(dataObj);
    }
}
//也就是说这个代码就是为ICanSendCommand 这个接口拓展了一个成员方法继承了这个接口的类实例化之后就可以.使用这个方法
