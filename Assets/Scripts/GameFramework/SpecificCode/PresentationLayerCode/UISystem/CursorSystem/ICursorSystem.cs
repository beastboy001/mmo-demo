using UnityEngine;
//*****************************************
//功能说明：光标系统接口
// 游戏中隐藏光标；打开背包等 UI 时显示
//*****************************************
public interface ICursorSystem : ISystem
{
    bool IsVisible { get; }
    int UiLockCount { get; }

    /// <summary>进入可操作玩法：隐藏并锁定光标，清空 UI 引用计数</summary>
    void EnterGameplay();

    /// <summary>回到登录/选人等菜单：显示光标，清空引用计数</summary>
    void EnterMenu();

    /// <summary>打开需要鼠标的 UI（可叠加，引用计数 +1）</summary>
    void ShowForUI();

    /// <summary>关闭需要鼠标的 UI（引用计数 -1，归零后回到玩法隐藏）</summary>
    void HideForUI();
}
