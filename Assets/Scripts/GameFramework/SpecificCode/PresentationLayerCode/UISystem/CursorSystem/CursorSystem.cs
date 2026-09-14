using UnityEngine;
//*****************************************
//功能说明：光标管理器
// 玩法中隐藏光标；打开背包等 UI 时 ShowForUI，关闭时 HideForUI
// 多个 UI 叠加用引用计数，全部关完才重新隐藏
//*****************************************
public class CursorSystem : ICursorSystem
{
    private int uiLockCount;
    private bool inGameplay;

    public bool IsVisible => Cursor.visible;
    public int UiLockCount => uiLockCount;

    public void Init()
    {
        // 启动先进登录菜单，光标可见
        EnterMenu();
    }

    public void EnterGameplay()
    {
        inGameplay = true;
        uiLockCount = 0;
        ApplyHide();
    }

    public void EnterMenu()
    {
        inGameplay = false;
        uiLockCount = 0;
        ApplyShow();
    }

    public void ShowForUI()
    {
        uiLockCount++;
        ApplyShow();
    }

    public void HideForUI()
    {
        uiLockCount = Mathf.Max(0, uiLockCount - 1);
        if (uiLockCount > 0)
        {
            ApplyShow();
            return;
        }

        if (inGameplay)
        {
            ApplyHide();
        }
        else
        {
            ApplyShow();
        }
    }

    private static void ApplyShow()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private static void ApplyHide()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}
