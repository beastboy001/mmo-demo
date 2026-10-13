using UnityEngine;

//*****************************************
// 功能说明：热更层入口
// AOT 的 GameStarter 通过【反射】调用这里的 Start()，
// 整个游戏（架构 + 所有 System）从这里开始运转。
//
// 为什么要绕一层反射？
//   因为 AOT 程序集【不能编译期引用】热更程序集，
//   反射是唯一能跨过去的方式。
//*****************************************
public static class GameBootstrap
{
    private static IArchitecture architecture;

    public static void Start()
    {
        Debug.Log("[GameBootstrap] 热更层启动");

        // 这一步以前写在 GameStarter 里，现在搬到热更层
        architecture = new GameArchitercture();
        StartArchitecture.Instance.SetGameArchitecture(architecture);
        StartArchitecture.Instance.InitAllModulesInArchitecture();

        Debug.Log("[GameBootstrap] 架构已启动（来自热更层 Game.Hotfix）");
    }
}
