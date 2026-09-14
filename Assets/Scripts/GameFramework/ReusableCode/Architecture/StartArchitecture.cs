using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//*****************************************
//功能说明：启动框架的启动器（启动入口）//在GameStarter中实例化并且使用
//***************************************** 
public class StartArchitecture : Singleton<StartArchitecture>, ISingleton
{
    private IArchitecture gameArchitecture;

    //构造函数//private防止外界通过 new StartArchitecture() 随意创建多个实例，保证单例的唯一性。
    private StartArchitecture() { Init(); }

    public void Init() { }
    /// <summary>
    /// 实例化并设置不同项目的架构
    /// </summary>
    public void SetGameArchitecture(IArchitecture architecture)
    {
        gameArchitecture = architecture;    // 给字段赋值
    }
    /// <summary>
    /// 获取架构实例
    /// </summary>
    public IArchitecture GetArchitecture()
    {
        return gameArchitecture;       // 返回字段的值
    }
    /// <summary>
    /// 初始化框架中的所有模块
    /// </summary>
    public void InitAllModulesInArchitecture()
    {
        gameArchitecture.InitAllModules();
    }
}
