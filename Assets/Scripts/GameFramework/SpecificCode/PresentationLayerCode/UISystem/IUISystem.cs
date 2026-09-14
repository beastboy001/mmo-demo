using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//*****************************************
//功能说明：UI系统接口
//***************************************** 
public interface IUISystem : ISystem
{
    /// <summary>
    /// 打开该类型的面板
    /// </summary>
    /// <param name="panelType"></param>
    public void OpenPanel<T>(params object[] objs) where T : BasePanel;
    /// <summary>
    /// 关闭该类型的面板
    /// </summary>
    /// <param name="panelType"></param>
    public void ClosePanel(string panelType);
    /// <summary>
    /// 加载游戏面板
    /// </summary>
    public void UnLoadLoginPanelsAndLoadGamePanels();
    /// <summary>
    /// 当前选中 / 正在编辑的角色（全局唯一一份，各处直接读写）
    /// </summary>
    PlayerData ChoiePlayerData { get; }

    /// <summary>
    /// 打开创建角色界面时调用：重置为新建角色默认值
    /// </summary>
    void ResetCreatePlayerData();

    /// <summary>
    /// 从外部数据覆盖当前角色（选角、服务器下发时用，不会 new 第二份）
    /// </summary>
    void SetChoiePlayerData(PlayerData data);

    /// <summary>账号下所有角色（选角列表用）</summary>
    IReadOnlyList<PlayerData> PlayerDataList { get; }

    int PlayerDataCount { get; }

    PlayerData GetPlayerDataAt(int index);

    void AddPlayerData(PlayerData data);

    void RemovePlayerDataAt(int index);

    void ClearPlayerDataList();

    void RemovePlayerDataByCharacterId(int characterId);
}
