using UnityEngine;
//*****************************************
//功能说明：游戏架构柜子。对照魔兽课 WowArchitercture。
// Init 里只注册模块；NetSystem / UISystem 等你自己写好后再取消注释。
////在GameStarter中实例化并且使用
//*****************************************
public class GameArchitercture : Architecture<GameArchitercture>
{
    protected override void Init()
    {
        this.RegistSystem<INetSystem>(new NetSystem());
        this.RegistSystem<ICursorSystem>(new CursorSystem());
        this.RegistSystem<IUISystem>(new UISystem());

        Debug.Log("[GameArchitercture] Init：已注册 NetSystem / CursorSystem / UISystem");
    }
}
