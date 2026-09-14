using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
//*****************************************
//功能说明：事件
//事件委托容器
//***************************************** 
public class EventRegistration : IEventRegistraion
{
    //Action表示无参无返回值类型的委托 
    //Action<T>表示有参无返回值类型的委托 
    public Action<object> OnEvent = (object obj) => { };
    //OnEvent 是一个可以装很多方法的“委托链
}




//// 等价于以下完整写法：
//public Action<object> OnEvent = delegate (object obj)  //这个是匿名函数
//{
//    // 空实现，什么也不做
//};

//    // 也等价于：
//    public Action<object> OnEvent = (object obj) =>     //lambad表达式
//    {
//        // 空实现
//    };

//    // 或者用方法定义：
//    public Action<object> OnEvent = DefaultEventHandler;

//private static void DefaultEventHandler(object obj)
//{
//    // 空实现
//}
