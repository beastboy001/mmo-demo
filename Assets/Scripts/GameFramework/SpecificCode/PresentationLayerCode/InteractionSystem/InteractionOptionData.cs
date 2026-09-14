using System;
using UnityEngine;

//*****************************************
//功能说明：左侧交互列表里「一条选项」的数据（不是 UI 预制体）
// 例：走近 NPC 后出现的「对话」这一条
// 真正画出来的是 InteractionOption 预制体 + InteractionOptionView
//*****************************************
public class InteractionOptionData
{
    /// <summary>显示名，写在 InteractionContent 上，例如「对话」</summary>
    public string displayName;

    /// <summary>按键提示文字，写在 TextKeyTip 上，例如 "F"（只负责显示）</summary>
    public string keyTip = "F";

    /// <summary>真正检测的按键。玩家按下这个键才会执行 onExecute</summary>
    public KeyCode key = KeyCode.F;

    /// <summary>
    /// 这条选项属于谁（一般是 NpcInteractable）
    /// 离开范围时用 owner 把该 NPC 的所有选项一次性删掉
    /// </summary>
    public object owner;

    /// <summary>按确认键后要做的事（NPC 对话时通常是 StartTalk）</summary>
    public Action onExecute;
}
