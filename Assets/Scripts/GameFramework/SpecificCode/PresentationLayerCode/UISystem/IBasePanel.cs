using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//*****************************************
//功能说明：面板基类接口
//***************************************** 
public interface IBasePanel: IController
{
    /// <summary>
    /// 初始化
    /// </summary>
    public void OnInit();
    /// <summary>
    /// 显示
    /// </summary>
    public void OnShow(params object[] objs);
    /// <summary>
    /// 关闭
    /// </summary>
    public void OnClose();
}
