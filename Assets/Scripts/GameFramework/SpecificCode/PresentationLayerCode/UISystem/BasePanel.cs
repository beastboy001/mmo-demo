using UnityEngine;
//*****************************************
//功能说明：面板基类
// needCursor = true 的面板：打开显示光标，关闭按引用计数隐藏
//***************************************** 
public class BasePanel : MonoBehaviour, IBasePanel
{
    protected IUISystem uiSystem;
    protected ICursorSystem cursorSystem;

    /// <summary>是否需要鼠标（背包、设置等）。登录菜单本身已在 CursorSystem.EnterMenu 显示，一般不用开。</summary>
    protected virtual bool NeedCursor => false;

    private bool cursorRequested;

    public virtual void OnClose()
    {
        ReleaseCursorIfNeeded();
        gameObject.SetActive(false);
    }

    public virtual void OnInit()
    {
        uiSystem = this.GetSystem<IUISystem>();
        cursorSystem = this.GetSystem<ICursorSystem>();
        gameObject.SetActive(false);
    }

    public virtual void OnShow(params object[] objs)
    {
        gameObject.SetActive(true);
        RequestCursorIfNeeded();
    }

    private void RequestCursorIfNeeded()
    {
        if (!NeedCursor || cursorRequested || cursorSystem == null)
        {
            return;
        }

        cursorSystem.ShowForUI();
        cursorRequested = true;
    }

    private void ReleaseCursorIfNeeded()
    {
        if (!cursorRequested || cursorSystem == null)
        {
            return;
        }

        cursorSystem.HideForUI();
        cursorRequested = false;
    }
}
