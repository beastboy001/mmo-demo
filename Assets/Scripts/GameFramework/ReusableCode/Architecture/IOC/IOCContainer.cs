using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
//*****************************************
//功能说明：IOC容器，保存所有层级以及各个模块的实例
//***************************************** 
public class IOCContainer
{
    /// <summary>
    /// 实例字典
    /// </summary>
    private Dictionary<Type, object> instancesDict = new Dictionary<Type, object>();
    /// <summary>
    /// 注册
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="instance"></param>
    public void Register<T>(T instance)
    {
        var key = typeof(T);
        if (instancesDict.ContainsKey(key))
        {
            instancesDict[key] = instance;
        }
        else
        {
            instancesDict.Add(key,instance);
        }
    }
    /// <summary>
    /// 获取实例
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T Get<T>() where T : class
    {
        var key = typeof(T);
        object obj = null;
        if (instancesDict.TryGetValue(key,out obj))    //key：要查找的键      //instancesDict.TryGetValue(key, out obj) 可以作为判断条件是因为它的返回值是 bool 类型。
        {
            return obj as T;
        }
        else
        {
            Debug.Log("想要获取的对象为空");
        }
        return null;
    }
    /// <summary>
    /// 调用容器中所有实例的初始化方法
    /// </summary>
    public void InitAllModules()
    {
        foreach (var item in instancesDict)
        {
            ((INeedInit)item.Value).Init();    //这里的Value是字典中键对应的值（object），强制转换成接口类型
        }
    }
}
