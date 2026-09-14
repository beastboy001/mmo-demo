using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
//*****************************************
//功能说明：游戏架构接口
//***************************************** 
public interface IArchitecture
{
    /// <summary>
    /// 注册系统
    /// </summary>
    /// <typeparam name="U"></typeparam>
    /// <param name="instance"></param>
    void RegistSystem<U>(U instance) where U : ISystem;
    /// <summary>
    /// 注册模型
    /// </summary>
    /// <typeparam name="U"></typeparam>
    /// <param name="instance"></param>
    void RegistModel<U>(U instance) where U : IModel;
    /// <summary>
    /// 注册工具
    /// </summary>
    /// <typeparam name="U"></typeparam>
    /// <param name="instance"></param>
    void RegistUtility<U>(U instance) where U : IUtility;
    /// <summary>
    /// 获取系统层对象
    /// </summary>
    /// <typeparam name="U"></typeparam>
    /// <returns></returns>
    public U GetSystem<U>() where U :class,ISystem;
    /// <summary>
    /// 获取模型层对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public U GetModel<U>() where U : class, IModel;
    /// <summary>
    /// 获取工具层对象
    /// </summary>
    /// <typeparam name="U"></typeparam>
    /// <returns></returns>
    public U GetUtility<U>() where U : class, IUtility;
    /// <summary>
    /// 注册事件
    /// </summary>
    /// <typeparam name="U"></typeparam>
    /// <param name="onEvent"></param>
    /// <returns></returns>
    void RegistEvent<U>(Action<object> onEvent) where U : new();
    /// <summary>
    /// 注销事件
    /// </summary>
    /// <typeparam name="U"></typeparam>
    /// <param name="onEvent"></param>
    void UnRegistEvent<U>(Action<object> onEvent) where U : new();
    /// <summary>
    /// 发送事件
    /// </summary>
    /// <typeparam name="U"></typeparam>
    /// <param name="dataObj"></param>
    void SendEvent<U>(object dataObj) where U : new();
    /// <summary>
    /// 发送命令
    /// </summary>
    /// <typeparam name="U"></typeparam>
    /// <param name="dataObj"></param>
    void SendCommand<U>(object dataObj) where U :ICommand, new();
    /// <summary>
    /// 初始化框架中所有的模块
    /// </summary>
    void InitAllModules();
}
